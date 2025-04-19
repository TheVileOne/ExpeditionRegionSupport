using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LogUtils.Console
{
    public static class LogConsole
    {
        /// <summary>
        /// Indicates whether the host machine supports ANSI color codes
        /// </summary>
        public static bool ANSIColorSupport;

        private static MethodInfo setConsoleColor; //Taken from BepInEx through reflection

        public static readonly List<ConsoleLogWriter> Writers = new List<ConsoleLogWriter>();

        /// <summary>
        /// Finds the writer associated with a given ConsoleID
        /// </summary>
        public static ConsoleLogWriter FindWriter(ConsoleID target, bool enabledOnly)
        {
            return Writers.Find(console => target == console.ID && (!enabledOnly || console.IsEnabled));
        }

        /// <summary>
        /// Finds all writers associated with the given ConsoleIDs
        /// </summary>
        public static ConsoleLogWriter[] FindWriters(IEnumerable<ConsoleID> targets, bool enabledOnly)
        {
            return Writers.FindAll(console => targets.Contains(console.ID) && (!enabledOnly || console.IsEnabled)).ToArray();
        }

        internal static void Initialize()
        {
            if (ConsoleVirtualization.TryEnableVirtualTerminal(out int errorCode))
            {
                ANSIColorSupport = true;
            }
            else
            {
                UtilityLogger.LogWarning($"[ERROR CODE {errorCode}] ANSI color codes are unsupported - using fallback method");
            }

            //Reflection allows us to interact with the BepInEx defined console stream. We cannot access it directly in BepInEx ver. 5.4.17.0. ConsoleManager is an internal class.
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

                setConsoleColor = consoleManagerType.GetMethod("SetConsoleColor");

                if (setConsoleColor == null)
                    UtilityLogger.LogError(new ConsoleLoadException("SetConsoleColor method not found on ConsoleManager type."));

                //Retrieve the static ConsoleStream property.
                consoleStreamProperty = consoleManagerType.GetProperty(
                    "ConsoleStream",
                    BindingFlags.Static | BindingFlags.Public) ??
                    throw new ConsoleLoadException("ConsoleStream property not found on ConsoleManager type.");

                TextWriter stream = consoleStreamProperty.GetValue(null) as TextWriter;

                if (stream == null)
                    throw new ConsoleLoadException("ConsoleStream is null.");

                Writers.RemoveAll(console => console.ID == ConsoleID.BepInEx);
                Writers.Add(new ConsoleLogWriter(ConsoleID.BepInEx, TextWriter.Synchronized(stream)));
            }
            catch (ConsoleLoadException ex)
            {
                UtilityLogger.LogError(ex);
            }
        }

        /// <summary>
        /// Fallback method for adjusting text color in the console
        /// </summary>
        public static void SetConsoleColor(ConsoleColor color)
        {
            setConsoleColor.Invoke(null, [color]);
        }

        private static bool matchConsoleManager(Type type) => type.Namespace == "BepInEx" && type.Name == "ConsoleManager";

        /// <summary>
        /// Writes a message to the BepInEx console (when enabled)
        /// </summary>
        /// <param name="category">The category associated with the message</param>
        /// <param name="message">The message to write</param>
        public static void WriteLine(LogCategory category, string message)
        {
            WriteLine(UtilityLogger.Logger, category, message);
        }

        public static void WriteLine(ILogSource source, LogCategory category, string message)
        {
            ConsoleLogWriter console = FindWriter(ConsoleID.BepInEx, enabledOnly: true);

            if (console == null) return;

            console.WriteFrom(new ConsoleLogRequest(new Events.LogMessageEventArgs(LogID.BepInEx, message, category)
            {
                LogSource = source
            }));
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
