using BepInEx.Logging;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace ExpeditionRegionSupport.Logging
{
    public class Logger : IDisposable
    {
        private static LoggingFileWatcher logFileWatcher;

        /*private static bool? logManagerActive_;
        internal static bool LogManagerActive
        {
            get
            {
                if (logManagerActive_ == null)
                    logManagerActive_ = ModManager.ActiveMods.Exists(m => m.id == "fluffball.logmanager");
                return logManagerActive_.Value;
            }
        }*/

        /// <summary>
        /// The name of the combined mod log file in the Logs directory. Only produced with LogManager plugin.
        /// </summary>
        public static readonly string OUTPUT_NAME = "mod-log";

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
        /// The default directory where logs are stored. (DO NOT CHANGE)
        /// </summary>
        public static string BaseDirectory;

        public Logger(ManualLogSource logger)
        {
            BaseLogger = new LogModule(logger);
            AttachLogger(BaseLogger);

            attachEvents();
        }

        public Logger(LogModule logModule, bool overwrite = false)
        {
            if (logModule == null)
                throw new ArgumentNullException(nameof(logModule));

            BaseLogger = logModule;
            AttachLogger(BaseLogger);

            attachEvents();
        }

        public Logger(string logName, bool overwrite = false)
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
                Debug.LogError("Unable to replace existing log file");
                Debug.LogError(ex);
            }

            attachEvents();
        }

        private void attachEvents()
        {
            OnMoveComplete += onLogDirectoryPathChanged;
            //logFileWatcher.OnDirectoryUpdate += onLogDirectoryUpdate;
        }

        private void detachEvents()
        {
            OnMoveComplete -= onLogDirectoryPathChanged;
            //logFileWatcher.OnDirectoryUpdate -= onLogDirectoryUpdate;
        }

        private void onLogDirectoryPathChanged(string path)
        {
            Debug.Log("Log directory changed to " + path);

            //Update all loggers with new path information
            foreach (LogModule logger in AllLoggers)
            {
                if (isBaseLogPath(logger.LogPath))
                    logger.LogPath = Path.Combine(path, Path.GetFileName(logger.LogPath));
            }

            //This gets updated last. It is needed for comparison purposes.
            BaseDirectory = path;
        }

        private void onLogDirectoryUpdate(object sender, FileSystemEventArgs fileSystemEvent)
        {
            string logPath = getValidPathMatch(fileSystemEvent.FullPath);

            if (!isBaseLogPath(logPath))
            {
                Debug.Log("Log directory changed to " + logPath);

                //Update all loggers with new path information
                foreach (LogModule logger in AllLoggers)
                {
                    if (isBaseLogPath(logger.LogPath))
                        logger.LogPath = Path.Combine(logPath, Path.GetFileName(logger.LogPath));
                }

                //This gets updated last. It is needed for comparison purposes.
                BaseDirectory = logPath;
            }
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

        /// <summary>
        /// Check that a path matches one, of the two supported Logs directories.
        /// </summary>
        private static string getValidPathMatch(string path)
        {
            if (path == null) return null;

            path = Path.GetFullPath(path).TrimEnd('\\');

            if (string.Equals(path, DefaultLogPath, StringComparison.InvariantCultureIgnoreCase))
                return DefaultLogPath;

            if (string.Equals(path, AlternativeLogPath, StringComparison.InvariantCultureIgnoreCase))
                return AlternativeLogPath;

            //This shouldn't run under expected circumstances
            Debug.LogWarning("Invalid log directory");
            return null;
        }

        private bool isBaseLogPath(string path)
        {
            if (path == null) return false;

            //Strip the filename if one exists
            if (Path.HasExtension(path))
                path = Path.GetDirectoryName(path);
            else
                path = Path.GetFullPath(path).TrimEnd('\\');

            return string.Equals(path, BaseDirectory, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// No logs can be handled when this flag is false. Only set to true when there is a valid directory to put logs within.
        /// </summary>
        internal static bool DirectoryMustBeUpdated;

        /// <summary>
        /// This flag indicates that a LogManager event invocation is expected
        /// </summary>
        internal static bool AwaitingLogManager;

        public static bool HasInitialized;
        public static void InitializeLogDirectory()
        {
            if (HasInitialized) return;

            BaseDirectory = FindLogsDirectory();
            try
            {
                //The found directory needs to be created if it doesn't yet exist, and the alternative directory removed
                if (!Directory.Exists(BaseDirectory))
                {
                    Plugin.Logger.LogInfo("Creating directory: " + BaseDirectory);
                    Directory.CreateDirectory(BaseDirectory);
                }

                string alternativeLogPath = string.Equals(BaseDirectory, DefaultLogPath) ? AlternativeLogPath : DefaultLogPath;

                try
                {
                    if (Directory.Exists(alternativeLogPath))
                    {
                        Plugin.Logger.LogInfo("Removing directory: " + alternativeLogPath);
                        Directory.Delete(alternativeLogPath, true);
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError("Unable to delete log directory");
                    Plugin.Logger.LogError(ex);
                }

                initializeFileWatcher();
            }
            catch (Exception ex)
            {
                Debug.LogError("Unable to create Logs directory");
                Debug.LogError(ex);
            }

            DirectoryMustBeUpdated = false;
            HasInitialized = true;
        }

        private static void initializeFileWatcher()
        {
            return;
            if (logFileWatcher != null)
                logFileWatcher.Dispose();

            logFileWatcher = new LoggingFileWatcher(Path.GetDirectoryName(DefaultLogPath));//Path.GetDirectoryName(BaseDirectory));
            logFileWatcher.OnDirectoryUpdate += (sender, eventArgs) =>
            {
                /*Debug.Log("PATH CHANGED");

                FileSystemEventArgs fileMoveEvent = (FileSystemEventArgs)eventArgs;

                if (fileMoveEvent.Name != LOGS_FOLDER_NAME)
                    Debug.LogWarning("Invalid log folder name");
                */
            };
            logFileWatcher.LogFolderRenamedOrDeleted = () =>
            {
                Debug.Log("PATH RENAMED/DELETED");

                //Log folder is no longer accesible by this logger. No longer monitor this path.
                logFileWatcher.EnableRaisingEvents = false;
                logFileWatcher.Dispose();
                logFileWatcher = null;

                AwaitingLogManager = true;
                DirectoryMustBeUpdated = true;
            };
            logFileWatcher.Created += (sender, eventArgs) =>
            {
                Debug.Log("PATH CREATED");

                FileSystemEventArgs fileCreateEvent = (FileSystemEventArgs)eventArgs;

                if (fileCreateEvent.Name == LOGS_FOLDER_NAME)
                {
                    AwaitingLogManager = false;
                    DirectoryMustBeUpdated = false;

                    BaseDirectory = fileCreateEvent.FullPath;
                }
            };
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
        /// Change the extension of any filename to .log
        /// </summary>
        public static string FormatLogFile(string filename)
        {
            return Path.ChangeExtension(filename, ".log");
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
        public static void ApplyHooks()
        {
            On.RainWorld.Update += RainWorld_Update;
        }

        private static void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
        {
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

            orig(self);
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

        private static void processLogSignal(string signal)
        {
            if (signal == "Signal.None") return;

            string[] signalData = Regex.Split(signal, ".");
            string signalWord = signalData[1]; //Signal.Keyword.Other data

            //Remote loggers need to be informed of when the Logs folder is moved.
            //The folder cannot be moved if any log file has an open filestream active
            if (signalWord == "MovePending")
                OnMovePending?.Invoke();
            else if (signalWord == "MoveComplete")
                OnMoveComplete?.Invoke(signalData[2]);
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
            detachEvents();
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
            LogPath = Logger.FormatLogFile(logPath);
        }

        public bool IsLog(string logName)
        {
            return LogPath != null && LogPath == Logger.FormatLogFile(logName);
        }

        public bool IsLog(LogModule logger)
        {
            return (LogSource != null && LogSource == logger.LogSource) || (LogPath == logger.LogPath);
        }

        public void Log(object data, LogLevel level)
        {
            if (!Enabled) return;

            /*if (Logger.DirectoryMustBeUpdated && LogPath != null && Path.GetDirectoryName(LogPath) == Logger.LOGS_FOLDER_NAME)
            {
                if (Logger.AwaitingLogManager && Logger.LogManagerActive)
                {
                    return; //Logging is disabled until LogManager can resolve directory discrepancies
                }

                Logger.AwaitingLogManager = false;
                Logger.HasInitialized = false;
                Logger.InitializeLogDirectory();
            }*/

            //TODO: This is an old and probably non-thread safe way of logging. Replace with new method
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
                Debug.Log(data);
            }
        }
    }

    public class LoggingFileWatcher : FileSystemWatcher
    {
        public FileSystemEventHandler OnDirectoryUpdate;
        public Action LogFolderRenamedOrDeleted; //Not called on a Move

        /// <summary>
        /// Keeps track of the Logs folder when it is moved, renamed, or deleted
        /// </summary>
        /// <param name="containingPath">The folder path containing the Logs folder</param>
        public LoggingFileWatcher(string containingPath) : base(containingPath)
        {
            EnableRaisingEvents = true;
            IncludeSubdirectories = true;
            //NotifyFilter = NotifyFilters.DirectoryName;
            Deleted += OnDeleted;
            Created += OnCreated;
            Renamed += OnRenamed;
            Changed += OnChanged;
        }

        private Timer fileMoveCheck;
        private bool createdFlag, deletedFlag;
        private FileSystemEventArgs fileMoveEvent;

        private void OnChanged(object sender,  FileSystemEventArgs fileSystemEvent)
        {
            if (System.IO.Path.GetFileName(fileSystemEvent.Name) == Logger.LOGS_FOLDER_NAME)
                OnDirectoryUpdate?.Invoke(this, fileSystemEvent);


            /*Debug.Log(fileSystemEvent.ChangeType);
            Debug.Log(fileSystemEvent.Name);
            Debug.Log(fileSystemEvent.FullPath);
            if (fileSystemEvent.Name == Logger.LOGS_FOLDER_NAME)
            {
            }*/
        }

        private void OnCreated(object sender, FileSystemEventArgs fileSystemEvent)
        {
            Debug.Log("CREATED");
            return;
            if (fileSystemEvent.Name == Logger.LOGS_FOLDER_NAME) //We need to check a deletion event followed by a creation event to track a directory move 
            {
                createdFlag = true;
                fileMoveEvent = fileSystemEvent;
            }
        }

        private void OnDeleted(object sender, FileSystemEventArgs fileSystemEvent)
        {
            Debug.Log("DELETED");
            return;
            /*if (fileSystemEvent.Name == Logger.LOGS_FOLDER_NAME) //We need to check a deletion event followed by a creation event to track a directory move 
            {
                deletedFlag = true;

                fileMoveCheck = new Timer(t =>
                {
                    if (deletedFlag)
                    {
                        if (createdFlag)
                            LogPathChanged.Invoke(this, fileMoveEvent);
                        else
                            LogFolderRenamedOrDeleted?.Invoke();
                    }

                    deletedFlag = false;
                    createdFlag = false;
                    fileMoveEvent = null;
                    fileMoveCheck.Dispose();
                }, fileMoveCheck, 3000, Timeout.Infinite);
            }*/
        }

        private void OnRenamed(object sender, RenamedEventArgs fileRenameEvent)
        {
            Debug.Log("RENAMED");
            return;
            //TODO: Handle cut/copy
            if (fileRenameEvent.OldName == Logger.LOGS_FOLDER_NAME && fileRenameEvent.Name != Logger.LOGS_FOLDER_NAME)
                LogFolderRenamedOrDeleted?.Invoke();
        }
    }

    static class ExtendedILogListener
    {
        /// <summary>
        /// Fetches signal data produced by a custom ILogListener
        /// </summary>
        public static string GetSignal(this ILogListener self)
        {
            string stringToProcess = self.ToString();

            return stringToProcess.StartsWith("Signal") ? stringToProcess : null;
        }
    }
}
