using BepInEx;
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace LogUtils.Helpers
{
    public static class AssemblyUtils
    {
        public static List<PluginInfo> PluginInfos;

        public static void BuildInfoCache()
        {
            PluginInfos = Chainloader.PluginInfos.Values.OrderBy(p => p.Location).ToList();
        }

        /// <summary>
        /// Get all types from each assembly in the current domain
        /// </summary>
        public static IEnumerable<Type> GetAllTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
        }

        public static BaseUnityPlugin GetPlugin(Assembly assembly)
        {
            return GetPluginInfo(assembly)?.Instance;
        }

        public static PluginInfo GetPluginInfo(Assembly assembly)
        {
            if (assembly == null)
                return null;

            return PluginInfos.Find(p => p.Location == assembly.Location);
        }

        public static BepInPlugin GetPluginMetadata(Assembly assembly)
        {
            return GetPluginInfo(assembly)?.Metadata;
        }

        /// <summary>
        /// Get the first calling assembly that is not the executing assembly via a stack trace
        /// </summary>
        /// Credit for this code goes to WilliamCruisoring
        public static Assembly GetCallingAssembly()
        {
            Assembly thisAssembly = UtilityCore.Assembly;

            StackTrace stackTrace = new StackTrace();
            StackFrame[] frames = stackTrace.GetFrames();

            foreach (var stackFrame in frames)
            {
                var ownerAssembly = stackFrame.GetMethod().DeclaringType.Assembly;
                if (ownerAssembly != thisAssembly)
                    return ownerAssembly;
            }
            return thisAssembly;
        }
    }
}
