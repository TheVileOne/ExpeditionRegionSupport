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
        public bool BaseLoggingEnabled
        {
            get => BaseLogger.Enabled;
            set => BaseLogger.Enabled = value;
        }

        private bool headersEnabled;

        /// <summary>
        /// A flag that affects whether log levels are included in logged output for all loggers. Does not affect BepInEx logger
        /// </summary>
        public bool LogHeadersEnabled
        {
            get => headersEnabled;
            set
            {
                headersEnabled = value;
                AllLoggers.ForEach(logger => logger.HeadersEnabled = value);
            }
        }

        public LogModule BaseLogger { get; private set; }
        public LogModule ActiveLogger;

        public List<LogModule> AllLoggers = new List<LogModule>();

        /// <summary>
        /// The default directory where logs are stored
        /// </summary>
        private string baseDirectory;

        public Logger(ManualLogSource logger)
        {
            BaseLogger = new LogModule(logger);
            AttachLogger(BaseLogger);
        }

        public Logger(string logName, bool overwrite = false)
        {
            baseDirectory = AssetManager.ResolveDirectory("logs");//Path.Combine(Custom.RootFolderDirectory(), "logs");

            try
            {
                Directory.CreateDirectory(baseDirectory);

                BaseLogger = new LogModule(Path.Combine(baseDirectory, logName + ".txt"));
                AttachLogger(BaseLogger);

                if (overwrite)
                    File.Delete(BaseLogger.LogPath);
            }
            catch
            {
            }
        }

        public void AttachLogger(string logName, string logDirectory = null)
        {
            if (logDirectory == null)
                logDirectory = baseDirectory;

            AttachLogger(new LogModule(Path.Combine(logDirectory, logName + ".txt")));
        }

        public void AttachLogger(LogModule logModule)
        {
            if (AllLoggers.Exists(logger => logger.IsLog(logModule))) return;

            if (BaseLogger != logModule) //The latest logger added will be set as the ActiveLogger
                ActiveLogger = logModule;

            logModule.HeadersEnabled = LogHeadersEnabled;
            AllLoggers.Add(logModule);
        }

        public void AttachBaseLogger(string logName)
        {
            AttachBaseLogger(logName, baseDirectory);
        }

        public void AttachBaseLogger(string logName, string logDirectory)
        {
            if (BaseLogger.IsLog(logName)) return;

            BaseLogger.LogPath = Path.Combine(logDirectory, logName + ".txt");
        }

        public void AttachBaseLogger(LogModule logger)
        {
            AllLoggers.Remove(BaseLogger);
            BaseLogger = logger;
            AttachLogger(BaseLogger);
        }

        public void AttachBaseLogger(ManualLogSource logSource)
        {
            BaseLogger.LogSource = logSource;
        }

        /// <summary>
        /// Removes, and sets to null the ActiveLogger
        /// </summary>
        public void DetachLogger()
        {
            if (ActiveLogger == null) return;

            AllLoggers.Remove(ActiveLogger);
            ActiveLogger = null;
        }

        /// <summary>
        /// Disables base logger, or removes logger with given logName
        /// </summary>
        public void DetachLogger(string logName)
        {
            //The base logger cannot be detached
            if (BaseLogger.IsLog(logName))
            {
                BaseLogger.Enabled = false;
                return;
            }

            if (ActiveLogger != null && ActiveLogger.IsLog(logName))
                ActiveLogger = null;

            AllLoggers.RemoveAll(logger => logger.IsLog(logName));
        }

        public void SetActiveLogger(string logName)
        {
            LogModule found = AllLoggers.Find(logger => logger.IsLog(logName));

            if (found != null)
                ActiveLogger = found;
            else
                AttachLogger(logName);
        }

        public void Log(object data, LogLevel level = LogLevel.None)
        {
            if (level == LogLevel.All)
            {
                UnityEngine.Debug.Log(data);
                level = LogLevel.Info;

                AllLoggers.ForEach(logger => logger.Log(data, level));
                return;
            }

            BaseLogger.Log(data, level);
            ActiveLogger?.Log(data, level);
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
            LogPath = logPath;
        }

        public bool IsLog(string logName)
        {
            return LogPath != null && Path.GetFileNameWithoutExtension(LogPath) == logName;
        }

        public bool IsLog(LogModule logger)
        {
            return (LogSource != null && LogSource == logger.LogSource) || (LogPath == logger.LogPath);
        }

        public void Log(object data, LogLevel level)
        {
            if (!Enabled) return;

            if (LogPath != null)
            {
                int spacesRequired = Math.Max(7 - level.ToString().Length, 0);

                string logOutput = (HeadersEnabled ? $"[{level}" + new string(' ', spacesRequired) + "] " : string.Empty) + data?.ToString() ?? "NULL";
                File.AppendAllText(LogPath, Environment.NewLine + logOutput);
            }
            else if (LogSource != null)
            {
                LogSource.Log(level, data);
            }
            else
            {
                UnityEngine.Debug.Log(data);
            }
        }
    }
}
