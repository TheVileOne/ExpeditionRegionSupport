using BepInEx.Logging;
using System;
using System.IO;
using UnityEngine;

namespace LogUtils.Legacy
{
    /// <summary>
    /// Contains components of the logger
    /// </summary>
    public class LogModule
    {
        private ManualLogSource logSource;
        public ManualLogSource LogSource
        {
            get => logSource;
            set
            {
                if (value != null)
                    LogPath = null;
                logSource = value;
            }
        }

        /// <summary>
        /// A flag that determines if log details should be written to file
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// A flag that determines whether log levels should be displayed as header information.
        /// Does not apply to BepInEx logger
        /// </summary>
        public bool HeadersEnabled = true;

        /// <summary>
        /// The full path for this Logger
        /// </summary>
        public string LogPath;

        public LogModule(ManualLogSource logSource)
        {
            LogSource = logSource;
        }

        public LogModule(string logPath)
        {
            LogPath = Logger.FormatLogFile(logPath);
        }

        public bool IsLog(string logName)
        {
            return LogPath != null && LogPath == Logger.FormatLogFile(logName);
        }

        public bool IsLog(LogModule logger)
        {
            return LogSource != null && LogSource == logger.LogSource || LogPath == logger.LogPath;
        }

        public void Log(object data, LogLevel level)
        {
            if (!Enabled) return;

            //TODO: This is an old and probably non-thread safe way of logging. Replace with new method
            if (LogPath != null)
            {
                string logMessage = Environment.NewLine + FormatLogMessage(data?.ToString(), level);
                File.AppendAllText(LogPath, logMessage);
            }
            else if (LogSource != null)
            {
                LogSource.Log(level, data);
            }
            else
            {
                Debug.Log(data);
            }
        }

        public string FormatLogMessage(string message, LogLevel level)
        {
            string header = FormatHeader(level) + (HeadersEnabled ? ' ' : string.Empty);

            return header + (message ?? "NULL");
        }

        public string FormatHeader(LogLevel level)
        {
            if (HeadersEnabled)
            {
                int spacesRequired = Math.Max(7 - level.ToString().Length, 0);

                return $"[{level}" + new string(' ', spacesRequired) + "]";
            }

            return string.Empty;
        }
    }
}
