using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils
{
    internal sealed class DirectToFileLogger : ILogger
    {
        public const string DEFAULT_LOG_NAME = "debug";

        public bool AllowLogging;

        public string Name { get; }

        public string LogPath { get; }

        public MessageBuffer WriteBuffer;

        public DirectToFileLogger(string name, bool enabledByDefault) : base()
        {
            AllowLogging = enabledByDefault;
            Name = name;
            LogPath = getLogPath();
            WriteBuffer = new MessageBuffer();
        }

        private string getLogPath()
        {
            int processID = Process.GetCurrentProcess().Id;
            string filename = string.Format("{0}[{1}]{2}", Name, processID, FileExt.DEFAULT);

            return Path.Combine(RainWorldPath.RootPath, filename);
        }

        /// <summary>
        /// Attempts to delete the debug log for the current Rain World process
        /// </summary>
        internal void Delete()
        {
            FileUtils.TryDelete(LogPath);
        }

        /// <summary>
        /// Attempts to cleanup stray debug logs that were created by LogUtils belonging to different Rain World processes
        /// </summary>
        internal void DeleteAll()
        {
            foreach (string file in Directory.GetFiles(RainWorldPath.RootPath, Name + "[*", SearchOption.TopDirectoryOnly))
                FileUtils.TryDelete(file);
        }

        public bool TryFlush()
        {
            if (!WriteBuffer.HasContent)
                return true;

            try
            {
                Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Flush()
        {
            //Always handle buffered write data first
            if (WriteBuffer.HasContent)
            {
                FileUtils.AppendLine(LogPath, WriteBuffer.ToString());
                WriteBuffer.Clear();
            }
        }

        public void Log(object messageObj)
        {
            if (!AllowLogging) return;

            string message = messageObj?.ToString();

            //not allowed to write to file when in buffering mode
            if (WriteBuffer.IsBuffering)
            {
                WriteBuffer.AppendMessage(message);
                return;
            }

            try
            {
                Flush();
                FileUtils.AppendLine(LogPath, message);
            }
            catch
            {
                WriteBuffer.AppendMessage(message);
            }
        }

        public void Log(LogType category, object messageObj)
        {
            Log(messageObj);
        }

        public void Log(LogLevel category, object messageObj)
        {
            Log(messageObj);
        }

        public void Log(LogCategory category, object messageObj)
        {
            Log(messageObj);
        }

        public void LogDebug(object messageObj)
        {
            Log(messageObj);
        }

        public void LogError(object messageObj)
        {
            Log(messageObj);
        }

        public void LogFatal(object messageObj)
        {
            Log(messageObj);
        }

        public void LogImportant(object messageObj)
        {
            Log(messageObj);
        }

        public void LogInfo(object messageObj)
        {
            Log(messageObj);
        }

        public void LogMessage(object messageObj)
        {
            Log(messageObj);
        }

        public void LogWarning(object messageObj)
        {
            Log(messageObj);
        }
    }
}
