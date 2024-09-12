using BepInEx.Logging;
using Expedition;
using LogUtils.Helpers;
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
                UtilityCore.BaseLogger.LogError("Error occurred while loading hooks");
                UtilityCore.BaseLogger.LogError(ex);
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

            //Signal system
            On.RainWorld.Update += RainWorld_Update;

            //Log property handling
            On.RainWorld.HandleLog += RainWorld_HandleLog;
            IL.RainWorld.HandleLog += RainWorld_HandleLog;

            IL.Expedition.ExpLog.ClearLog += ExpLog_ClearLog;
            IL.Expedition.ExpLog.Log += expeditionLogProcessHookIL; //This needs to be handled before LogOnce hook
            IL.Expedition.ExpLog.LogOnce += expeditionLogProcessHookIL;
            IL.Expedition.ExpLog.LogChallengeTypes += ExpLog_LogChallengeTypes;

            IL.JollyCoop.JollyCustom.CreateJollyLog += JollyCustom_CreateJollyLog;
            IL.JollyCoop.JollyCustom.Log += JollyCustom_Log;
            IL.JollyCoop.JollyCustom.WriteToLog += JollyCustom_WriteToLog;

            On.Expedition.ExpLog.Log += ExpLog_Log;
            On.Expedition.ExpLog.LogOnce += ExpLog_LogOnce;
            On.JollyCoop.JollyCustom.Log += JollyCustom_Log;

            managedHooks.ForEach(hook => hook.Apply());
            UtilityCore.BaseLogger.LogInfo("Hooks loaded successfully");
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

            expeditionLogProcessFlag = true;

            On.RainWorld.Awake -= RainWorld_Awake;
            IL.RainWorld.Awake -= RainWorld_Awake;
            On.RainWorld.OnDestroy -= RainWorld_OnDestroy;
            On.RainWorld.OnModsInit -= RainWorld_OnModsInit;
            On.RainWorld.PostModsInit -= RainWorld_PostModsInit;
            On.RainWorld.PreModsInit -= RainWorld_PreModsInit;
            On.RainWorld.Update -= RainWorld_Update;

            On.RainWorld.HandleLog -= RainWorld_HandleLog;
            IL.RainWorld.HandleLog -= RainWorld_HandleLog;

            IL.Expedition.ExpLog.ClearLog -= ExpLog_ClearLog;
            IL.Expedition.ExpLog.Log -= expeditionLogProcessHookIL;
            IL.Expedition.ExpLog.LogOnce -= expeditionLogProcessHookIL;
            IL.Expedition.ExpLog.LogChallengeTypes -= ExpLog_LogChallengeTypes;

            IL.JollyCoop.JollyCustom.CreateJollyLog -= JollyCustom_CreateJollyLog;
            IL.JollyCoop.JollyCustom.Log -= JollyCustom_Log;
            IL.JollyCoop.JollyCustom.WriteToLog -= JollyCustom_WriteToLog;

            On.Expedition.ExpLog.Log -= ExpLog_Log;
            On.Expedition.ExpLog.LogOnce -= ExpLog_LogOnce;
            On.JollyCoop.JollyCustom.Log -= JollyCustom_Log;

            managedHooks.ForEach(hook => hook.Free());
        }

        private static void RainWorld_Awake(On.RainWorld.orig_Awake orig, RainWorld self)
        {
            if (RWInfo.LatestSetupPeriodReached < SetupPeriod.RWAwake)
                RWInfo.LatestSetupPeriodReached = SetupPeriod.RWAwake;

            UtilityCore.ThreadID = Thread.CurrentThread.ManagedThreadId; //Used for debug purposes
            RainWorld._loggingLock = UtilityCore.RequestHandler.RequestProcessLock;
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

            orig(self);

            //Leave enough time for mods to handle the old log files, before removing them from the folder
            if (RWInfo.LatestSetupPeriodReached == SetupPeriod.ModsInit)
                LogProperties.PropertyManager.CompleteStartupRoutine();
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

            LogProperties.PropertyManager.Properties.ForEach(prop => prop.ReadOnly = true);
            LogProperties.PropertyManager.IsEditGracePeriod = false;
        }

        private static bool listenerCheckComplete;

        /// <summary>
        /// This is required for the signaling system. All remote loggers should use this hook to ensure that the logger is aware of the Logs directory being moved
        /// </summary>
        private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
        {
            orig(self);

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

        private static void RainWorld_HandleLog(On.RainWorld.orig_HandleLog orig, RainWorld self, string logString, string stackTrace, LogType logLevel)
        {
            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                logWhenNotOnMainThread(LogID.Unity);

                LogID logFile = !LogCategory.IsUnityErrorCategory(logLevel) ? LogID.Unity : LogID.Exception;



                if (logFile == LogID.Exception)
                {
                }

                int gameLoggerRequestCounter = UtilityCore.RequestHandler.GameLogger.GameLoggerRequestCounter;

                gameHookRequestCounter++;

                if (gameLoggerRequestCounter > 0 && gameHookRequestCounter != gameLoggerRequestCounter)
                {
                    UtilityCore.BaseLogger.LogWarning("Potential recursive log request handling detected");
                    UtilityCore.BaseLogger.LogWarning("Logger Counter: " + gameLoggerRequestCounter);
                    UtilityCore.BaseLogger.LogWarning("Hook Counter: " + gameHookRequestCounter);

                    //While requests are being handled in the pipeline, we cannot handle this request
                    UtilityCore.RequestHandler.HandleOnNextAvailableFrame.Enqueue(createRequest());
                    gameHookRequestCounter--;
                    return;
                }

                bool processFinished = false;
                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                try
                {
                    if (request == null)
                    {
                        request = UtilityCore.RequestHandler.Submit(createRequest(), false);

                        if (request.Status == RequestStatus.Rejected)
                            return;
                    }

                    orig(self, logString, stackTrace, logLevel);
                    processFinished = true;
                }
                finally
                {
                    if (!processFinished && request.Status != RequestStatus.Rejected) //Unknown issue - don't retry request
                        request.Reject(RejectionReason.FailedToWrite);
                    gameHookRequestCounter--;
                }

                LogRequest createRequest()
                {
                    return new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(logFile, logString, LogCategory.ToCategory(logLevel)));
                }
            }
        }

        private static void RainWorld_HandleLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            ILLabel branchLabel = null;

            bool exceptionLogged = false;

            cursor.GotoNext(MoveType.After, x => x.MatchLdfld<RainWorld>(nameof(RainWorld.lastLoggedStackTrace)));
            cursor.GotoNext(MoveType.After, x => x.MatchBrfalse(out branchLabel)); //Get duplicate report branch label

            gotoWriteInstruction(cursor, "exceptionLog.txt");

            //Cursor is now positioned before exception strings are written to file. Intercept and handle the strings using a LogWriter
            cursor.Emit(OpCodes.Ldarg_1); //logString
            cursor.Emit(OpCodes.Ldarg_2); //stackTrace
            cursor.EmitDelegate((string exceptionString, string stackTrace) =>
            {
                exceptionLogged = true;

                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                //Complete the exception log request in order to handle the two exception string arguments as separate requests
                if (request != null && request.Data.ID == LogID.Exception)
                {
                    if (stackTrace == string.Empty) //Should we treat this like a normal log request
                    {
                        LogWriter.Writer.WriteToFile();
                        return;
                    }

                    request.Complete();
                    UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
                }

                writeToFile(new LogEvents.LogMessageEventArgs(LogID.Exception, exceptionString, LogType.Exception));
                writeToFile(new LogEvents.LogMessageEventArgs(LogID.Exception, stackTrace, LogType.Exception));
                //UtilityCore.RequestHandler.CheckForHandledRequests = true;
            });

            ILLabel afterWriteLabel = cursor.DefineLabel();

            cursor.Emit(OpCodes.Br, afterWriteLabel); //Bypass write attempts
            cursor.GotoNext(MoveType.Before, x => x.MatchLdarg(0), x => x.MatchLdarg(1));
            cursor.MarkLabel(afterWriteLabel);

            cursor.GotoLabel(branchLabel); //Move to label, and emit next instructions at label position

            cursor.EmitDelegate(() =>
            {
                if (!exceptionLogged) //Check if the duplicate exception log check prevented logging
                {
                    //This request will only be created from a utility Logger class, interfacing with the Unity logger directly wont create this
                    LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                    //Reject the log request, for it has already been logged
                    if (request != null && request.Data.ID == LogID.Exception)
                        request.Reject(RejectionReason.ExceptionAlreadyReported);
                }
                exceptionLogged = false;
            });

            //Prepare to bypass second write attempt
            cursor.GotoNext(MoveType.After, x => x.MatchCall<ModManager>("get_ModdingEnabled"));
            cursor.GotoNext(x => x.MatchBrfalse(out afterWriteLabel));

            gotoWriteInstruction(cursor, "consoleLog.txt");

            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate((string message) => //If a mod changes the message string, utility Logger requests will not be affected here
            {
                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;
                writeToFile(request.Data);
            });
            cursor.Emit(OpCodes.Br, afterWriteLabel); //Bypass write attempt

            static void writeToFile(LogEvents.LogMessageEventArgs requestData)
            {
                LogRequest request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, requestData), false);

                if (request.Status != RequestStatus.Rejected)
                    LogWriter.Writer.WriteToFile();
            }
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

        private static bool expeditionOrJollyLogInProgress;

        private static void ExpLog_LogChallengeTypes(ILContext il)
        {
            showLogsBypassHook(new ILCursor(il), LogID.Expedition);
        }

        private static void ExpLog_Log(On.Expedition.ExpLog.orig_Log orig, string message)
        {
            expeditionLogProcessHook(orig, message);
        }

        private static void ExpLog_LogOnce(On.Expedition.ExpLog.orig_LogOnce orig, string message)
        {
            //Utility doesn't yet support LogRequests that target this functionality. Request system does not need to be checked here
            if (!ExpLog.onceText.Contains(message))
                expeditionLogProcessHook(orig, message);
        }

        private static void expeditionLogProcessHook(Delegate orig, string message)
        {
            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                //Ensure that request is always constructed before a message is logged
                if (request == null)
                {
                    request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.Expedition, message)), false);

                    if (request.Status == RequestStatus.Rejected)
                        return;
                }

                try
                {
                    expeditionOrJollyLogInProgress = true;
                    orig.DynamicInvoke(message);
                }
                catch
                {
                    request.Reject(RejectionReason.FailedToWrite);
                }
                finally
                {
                    expeditionOrJollyLogInProgress = false;
                    LogWriter.FinishWriteProcess(request);
                }
            }
        }

        /// <summary>
        /// This is a hacky way of adding a necessary OpCodes.Pop to ExpLog.Log, but not ExpLog.LogOnce
        /// </summary>
        private static bool expeditionLogProcessFlag = true;

        private static void expeditionLogProcessHookIL(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.Expedition);
            replaceLogPathHook(cursor, LogID.Expedition);

            //Match this instruction to ensure we are probably matching the correct branch instruction
            cursor.GotoNext(x => x.MatchCall(typeof(ExpLog), nameof(ExpLog.ClearLog)));

            ILLabel branchLabel = null;

            //Find a suitable place to start the log write process
            cursor.GotoPrev(x => x.MatchBrtrue(out branchLabel)); //Take the label, so we can move to it
            cursor.GotoLabel(branchLabel);
            cursor.MoveAfterLabels();

            beginWriteProcessHook(cursor, expeditionLogProcessFlag);
            expeditionLogProcessFlag = false;

            cursor.GotoNext(MoveType.After, x => x.MatchLdarg(0)); //Move to after message is about to be logged
            cursor.EmitDelegate<Func<string, string>>(message =>
            {
                return LogWriter.Writer.ApplyRules(LogID.Expedition, message);
            });
        }

        private static void ExpLog_ClearLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.Expedition);

            cursor.GotoNext(MoveType.After, x => x.MatchBrfalse(out _)); //Move just after the ShowLogs check
            cursor.EmitDelegate(LogID.Expedition.Properties.EndLogSession); //End the last log session if it exists

            replaceLogPathHook(cursor, LogID.Expedition);

            //Allows Expedition log file to be overwritten with a new log file with the hardcoded intro message.
            //To have proper control over this file, this process needs to be rewritten to prevent overwriting
            cursor.GotoNext(MoveType.After, x => x.MatchCall(typeof(File), nameof(File.WriteAllText)));

            //By running this after file is overwriting, the hardcoded intro message will appear first
            cursor.EmitDelegate(() =>
            {
                if (!expeditionOrJollyLogInProgress) //Allow it to be handled in the log method
                    LogID.Expedition.Properties.BeginLogSession();
            });
        }

        private static void JollyCustom_Log(On.JollyCoop.JollyCustom.orig_Log orig, string message, bool throwException)
        {
            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                //Ensure that request is always constructed before a message is logged
                if (request == null)
                {
                    request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.JollyCoop, message)), false);

                    if (request.Status == RequestStatus.Rejected)
                        return;
                }

                orig(message, throwException);
                UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
            }
        }

        private static void JollyCustom_WriteToLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.JollyCoop);
            replaceLogPathHook(cursor, LogID.JollyCoop);
        }

        private static void JollyCustom_Log(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.JollyCoop);
            replaceLogPathHook(cursor, LogID.JollyCoop);
        }

        private static void JollyCustom_CreateJollyLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.JollyCoop);
            replaceLogPathHook(cursor, LogID.JollyCoop);
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
