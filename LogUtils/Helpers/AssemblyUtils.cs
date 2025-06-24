using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace LogUtils.Helpers
{
    public static class AssemblyUtils
    {
        /// <summary>
        /// Get all types from each assembly in the current domain
        /// </summary>
        public static IEnumerable<Type> GetAllTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypesSafely());
        }

        public static IEnumerable<Type> GetTypesSafely(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException loadError)
            {
                return loadError.Types.Where(t => t != null);
            }
        }

        /// <summary>
        /// Get the first calling assembly that is not the executing assembly via a stack trace
        /// </summary>
        /// <remarks>Credit for this code goes to WilliamCruisoring</remarks>
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
