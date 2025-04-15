using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils.Helpers.Console
{
    public static class Console
    {
        public static bool IsEnabled => consoleStream != null;

        private static TextWriter consoleStream;

        private static bool matchConsoleManager(Type type) => type.Namespace == "BepInEx" && type.Name == "ConsoleManager";

        public static void Initialize()
        {
            try
            {
                //Locate the ConsoleManager type from all loaded assemblies.
                Type consoleManagerType = AssemblyUtils.GetAllTypes()
                    .FirstOrDefault(matchConsoleManager) ??
                    throw new ConsoleLoadException("ConsoleManager type not found in loaded assemblies.");

                PropertyInfo consoleActiveProperty, consoleStreamProperty;

                //Retrieve the static ConsoleActive property.
                consoleActiveProperty = consoleManagerType.GetProperty(
                    "ConsoleActive",
                    BindingFlags.Static | BindingFlags.Public) ??
                    throw new ConsoleLoadException("ConsoleActive property not found on ConsoleManager type.");

                bool consoleEnabled = (bool)consoleActiveProperty.GetValue(null);

                if (!consoleEnabled)
                {
                    UtilityLogger.Log("BepInEx console not enabled");
                    return;
                }

                //Retrieve the static ConsoleStream property.
                consoleStreamProperty = consoleManagerType.GetProperty(
                    "ConsoleStream",
                    BindingFlags.Static | BindingFlags.Public) ??
                    throw new ConsoleLoadException("ConsoleStream property not found on ConsoleManager type.");

                consoleStream = consoleStreamProperty.GetValue(null) as TextWriter;

                if (consoleStream == null)
                    throw new ConsoleLoadException("ConsoleStream is null.");
            }
            catch (ConsoleLoadException ex)
            {
                UtilityLogger.LogError(ex);
            }
        }
    }

    public class ConsoleLoadException : Exception
    {
        public ConsoleLoadException() : base()
        {
        }

        public ConsoleLoadException(string message) : base(message)
        {
        }

        public ConsoleLoadException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
