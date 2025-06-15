using BepInEx.Logging;
using LogUtils.Console;
using LogUtils.Enums;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LogUtils.Compatibility.BepInEx.Listeners
{
    public class ConsoleLogListener : ILogListener, IDisposable
    {
        private readonly TextWriter _consoleStream;

        private bool MatchConsoleManager(Type type) => type.Namespace == "BepInEx" && type.Name == "ConsoleManager";

        public ConsoleLogListener()
        {
            // Locate the ConsoleManager type from all loaded assemblies.
            var consoleManagerType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(MatchConsoleManager) ??
                throw new Exception("ConsoleManager type not found in loaded assemblies.");

            // Retrieve the static ConsoleStream property.
            var propertyInfo = consoleManagerType.GetProperty(
                "ConsoleStream",
                BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("ConsoleStream property not found on ConsoleManager type.");

            _consoleStream = propertyInfo.GetValue(null) as TextWriter;
            if (_consoleStream == null)
                throw new Exception("ConsoleStream is null.");
        }

        /// <summary>
        /// Called when a log event occurs. Converts the LogLevel to a LogCategory,
        /// builds an ANSI escape code string from its Unity color, and writes the log message.
        /// </summary>
        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            LogConsole.WriteLine(LogCategory.ToCategory(eventArgs.Level), eventArgs.Data?.ToString());
        }

        /// <summary>
        /// Cleanup any resources if necessary.
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose in this implementation.
        }
    }
}
