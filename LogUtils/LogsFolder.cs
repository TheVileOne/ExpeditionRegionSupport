using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils
{
    public static class LogsFolder
    {
        /// <summary>
        /// Event signals that the log directory is about to be moved, or renamed
        /// </summary>
        public static event Action OnMovePending;

        /// <summary>
        /// Event signals that the log directory was unable to be moved, or renamed
        /// </summary>
        public static event Action OnMoveAborted;

        /// <summary>
        /// Event signals that the log directory has successfully been moved, or renamed
        /// </summary>
        public static event Action OnMoveComplete;

        /// <summary>
        /// The default directory name
        /// </summary>
        public const string LOGS_FOLDER_NAME = "Logs";

        /// <summary>
        /// A list of valid paths that may contain the log directory
        /// </summary>
        public static readonly List<string> AvailablePaths = new List<string>();

        /// <summary>
        /// The path containing, or selected to contain the log directory
        /// </summary>
        public static string ContainingPath { get; private set; }

        /// <summary>
        /// The currently selected path (including directory name) of the log directory (whether it exists or not)
        /// </summary>
        public static string CurrentPath => Path.Combine(ContainingPath, Name);

        /// <summary>
        /// The currently selected directory name
        /// </summary>
        public static string Name { get; private set; }

        /// <summary>
        /// Checks that log directory exists at its currently set path
        /// </summary>
        public static bool Exists => Directory.Exists(CurrentPath);

        /// <summary>
        /// Checks a path against the current log directory path
        /// </summary>
        public static bool IsCurrentPath(string path) => PathUtils.PathsAreEqual(CurrentPath, path);

        /// <summary>
        /// A flag that indicates whether the log directory contains eligible log files
        /// </summary>
        public static bool IsManagingFiles { get; private set; }

        static LogsFolder()
        {
            UtilityCore.EnsureInitializedState();
        }

        /// <summary>
        /// Attempts to create a new log directory at the currently set path
        /// </summary>
        /// <remarks>This method does nothing when the folder already exists</remarks>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public static void Create()
        {
            //The containing directory must exist to create this directory
            if (!Directory.Exists(ContainingPath))
                throw new DirectoryNotFoundException("Cannot create log directory - Containing path does not exist");

            //May throw UnauthroizedAccessException, or IOException, leave responsibility of caller to handle
            UtilityLogger.Log("Creating log directory: " + CurrentPath);
            Directory.CreateDirectory(CurrentPath);
        }

        /// <summary>
        /// Initializes the log directory path
        /// </summary>
        /// <remarks>LogUtils does not create this directory by default</remarks>
        public static void Initialize()
        {
            AvailablePaths.Add(RainWorldPath.RootPath);            //Default path
            AvailablePaths.Add(RainWorldPath.StreamingAssetsPath); //Alternate path

            Name = LOGS_FOLDER_NAME;

            PathResult result = FindLogsDirectory();

            string targetPath = result.Target;

            if (targetPath == null)
            {
                ContainingPath = AvailablePaths[0];
                return;
            }

            ContainingPath = Path.GetDirectoryName(targetPath);
            Name = Path.GetFileName(targetPath);

            if (!result.IsResultFromPathHistory)
                PathHistory.Update();
        }

        /// <summary>
        /// Checks existing path history, and available paths, and returns the first existing directory, or null if none of the directory candidates exist
        /// </summary>
        public static PathResult FindLogsDirectory()
        {
            string[] pathHistory = PathHistory.ReadFromFile();

            //Find the last history entry that contains path info, and parse the path info from the string
            string targetPath = null;
            for (int i = pathHistory.Length - 1; i >= 0; i--)
            {
                string entry = pathHistory[i];
                int pathStart = entry.IndexOf("path:");

                if (pathStart == -1) continue;

                targetPath = entry.Substring(pathStart + 5); //Accounts for length of prefix
                break;
            }

            if (targetPath != null && !Directory.Exists(targetPath))
            {
                UtilityLogger.Log("Could not find previous logs folder path - it may have been moved, or belongs to a temporary directory");
                targetPath = null;
            }

            bool entryFound = targetPath != null; //The file contained a path to an existing log directory

            if (!entryFound)
            {
                //Since we could not find this data in path history, we need to check if any of the available paths exist
                foreach (string path in AvailablePaths)
                {
                    if (Directory.Exists(targetPath = Path.Combine(path, Name)))
                        break;
                    targetPath = null; //We need to reset if we don't find the correct path
                }
            }

            PathResult result = new PathResult()
            {
                Target = targetPath,
                IsResultFromPathHistory = entryFound
            };
            return result;
        }

        /// <summary>
        /// Returns all registered LogIDs representing log files within the current log directory or otherwise target it as a write path
        /// </summary>
        public static IEnumerable<LogID> GetContainedLogFiles()
        {
            return LogID.FindAll(properties => PathUtils.PathsAreEqual(properties.CurrentFolderPath, CurrentPath));
        }

        public static void OnEligibilityChanged(LogEventArgs e)
        {
            LogProperties properties = e.Properties;

            if (!properties.IsNewInstance || !Exists) return; //Eligibility only applies to newly created log properties

            if (properties.LogsFolderEligible && properties.LogsFolderAware)
                AddToFolder(properties);
            else
                RemoveFromFolder(properties); //TODO: Need a way to ignore this when LogsFolderAware is set to false
        }

        /// <summary>
        /// Moves eligible log files to current log directory
        /// </summary>
        public static void MoveFilesToFolder()
        {
            if (!UtilityCore.IsControllingAssembly) return;

            if (!Exists)
            {
                UtilityLogger.Log("Logs folder unavailable - Files not moved");
                return;
            }

            UtilityLogger.Log("Logs folder available");

            if (IsManagingFiles)
            {
                UtilityLogger.Log("Log files already in folder");
                return;
            }

            UtilityLogger.Log("Moving eligible log files");
            IsManagingFiles = true;
            foreach (LogProperties properties in LogProperties.PropertyManager.Properties)
                AddToFolder(properties);
        }

        /// <summary>
        /// Restores log files that are part of the current log directory to their original folder paths
        /// </summary>
        public static void RestoreFiles()
        {
            if (!UtilityCore.IsControllingAssembly) return;

            if (!IsManagingFiles || !Exists)
            {
                string reportMessage;
                if (!IsManagingFiles)
                    reportMessage = "Log files already restored";
                else
                    reportMessage = "No log files to restore";

                UtilityLogger.Log(reportMessage);
                return;
            }
            IsManagingFiles = false;
            foreach (LogProperties properties in LogProperties.PropertyManager.Properties)
                RemoveFromFolder(properties);
        }

        /// <summary>
        /// Transfers a log file to the Logs folder (when it exists)
        /// </summary>
        internal static void AddToFolder(LogProperties properties)
        {
            if (!UtilityCore.IsControllingAssembly) return;

            LogID logFile = properties.ID;

            if (!properties.LogsFolderEligible)
            {
                UtilityLogger.Log($"{logFile} is currently ineligible to be moved to Logs folder");
                return;
            }

            //When moving a file into this folder, we should rename it to its alternate filename if it has one set
            string newPath = getMovePath(properties.CurrentFilename, properties.AltFilename, CurrentPath);

            if (!properties.FileExists)
            {
                properties.ChangePath(newPath);
                return;
            }

            bool isMoveRequired = !PathUtils.PathsAreEqual(properties.CurrentFolderPath, newPath);

            if (isMoveRequired)
            {
                UtilityLogger.Log($"Moving {logFile} to Logs folder");
                LogFile.Move(logFile, newPath);
            }
        }

        /// <summary>
        /// Transfers a log file from the Logs folder (when it exists)
        /// </summary>
        internal static void RemoveFromFolder(LogProperties properties)
        {
            if (!UtilityCore.IsControllingAssembly) return;

            LogID logFile = properties.ID;

            if (IsCurrentPath(properties.FolderPath))
            {
                //We cannot move this file unless we have a destination path to move it into
                UtilityLogger.Log($"Unable to move file {logFile}");
                return;
            }

            if (!IsCurrentPath(properties.CurrentFolderPath))
            {
                UtilityLogger.Log($"{logFile} file is not a part of Logs folder");
                return;
            }

            //When moving a file out of this folder, we should rename it to what it was before it was moved into the folder
            string newPath = getMovePath(properties.CurrentFilename, properties.ReserveFilename ?? properties.Filename, properties.FolderPath);

            if (!properties.FileExists)
            {
                properties.ChangePath(newPath);
                return;
            }

            UtilityLogger.Log($"Moving {logFile} out of Logs folder");
            LogFile.Move(logFile, newPath);
        }

        private static string getMovePath(LogFilename currentFilename, LogFilename preferredFilename, string logPath)
        {
            //The only time we want to use the preferred option is if we can construct a valid filename with it
            if (preferredFilename == null || !preferredFilename.IsValid)
                return Path.Combine(logPath, currentFilename.WithExtension());

            if (!currentFilename.Equals(preferredFilename))
                UtilityLogger.Log($"Renaming file to {preferredFilename}");
            return Path.Combine(logPath, preferredFilename.WithExtension());
        }

        internal static bool TryMove(string path)
        {
            if (!UtilityCore.IsControllingAssembly || !Exists || !DirectoryUtils.ParentExists(path) || PathUtils.ContainsOtherPath(CurrentPath, path))
                return false;
            try
            {
                UtilityLogger.Log("Attempting to move log directory");
                UtilityLogger.DebugLog("Attempting to move log directory");
                OnMovePending?.Invoke();
                Move(path);

                UtilityLogger.Log("Move successful");
                UtilityLogger.DebugLog("Move successful");
                OnMoveComplete?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                UtilityLogger.DebugLog(ex);
                UtilityLogger.LogError(ex);
                OnMoveAborted?.Invoke();
                return false;
            }
        }

        internal static void Move(string path)
        {
            var logFilesInFolder = GetContainedLogFiles();

            ThreadSafeWorker worker = new ThreadSafeWorker(logFilesInFolder.Select(logFile => logFile.Properties.FileLock));

            worker.DoWork(() =>
            {
                bool moveCompleted = false;

                List<MessageBuffer> activeBuffers = new List<MessageBuffer>();
                List<StreamResumer> streamsToResume = new List<StreamResumer>();
                try
                {
                    UtilityCore.RequestHandler.BeginCriticalSection();
                    foreach (LogID logFile in logFilesInFolder)
                    {
                        MessageBuffer writeBuffer = logFile.Properties.WriteBuffer;

                        writeBuffer.SetState(true, BufferContext.CriticalArea);
                        activeBuffers.Add(writeBuffer);

                        logFile.Properties.FileLock.SetActivity(FileAction.Move); //Lock activated by ThreadSafeWorker
                        logFile.Properties.NotifyPendingMove(path);

                        //The move operation requires that all persistent file activity be closed until move is complete
                        streamsToResume.AddRange(logFile.Properties.PersistentStreamHandles.InterruptAll());
                    }
                    Directory.Move(CurrentPath, path);
                    moveCompleted = true;

                    //Update path info for affected log files
                    foreach (LogID logFile in logFilesInFolder)
                        logFile.Properties.ChangePath(path);
                }
                finally
                {
                    if (!moveCompleted)
                    {
                        foreach (LogID logFile in logFilesInFolder)
                            logFile.Properties.NotifyPendingMoveAborted();
                    }

                    //Reopen the streams
                    streamsToResume.ResumeAll();
                    activeBuffers.ForEach(buffer => buffer.SetState(false, BufferContext.CriticalArea));
                    UtilityCore.RequestHandler.EndCriticalSection();
                }
            });
        }

        /// <summary>
        /// Targets a directory path to contain a logs folder
        /// </summary>
        /// <param name="path">A valid directory path</param>
        public static void SetContainingPath(string path)
        {
            string targetPath = Path.Combine(path, Name);
            SetPath(targetPath);
        }

        /// <summary>
        /// Targets a directory path to become the new logs folder
        /// </summary>
        /// <remarks>DO NOT set to any directory you don't want moved around</remarks>
        /// <param name="path">A valid directory path</param>
        public static void SetPath(string path)
        {
            if (IsCurrentPath(path)) return;

            bool didMove = false;
            if (!Exists || (didMove = TryMove(path))) //Path data must remain the same if the existing directory cannot be moved
            {
                ContainingPath = Path.GetDirectoryName(path);
                Name = Path.GetFileName(path);
            }

            if (didMove) //Only record a path history event when the directory is moved, or renamed
                PathHistory.Update();
        }

        internal static class PathHistory
        {
            public readonly static string FilePath = Path.Combine(RainWorldPath.StreamingAssetsPath, "logsfolder.history");

            public static string[] ReadFromFile()
            {
                try
                {
                    if (File.Exists(FilePath))
                        return File.ReadAllLines(FilePath);
                }
                catch
                {
                    UtilityLogger.LogWarning("Unable to read log folder history");
                }
                return [];
            }

            /// <summary>
            /// Appends a new path entry into the path history file
            /// </summary>
            public static void Update()
            {
                UtilityLogger.Log("Updating path history");
                try
                {
                    File.AppendAllText(FilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - path:{CurrentPath}{Environment.NewLine}");
                }
                catch (Exception ex)
                {
                    UtilityLogger.LogError("Unable to update path history", ex);
                }
            }
        }

        /// <summary>
        /// The result of a Logs folder path search
        /// </summary>
        public struct PathResult
        {
            /// <summary>
            /// The result of a the path search
            /// </summary>
            public string Target;

            /// <summary>
            /// The result is associated with an accurate path record
            /// </summary>
            public bool IsResultFromPathHistory;
        }
    }
}
