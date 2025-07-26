using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Helpers.Extensions;
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
        /// The currently selected path (including directory name) of the log directory (whether it exists or not)
        /// </summary>
        public static string CurrentPath { get; private set; }

        /// <summary>
        /// The currently selected directory name
        /// </summary>
        public static string Name = LOGS_FOLDER_NAME;

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
            if (!PathUtils.PathRootExists(CurrentPath, 1))
                throw new DirectoryNotFoundException("Cannot create log directory. Current path does not exist");

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

            PathResult result = FindLogsDirectory();

            string targetPath = result.Target;

            if (targetPath == null)
            {
                CurrentPath = Path.Combine(AvailablePaths[0], Name);
                return;
            }

            CurrentPath = targetPath;

            if (result.IsResultFromPathHistory)
                PathHistory.Update();
        }

        /// <summary>
        /// Checks existing path history, and available paths, and returns the first existing directory, or null if no directories exist
        /// </summary>
        public static PathResult FindLogsDirectory()
        {
            string[] pathHistory = PathHistory.ReadFromFile();

            //Find the last history entry that contains path info, and parse the path info from the string
            string targetPath = null;
            for (int i = pathHistory.Length; i >= 0; i--)
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
                targetPath = AvailablePaths.Find(root => Directory.Exists(Path.Combine(root, Name)));
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
            if (IsManagingFiles || !Exists)
            {
                string reportMessage;
                if (IsManagingFiles)
                    reportMessage = "Log files already moved";
                else
                    reportMessage = "Unable to move files to log directory";

                UtilityLogger.Log(reportMessage);
                return;
            }
            IsManagingFiles = true;
            foreach (LogProperties properties in LogProperties.PropertyManager.Properties)
                AddToFolder(properties);
        }

        /// <summary>
        /// Restores log files that are part of the current log directory to their original folder paths
        /// </summary>
        public static void RestoreFiles()
        {
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

            string newPath = CurrentPath;

            LogFilename filename = logFile.Properties.AltFilename;

            if (filename != null)
            {
                if (!logFile.Properties.CurrentFilename.Equals(filename))
                    UtilityLogger.Log($"Renaming file to {filename}");

                newPath = Path.Combine(CurrentPath, filename.WithExtension());
            }

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
            LogFilename filename = properties.ReserveFilename ?? properties.Filename;

            if (!properties.CurrentFilename.Equals(filename))
                UtilityLogger.Log($"Renaming file to {filename}");

            string newPath = Path.Combine(properties.FolderPath, filename.WithExtension());

            if (!properties.FileExists)
            {
                properties.ChangePath(newPath);
                return;
            }

            UtilityLogger.Log($"Moving {logFile} out of Logs folder");
            LogFile.Move(logFile, newPath);
        }

        internal static bool TryMove(string path)
        {
            try
            {
                OnMovePending?.Invoke();
                Move(path);

                OnMoveComplete?.Invoke();
                return true;
            }
            catch
            {
                OnMoveAborted?.Invoke();
                return false;
            }
        }

        internal static void Move(string path)
        {
            if (path == null)
                throw new ArgumentNullException("Path argument cannot be null");

            var logFilesInFolder = GetContainedLogFiles();

            ThreadSafeWorker worker = new ThreadSafeWorker(logFilesInFolder.Select(logFile => logFile.Properties.FileLock));

            worker.DoWork(() =>
            {
                var streamsToResume = new List<StreamResumer>();
                try
                {
                    using (UtilityCore.RequestHandler.BeginCriticalSection())
                    {
                        foreach (LogID logFile in logFilesInFolder)
                        {
                            logFile.Properties.FileLock.SetActivity(logFile, FileAction.Move); //Lock activated by ThreadSafeWorker
                            logFile.Properties.NotifyPendingMove(path);

                            //The move operation requires that all persistent file activity be closed until move is complete
                            streamsToResume.AddRange(logFile.Properties.PersistentStreamHandles.InterruptAll());
                        }
                    }
                    Directory.Move(CurrentPath, path);

                    //Update path info for affected log files
                    foreach (LogID logFile in logFilesInFolder)
                        logFile.Properties.ChangePath(path);
                }
                finally
                {
                    //Reopen the streams
                    streamsToResume.ResumeAll();
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
            if (IsCurrentPath(path))
                return;

            string dirName = Path.GetFileName(path);

            bool didMove = false;
            if (!Exists || (didMove = TryMove(path))) //Path data must remain the same if the existing directory cannot be moved
            {
                Name = dirName;
                CurrentPath = Path.GetDirectoryName(path);
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
