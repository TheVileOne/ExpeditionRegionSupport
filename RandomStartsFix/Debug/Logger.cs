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
        public bool BaseLoggingEnabled = true;
        public string LogPath { get; private set; }

        /// <summary>
        /// When set this will completely replace the BepInEx logger as the base logger 
        /// </summary>
        public string BaseLogPath {  get; private set; }

        public string BaseLogDirectory { get; private set; }

        private ManualLogSource baseLogger;

        public Logger(ManualLogSource logger)
        {
            baseLogger = logger;
        }

        public Logger(string filename, bool overwrite = false)
        {
            BaseLogDirectory = AssetManager.ResolveDirectory("logs");//Path.Combine(Custom.RootFolderDirectory(), "logs");
            BaseLogPath = Path.Combine(BaseLogDirectory, filename);

            try
            {
                Directory.CreateDirectory(BaseLogDirectory);

                if (overwrite)
                    File.Delete(BaseLogPath);
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
                LogPath = null;
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
            LogPath = path;
        }

        public void Log(object data, LogLevel level = LogLevel.None)
        {
            if (level == LogLevel.All)
            {
                UnityEngine.Debug.Log(data);
                level = LogLevel.Info;
            }

            //Send data to the BepInEx logger if enabled
            if (BaseLoggingEnabled)
            {
                if (BaseLogPath == null)
                {
                    baseLogger?.Log(level, data);
                }
                else //Check for a custom base log path
                {
                    //StreamWriter writeStream = new StreamWriter(File.OpenWrite(BaseLogPath));
                    //Log(writeStream, data);
                    Log(BaseLogPath, data?.ToString() ?? "NULL");
                }
            }

            //Check for a custom log path
            if (LogPath != null)
            {
                //StreamWriter writeStream = new StreamWriter(File.OpenWrite(LogPath));
                //Log(writeStream, data);
                Log(LogPath, data?.ToString() ?? "NULL");
            }
        }

        public void Log(string path, string data)
        {
            File.AppendAllText(path, Environment.NewLine + data);
        }

        public void Log(StreamWriter stream, object data)
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
