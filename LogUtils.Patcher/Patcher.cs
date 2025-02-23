using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using System.Reflection;
using System.IO;
using MonoMod.RuntimeDetour;
using BepInEx.Bootstrap;
using BepInEx;
using HarmonyLib;

namespace LogUtils.Patcher
{
    public static class Patcher
    {
        //private static ManualLogSource Logger = new ManualLogSource("LogUtils.Patcher");

        private delegate void LogMethodDelegate(object data);

        private static readonly LogMethodDelegate LogInfoDelegate;

        static Patcher()
        {
            Init();
        }

        public static void Init()
        {
            //TypeLoader.AssemblyResolve += Patcher.TypeLoaderOnAssemblyResolve;
            Harmony harmony = new Harmony("ttt");
            harmony.Patch(AccessTools.Method(typeof(TypeLoader), "FindPluginTypes", null, null).MakeGenericMethod(new Type[]
            {
                typeof(PluginInfo)
            }), new HarmonyMethod(AccessTools.Method(typeof(Patcher), "PreFindPluginTypes", null, null)), new HarmonyMethod(AccessTools.Method(typeof(Patcher), "PostFindPluginTypes", null, null)), null, null, null);
        }

        private static AssemblyDefinition TypeLoaderOnAssemblyResolve(object sender, AssemblyNameReference reference)
        {
            /*
                AssemblyName assemblyName = new AssemblyName(reference.FullName);
                foreach (string directory in ModManager.GetPluginDirs())
                {
                    AssemblyDefinition result;
                    if (Utility.TryResolveDllAssembly(assemblyName, directory, TypeLoader.ReaderParameters, out result))
                    {
                        return result;
                    }
                }
            */
            return null;

        }

        public static void PreFindPluginTypes(string directory)
        {
            if (directory != Paths.PluginPath)
            {
                return;
            }
            applyPatch();
        }

        public static void PostFindPluginTypes(string directory)
        {
        }

        public static IEnumerable<string> TargetDLLs
        {
            get
            {
                //applyPatch();
                return Array.Empty<string>();
            }
        }

        private static void applyPatch()
        {
            var writer = File.CreateText("DEBUG_TEST.txt");

            writer.WriteLine("Applying patch");
            //Logger.LogMessage("Applying patch");

            try
            {
                //Init logic will be injected after the ManagerObject is assigned through this hook
                //ILHook hook = new ILHook(typeof(BepInEx.Bootstrap.Chainloader).GetMethod("FindPluginTypes"), il =>
                //{
                    //ILCursor cursor = new ILCursor(il);

                    //cursor.GotoNext(MoveType.After, x => x.MatchNewobj<UnityEngine.GameObject>(),
                    //                                x => x.Match(OpCodes.Call));
                    //cursor.EmitDelegate(() =>
                    //{
                    //TODO: Find and load latest assembly here

                    //UtilityCore.EnsureInitializedState();
                    //});
               // });

                /*
                Type loggerType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    loggerType = asm.GetType("BepInEx.Logging.Logger");
                    if (loggerType != null)
                        break;
                }

                if (loggerType == null)
                    throw new Exception("BepInEx.Logging.Logger type not found.");

                var methodInfo = loggerType.GetMethod("LogInfo", BindingFlags.Static | BindingFlags.NonPublic);
                if (methodInfo == null)
                    throw new Exception("LogInfo method not found in BepInEx.Logging.Logger.");

                LogInfoDelegate = (LogMethodDelegate)Delegate.CreateDelegate(typeof(LogMethodDelegate), methodInfo);
                */
                writer.WriteLine("Patch success");
                //Logger.LogMessage("Patch successful");
            }
            catch (Exception ex)
            {
                writer.WriteLine("Patch failed " + ex);
                //Logger.LogMessage("Patch failed: " + ex);
            }

            writer.Dispose();
        }

        public static void Patch(AssemblyDefinition assembly)
        {
            /*
            var writer = File.CreateText("DEBUG_TEST.txt");

            writer.WriteLine("Patcher started");
            writer.Dispose();
            LogInfo("Initializing Patcher");

            var chainloaderType = assembly.MainModule.Types.FirstOrDefault(t => t.FullName == "BepInEx.Bootstrap.Chainloader");
            if (chainloaderType == null)
                return;

            var initMethod = chainloaderType.Methods.FirstOrDefault(m => m.Name == "Initialize" && m.HasBody);
            if (initMethod == null)
                return;

            var processor = initMethod.Body.GetILProcessor();
            bool patched = false;

            foreach (var instr in initMethod.Body.Instructions)
            {
                if (instr.OpCode == OpCodes.Ldstr &&
                    instr.Operand is string s &&
                    s.Contains("Chainloader ready"))
                {
                    var ensureInitMethod = typeof(UtilityCore).GetMethod("EnsureInitializedState", BindingFlags.Public | BindingFlags.Static);
                    if (ensureInitMethod == null)
                        break;

                    var importedMethod = assembly.MainModule.ImportReference(ensureInitMethod);

                    var callInstr = processor.Create(OpCodes.Call, importedMethod);

                    processor.InsertAfter(instr, callInstr);
                    patched = true;
                    break;
                }
            }

            if (!patched)
            {
                LogInfo("Patcher Failed");
                return;
            }
            */
        }

        public static void Finish()
        {
            //LogInfo("Patcher finished");
        }

        /// <summary>
        /// Logs an info-level message using the reflection call.
        /// </summary>
        public static void LogInfo(object data)
        {
            LogInfoDelegate?.Invoke(data);
        }
    }
}