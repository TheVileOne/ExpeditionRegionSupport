using BepInEx.Logging;
using LogUtils.Properties;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace LogUtils
{
    public static class GameHooks
    {
        private static List<IDetour> managedHooks = new List<IDetour>();

        /// <summary>
        /// Generates managed hooks, and applies all hooks used by the utility module
        /// </summary>
        internal static void Initialize()
        {
            try
            {
                Type type = typeof(ManualLogSource);
                MethodInfo method = type.GetMethod(nameof(ManualLogSource.Log));

                //Allows LogRules to apply to BepInEx log traffic
                managedHooks.Add(new Hook(method, bepInExLogProcessHook));
                managedHooks.Add(new ILHook(method, bepInExLogProcessHookIL));
                Apply();
            }
            catch (Exception ex)
            {
                UtilityCore.LogError("Error occurred while loading hooks", ex);
            }
        }

        /// <summary>
        /// Apply hooks used by the utility module
        /// </summary>
        internal static void Apply()
        {
            if (!UtilityCore.IsControllingAssembly) return; //Only the controlling assembly is allowed to apply the hooks

            On.RainWorld.Awake += RainWorld_Awake;
            IL.RainWorld.Awake += RainWorld_Awake;
            On.RainWorld.OnDestroy += RainWorld_OnDestroy;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            On.RainWorld.PreModsInit += RainWorld_PreModsInit;
            On.ModManager.WrapModInitHooks += ModManager_WrapModInitHooks;

            //Signal system
            On.RainWorld.Update += RainWorld_Update;

            //Log property handling
            On.RainWorld.HandleLog += RainWorld_HandleLog;
            IL.RainWorld.HandleLog += RainWorld_HandleLog;

            On.Expedition.ExpLog.Log += ExpLog_Log;
            On.Expedition.ExpLog.LogOnce += ExpLog_LogOnce;

            On.Expedition.ExpLog.ClearLog += ExpLog_ClearLog;
            IL.Expedition.ExpLog.ClearLog += ExpLog_ClearLog;
            IL.Expedition.ExpLog.Log += ExpLog_Log;
            IL.Expedition.ExpLog.LogOnce += ExpLog_LogOnce;
            IL.Expedition.ExpLog.LogChallengeTypes += ExpLog_LogChallengeTypes;

            On.JollyCoop.JollyCustom.Log += JollyCustom_Log;
            On.JollyCoop.JollyCustom.CreateJollyLog += JollyCustom_CreateJollyLog;
            IL.JollyCoop.JollyCustom.CreateJollyLog += JollyCustom_CreateJollyLog;
            IL.JollyCoop.JollyCustom.Log += JollyCustom_Log;
            IL.JollyCoop.JollyCustom.WriteToLog += JollyCustom_WriteToLog;

            managedHooks.ForEach(hook => hook.Apply());
            UtilityCore.BaseLogger.LogInfo("Hooks loaded successfully");
        }

        private static void ModManager_WrapModInitHooks(On.ModManager.orig_WrapModInitHooks orig)
        {
            //Applying this here as it is guaranteed to run before any hooks are applied in typical channels
            LogFilter.ActivateKeyword(UtilityConsts.FilterKeywords.ACTIVATION_PERIOD_STARTUP);
            orig();
        }

        /// <summary>
        /// Releases, and then reapply hooks used by the utility module 
        /// </summary>
        public static void Reload()
        {
            if (!UtilityCore.IsControllingAssembly) return;

            Unload();
            Apply();
        }

        /// <summary>
        /// Releases all hooks used by the utility module
        /// </summary>
        public static void Unload()
        {
            if (!UtilityCore.IsControllingAssembly) return;

            On.RainWorld.Awake -= RainWorld_Awake;
            IL.RainWorld.Awake -= RainWorld_Awake;
            On.RainWorld.OnDestroy -= RainWorld_OnDestroy;
            On.RainWorld.OnModsInit -= RainWorld_OnModsInit;
            On.RainWorld.PostModsInit -= RainWorld_PostModsInit;
            On.RainWorld.PreModsInit -= RainWorld_PreModsInit;
            On.ModManager.WrapModInitHooks -= ModManager_WrapModInitHooks;

            //Signal system
            On.RainWorld.Update -= RainWorld_Update;

            On.RainWorld.HandleLog -= RainWorld_HandleLog;
            IL.RainWorld.HandleLog -= RainWorld_HandleLog;

            On.Expedition.ExpLog.Log -= ExpLog_Log;
            On.Expedition.ExpLog.LogOnce -= ExpLog_LogOnce;

            On.Expedition.ExpLog.ClearLog -= ExpLog_ClearLog;
            IL.Expedition.ExpLog.ClearLog -= ExpLog_ClearLog;
            IL.Expedition.ExpLog.Log -= ExpLog_Log;
            IL.Expedition.ExpLog.LogOnce -= ExpLog_LogOnce;
            IL.Expedition.ExpLog.LogChallengeTypes -= ExpLog_LogChallengeTypes;

            On.JollyCoop.JollyCustom.Log -= JollyCustom_Log;
            On.JollyCoop.JollyCustom.CreateJollyLog -= JollyCustom_CreateJollyLog;
            IL.JollyCoop.JollyCustom.CreateJollyLog -= JollyCustom_CreateJollyLog;
            IL.JollyCoop.JollyCustom.Log -= JollyCustom_Log;
            IL.JollyCoop.JollyCustom.WriteToLog -= JollyCustom_WriteToLog;

            managedHooks.ForEach(hook => hook.Free());
        }

        private static void RainWorld_Awake(On.RainWorld.orig_Awake orig, RainWorld self)
        {
            if (RWInfo.LatestSetupPeriodReached < SetupPeriod.RWAwake)
            {
                RWInfo.LatestSetupPeriodReached = SetupPeriod.RWAwake;

                //Any mod that wishes to access old files before they are removed must do so in their plugin's OnEnable, or Awake method
                LogProperties.PropertyManager.CompleteStartupRoutine();
            }

            UtilityCore.ThreadID = Thread.CurrentThread.ManagedThreadId; //Used for debug purposes
            RainWorld._loggingLock = UtilityCore.RequestHandler.RequestProcessLock;

            //Utility bypasses attempts to define this from the game code. Avoid any potential null references 
            JollyCoop.JollyCustom.logCache = new Queue<JollyCoop.LogElement>();
            orig(self);
        }

        private static void RainWorld_Awake(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //Move to just before Unity logs are defined
            cursor.GotoNext(MoveType.After, x => x.MatchCall(typeof(System.Globalization.CultureInfo), "set_DefaultThreadCurrentCulture"));

            //Intercept attempt to delete Unity log files
            for (int i = 0; i < 2; i++)
            {
                if (cursor.TryGotoNext(MoveType.Before, x => x.MatchCall(typeof(File), nameof(File.Exists))))
                {
                    cursor.Emit(OpCodes.Pop);
                    cursor.Emit(OpCodes.Ldnull); //Replace filename string with null
                }
            }

            cursor.GotoNext(MoveType.After, x => x.MatchLdarg(0), x => x.Match(OpCodes.Ldftn)); //Ldftn is HandleLog instruction
            cursor.GotoNext(MoveType.Before, x => x.MatchLdarg(0));

            //After HandleLog is assigned, it is safe to handle unprocessed log requests
            cursor.EmitDelegate(() =>
            {
                //The game will take over handling of Unity log requests shortly after - unsubscribe listener
                if (RWInfo.LatestSetupPeriodReached == SetupPeriod.RWAwake)
                {
                    UtilityCore.RequestHandler.ProcessRequests();
                    Application.logMessageReceivedThreaded -= UtilityCore.HandleUnityLog;
                }
            });
        }

        private static void RainWorld_OnDestroy(On.RainWorld.orig_OnDestroy orig, RainWorld self)
        {
            //End all active log sessions
            LogProperties.PropertyManager.Properties.ForEach(p => p.EndLogSession());

            //BepInEx log file requires special treatment. This log file cannot be replaced on game start like the other log files
            //To account for this, replace this log file when the game closes
            LogID logFile = LogID.BepInEx;

            if (logFile.Properties.FileExists)
                logFile.Properties.CreateTempFile(true);

            LogProperties.PropertyManager.SaveToFile();
            orig(self);
        }

        private static void RainWorld_PreModsInit(On.RainWorld.orig_PreModsInit orig, RainWorld self)
        {
            if (RWInfo.LatestSetupPeriodReached < SetupPeriod.PreMods)
            {
                RWInfo.LatestSetupPeriodReached = SetupPeriod.PreMods;
                UtilityCore.RequestHandler.ProcessRequests();
            }

            orig(self);
        }

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            if (RWInfo.LatestSetupPeriodReached < SetupPeriod.ModsInit)
            {
                RWInfo.LatestSetupPeriodReached = SetupPeriod.ModsInit;
                UtilityCore.RequestHandler.ProcessRequests();
            }

            disableLogClearing = true;
            orig(self);
            disableLogClearing = false;
        }

        /// <summary>
        /// Ends the grace period in which newly initialized properties can be freely modified
        /// </summary>
        private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            if (RWInfo.LatestSetupPeriodReached < SetupPeriod.PostMods)
            {
                RWInfo.LatestSetupPeriodReached = SetupPeriod.PostMods;
                UtilityCore.RequestHandler.ProcessRequests();
            }

            orig(self);

            //TODO: It could be guaranteed that this runs after all hooks by setting a flag here, that is checked in ModManager.CheckInitIssues,
            //or we could possibly use the scheduler
            LogProperties.PropertyManager.Properties.ForEach(prop => prop.ReadOnly = true);
            LogProperties.PropertyManager.IsEditGracePeriod = false;

            LogFilter.DeactivateKeyword(UtilityConsts.FilterKeywords.ACTIVATION_PERIOD_STARTUP);
        }

        private static bool listenerCheckComplete;

        /// <summary>
        /// This is required for the signaling system. All remote loggers should use this hook to ensure that the logger is aware of the Logs directory being moved
        /// </summary>
        private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
        {
            orig(self);

            //Functionally similar to how JollyCoop handles its logging
            foreach (Logger logger in UtilityCore.RequestHandler.AvailableLoggers)
            {
                //TODO: Maybe an interface is better here
                QueueLogWriter queueWriter = logger.Writer as QueueLogWriter;

                if (queueWriter != null)
                    queueWriter.Flush();
            }

            //Logic is handled after orig for several reasons. The main reason is that all remote loggers are guaranteed to receive any signals set during update
            //no matter where they are in the load order. Signals are created pre-update, or during update only.
            if (self.started)
            {
                if (!listenerCheckComplete)
                {
                    UtilityCore.FindManagedListener();
                    listenerCheckComplete = true;
                }

                UtilityCore.HandleLogSignal();
            }
        }

        /// <summary>
        /// Stores the amount of invocation requests received by the game's logging hooks
        /// </summary>
        private static int gameHookRequestCounter = 0;

        private static bool checkRecursionRequestCounters()
        {
            int gameLoggerRequestCounter = UtilityCore.RequestHandler.GameLogger.GameLoggerRequestCounter;
            return gameLoggerRequestCounter > 0 && gameHookRequestCounter != gameLoggerRequestCounter;
        }

        private static void RainWorld_HandleLog(On.RainWorld.orig_HandleLog orig, RainWorld self, string logString, string stackTrace, LogType logLevel)
        {
            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                logWhenNotOnMainThread(LogID.Unity);

                gameHookRequestCounter++;

                bool recursionDetected = checkRecursionRequestCounters();

                object logTarget = logString;

                LogID logFile = LogCategory.GetUnityLogID(logLevel);

                if (logFile == LogID.Exception)
                {
                    //Compile the log strings provided by Unity's logging API into an ExceptionInfo object
                    ExceptionInfo exceptionInfo = new ExceptionInfo(logString, stackTrace);

                    //Check that the last exception reported matches information stored
                    if (!RWInfo.CheckExceptionMatch(LogID.Exception, exceptionInfo))
                    {
                        RWInfo.ReportException(LogID.Exception, exceptionInfo);

                        //The game is no longer able to set these accurately, and probably better to handle off the stack anyways
                        self.lastLoggedException = logString;
                        self.lastLoggedStackTrace = stackTrace;

                        //Replace existing log string with the compiled exception message and stack trace
                        logString = exceptionInfo.ToString();
                        logTarget = exceptionInfo;
                    }
                    else
                    {
                        gameHookRequestCounter--;
                        return;
                    }
                }

                if (recursionDetected)
                {
                    UtilityCore.BaseLogger.LogWarning("Potential recursive log request handling detected");

                    //While requests are being handled in the pipeline, we cannot handle this request
                    UtilityCore.RequestHandler.HandleOnNextAvailableFrame.Enqueue(createRequest());
                    gameHookRequestCounter--;
                    return;
                }

                bool processFinished = false;

                try
                {
                    orig(self, logString, stackTrace, logLevel);
                    processFinished = true;
                }
                finally
                {
                    if (!processFinished)
                    {
                        UtilityCore.BaseLogger.LogWarning("Logging operation has ended unexpectedly");

                        LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                        if (request != null)
                        {
                            if (request.Status != RequestStatus.Rejected) //Unknown issue - don't retry request
                                request.Reject(RejectionReason.FailedToWrite);

                            UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
                        }
                    }
                    gameHookRequestCounter--;
                }

                LogRequest createRequest()
                {
                    return new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(logFile, logTarget, LogCategory.ToCategory(logLevel)));
                }
            }
        }

        private static void RainWorld_HandleLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            handleLog_ExceptionLog(cursor);
            handleLog_ConsoleLog(cursor);
        }

        private static void handleLog_ExceptionLog(ILCursor cursor)
        {
            //Get the label pointing to the instructions that handle exception log messages
            ILLabel label = null;
            cursor.GotoNext(x => x.MatchLdarg(3), x => x.MatchBrfalse(out label));
            cursor.GotoLabel(label);
            cursor.MoveAfterLabels(); //Move after any labels to ensure emits will be run

            //Handle log request
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate((string exceptionString) =>
            {
                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                if (request == null)
                {
                    request = UtilityCore.RequestHandler.TrySubmit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.Exception, exceptionString)), false);

                    if (request.Status == RequestStatus.Rejected)
                        return;
                }
                LogWriter.Writer.WriteFrom(request);
            });

            //Branch over all exception string handling straight to the next OpCodes.Leave
            ILLabel returnLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Br, returnLabel);
            cursor.GotoNext(x => x.MatchLeaveS(out _)); //Find the next Leave instruction
            cursor.MarkLabel(returnLabel);
        }

        private static void handleLog_ConsoleLog(ILCursor cursor)
        {
            //Get the label to the instructions that send messages to the DevTools console display
            ILLabel consoleLabel = null;
            cursor.GotoNext(MoveType.Before, x => x.MatchCallOrCallvirt(typeof(ModManager), "get_ModdingEnabled"));
            cursor.GotoNext(MoveType.Before, x => x.MatchBrfalse(out consoleLabel));

            gotoWriteInstruction(cursor, "consoleLog.txt"); //Position cursor before log operation occurs

            //Handle log request
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate((string logString) =>
            {
                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                if (request == null)
                {
                    request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.Unity, logString)), false);

                    if (request.Status == RequestStatus.Rejected)
                        return false;
                }
                LogWriter.Writer.WriteFrom(request);
                return true;
            });

            //Branch over log handling instructions

            //When true, branch to console handling instructions
            cursor.Emit(OpCodes.Brtrue, consoleLabel);

            //When false, branch to next Leave instruction
            ILLabel returnLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Br, returnLabel);
            cursor.GotoNext(x => x.MatchLeaveS(out _));
            cursor.MarkLabel(returnLabel);
        }

        private static void gotoWriteInstruction(ILCursor cursor, string filename)
        {
            //Someone must have tampered with the filename for this to fail
            if (!cursor.TryGotoNext(MoveType.Before, x => x.MatchLdstr(filename)))
            {
                //Fallback IL
                cursor.GotoNext(x => x.MatchCall(typeof(File), nameof(File.AppendAllText)));
                cursor.GotoPrev(MoveType.Before, x => x.MatchLdstr(out _));
            }
        }

        private static void ExpLog_LogChallengeTypes(ILContext il)
        {
            showLogsBypassHook(new ILCursor(il), LogID.Expedition);
        }

        private static void ExpLog_Log(On.Expedition.ExpLog.orig_Log orig, string logString)
        {
            try
            {
                gameHookRequestCounter++;
                orig(logString);
            }
            finally
            {
                gameHookRequestCounter--;
            }
        }

        private static void ExpLog_LogOnce(On.Expedition.ExpLog.orig_LogOnce orig, string logString)
        {
            try
            {
                gameHookRequestCounter++;
                orig(logString);
            }
            finally
            {
                gameHookRequestCounter--;
            }
        }

        private static void ExpLog_Log(ILContext il)
        {
            expeditionLogProcessHookIL(il, shouldFilter: false);
        }

        private static void ExpLog_LogOnce(ILContext il)
        {
            expeditionLogProcessHookIL(il, shouldFilter: true);
        }

        private static void expeditionLogProcessHookIL(ILContext il, bool shouldFilter)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.Emit(OpCodes.Ldarg_0); //Static method, this is the log string
            cursor.EmitDelegate((string logString) =>
            {
                lock (UtilityCore.RequestHandler.RequestProcessLock)
                {
                    LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                    //Ensure that request is always constructed before a message is logged
                    if (request == null)
                    {
                        request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.Expedition, logString)), false);

                        if (request.Status == RequestStatus.Rejected)
                            return;
                    }

                    if (shouldFilter)
                    {
                        request.Data.ShouldFilter = true;
                        request.Data.FilterDuration = FilterDuration.OnClose;
                    }

                    LogWriter.Writer.WriteFrom(request);
                }
            });

            branchToReturn(cursor);
        }

        /// <summary>
        /// This flag prevents clear log functions from activating for Expedition, and JollyCoop
        /// </summary>
        private static bool disableLogClearing;

        private static void ExpLog_ClearLog(On.Expedition.ExpLog.orig_ClearLog orig)
        {
            if (disableLogClearing) return;

            orig();
        }

        private static void ExpLog_ClearLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.Expedition);

            cursor.GotoNext(MoveType.After, x => x.MatchBrfalse(out _)); //Move just after the ShowLogs check
            cursor.EmitDelegate(() =>
            {
                LogWriter.Writer.ResetFile(LogID.Expedition);
            });
            branchToReturn(cursor);
        }

        private static void JollyCustom_Log(On.JollyCoop.JollyCustom.orig_Log orig, string message, bool throwException)
        {
            try
            {
                gameHookRequestCounter++;
                orig(message, throwException);
            }
            finally
            {
                gameHookRequestCounter--;
            }
        }

        private static void JollyCustom_WriteToLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.JollyCoop);

            ILLabel label = null;

            cursor.GotoNext(MoveType.After, x => x.MatchBrfalse(out label)); //Move just after the ShowLogs check
            cursor.EmitDelegate(() =>
            {
                //Handle logging using a custom log writer designed to imitate the JollyCoop writer
                JollyCoop.JollyCustom.logCache.Clear();
                LogWriter.JollyWriter.Flush();
            });
            cursor.Emit(OpCodes.Br, label); //Branch over the rest of the instructions, and return
        }

        private static void JollyCustom_Log(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.JollyCoop);

            cursor.GotoNext(MoveType.After, x => x.MatchBrfalse(out _)); //ShowLogs branch check
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate((string logString, bool isErrorMessage) =>
            {
                lock (UtilityCore.RequestHandler.RequestProcessLock)
                {
                    LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                    //Ensure that request is always constructed before a message is logged
                    if (request == null)
                    {
                        LogCategory category = !isErrorMessage ? LogCategory.Default : LogCategory.Error;
                        request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.JollyCoop, logString, category)), false);

                        if (request.Status == RequestStatus.Rejected)
                            return false;
                    }
                    LogWriter.JollyWriter.WriteFrom(request);
                    return request.Status == RequestStatus.Complete;
                }
            });

            //Branch to place where LogElement is added to log queue. Queue is no longer used, but populate it anyways for legacy purposes
            ILLabel branchLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Brtrue, branchLabel);
            cursor.Emit(OpCodes.Ret);

            //Branching to here bypasses file creation process
            cursor.GotoNext(MoveType.After, x => x.MatchCall(typeof(JollyCoop.JollyCustom), "CreateJollyLog"));
            cursor.MoveAfterLabels();
            cursor.MarkLabel(branchLabel);
        }

        private static void JollyCustom_CreateJollyLog(On.JollyCoop.JollyCustom.orig_CreateJollyLog orig)
        {
            if (disableLogClearing) return;

            orig();
        }

        private static void JollyCustom_CreateJollyLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.JollyCoop);
            cursor.GotoNext(MoveType.After, x => x.MatchBrfalse(out _)); //Move just after the ShowLogs check
            cursor.EmitDelegate(() =>
            {
                LogWriter.Writer.ResetFile(LogID.JollyCoop);
            });

            //Branch over file create instructions
            ILLabel label = cursor.DefineLabel();
            cursor.Emit(OpCodes.Br, label);
            cursor.GotoNext(MoveType.After, x => x.MatchEndfinally());
            cursor.MarkLabel(label);
        }

        private static void showLogsBypassHook(ILCursor cursor, LogID logFile)
        {
            cursor.GotoNext(MoveType.After, x => x.MatchCall(typeof(RainWorld).GetMethod("get_ShowLogs")));
            cursor.EmitDelegate((bool showLogs) =>
            {
                return showLogs || !logFile.Properties.ShowLogsAware;
            });
        }

        private static void replaceLogPathHook(ILCursor cursor, LogID logFile)
        {
            if (cursor.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(RWCustom.Custom).GetMethod("RootFolderDirectory"))))
            {
                cursor.Emit(OpCodes.Pop); //Get method return value off the stack
                cursor.EmitDelegate(() => logFile.Properties.CurrentFolderPath);//Load current filepath onto stack
                cursor.GotoNext(MoveType.After, x => x.Match(OpCodes.Ldstr));
                cursor.Emit(OpCodes.Pop); //Replace filename extension with new one
                cursor.EmitDelegate(() => logFile.Properties.CurrentFilenameWithExtension);//Load current filename onto stack
            }
            else
            {
                UtilityCore.BaseLogger.LogError("Expected directory IL could not be found");
            }
        }

        private static void bepInExLogProcessHook(Action<ManualLogSource, LogLevel, object> orig, ManualLogSource self, LogLevel category, object data)
        {
            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                if (self.SourceName != UtilityConsts.UTILITY_NAME)
                    logWhenNotOnMainThread(LogID.BepInEx);

                orig(self, category, data);
            }
        }

        private static void bepInExLogProcessHookIL(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, x => x.MatchLdarg(0));
            cursor.Emit(OpCodes.Ldarg_1)
                  .Emit(OpCodes.Ldarg_2);

            bool isUtilityLogger = false;
            object transferObject = null;

            cursor.EmitDelegate(onLogReceived);

            //Check that LogRequest has passed validation
            ILLabel branchLabel = cursor.DefineLabel();

            cursor.Emit(OpCodes.Brtrue, branchLabel);
            cursor.Emit(OpCodes.Ret); //Return early if validation check failed

            cursor.MarkLabel(branchLabel);

            //Take handled log message and store it back on the stack
            cursor.EmitDelegate(() => transferObject);
            cursor.Emit(OpCodes.Starg, 2);
            cursor.Emit(OpCodes.Ldarg_0); //Replace the one taken off the stack

            //Whenever this method returns, treat this request as complete. BepInEx uses a flush timer, so message may not log immediately on completion
            while (cursor.TryGotoNext(MoveType.Before, x => x.MatchRet()))
            {
                cursor.EmitDelegate(() =>
                {
                    transferObject = null;

                    if (!isUtilityLogger)
                    {
                        LogRequest request = UtilityCore.RequestHandler.CurrentRequest;
                        LogWriter.FinishWriteProcess(request);
                    }
                    isUtilityLogger = false;
                });
                cursor.Index++;
            }

            bool onLogReceived(ManualLogSource self, LogLevel category, object data)
            {
                isUtilityLogger = self.SourceName == UtilityConsts.UTILITY_NAME;

                if (!isUtilityLogger) //Utility must be allowed to log without disturbing utility functions
                {
                    LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                    if (request == null)
                    {
                        request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.BepInEx, data, category)), false);

                        if (request.Status == RequestStatus.Rejected)
                            return false;
                    }

                    LogWriter.BeginWriteProcess(request);

                    if (request.Status == RequestStatus.Rejected)
                        return false;

                    //Prepare data to be put back on the stack
                    //category = request.Data.BepInExCategory;
                    transferObject = request.Data.Message;
                }
                else
                    transferObject = data;

                FileUtils.WriteLine("test.txt", "LOG MESSAGE: " + transferObject?.ToString());

                //TODO: LogRules need to be applied here
                return true;
            }
        }

        private static void beginWriteProcessHook(ILCursor cursor, bool requiresExtraPop = false)
        {
            //Begin write process from the IL stack
            cursor.EmitDelegate(() =>
            {
                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                LogWriter.BeginWriteProcess(request);
                return request.Status != RequestStatus.Rejected;
            });

            //Check that LogRequest has passed validation
            ILLabel branchLabel = cursor.DefineLabel();

            cursor.Emit(OpCodes.Brtrue, branchLabel);

            if (requiresExtraPop)
                cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Ret); //Return early if validation check failed
            cursor.MarkLabel(branchLabel);
        }

        private static void branchToReturn(ILCursor cursor)
        {
            ILLabel returnLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Br, returnLabel);
            cursor.GotoNext(MoveType.Before, x => x.MatchRet());
            cursor.MarkLabel(returnLabel);
        }

        private static void logWhenNotOnMainThread(LogID logFile)
        {
            if (RWInfo.LatestSetupPeriodReached >= SetupPeriod.RWAwake && UtilityCore.ThreadID != Thread.CurrentThread.ManagedThreadId)
            {
                UtilityCore.BaseLogger.LogDebug("Log request not handled on main thread");
                UtilityCore.BaseLogger.LogDebug($"ThreadInfo: Id [{Thread.CurrentThread.ManagedThreadId}] Source [{logFile}]");
            }
        }
    }
}
