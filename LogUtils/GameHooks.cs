using BepInEx.Logging;
using LogUtils.Helpers;
using LogUtils.Properties;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
                managedHooks.Add(new ILHook(method, bepInExLogProcessHook));
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
            IL.Expedition.ExpLog.Log += ExpLog_Log;
            IL.Expedition.ExpLog.LogOnce += ExpLog_LogOnce;
            IL.Expedition.ExpLog.LogChallengeTypes += ExpLog_LogChallengeTypes;

            IL.JollyCoop.JollyCustom.CreateJollyLog += JollyCustom_CreateJollyLog;
            IL.JollyCoop.JollyCustom.Log += JollyCustom_Log;
            IL.JollyCoop.JollyCustom.WriteToLog += JollyCustom_WriteToLog;

            On.Expedition.ExpLog.Log += ExpLog_Log;
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
            IL.Expedition.ExpLog.Log -= ExpLog_Log;
            IL.Expedition.ExpLog.LogOnce -= ExpLog_LogOnce;
            IL.Expedition.ExpLog.LogChallengeTypes -= ExpLog_LogChallengeTypes;

            IL.JollyCoop.JollyCustom.CreateJollyLog -= JollyCustom_CreateJollyLog;
            IL.JollyCoop.JollyCustom.Log -= JollyCustom_Log;
            IL.JollyCoop.JollyCustom.WriteToLog -= JollyCustom_WriteToLog;

            On.Expedition.ExpLog.Log -= ExpLog_Log;
            On.JollyCoop.JollyCustom.Log -= JollyCustom_Log;

            managedHooks.ForEach(hook => hook.Free());
        }

        private static void RainWorld_Awake(On.RainWorld.orig_Awake orig, RainWorld self)
        {
            if (RWInfo.LatestSetupPeriodReached < SetupPeriod.RWAwake)
                RWInfo.LatestSetupPeriodReached = SetupPeriod.RWAwake;

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

        private static void RainWorld_HandleLog(On.RainWorld.orig_HandleLog orig, RainWorld self, string logString, string stackTrace, LogType logLevel)
        {
            LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

            LogID logFile = !LogCategory.IsUnityErrorCategory(logLevel) ? LogID.Unity : LogID.Exception;

            bool requestOverride = false;

            //Ensure that request is always constructed before a message is logged
            if (request == null)
            {
                request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(logFile, logString, LogCategory.ToCategory(logLevel))), false);

                if (request.Status == RequestStatus.Rejected)
                    return;
            }
            else if (logFile == LogID.Exception && request.Data.ID != LogID.Exception)
            {
                UtilityCore.BaseLogger.LogWarning("Exception message forcefully logged to file");
                requestOverride = true;
            }

            try
            {
                orig(self, logString, stackTrace, logLevel);
            }
            catch
            {
                if (!requestOverride)
                    request.Reject(RejectionReason.FailedToWrite);
            }
            finally
            {
                if (!requestOverride)
                {
                    if (request.Status != RequestStatus.Rejected)
                        request.Complete();
                    UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
                }
                requestOverride = false;
            }
        }

        private static void RainWorld_HandleLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            //Replace all instances of exceptionLog.txt with a full path version
            int entriesToFind = 2;
            while (entriesToFind > 0 && cursor.TryGotoNext(MoveType.After, x => x.MatchLdstr("exceptionLog.txt")))
            {
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => LogID.Exception.Properties.CurrentFilename + ".log");
                //cursor.Emit(OpCodes.Ldstr, LogID.Exception.Properties.CurrentFilename + ".log");
                cursor.Emit(OpCodes.Ldc_I4_1); //Push a true value on the stack to satisfy second argument
                cursor.EmitDelegate(() => LogID.Exception.Properties.CurrentFilePath);
                //cursor.EmitDelegate(Logger.ApplyLogPathToFilename);
                entriesToFind--;
            }

            if (entriesToFind > 0)
                UtilityCore.BaseLogger.LogError("IL hook couldn't find exceptionLog.txt");

            //Replace a single instance of consoleLog.txt with a full path version
            if (cursor.TryGotoNext(MoveType.After, x => x.MatchLdstr("consoleLog.txt")))
            {
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => LogID.Unity.Properties.CurrentFilename + ".log");
                //cursor.Emit(OpCodes.Ldstr, LogID.Unity.Properties.CurrentFilename + ".log");
                cursor.Emit(OpCodes.Ldc_I4_1); //Push a true value on the stack to satisfy second argument
                cursor.EmitDelegate(() => LogID.Unity.Properties.CurrentFilePath);
                //cursor.EmitDelegate(Logger.ApplyLogPathToFilename);
            }
            else
            {
                UtilityCore.BaseLogger.LogError("IL hook couldn't find consoleLog.txt");
            }
        }

        private static void ExpLog_Log(On.Expedition.ExpLog.orig_Log orig, string message)
        {
            LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

            //Ensure that request is always constructed before a message is logged
            if (request == null)
            {
                request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.Expedition, message)), false);

                if (request.Status == RequestStatus.Rejected)
                    return;
            }

            orig(message);
            UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
        }

        private static void ExpLog_LogChallengeTypes(ILContext il)
        {
            showLogsBypassHook(new ILCursor(il), LogID.Expedition);
        }

        private static void ExpLog_Log(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.Expedition);
            replaceLogPathHook(cursor, LogID.Expedition);
        }

        private static void ExpLog_LogOnce(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.Expedition);
            replaceLogPathHook(cursor, LogID.Expedition);
        }

        private static void ExpLog_ClearLog(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            showLogsBypassHook(cursor, LogID.Expedition);
            replaceLogPathHook(cursor, LogID.Expedition);
        }

        private static void JollyCustom_Log(On.JollyCoop.JollyCustom.orig_Log orig, string message, bool throwException)
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
                //cursor.Emit(OpCodes.Ldstr, logFile.Properties.ContainingFolderPath);
                cursor.GotoNext(MoveType.After, x => x.Match(OpCodes.Ldstr));
                cursor.Emit(OpCodes.Pop); //Replace filename extension with new one
                cursor.EmitDelegate(() => logFile.Properties.CurrentFilename + ".log");//Load current filename onto stack
                //cursor.Emit(OpCodes.Ldstr, logFile.Properties.CurrentFilename + ".log");
            }
            else
            {
                UtilityCore.BaseLogger.LogError("Expected directory IL could not be found");
            }
        }

        private static void bepInExLogProcessHook(ILContext il)
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
                    FileUtils.WriteLine("test.txt", "Completing BepInEx log request");
                    transferObject = null;

                    if (!isUtilityLogger)
                    {
                        LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                        request.Complete();
                        UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
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
                    //else if (request.Data.ID != LogID.BepInEx)
                    //{
                    //    FileUtils.WriteLine("test.txt", "Cannot handle request - request already in progress");
                    //    FileUtils.WriteLine("test.txt", data?.ToString());
                    //    return false;
                    //}

                    LogProperties properties = request.Data.Properties;

                    if (!properties.LogSessionActive)
                    {
                        properties.BeginLogSession();

                        if (!properties.LogSessionActive) //Unable to create log file for some reason
                        {
                            FileUtils.WriteLine("test.txt", "Logger from " + self.SourceName);
                            FileUtils.WriteLine("test.txt", "Tried to log " + data);
                            FileUtils.WriteLine("test.txt", request.ToString());
                            FileUtils.WriteLine("test.txt", "FAILED TO WRITE");
                            request.Reject(RejectionReason.FailedToWrite);
                            return false;
                        }
                    }

                    FileUtils.WriteLine("test.txt", "Request processed");

                    //Notify that a request has been processed
                    LogEvents.OnMessageReceived?.Invoke(request.Data);

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
    }
}
