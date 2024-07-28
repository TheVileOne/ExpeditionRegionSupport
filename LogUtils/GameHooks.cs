using BepInEx.Logging;
using LogUtils.Properties;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            Type type = typeof(ManualLogSource);
            MethodInfo method = type.GetMethod(nameof(ManualLogSource.Log));

            //Allows LogRules to apply to BepInEx log traffic
            managedHooks.Add(new ILHook(method, bepInExLogProcessHook));
            Apply();
        }

        /// <summary>
        /// Apply hooks used by the utility module
        /// </summary>
        internal static void Apply()
        {
            if (!UtilityCore.IsControllingAssembly) return; //Only the controlling assembly is allowed to apply the hooks

            On.RainWorld.Awake += RainWorld_Awake;
            On.RainWorld.PreModsInit += RainWorld_PreModsInit;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;

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
            On.RainWorld.PreModsInit -= RainWorld_PreModsInit;
            On.RainWorld.OnModsInit -= RainWorld_OnModsInit;
            On.RainWorld.PostModsInit -= RainWorld_PostModsInit;
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
            {
                RWInfo.LatestSetupPeriodReached = SetupPeriod.RWAwake;
                UtilityCore.RequestHandler.ProcessRequests();
            }

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

        private static void RainWorld_HandleLog(On.RainWorld.orig_HandleLog orig, RainWorld self, string logString, string stackTrace, LogType type)
        {
            LogID logFile = LogID.Unity;

            if (type == LogType.Error || type == LogType.Exception)
                logFile = LogID.Exception;

            if (logFile.Properties.ShowLogsAware && !RainWorld.ShowLogs) return; //Don't handle request, orig doesn't get called in this circumstance

            orig(self, logString, stackTrace, type);
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

        private static void ExpLog_Log(On.Expedition.ExpLog.orig_Log orig, string text)
        {
            //Ensure that request is always constructed before a message is logged
            if (UtilityCore.RequestHandler.CurrentRequest == null)
                UtilityCore.RequestHandler.Submit(new LogRequest(new LogEvents.LogMessageEventArgs(LogID.Expedition, text)));
            orig(text);
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

        private static void JollyCustom_Log(On.JollyCoop.JollyCustom.orig_Log orig, string logText, bool throwException)
        {
            //Ensure that request is always constructed before a message is logged
            if (UtilityCore.RequestHandler.CurrentRequest == null)
                UtilityCore.RequestHandler.Submit(new LogRequest(new LogEvents.LogMessageEventArgs(LogID.JollyCoop, logText)));

            orig(logText, throwException);
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
            cursor.Emit(OpCodes.Pop); //Just need to emit after this instruction. We don't need the reference
            cursor.Emit(OpCodes.Ldarg_1)
                  .Emit(OpCodes.Ldarg_2);
            cursor.EmitDelegate(onLogReceived);

            //Check that LogRequest has passed validation
            ILLabel branchLabel = cursor.DefineLabel();

            cursor.Emit(OpCodes.Brtrue, branchLabel);
            cursor.Emit(OpCodes.Ret); //Return early if validation check failed

            cursor.MarkLabel(branchLabel);

            MethodInfo getRequest = typeof(LogRequestHandler).GetProperty(nameof(LogRequestHandler.CurrentRequest)).GetGetMethod();
            MethodInfo getCategory = typeof(LogEvents.LogMessageEventArgs).GetProperty(nameof(LogEvents.LogMessageEventArgs.Category)).GetGetMethod();
            MethodInfo getSpecificCategory = typeof(LogCategory).GetProperty(nameof(LogCategory.BepInExCategory)).GetGetMethod();
            //MethodInfo getMessage = typeof(LogEvents.LogMessageEventArgs).GetProperty(nameof(LogEvents.LogMessageEventArgs.Message)).GetGetMethod();

            //First Data access
            cursor.EmitReference(UtilityCore.RequestHandler);
            cursor.Emit(OpCodes.Call, getRequest);
            cursor.Emit(OpCodes.Ldfld, nameof(LogRequest.Data)); //Access the Data field
            cursor.Emit(OpCodes.Call, getCategory); //Access Category property from Data
            cursor.Emit(OpCodes.Call, getSpecificCategory); //Access BepInEx conversion of Category
            cursor.Emit(OpCodes.Starg, 1); //Overwrite local argument (LogLevel)

            /*
            //Second Data access
            cursor.EmitReference(UtilityCore.RequestHandler);
            cursor.Emit(OpCodes.Call, getRequest);
            cursor.Emit(OpCodes.Ldfld, nameof(LogRequest.Data)); //Access the Data field
            cursor.Emit(OpCodes.Call, getMessage); //Access Category property from Data
            cursor.Emit(OpCodes.Starg, 2); //Overwrite local argument (data)
            */

            /*
            //First Data access
            cursor.Emit(OpCodes.Ldfld, nameof(LogEvents.LogMessageEventArgs.Category)); //Access Category property from Data
            cursor.Emit(OpCodes.Ldfld, nameof(LogCategory.BepInExCategory)); //Access BepInEx conversion of Category
            cursor.Emit(OpCodes.Starg, 1); //Overwrite local argument (LogLevel)

            //Second Data access
            cursor.Emit(OpCodes.Call, getRequest);
            cursor.Emit(OpCodes.Ldfld, nameof(LogRequest.Data)); //Access the Data field
            cursor.Emit(OpCodes.Ldfld, nameof(LogEvents.LogMessageEventArgs.Message)); //Access Category property from Data
            cursor.Emit(OpCodes.Starg, 2); //Overwrite local argument (data)
            */

            bool onLogReceived(LogLevel category, object data)
            {
                string message = data?.ToString();

                if (UtilityCore.RequestHandler.CurrentRequest == null)
                    UtilityCore.RequestHandler.Submit(new LogRequest(new LogEvents.LogMessageEventArgs(LogID.BepInEx, message)));

                //Make sure request is valid and can be processed
                bool acceptRequest = !LogID.BepInEx.Properties.ShowLogsAware || RainWorld.ShowLogs;

                if (!acceptRequest)
                {
                    UtilityCore.RequestHandler.CurrentRequest.Reject(RejectionReason.LogDisabled);
                    return false;
                }

                UtilityCore.RequestHandler.CurrentRequest.Complete();
                return true;
            }
        }
    }
}
