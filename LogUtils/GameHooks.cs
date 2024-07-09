using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            managedHooks.Add(new ILHook(method, bepInExFormatHook));
            Apply();
        }

        /// <summary>
        /// Apply hooks used by the utility module
        /// </summary>
        internal static void Apply()
        {
            if (!UtilityCore.IsControllingAssembly) return; //Only the controlling assembly is allowed to apply the hooks

            //Signal system
            On.RainWorld.Update += RainWorld_Update;

            //Log property handling
            IL.RainWorld.HandleLog += RainWorld_HandleLog;
            IL.Expedition.ExpLog.ClearLog += replaceLogPathHook_Expedition;
            IL.Expedition.ExpLog.Log += replaceLogPathHook_Expedition;
            IL.Expedition.ExpLog.LogOnce += replaceLogPathHook_Expedition;
            IL.JollyCoop.JollyCustom.CreateJollyLog += replaceLogPathHook_JollyCoop;
            IL.JollyCoop.JollyCustom.Log += replaceLogPathHook_JollyCoop;
            IL.JollyCoop.JollyCustom.WriteToLog += replaceLogPathHook_JollyCoop;

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

            On.RainWorld.Update -= RainWorld_Update;

            IL.RainWorld.HandleLog -= RainWorld_HandleLog;
            IL.Expedition.ExpLog.ClearLog -= replaceLogPathHook_Expedition;
            IL.Expedition.ExpLog.Log -= replaceLogPathHook_Expedition;
            IL.Expedition.ExpLog.LogOnce -= replaceLogPathHook_Expedition;
            IL.JollyCoop.JollyCustom.CreateJollyLog -= replaceLogPathHook_JollyCoop;
            IL.JollyCoop.JollyCustom.Log -= replaceLogPathHook_JollyCoop;
            IL.JollyCoop.JollyCustom.WriteToLog -= replaceLogPathHook_JollyCoop;

            managedHooks.ForEach(hook => hook.Free());
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

        private static void replaceLogPathHook_Expedition(ILContext il)
        {
            replaceLogPath(il, LogID.Expedition);
        }

        private static void replaceLogPathHook_JollyCoop(ILContext il)
        {
            replaceLogPath(il, LogID.JollyCoop);
        }

        private static void replaceLogPath(ILContext il, LogID logFile)
        {
            ILCursor cursor = new ILCursor(il);

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

        private static void bepInExFormatHook(ILContext il)
        {
            //Stub
            ILCursor cursor = new ILCursor(il);
        }
    }
}
