using BepInEx.Logging;
using ExpeditionRegionSupport.Data.Logging.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class Logger : IDisposable
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

        public Logger(LogModule logModule)
        {
            if (logModule == null)
                throw new ArgumentNullException(nameof(logModule));

            BaseLogger = logModule;
            AttachLogger(BaseLogger);

            applyMoveEvents();

            if (!LogProperties.HasReadPropertiesFile)
                LogProperties.RequestLoad = true;
        }

        public Logger(ManualLogSource logger) : this(new LogModule(logger))
        {
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

            applyMoveEvents();

            if (!LogProperties.HasReadPropertiesFile)
                LogProperties.RequestLoad = true;
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
                    Debug.Log("Creating directory: " + BaseDirectory);
                    Directory.CreateDirectory(BaseDirectory);
                }

                string alternativeLogPath = string.Equals(BaseDirectory, DefaultLogPath) ? AlternativeLogPath : DefaultLogPath;

                try
                {
                    if (Directory.Exists(alternativeLogPath))
                    {
                        Debug.Log("Removing directory: " + alternativeLogPath);
                        Directory.Delete(alternativeLogPath, true);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Unable to delete log directory");
                    Debug.LogError(ex);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Unable to create log directory");
                Debug.LogError(ex);
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
        public static void ApplyHooks()
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

                if (LogProperties.RequestLoad || !LogProperties.HasReadPropertiesFile)
                    LogProperties.LoadProperties();
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

            Debug.Log("Comparing " + path + " to  base " + basePath);

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

                Debug.Log("Log directory changed to " + path);

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

        private void applyMoveEvents()
        {
            OnMoveComplete += onLogDirectoryPathChanged;
        }

        public void removeMoveEvents()
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
            removeMoveEvents();
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

    namespace Utils
    {
        public static class LogPath
        {
            public static bool ComparePaths(string path1, string path2)
            {
                if (path1 == null)
                    return path2 == null;

                if (path2 == null)
                    return false;

                path1 = Path.GetFullPath(path1).TrimEnd('\\');
                path2 = Path.GetFullPath(path2).TrimEnd('\\');

                Debug.Log("Comparing paths");

                bool pathsAreEqual = string.Equals(path1, path2, StringComparison.InvariantCultureIgnoreCase);

                if (pathsAreEqual)
                    Debug.Log("Paths are equal");
                else
                {
                    Debug.Log("Comparing path: " + path1);
                    Debug.Log("Comparing path: " + path2);

                    Debug.Log("Paths are not equal");
                }

                return pathsAreEqual;
            }
        }

        public class LogFileMover
        {
            private string sourcePath, destPath;

            /// <summary>
            /// Creates an object capable of moving, or copying log files to a new destination
            /// </summary>
            /// <param name="sourceLogPath">The full path to the log file that needs to be moved (including filename + ext)</param>
            /// <param name="destLogPath">The full path to the destination of the log file. Log filename is optional.</param>
            public LogFileMover(string sourceLogPath, string destLogPath)
            {
                sourcePath = sourceLogPath;
                destPath = destLogPath;
            }

            /// <summary>
            /// Moves a log file from one place to another. Allows file renaming.
            /// </summary>
            public FileStatus MoveFile()
            {
                LogValidator logValidator = new LogValidator(sourcePath, destPath);

                if (logValidator.Validate())
                {
                    FileStatus status;
                    try
                    {
                        status = PrepareToMoveFile(logValidator);

                        if (status == FileStatus.MoveRequired)
                        {
                            FileInfo sourceFilePath = logValidator.SourceFile;
                            FileInfo destFilePath = logValidator.DestinationFile;

                            sourceFilePath.MoveTo(destFilePath.FullName);
                            return FileStatus.MoveComplete;
                        }
                    }
                    catch (Exception ex)
                    {
                        logMoveError(ex);
                        status = _CopyFile(logValidator);
                    }

                    return status;
                }

                return FileStatus.ValidationFailed;
            }

            /// <summary>
            /// Copies a log file from one place to another. Allows file renaming.
            /// </summary>
            public FileStatus CopyFile()
            {
                LogValidator logValidator = new LogValidator(sourcePath, destPath);

                if (logValidator.Validate())
                {
                    FileStatus status;
                    try
                    {
                        status = PrepareToMoveFile(logValidator);

                        if (status == FileStatus.MoveRequired)
                            return _CopyFile(logValidator);
                    }
                    catch (Exception ex)
                    {
                        logCopyError(ex);
                        status = FileStatus.Error;
                    }

                    return status;
                }

                return FileStatus.ValidationFailed;
            }

            internal FileStatus _CopyFile(LogValidator logValidator)
            {
                FileInfo sourceFilePath = logValidator.SourceFile;
                FileInfo destFilePath = logValidator.DestinationFile;

                FileStatus status;
                try
                {
                    sourceFilePath.CopyTo(destFilePath.FullName, true);
                    status = FileStatus.CopyComplete;
                }
                catch (Exception ex)
                {
                    logCopyError(ex);
                    status = FileStatus.Error;
                }

                return status;
            }

            /// <summary>
            /// Handles FileSystem operations that are necessary before a move/copy operation can be possible
            /// </summary>
            internal FileStatus PrepareToMoveFile(LogValidator logValidator)
            {
                FileInfo sourceFilePath = logValidator.SourceFile;
                FileInfo destFilePath = logValidator.DestinationFile;

                DirectoryInfo sourceFileDir = sourceFilePath.Directory;
                DirectoryInfo destFileDir = destFilePath.Directory;

                //Files are in the same folder
                if (sourceFileDir.FullName == destFileDir.FullName)
                {
                    string sourceFilename = sourceFilePath.Name;
                    string destFilename = destFilePath.Name;

                    if (LogValidator.ExtensionsMatch(sourceFilename, destFilename) && sourceFilename == destFilename)
                        return FileStatus.NoActionRequired; //Same file, no copy necessary

                    destFilePath.Delete(); //Move will fail if a file already exists
                }
                else if (destFileDir.Exists)
                {
                    destFilePath.Delete();
                }
                else
                {
                    destFileDir.Create(); //Make sure the directory exists at the destination
                }

                return FileStatus.MoveRequired;
            }

            private void logMoveError(Exception ex)
            {
                Debug.LogError("Unable to move file. Attempt copy instead");
                Debug.LogError(ex);
            }

            private void logCopyError(Exception ex)
            {
                Debug.LogError("Unable to copy file");
                Debug.LogError(ex);
            }
        }

        public enum FileStatus
        {
            AwaitingStatus,
            NoActionRequired,
            MoveRequired,
            MoveComplete,
            CopyComplete,
            ValidationFailed,
            Error
        }

        public class LogValidator
        {
            public FileInfo SourceFile;
            public FileInfo DestinationFile;

            internal string UnvalidatedSourcePath, UnvalidatedDestinationPath;

            public LogValidator(string sourceLogPath, string destLogPath)
            {
                UnvalidatedSourcePath = sourceLogPath;
                UnvalidatedDestinationPath = destLogPath;
            }

            public bool Validate()
            {
                string sourcePath = UnvalidatedSourcePath ?? SourceFile.FullName;
                string destPath = UnvalidatedDestinationPath ?? DestinationFile.FullName;

                UnvalidatedSourcePath = UnvalidatedDestinationPath = null;
                SourceFile = DestinationFile = null;

                if (!IsValidLogFileExt(sourcePath)) return false; //We don't want to handle random filetypes

                //A valid filetype is all we need to validate the source path
                SourceFile = new FileInfo(sourcePath);


                //Should we treat it as a directory, or a file
                if (Path.HasExtension(destPath))
                {
                    string destFilename = Path.GetFileName(destPath);

                    if (!ExtensionsMatch(SourceFile.Name, destFilename) && !IsValidLogFileExt(destFilename))
                        return false; //We can only replace log files

                    DestinationFile = new FileInfo(destPath);
                }
                else
                {
                    DestinationFile = new FileInfo(Path.Combine(destPath, SourceFile.Name));
                }

                return true;
            }

            /// <summary>
            /// Returns true if filename has either .log, or .txt as a file extension
            /// </summary>
            public static bool IsValidLogFileExt(string filename)
            {
                string fileExt = Path.GetExtension(filename); //Case-insensitive file extensions not supported

                return fileExt == ".log" || fileExt == ".txt";
            }

            public static bool ExtensionsMatch(string filename, string filename2)
            {
                string fileExt = Path.GetExtension(filename);
                string fileExt2 = Path.GetExtension(filename2);

                return fileExt == fileExt2;
            }
        }

        /// <summary>
        /// Stores two versions of a log path allowing logs to be moved between two stored locations
        /// </summary>
        public class LogFileSwitcher
        {
            public PathSwitchMode SwitchMode;

            /// <summary>
            /// Determines which side of the KeyValuePair to move from. True means left.
            /// </summary>
            public bool SwitchStartPosition = true;

            public List<ValuePairToggle> PathStrings = new List<ValuePairToggle>();

            public LogFileSwitcher(PathSwitchMode mode)
            {
                SwitchMode = mode;
            }

            public void AddPaths(string path1, string path2)
            {
                ValuePairToggle valuePair = new ValuePairToggle(path1, path2);

                if (SwitchMode == PathSwitchMode.Collective)
                    valuePair.ToggleFlag = SwitchStartPosition;

                PathStrings.Add(valuePair);
            }

            public void AddPaths(string path1, string path2, bool toggleValue)
            {
                if (SwitchMode == PathSwitchMode.Collective)
                    throw new InvalidOperationException();

                PathStrings.Add(new ValuePairToggle(path1, path2)
                {
                    ToggleFlag = toggleValue
                });
            }

            /// <summary>
            /// Moves all files to their alternate location at once
            /// </summary>
            public void SwitchPaths()
            {
                if (SwitchMode == PathSwitchMode.Singular)
                    throw new InvalidOperationException();

                string path1, path2;
                foreach (ValuePairToggle valuePair in PathStrings)
                {
                    //Retrieve our paths
                    path1 = valuePair.ActiveValue;
                    path2 = valuePair.InactiveValue;

                    //If last move was unsuccessful, it is okay to do nothing here 
                    if (valuePair.ToggleFlag == SwitchStartPosition)
                    {
                        valuePair.Status = Logger.MoveLog(path1, path2);

                        //Don't allow file position to get desynced with toggle position due to move fail
                        if (valuePair.LastMoveSuccessful)
                            valuePair.Toggle();
                    }
                }

                SwitchStartPosition = !SwitchStartPosition;
            }

            /// <summary>
            /// Looks for a matching path, and attempts to move log file to that path
            /// </summary>
            public void SwitchToPath(string path)
            {
                if (SwitchMode == PathSwitchMode.Collective)
                    throw new InvalidOperationException();

                //Retrieve our path
                ValuePairToggle valuePair = PathStrings.Find(vp => vp.ActiveValue == path || vp.InactiveValue == path);

                if (valuePair != null && valuePair.ActiveValue != path)
                {
                    valuePair.Status = Logger.MoveLog(valuePair.ActiveValue, path);

                    //Don't allow file position to get desynced with toggle position due to move fail
                    if (valuePair.LastMoveSuccessful)
                        valuePair.Toggle();
                }
            }

            /// <summary>
            /// Changes the directory of all paths on the right side of the value toggles
            /// </summary>
            public void UpdateTogglePath(string pathDir)
            {
                foreach (ValuePairToggle valuePair in PathStrings)
                    valuePair.ValuePair = new KeyValuePair<string, string>(valuePair.ValuePair.Key, Path.Combine(pathDir, Path.GetFileName(valuePair.ValuePair.Value)));
            }

            public class ValuePairToggle
            {
                /// <summary>
                /// Determines which side of the KeyValuePair to move from. True means left.
                /// </summary>
                public bool ToggleFlag = true;

                /// <summary>
                /// Stores data for ValuePairToggle
                /// </summary>
                public KeyValuePair<string, string> ValuePair;

                public string ActiveValue => ToggleFlag ? ValuePair.Key : ValuePair.Value;
                public string InactiveValue => ToggleFlag ? ValuePair.Value : ValuePair.Key;

                /// <summary>
                /// The status of the last file move attempt. This is stored in case a failed move creates a desync
                /// </summary>
                public FileStatus Status = FileStatus.AwaitingStatus;

                public bool LastMoveSuccessful => Status != FileStatus.Error && Status != FileStatus.ValidationFailed;

                public ValuePairToggle(string value1, string value2)
                {
                    ValuePair = new KeyValuePair<string, string>(value1, value2);
                }

                public void Toggle()
                {
                    ToggleFlag = !ToggleFlag;
                }
            }

            public enum PathSwitchMode
            {
                Singular,
                Collective
            }
        }
    }

    public static class ExtendedILogListener
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
