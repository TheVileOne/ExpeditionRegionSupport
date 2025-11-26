using BepInEx.Logging;
using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Requests;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils.Console
{
    /// <summary>
    /// This class is responsible for accessing the BepInEx console
    /// </summary>
    public static class LogConsole
    {
        /// <summary>
        /// Indicates whether the host machine supports ANSI color codes
        /// </summary>
        public static bool ANSIColorSupport;

        public static bool IsEnabled { get; private set; }

        /// <summary>
        /// This lock is used for interacting with BepInEx log console
        /// </summary>
        public static readonly Lock WriteLock = new Lock();

        public static readonly List<ConsoleLogWriter> Writers = new List<ConsoleLogWriter>();

        internal static void EnableAnsiSupport()
        {
            ANSIColorSupport = ConsoleVirtualization.TryEnableVirtualTerminal(out int errorCode);

            if (!ANSIColorSupport)
                UtilityLogger.LogWarning($"[ERROR CODE {errorCode}] ANSI color codes are unsupported");
        }

        /// <summary>
        /// Finds the writer associated with a given <see cref="ConsoleID"/> instance
        /// </summary>
        public static ConsoleLogWriter FindWriter(ConsoleID target, bool enabledOnly)
        {
            return Writers.Find(console => target == console.ID && (!enabledOnly || console.IsEnabled));
        }

        /// <summary>
        /// Finds all writers associated with the given <see cref="ConsoleID"/> instances
        /// </summary>
        public static ConsoleLogWriter[] FindWriters(IEnumerable<ConsoleID> targets, bool enabledOnly)
        {
            return Writers.FindAll(console => targets.Contains(console.ID) && (!enabledOnly || console.IsEnabled)).ToArray();
        }

        internal static bool HasCompatibleWriter()
        {
            //Users should not be tampering with ConsoleID.BepInEx. This should be a reliable way of telling if console state should be active
            var writer = FindWriter(ConsoleID.BepInEx, enabledOnly: false);
            return writer != null;
        }

        internal static void HandleRequest(LogRequest request)
        {
            try
            {
                //Get all pending console requests to handle
                var pendingIDs = request.Data.PendingConsoleIDs.ToArray();

                foreach (ConsoleID consoleID in pendingIDs)
                {
                    //Find a writer that is able to process the request
                    ConsoleLogWriter console = FindWriter(consoleID, enabledOnly: false);

                    if (console == null)
                    {
                        request.Reject(RejectionReason.LogUnavailable, consoleID);
                        continue;
                    }

                    //Send data to the console
                    console.WriteFrom(request);
                }
                Assert.That(request.Data.PendingConsoleIDs.Any()).IsFalse();
            }
            finally
            {
                //It should not be possible for ConsoleIDs to be pending here
                if (request.Type == RequestType.Console)
                {
                    //Console requests are not designed to be tried multiple times
                    if (request.Status != RequestStatus.Rejected)
                        request.Complete();

                    UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
                }
            }
        }

        internal static void Initialize()
        {
            UtilityLogger.Log("Checking for console availability");

            TextWriter consoleStream = null;
            if (BepInEx.ConsoleManager.ConsoleActive)
            {
                EnableAnsiSupport();
                consoleStream = BepInEx.ConsoleManager.ConsoleStream;
                if (consoleStream != null) //I don't know if it is possible for the stream to be null here
                {
                    //TODO: Override BepInEx console config setting
                    AddWriter(new ConsoleLogWriter(ConsoleID.BepInEx));
                }
            }

            //Console is considered in a functional state when the log writer could be instantiated successfully
            IsEnabled = consoleStream != null;

            if (IsEnabled)
                UtilityLogger.Log("Console stream started");
            else
                UtilityLogger.Log("Console is disabled");
        }

        /// <summary>
        /// Registers a <see cref="ConsoleLogWriter"/> instance
        /// </summary>
        public static void AddWriter(ConsoleLogWriter writer)
        {
            Writers.RemoveAll(console => console.ID == writer.ID); //Ensure only one instance is active per ID
            Writers.Add(writer);
        }

        /// <summary>
        /// Fallback method for adjusting text color in the console
        /// </summary>
        public static void SetConsoleColor(ConsoleColor color)
        {
            BepInEx.ConsoleManager.SetConsoleColor(color);
        }

        /// <summary>
        /// Sets the enabled state for the BepInEx console (when it is supported) 
        /// </summary>
        public static void SetEnabledState(bool state)
        {
            if (IsEnabled == state) return;

            try
            {
                WriteLock.Acquire();

                var consoleStream = BepInEx.ConsoleManager.ConsoleStream;
                var consoleWriters = Writers.FindAll(console => console.ID == ConsoleID.BepInEx || console.Stream == consoleStream).ToArray();
                if (state)
                {
                    BepInEx.ConsoleManager.CreateConsole();

                    EnableAnsiSupport();
                    foreach (var console in consoleWriters)
                        console.ReloadStream();

                    UtilityLogger.Log("Creating console window"); //Needs to log after console construction to show up in the console
                }
                else
                {
                    UtilityLogger.Log("Destroying console window");
                    BepInEx.ConsoleManager.DetachConsole();
                }

                //TODO: Can this be implemented?
                //foreach (var writer in consoleWriters)
                //    writer.IsEnabled = state;
                IsEnabled = state;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(ex);
            }
            finally
            {
                WriteLock.Release();
            }
        }

        /// <summary>
        /// Writes a message to the BepInEx console (when enabled)
        /// </summary>
        /// <param name="category">The category associated with the message</param>
        /// <param name="message">The message to write</param>
        public static void WriteLine(LogCategory category, string message)
        {
            WriteLine(UtilityLogger.Logger, category, message);
        }

        /// <inheritdoc cref="WriteLine(LogCategory, string)"/>
        public static void WriteLine(string message)
        {
            WriteLine(LogCategory.Info, message);
        }

        public static void WriteLine(ILogSource source, LogCategory category, string message)
        {
            ConsoleLogWriter console = FindWriter(ConsoleID.BepInEx, enabledOnly: true);

            if (console == null)
                return;

            LogRequest request = new LogRequest(RequestType.Console, new LogRequestEventArgs(ConsoleID.BepInEx, message, category)
            {
                LogSource = source
            });
            console.WriteFrom(request);
        }
    }
}
