using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Events;
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

        private static MethodInfo createConsole;
        private static MethodInfo detachConsole;
        private static MethodInfo setConsoleColor;

        /// <summary>
        /// Initialization process was unable to complete. This is an indication that the state is invalid
        /// </summary>
        public static bool InitializedWithErrors { get; private set; }

        public static bool IsInitialized { get; private set; }

        public static bool IsEnabled { get; private set; }

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
            UtilityLogger.Log("Checking for console availability");

            var consoleState = getManagedBepInExState();

            InitializedWithErrors = consoleState.ProcessedWithErrors;

            if (InitializedWithErrors)
                UtilityLogger.LogError(consoleState.Exception);

            createConsole = consoleState.CreateConsole;
            detachConsole = consoleState.DetachConsole;
            setConsoleColor = consoleState.SetConsoleColor;

            ConsoleLogWriter writer = null;
            if (consoleState.IsEnabled)
            {
                if (ConsoleVirtualization.TryEnableVirtualTerminal(out int errorCode))
                {
                    ANSIColorSupport = true;
                }
                else
                {
                    UtilityLogger.LogWarning($"[ERROR CODE {errorCode}] ANSI color codes are unsupported - using fallback method");
                }

                if (consoleState.ConsoleStream != null) //I don't know if it is possible for the stream to be null here
                {
                    //TODO: Writer may need to be included at a later time (Override BepInEx console config setting) 
                    Writers.RemoveAll(console => console.ID == ConsoleID.BepInEx);
                    Writers.Add(writer = new ConsoleLogWriter(ConsoleID.BepInEx, TextWriter.Synchronized(consoleState.ConsoleStream)));
                }
            }

            //Console is considered in a functional state when the log writer could be instantiated successfully
            if (writer == null)
            {
                UtilityLogger.Log("Console is disabled");
                IsEnabled = false;
            }
            else
            {
                UtilityLogger.Log("Console is enabled");
                IsEnabled = true;
            }

            IsInitialized = true;
        }

        private static ReflectionResult getManagedBepInExState()
        {
            ReflectionResult result = new ReflectionResult();

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

                result.IsEnabled = (bool)consoleActiveProperty.GetValue(null);

                result.SetConsoleColor = consoleManagerType.GetMethod("SetConsoleColor");
                result.CreateConsole = consoleManagerType.GetMethod("CreateConsole");
                result.DetachConsole = consoleManagerType.GetMethod("DetachConsole");

                if (!result.MethodStatesAreValid)
                    UtilityLogger.LogError(new ConsoleLoadException("One or more required methods could not be found on ConsoleManager type. Check BepInEx version"));

                //Retrieve the static ConsoleStream property.
                consoleStreamProperty = consoleManagerType.GetProperty(
                    "ConsoleStream",
                    BindingFlags.Static | BindingFlags.Public) ??
                    throw new ConsoleLoadException("ConsoleStream property not found on ConsoleManager type.");

                result.ConsoleStream = consoleStreamProperty.GetValue(null) as TextWriter;

                if (result.IsEnabled && result.ConsoleStream == null)
                    throw new ConsoleLoadException("ConsoleStream is null.");
            }
            catch (ConsoleLoadException ex)
            {
                result.Exception = ex;
            }
            return result;
        }

        /// <summary>
        /// Fallback method for adjusting text color in the console
        /// </summary>
        public static void SetConsoleColor(ConsoleColor color)
        {
            setConsoleColor.Invoke(null, [color]);
        }

        /// <summary>
        /// Sets the enabled state for the BepInEx console (when it supported) 
        /// </summary>
        public static void SetEnabledState(bool state)
        {
            if (IsEnabled == state) return;

            try
            {
                if (state)
                {
                    createConsole.Invoke(null, null);
                    UtilityLogger.Log("Creating console window"); //Needs to log after console construction to show up in the console
                }
                else
                {
                    UtilityLogger.Log("Destroying console window");
                    detachConsole.Invoke(null, null);
                }

                //TODO: Confirm that console will always show, or not be shown if execution makes it to this point
                IsEnabled = state;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(ex);
            }
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

            LogRequest request = new LogRequest(RequestType.Console, new LogMessageEventArgs(LogID.BepInEx, message, category)
            {
                LogSource = source
            });

            request.Data.ExtraArgs.Add(new ConsoleRequestEventArgs(ConsoleID.BepInEx));
            console.WriteFrom(request);
        }

        private struct ReflectionResult
        {
            internal MethodInfo CreateConsole;
            internal MethodInfo DetachConsole;
            internal MethodInfo SetConsoleColor; //Taken from BepInEx through reflection

            internal TextWriter ConsoleStream;
            internal bool IsEnabled;

            internal Exception Exception;

            internal bool MethodStatesAreValid => CreateConsole != null && DetachConsole != null && SetConsoleColor != null;

            internal bool ProcessedWithErrors => Exception != null;
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
