using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace LogUtils.Patcher
{
    public static class Patcher
    {
        private delegate void LogMethodDelegate(object data);

        private static readonly LogMethodDelegate LogInfoDelegate;

        static Patcher()
        {
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
        }

        public static IEnumerable<string> TargetDLLs { get; } = ["BepInEx.dll"];

        public static void Patch(AssemblyDefinition assembly)
        {
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
        }

        public static void Finish()
        {
            LogInfo("Patcher finished");
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