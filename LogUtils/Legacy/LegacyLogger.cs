using BepInEx.Logging;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LogUtils.Legacy
{
    public class LegacyLogger : IDisposable
    {
        /// <summary>
        /// The name of the combined mod log file in the Logs directory. Only produced with LogManager plugin.
        /// </summary>
        public static readonly string OUTPUT_NAME = "mods";

        /// <summary>
        /// The folder name that will store log files. Do not change this. It is case-sensitive.
        /// </summary>
        public static readonly string LOGS_FOLDER_NAME = "Logs";

        /// <summary>
        /// Returns whether BaseLogger contains a ManualLogSource object used by BepInEx
        /// </summary>
        public bool BepInExEnabled => BaseLogger.LogSource != null;

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
        /// The default directory where logs are stored. This is managed by the mod.
        /// </summary>
        public static string BaseDirectory;

        public LegacyLogger(LogModule logModule)
        {
            if (logModule == null)
                throw new ArgumentNullException(nameof(logModule));

            BaseLogger = logModule;
            AttachLogger(BaseLogger);

            applyEventHandlers();
        }

        public LegacyLogger(ManualLogSource logger) : this(new LogModule(logger))
        {
        }

        public LegacyLogger(string logName, bool overwrite = false)
        {
            InitializeLogDirectory();

            try
            {
                BaseLogger = new LogModule(Path.Combine(BaseDirectory, logName));
                AttachLogger(BaseLogger);

                if (overwrite)
                    File.Delete(BaseLogger.LogPath);
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Unable to replace existing log file", ex);
            }

            applyEventHandlers();
        }

        /// <summary>
        /// Rain World root folder
        /// Application.dataPath is RainWorld_data folder
        /// </summary>
        public static readonly string DefaultLogPath = Path.Combine(Path.GetDirectoryName(Application.dataPath)/*Directory.GetParent(Application.dataPath).FullName*/, LOGS_FOLDER_NAME);

        /// <summary>
        /// StreamingAssets folder
        /// </summary>
        public static readonly string AlternativeLogPath = Path.Combine(Application.streamingAssetsPath, LOGS_FOLDER_NAME);

        public static bool HasInitialized;

        #region Static Methods

        public static void InitializeLogDirectory()
        {
            if (HasInitialized) return;

            BaseDirectory = FindLogsDirectory();
            try
            {
                //The found directory needs to be created if it doesn't yet exist, and the alternative directory removed
                if (!Directory.Exists(BaseDirectory))
                {
                    UtilityLogger.Log("Creating directory: " + BaseDirectory);
                    Directory.CreateDirectory(BaseDirectory);
                }

                string alternativeLogPath = string.Equals(BaseDirectory, DefaultLogPath) ? AlternativeLogPath : DefaultLogPath;

                try
                {
                    if (Directory.Exists(alternativeLogPath))
                    {
                        UtilityLogger.Log("Removing directory: " + alternativeLogPath);
                        Directory.Delete(alternativeLogPath, true);
                    }
                }
                catch (Exception ex)
                {
                    UtilityLogger.LogError("Unable to delete log directory", ex);
                }
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Unable to create log directory", ex);
            }

            HasInitialized = true;
        }

        public static string FindExistingLogsDirectory()
        {
            if (Directory.Exists(DefaultLogPath))
                return DefaultLogPath;

            if (Directory.Exists(AlternativeLogPath))
                return AlternativeLogPath;

            return null;
        }

        public static string FindLogsDirectory()
        {
            return FindExistingLogsDirectory() ?? DefaultLogPath;
        }

        /// <summary>
        /// Takes a filename and attaches the path stored in BaseDirectory
        /// </summary>
        /// <param name="useLogExt">A flag to convert extension to .log</param>
        public static string ApplyLogPathToFilename(string filename, bool useLogExt = true)
        {
            if (useLogExt)
                filename = FormatLogFile(filename);

            return Path.Combine(BaseDirectory ?? FindLogsDirectory(), filename);
        }

        /// <summary>
        /// Change the extension of any filename to .log
        /// </summary>
        public static string FormatLogFile(string filename)
        {
            return Path.ChangeExtension(filename, ".log");
        }

        /// <summary>
        /// Moves a log file from one place to another. Allows file renaming.
        /// </summary>
        /// <param name="sourceLogPath">The full path to the log file that needs to be moved (including filename + ext)</param>
        /// <param name="destLogPath">The full path to the destination of the log file. Log filename is optional.</param>
        public static FileStatus MoveLog(string sourceLogPath, string destLogPath)
        {
            LogFileMover fileMover = new LogFileMover(sourceLogPath, destLogPath);

            return fileMover.MoveFile();
        }

        private static bool listenerCheckComplete;

        /// <summary>
        /// The ILogListener managed by the LogManager plugin. Null when LogManager isn't enabled.
        /// </summary>
        private static ILogListener managedLogListener;

        /// <summary>
        /// Hooks into RainWorld.Update
        /// This is required for the signaling system. All remote loggers should use this hook to ensure that the logger is aware
        /// of the Logs directory being moved.
        /// </summary>
        internal static void ApplyHooks()
        {
            On.RainWorld.Update += RainWorld_Update;
        }

        private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
        {
            orig(self);

            //Logic is handled after orig for several reasons. The main reason is that all remote loggers are guaranteed to receive any signals set during update
            //no matter where they are in the load order. Signals are created pre-update, or during update only.
            if (self.started)
            {
                if (!listenerCheckComplete)
                {
                    managedLogListener = findManagedListener();
                    listenerCheckComplete = true;
                }

                if (managedLogListener != null)
                    processLogSignal(managedLogListener.GetSignal());
            }
        }

        /// <summary>
        /// Check that a path matches one of the two supported Logs directories.
        /// </summary>
        public static bool IsValidLogPath(string path)
        {
            if (path == null) return false;

            path = Path.GetFullPath(path).TrimEnd('\\');

            return string.Equals(path, DefaultLogPath, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(path, AlternativeLogPath, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Checks a path against the BaseDirectory
        /// </summary>
        public static bool IsBaseLogPath(string path)
        {
            if (path == null) return false;

            //Strip the filename if one exists
            if (Path.HasExtension(path))
                path = Path.GetDirectoryName(path);
            else
                path = Path.GetFullPath(path).TrimEnd('\\');

            string basePath = Path.GetFullPath(BaseDirectory).TrimEnd('\\');

            UtilityLogger.Log("Comparing " + path + " to  base " + basePath);

            return string.Equals(path, basePath, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Action is invoked when the Logs directory is going to be moved on the next Update frame. May get called multiple times if move fails.
        /// If your logger uses a FileStream for logging to the Logs folder, please make sure it is closed upon activation of this event.
        /// </summary>
        public static Action OnMovePending;

        /// <summary>
        /// Action is invoked when all move attempts have failed. Signal will return to Signal.None on the following frame.
        /// </summary>
        public static Action OnMoveAborted;

        /// <summary>
        /// Action is invoked immediately after the Logs directory is successfully moved. The new path is given as an argument.
        /// If your logger uses a FileStream for logging to the Logs folder, it is safe to reenable it here.
        /// </summary>
        public static Action<string> OnMoveComplete;

        /// <summary>
        /// Handles an event based on a provided signal word
        /// </summary>
        private static void processLogSignal(string signal)
        {
            if (signal == "Signal.None") return;

            string[] signalData = Regex.Split(signal, "\\.");
            string signalWord = signalData[1]; //Signal.Keyword.Other data

            //Debug.Log("SIGNAL: " + signal);
            //Debug.Log("SIGNAL WORD: " + signalWord);

            //Remote loggers need to be informed of when the Logs folder is moved.
            //The folder cannot be moved if any log file has an open filestream active
            if (signalWord == "MovePending")
                OnMovePending?.Invoke();
            else if (signalWord == "MoveComplete")
            {
                string path = signalData[2];

                UtilityLogger.Log("Log directory changed to " + path);

                OnMoveComplete?.Invoke(path);
                BaseDirectory = path; //This gets updated last. It is needed for comparison purposes.
            }
            else if (signalWord == "MoveAborted")
                OnMoveAborted?.Invoke();
        }

        /// <summary>
        /// Look for an ILogListener that returns a signal sent through ToString().
        /// This listener belongs to the LogManager plugin.
        /// </summary>
        private static ILogListener findManagedListener()
        {
            //Look for an ILogListener with a signal. This listener belongs to the LogManager plugin
            IEnumerator<ILogListener> enumerator = BepInEx.Logging.Logger.Listeners.GetEnumerator();

            ILogListener managedListener = null;
            while (enumerator.MoveNext() && managedListener == null)
            {
                if (enumerator.Current.GetSignal() != null)
                    managedListener = enumerator.Current;
            }

            return managedListener;
        }

        #endregion

        private void applyEventHandlers()
        {
            OnMoveComplete += onLogDirectoryPathChanged;
        }

        private void removeEventHandlers()
        {
            OnMoveComplete -= onLogDirectoryPathChanged;
        }

        private void onLogDirectoryPathChanged(string path)
        {
            //Update all loggers with new path information
            foreach (LogModule logger in AllLoggers)
            {
                if (IsBaseLogPath(logger.LogPath))
                    logger.LogPath = Path.Combine(path, Path.GetFileName(logger.LogPath));
            }
        }

        public void AttachLogger(string logName, string logDirectory = null)
        {
            if (logDirectory == null)
                logDirectory = BaseDirectory;

            AttachLogger(new LogModule(Path.Combine(logDirectory, logName)));
        }

        public void AttachLogger(LogModule logModule)
        {
            if (BaseLogger != logModule) //The latest logger added will be set as the ActiveLogger
                ActiveLogger = logModule;

            if (AllLoggers.Exists(logger => logger.IsLog(logModule))) return;

            logModule.HeadersEnabled = LogHeadersEnabled;
            AllLoggers.Add(logModule);
        }

        public void AttachBaseLogger(string logName)
        {
            AttachBaseLogger(logName, BaseDirectory);
        }

        public void AttachBaseLogger(string logName, string logDirectory)
        {
            if (BaseLogger.IsLog(logName)) return;

            BaseLogger.LogPath = Path.Combine(logDirectory, FormatLogFile(logName));
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

        public void Log(LogEventArgs logEvent)
        {
            Log(logEvent.Data, logEvent.Level);
        }

        public void Log(object data, LogLevel level = LogLevel.None)
        {
            if (level == LogLevel.All)
            {
                Debug.Log(data);
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

        public void Dispose()
        {
            removeEventHandlers();
        }
    }
}