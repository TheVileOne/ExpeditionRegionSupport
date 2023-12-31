using BepInEx.Logging;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Debug
{
    public class Logger
    {
        /// <summary>
        /// A flag that disables the primary logging path
        /// </summary>
        public bool BaseLoggingEnabled = true;

        /// <summary>
        /// A flag that affects whether log levels are included in logged output. Does not affect BepInEx logger
        /// </summary>
        public bool LogHeadersEnabled = true;

        /// <summary>
        /// When set, this will serve as an alternative logging output path
        /// </summary>
        private string logPath;

        /// <summary>
        /// When set this will completely replace the BepInEx logger as the base logger 
        /// </summary>
        private string baseLogPath;

        /// <summary>
        /// The default directory where logs are stored
        /// </summary>
        private string baseLogDirectory;

        private ManualLogSource baseLogger;

        public Logger(ManualLogSource logger)
        {
            baseLogger = logger;
        }

        public Logger(string filename, bool overwrite = false)
        {
            baseLogDirectory = AssetManager.ResolveDirectory("logs");//Path.Combine(Custom.RootFolderDirectory(), "logs");
            baseLogPath = Path.Combine(baseLogDirectory, filename);

            try
            {
                Directory.CreateDirectory(baseLogDirectory);

                if (overwrite)
                    File.Delete(baseLogPath);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Attaches a BepInEx logger
        /// </summary>
        public void AttachLogger(ManualLogSource logger)
        {
            BaseLoggingEnabled = true;
            baseLogger = logger;
        }

        /// <summary>
        /// Disables base logger, or nulls current log path
        /// </summary>
        public void DetachLogger(bool detachBaseLogger)
        {
            if (detachBaseLogger)
                BaseLoggingEnabled = false;
            else
                logPath = null;
        }

        /// <summary>
        /// Formats an output path for the logger
        /// </summary>
        public void SetLogger(string filename, string directory)
        {
            if (directory != null)
                filename = Path.Combine(directory, filename);

            SetLogger(filename);
        }

        public void SetLogger(string path)
        {
            logPath = path;
        }

        public void Log(object data, LogLevel level = LogLevel.None)
        {
            if (level == LogLevel.All)
            {
                UnityEngine.Debug.Log(data);
                level = LogLevel.Info;
            }

            int spacesRequired = Math.Max(7 - level.ToString().Length, 0);

            string logOutput = (LogHeadersEnabled ? $"[{level}" + new string(' ', spacesRequired) + "] " : string.Empty) + data?.ToString() ?? "NULL";

            //Send data to the BepInEx logger if enabled
            if (BaseLoggingEnabled)
            {
                if (baseLogPath == null)
                {
                    baseLogger?.Log(level, data);
                }
                else //Check for a custom base log path
                {
                    //StreamWriter writeStream = new StreamWriter(File.OpenWrite(BaseLogPath));
                    //Log(writeStream, data);
                    Log(baseLogPath, logOutput);
                }
            }

            //Check for a custom log path
            if (logPath != null)
            {
                //StreamWriter writeStream = new StreamWriter(File.OpenWrite(LogPath));
                //Log(writeStream, data);
                Log(logPath, logOutput);
            }
        }

        public static void Log(string path, string data)
        {
            File.AppendAllText(path, Environment.NewLine + data);
        }

        public static void Log(StreamWriter stream, object data)
        {
            try
            {
                //Log data to file
                stream.WriteLine(data);
                stream.WriteLine(Environment.NewLine);
                stream.Close(); //Stream needs to be closed to avoid IOExceptions
            }
            catch
            {
            }
        }

        public void LogInfo(object data)
        {
            Log(data, LogLevel.Info);
        }

        public void LogMessage(object data)
        {
            Log(data, LogLevel.Message);
        }

        public void LogDebug(object data)
        {
            Log(data, LogLevel.Debug);
        }

        public void LogWarning(object data)
        {
            Log(data, LogLevel.Warning);
        }

        public void LogError(object data)
        {
            Log(data, LogLevel.Error);
        }
    }
}
