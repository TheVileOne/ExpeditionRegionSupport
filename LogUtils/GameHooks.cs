using LogUtils.Compatibility.Unity;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Formatting;
using LogUtils.Helpers;
using LogUtils.Helpers.Extensions;
using LogUtils.Properties;
using LogUtils.Requests;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
                ILHook hook = null;
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                managedHooks.Add(hook = new ILHook(typeof(StringBuilder).GetMethod("AppendFormatHelper", flags), StringBuilder_AppendFormatHelper));

                Apply();
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Error occurred while loading hooks", ex);
            }
        }

        /// <summary>
        /// Apply hooks used by the utility module
        /// </summary>
        internal static void Apply()
        {
            On.RainWorld.Awake += RainWorld_Awake;
            IL.RainWorld.Awake += RainWorld_Awake;
            On.RainWorld.OnDestroy += RainWorld_OnDestroy;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            On.RainWorld.PreModsInit += RainWorld_PreModsInit;
            On.ModManager.WrapModInitHooks += ModManager_WrapModInitHooks;

            //Event system
            On.RainWorld.Update += RainWorld_Update;
            On.MainLoopProcess.Update += MainLoopProcess_Update;

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
            UtilityLogger.Log("Hooks loaded successfully");
        }

        /// <summary>
        /// Releases, and then reapply hooks used by the utility module 
        /// </summary>
        public static void Reload()
        {
            Unload();
            Apply();
        }

        /// <summary>
        /// Releases all hooks used by the utility module
        /// </summary>
        public static void Unload()
        {
            On.RainWorld.Awake -= RainWorld_Awake;
            IL.RainWorld.Awake -= RainWorld_Awake;
            On.RainWorld.OnDestroy -= RainWorld_OnDestroy;
            On.RainWorld.OnModsInit -= RainWorld_OnModsInit;
            On.RainWorld.PostModsInit -= RainWorld_PostModsInit;
            On.RainWorld.PreModsInit -= RainWorld_PreModsInit;
            On.ModManager.WrapModInitHooks -= ModManager_WrapModInitHooks;

            //Event system
            On.RainWorld.Update -= RainWorld_Update;
            On.MainLoopProcess.Update -= MainLoopProcess_Update;

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
            ThreadUtils.MainThreadID = Environment.CurrentManagedThreadId; //Used for debug purposes
            RainWorld._loggingLock = UtilityCore.RequestHandler.RequestProcessLock;

            //Utility bypasses attempts to define this from the game code. Avoid any potential null references 
            JollyCoop.JollyCustom.logCache = new Queue<JollyCoop.LogElement>();

            RWInfo.NotifyOnPeriodReached(SetupPeriod.RWAwake);
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
                    UnityLogger.ReceiveUnityLogEvents = false;
                }
            });
        }

        private static void RainWorld_OnDestroy(On.RainWorld.orig_OnDestroy orig, RainWorld self)
        {
            RWInfo.IsShuttingDown = true;

            try
            {
                orig(self);
            }
            finally
            {
                UtilityCore.OnShutdown();
            }
        }

        private static void ModManager_WrapModInitHooks(On.ModManager.orig_WrapModInitHooks orig)
        {
            //Applying this here as it is guaranteed to run before any hooks are applied in typical channels
            LogFilter.ActivateKeyword(UtilityConsts.FilterKeywords.ACTIVATION_PERIOD_STARTUP);
            orig();
        }

        private static void RainWorld_PreModsInit(On.RainWorld.orig_PreModsInit orig, RainWorld self)
        {
            RWInfo.NotifyOnPeriodReached(SetupPeriod.PreMods);
            orig(self);
        }

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            RWInfo.NotifyOnPeriodReached(SetupPeriod.ModsInit);

            disableLogClearing = true;
            orig(self);
            disableLogClearing = false;
        }

        /// <summary>
        /// Ends the grace period in which newly initialized properties can be freely modified
        /// </summary>
        private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            RWInfo.NotifyOnPeriodReached(SetupPeriod.PostMods);
            orig(self);

            //TODO: It could be guaranteed that this runs after all hooks by setting a flag here, that is checked in ModManager.CheckInitIssues, or we could possibly use the scheduler
            foreach (LogProperties properties in LogProperties.PropertyManager.Properties)
                properties.ReadOnly = true;

            LogProperties.PropertyManager.IsEditGracePeriod = false;
            LogFilter.DeactivateKeyword(UtilityConsts.FilterKeywords.ACTIVATION_PERIOD_STARTUP);
        }

        private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
        {
            orig(self);

            //Functionally similar to how JollyCoop handles its logging
            var flushableWriters = UtilityCore.RequestHandler.AvailableLoggers.GetWriters().OfType<IFlushable>();

            foreach (IFlushable writeBuffer in flushableWriters)
                writeBuffer.Flush();
        }

        private static void MainLoopProcess_Update(On.MainLoopProcess.orig_Update orig, MainLoopProcess self)
        {
            if (self.manager?.currentMainLoop == self)
                UtilityEvents.OnNewUpdateSynced?.Invoke(self, EventArgs.Empty);
            orig(self);
        }

        private static void StringBuilder_AppendFormatHelper(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //Match the third char comparison check - that's where the starting curly brace code is handled
            cursor.GotoNext(MoveType.After, x => x.MatchBneUn(out _));
            cursor.GotoNext(MoveType.After, x => x.MatchBneUn(out _));
            cursor.GotoNext(MoveType.After, x => x.MatchBneUn(out _));

            cursor.Emit(OpCodes.Ldloc_3); //ICustomFormatter
            cursor.Emit(OpCodes.Ldloc_0); //Current index of format string
            cursor.EmitDelegate(formatPlaceholderStart);

            cursor.GotoNext(MoveType.After, x => x.MatchLdarga(3),       //args array is accessed
                                            x => x.MatchLdloc(out _),    //Array indexer
                                            x => x.Match(OpCodes.Call)); //Argument at the provided index is fetched from the args array

            //Handle the char position of the right-most curly brace
            cursor.Emit(OpCodes.Ldloc_3); //ICustomFormatter
            cursor.Emit(OpCodes.Ldarg_2); //Format string
            cursor.Emit(OpCodes.Ldloc, 4); //Argument index
            cursor.Emit(OpCodes.Ldloc, 6); //Argument comma value
            cursor.Emit(OpCodes.Ldloc_0); //Current index within format string
            cursor.EmitDelegate((object formatArgument, ICustomFormatter formatter, string format, int argIndex, int commaArg, int formatIndex) =>
            {
                var provider = formatter as IColorFormatProvider;

                if (provider != null)
                {
                    var formatData = provider.GetData();
                    var placeholderData = formatData.CurrentPlaceholder;

                    placeholderData.ArgumentIndex = argIndex;
                    placeholderData.CommaArgument = commaArg;

                    int placeholderStart = placeholderData.Position;
                    int placeholderLength = formatIndex - placeholderStart;

                    placeholderData.Format = format.Substring(placeholderStart, placeholderLength);

                    //UtilityLogger.Log("PLACEHOLDER FORMAT: " + placeholderData.Format);

                    if (formatArgument is Color)
                        formatArgument = new ColorPlaceholder((Color)formatArgument, placeholderData);

                    placeholderData.Argument = formatArgument;

                    //Replaced original struct with the more updated copy
                    formatData.CurrentPlaceholder = placeholderData;
                }
                return formatArgument;
            });

            cursor.GotoNext(MoveType.After, x => x.MatchStloc(11)); //Assignment of local variable responsible for padding spaces

            ILLabel branchTarget = il.DefineLabel();

            //Put it back on the stack, and check that is not 0
            cursor.Emit(OpCodes.Ldloc, 11);
            cursor.Emit(OpCodes.Brfalse, branchTarget);

            //When value is not 0, we need to check if we dealing with the right formatter object
            cursor.Emit(OpCodes.Ldloc, 11);
            cursor.Emit(OpCodes.Ldloc_3);
            cursor.EmitDelegate((int value, ICustomFormatter formatter) =>
            {
                var provider = formatter as IColorFormatProvider;

                if (provider != null)
                {
                    var formatData = provider.GetData();
                    var placeholderData = formatData.CurrentPlaceholder;

                    //The padding syntax is being borrowed - this will prevent any padding from being assigned when we are working with a color argument
                    if (placeholderData.Argument is ColorPlaceholder)
                        return 0;
                }
                return value;
            });
            cursor.Emit(OpCodes.Stloc, 11); //Update padding value
            cursor.MarkLabel(branchTarget);

            void formatPlaceholderStart(ICustomFormatter formatter, int index)
            {
                //We only need to touch CWT data in the context of dealing with a IColorFormatProvider implementation
                var provider = formatter as IColorFormatProvider;

                if (provider != null)
                {
                    //UtilityLogger.Log("Placeholder start");
                    var data = provider.GetData();
                    data.CurrentPlaceholder.Position = index;
                }
            }
        }

        private static void RainWorld_HandleLog(On.RainWorld.orig_HandleLog orig, RainWorld self, string logString, string stackTrace, LogType category)
        {
            using (UtilityCore.RequestHandler.BeginCriticalSection())
            {
                LogID logFile = LogCategory.GetUnityLogID(category);

                LogRequest request = UtilityCore.RequestHandler.GetRequestFromAPI(logFile, true);

                //if (RWInfo.LatestSetupPeriodReached >= SetupPeriod.RWAwake)
                //    ThreadUtils.AssertRunningOnMainThread(logFile);

                bool processFinished = false;

                if (logFile == LogID.Exception)
                {
                    handleExceptionMessage();

                    if (processFinished)
                        return;
                }

                try
                {
                    UtilityCore.RequestHandler.RecursionCheckCounter++;
                    orig(self, logString, stackTrace, category);
                    processFinished = true;
                }
                finally
                {
                    finishProcessing();
                    UtilityCore.RequestHandler.RecursionCheckCounter--;
                }

                void handleExceptionMessage()
                {
                    //Compile the log strings provided by Unity's logging API into an ExceptionInfo object
                    ExceptionInfo exceptionInfo = new ExceptionInfo(logString, stackTrace);

                    //Check that the last exception reported matches information stored
                    if (RWInfo.CheckExceptionMatch(LogID.Exception, exceptionInfo))
                    {
                        if (request != null && request.Data.ID == LogID.Exception)
                        {
                            request.Reject(RejectionReason.ExceptionAlreadyReported);
                            UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
                        }
                        processFinished = true;
                        return;
                    }

                    RWInfo.ReportException(LogID.Exception, exceptionInfo);

                    //The game is no longer able to set these accurately, and probably better to handle off the stack anyways
                    self.lastLoggedException = logString;
                    self.lastLoggedStackTrace = stackTrace;

                    //Replace existing log string with the compiled exception message and stack trace
                    logString = exceptionInfo.ToString();
                }

                void finishProcessing()
                {
                    if (processFinished && (request == null || request.IsCompleteOrRejected))
                        return;

                    UtilityLogger.LogWarning("Logging operation has ended unexpectedly");

                    if (request.Status != RequestStatus.Rejected) //Unknown issue - don't retry request
                    {
                        request.Reject(RejectionReason.FailedToWrite);
                        LogWriter.Writer.SendToBuffer(request.Data);
                    }

                    UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
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
                UtilityCore.RequestHandler.SanitizeCurrentRequest();

                LogRequest request = UtilityCore.RequestHandler.GetRequestFromAPI(LogID.Exception);

                if (request == null)
                {
                    request = UtilityCore.RequestHandler.TrySubmit(new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.Exception, exceptionString)), false);

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
            ILLabel consoleLabel = null;

            //Bypass the ModdingEnabled check. It is false early in the game's start process
            cursor.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(ModManager), "get_ModdingEnabled"));
            cursor.EmitDelegate((bool oldFlag) => true);

            //Get the label to the instructions that send messages to the DevTools console display
            cursor.GotoNext(MoveType.After, x => x.MatchBrfalse(out consoleLabel));

            //Someone must have tampered with the filename for this to fail
            if (!cursor.TryGotoNext(MoveType.Before, x => x.MatchLdstr("consoleLog.txt")))
            {
                //Fallback IL
                cursor.GotoNext(x => x.MatchCall(typeof(File), nameof(File.AppendAllText)));
                cursor.GotoPrev(MoveType.Before, x => x.MatchLdstr(out _));
            }

            //Handle log request
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate((string logString) =>
            {
                UtilityCore.RequestHandler.SanitizeCurrentRequest();
                LogRequest request = UtilityCore.RequestHandler.GetRequestFromAPI(LogID.Unity);

                if (request == null)
                {
                    request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.Unity, logString)), false);

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

        private static void ExpLog_LogChallengeTypes(ILContext il)
        {
            showLogsBypassHook(new ILCursor(il), LogID.Expedition);
        }

        private static void ExpLog_Log(On.Expedition.ExpLog.orig_Log orig, string logString)
        {
            using (UtilityCore.RequestHandler.BeginCriticalSection())
            {
                try
                {
                    UtilityCore.RequestHandler.RecursionCheckCounter++;
                    orig(logString);
                }
                finally
                {
                    UtilityCore.RequestHandler.RecursionCheckCounter--;
                }
            }
        }

        private static void ExpLog_LogOnce(On.Expedition.ExpLog.orig_LogOnce orig, string logString)
        {
            using (UtilityCore.RequestHandler.BeginCriticalSection())
            {
                try
                {
                    UtilityCore.RequestHandler.RecursionCheckCounter++;
                    orig(logString);
                }
                finally
                {
                    UtilityCore.RequestHandler.RecursionCheckCounter--;
                }
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
                UtilityCore.RequestHandler.SanitizeCurrentRequest();

                LogRequest request = UtilityCore.RequestHandler.GetRequestFromAPI(LogID.Expedition);

                //Ensure that request is always constructed before a message is logged
                if (request == null)
                {
                    request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.Expedition, logString)), false);

                    if (request.Status == RequestStatus.Rejected)
                        return;
                }

                if (shouldFilter)
                {
                    request.Data.ShouldFilter = true;
                    request.Data.FilterDuration = FilterDuration.OnClose;
                }

                LogWriter.Writer.WriteFrom(request);
            });

            branchToReturn(cursor);
        }

        /// <summary>
        /// This flag prevents clear log functions from activating for Expedition, and JollyCoop
        /// </summary>
        private static bool disableLogClearing;

        private static void ExpLog_ClearLog(On.Expedition.ExpLog.orig_ClearLog orig)
        {
            if (disableLogClearing || !UtilityCore.IsControllingAssembly) return;

            orig();
        }

        private static void ExpLog_ClearLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.Expedition);

            cursor.GotoNext(MoveType.After, x => x.MatchBrfalse(out _)); //Move just after the ShowLogs check
            cursor.EmitDelegate(() =>
            {
                LogFile.StartNewSession(LogID.Expedition);
            });
            branchToReturn(cursor);
        }

        private static void JollyCustom_Log(On.JollyCoop.JollyCustom.orig_Log orig, string message, bool throwException)
        {
            using (UtilityCore.RequestHandler.BeginCriticalSection())
            {
                try
                {
                    UtilityCore.RequestHandler.RecursionCheckCounter++;
                    orig(message, throwException);
                }
                finally
                {
                    UtilityCore.RequestHandler.RecursionCheckCounter--;
                }
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
                LogRequest request = UtilityCore.RequestHandler.GetRequestFromAPI(LogID.JollyCoop);

                //Ensure that request is always constructed before a message is logged
                if (request == null)
                {
                    LogCategory category = !isErrorMessage ? LogCategory.Default : LogCategory.Error;
                    request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.JollyCoop, logString, category)), false);

                    if (request.Status == RequestStatus.Rejected)
                        return false;
                }
                LogWriter.JollyWriter.WriteFrom(request);
                return request.Status == RequestStatus.Complete;
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
            if (disableLogClearing || !UtilityCore.IsControllingAssembly) return;

            orig();
        }

        private static void JollyCustom_CreateJollyLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.JollyCoop);
            cursor.GotoNext(MoveType.After, x => x.MatchBrfalse(out _)); //Move just after the ShowLogs check
            cursor.EmitDelegate(() =>
            {
                LogFile.StartNewSession(LogID.JollyCoop);
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

        private static void branchToReturn(ILCursor cursor)
        {
            ILLabel returnLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Br, returnLabel);
            cursor.GotoNext(MoveType.Before, x => x.MatchRet());
            cursor.MarkLabel(returnLabel);
        }
    }
}
