using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils
{
    public static partial class LogsFolder
    {
        /// <summary>
        /// Event publisher for path change events
        /// </summary>
        public static readonly PathChangeEventPublisher OnPathChange = new PathChangeEventPublisher();

        [Obsolete]
        internal static event PathChangeEventHandler OnMovePending
        {
            add => OnPathChange.PendingEvent += value;
            remove => OnPathChange.PendingEvent -= value;
        }

        [Obsolete]
        internal static event PathChangeEventHandler OnMoveAborted
        {
            add => OnPathChange.AbortedEvent += value;
            remove => OnPathChange.AbortedEvent -= value;
        }

        [Obsolete]
        internal static event PathChangeEventHandler OnMoveComplete
        {
            add => OnPathChange.CompletedEvent += value;
            remove => OnPathChange.CompletedEvent -= value;
        }

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

        private static bool? _existsCache;
        /// <summary>
        /// Checks that log directory exists at its currently set path
        /// </summary>
        public static bool Exists => _existsCache ?? Directory.Exists(CurrentPath);

        internal static void CacheExistsState() => _existsCache = Exists;
        internal static void ResetExistsCache() => _existsCache = null;

        /// <summary>
        /// Checks that the current path is located somewhere inside the current log directory path
        /// </summary>
        public static bool ContainsPath(string path) => PathUtils.ContainsOtherPath(path, CurrentPath);

        /// <summary>
        /// Checks a path against the current log directory path
        /// </summary>
        [Obsolete("Use LogsFolder.ContainsPath instead")]
        public static bool IsCurrentPath(string path) => PathUtils.PathsAreEqual(path, CurrentPath);

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
                throw new DirectoryNotFoundException("Unable to create log directory. Containing path does not exist");

            //May throw UnauthroizedAccessException, or IOException, leave responsibility of caller to handle
            UtilityLogger.Log("Creating log directory: " + CurrentPath);
            Directory.CreateDirectory(CurrentPath);
        }

        /// <summary>
        /// Initializes the log directory path
        /// </summary>
        /// <remarks>LogUtils does not create this directory by default</remarks>
        internal static void Initialize()
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
                UtilityLogger.Log("Could not find previous log directory path. It may have been moved, or belongs to a temporary directory.");
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
        /// Returns all registered <see cref="LogID"/> instances representing log files within the current log directory or otherwise target it as a write path
        /// </summary>
        public static IEnumerable<LogID> GetContainedLogFiles()
        {
            return LogID.FindAll(properties => ContainsPath(properties.CurrentFolderPath));
        }

        /// <summary>
        /// Returns all registered <see cref="LogGroupID"/> instances representing log groups targeting the current log directory or otherwise target it as a write path
        /// </summary>
        public static IEnumerable<LogGroupID> GetContainedLogGroups()
        {
            return LogGroup.GroupsSharingThisPath(CurrentPath);
        }

        internal static void OnEligibilityChanged(LogEventArgs e)
        {
            LogProperties properties = e.Properties;

            if (!properties.IsNewInstance || !Exists) return; //Eligibility only applies to newly created log properties

            if (!properties.LogsFolderEligible)
            {
                RemoveFromFolder(properties);
                return;
            }

            if (properties.LogsFolderAware)
                AddToFolder(properties);
        }

        /// <summary>
        /// Moves eligible log files to current log directory
        /// </summary>
        public static void MoveFilesToFolder()
        {
            if (!UtilityCore.IsControllingAssembly) return;

            if (!Exists)
            {
                UtilityLogger.Log("Log directory unavailable - Files not moved");
                return;
            }

            UtilityLogger.Log("Log directory available");
            if (IsManagingFiles)
            {
                UtilityLogger.Log("Log files already in folder");
                return;
            }

            UtilityLogger.Log("Moving eligible log files");
            IsManagingFiles = true;

            CacheExistsState();
            foreach (LogProperties properties in LogProperties.PropertyManager.AllProperties)
                AddToFolder(properties);
            ResetExistsCache();
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
            foreach (LogProperties properties in LogProperties.PropertyManager.AllProperties)
                RemoveFromFolder(properties);
        }

        private static bool suppressGroupMemberEligibilityLogging = false;
        /// <summary>
        /// Transfers log files, and folders associated with the properties instance to the current log directory
        /// </summary>
        internal static void AddToFolder(LogProperties properties)
        {
            if (!UtilityCore.IsControllingAssembly) return;

            if (!properties.ID.Registered)
            {
                if (properties.Group == null) //Only group managed instances are allowed to be unregistered
                    return;

                //Defer handling of unregistered group files to be handled with their respective groups
                if (RainWorldInfo.LatestSetupPeriodReached < SetupPeriod.PostMods)
                    return;
            }

            if (properties is LogGroupProperties groupProperties)
            {
                AddGroupToFolder(groupProperties);
                return;
            }

            if (!suppressGroupMemberEligibilityLogging && properties.Group != null)
                UtilityLogger.Log("Checking eligibility of log group member");

            LogID logFile = properties.ID;

            if (!properties.LogsFolderEligible)
            {
                UtilityLogger.Log($"{logFile} is currently ineligible to be moved to log directory");
                return;
            }

            if (!Exists) return;

            //When moving a file into this folder, we should rename it to its alternate filename if it has one set
            string newPath = getMovePath(properties.CurrentFilename, properties.AltFilename, CurrentPath);

            if (!properties.FileExists)
            {
                properties.ChangePath(newPath);
                return;
            }

            bool isMoveRequired = !ContainsPath(properties.CurrentFolderPath);

            if (isMoveRequired)
            {
                UtilityLogger.Log($"Moving {logFile} to log directory");
                LogFile.Move(logFile, newPath);
            }
        }

        internal static void AddGroupToFolder(LogGroupProperties properties)
        {
            UtilityLogger.Log("Checking eligibility of log group " + properties.ID);
            if (!UtilityCore.IsInitialized)
            {
                UtilityLogger.LogWarning("Group will not be assigned members this early");
                return;
            }

            var groupMembers = properties.Members;

            if (groupMembers.Count == 0)
            {
                UtilityLogger.Log("No eligible members");
                return;
            }

            //TODO: Moving an entire folder is possible here
            if (properties.IsFolderGroup)
                UtilityLogger.LogWarning("Group subpaths will not be respected after move operation");

            suppressGroupMemberEligibilityLogging = true;
            foreach (LogProperties memberProperties in groupMembers.GetProperties())
                AddToFolder(memberProperties);
            suppressGroupMemberEligibilityLogging = false;
        }

        /// <summary>
        /// Transfers a log file from the current log directory (when it exists)
        /// </summary>
        internal static void RemoveFromFolder(LogProperties properties)
        {
            if (!UtilityCore.IsControllingAssembly) return;

            if (properties is LogGroupProperties)
            {
                if (UtilityCore.Build == UtilitySetup.Build.RELEASE)
                {
                    UtilityLogger.LogWarning("Restoring group files is not yet supported");
                    return;
                }
                //TODO: This needs to be handled
                throw new NotImplementedException();
            }

            LogID logFile = properties.ID;

            if (ContainsPath(properties.FolderPath))
            {
                //We cannot move this file unless we have a destination path to move it into
                UtilityLogger.Log($"Unable to move file {logFile}");
                return;
            }

            if (!ContainsPath(properties.CurrentFolderPath))
            {
                UtilityLogger.Log($"{logFile} file is not a part of log directory");
                return;
            }

            //When moving a file out of this folder, we should rename it to what it was before it was moved into the folder
            string newPath = getMovePath(properties.CurrentFilename, properties.ReserveFilename ?? properties.Filename, properties.FolderPath);

            if (!properties.FileExists)
            {
                properties.ChangePath(newPath);
                return;
            }

            UtilityLogger.Log($"Moving {logFile} out of log directory");
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

        /// <summary>
        /// Targets a directory path to contain the current log directory
        /// </summary>
        /// <param name="path">A valid directory path</param>
        public static void SetContainingPath(string path)
        {
            path = Path.Combine(path, Name);

            if (TryMove(path))
                UtilityLogger.Log("Log directory path set successfully");
        }

        /// <summary>
        /// Targets a directory path to become the new current log directory
        /// </summary>
        /// <remarks>DO NOT set to any directory you don't want moved around</remarks>
        /// <param name="path">A valid directory path</param>
        public static void SetPath(string path)
        {
            if (TryMove(path))
                UtilityLogger.Log("Log directory path set successfully");
        }

        internal static bool TryMove(string newPath)
        {
            bool folderExists = Exists;
            if (!validatePath(newPath) || !UtilityCore.IsControllingAssembly && folderExists) //Unsure how to properly handle this - should it be presumed that this is synchronous operation across processes?
                return false;

            try
            {
                OnPathChange.OnPending(newPath);

                bool shouldAttemptMove = UtilityCore.IsControllingAssembly && folderExists;
                if (shouldAttemptMove)
                {
                    logMessage("Attempting to move log directory");

                    LogFolderInfo folderInfo = new LogFolderInfo(CurrentPath);
                    folderInfo.Move(newPath, checkPermissions: false); //Enforcing permissions inside log directory is unsupported

                    ContainingPath = Path.GetDirectoryName(newPath);
                    Name = Path.GetFileName(newPath);

                    PathHistory.Update(); //A path history event is recorded when the directory is moved, or renamed
                }
                else
                {
                    ContainingPath = Path.GetDirectoryName(newPath);
                    Name = Path.GetFileName(newPath);
                }
                OnPathChange.OnCompleted(newPath);
                return true;
            }
            catch (Exception ex)
            {
                UtilityLogger.DebugLog(ex);
                UtilityLogger.LogError(ex);

                OnPathChange.OnAbort(newPath);
            }
            return false;

            void logMessage(string message)
            {
                UtilityLogger.DebugLog(message);
                UtilityLogger.Log(message);
            }
        }

        private static bool validatePath(string path)
        {
            if (PathUtils.IsEmpty(path))
            {
                UtilityLogger.LogWarning("Unable to set log directory path. Path given was an empty string.");
                return false;
            }

            //Path hasn't changed, or is an sub-path (not currently supported)
            if (ContainsPath(path))
            {
                if (!PathUtils.PathsAreEqual(CurrentPath, path))
                    UtilityLogger.LogWarning("Unable to set log directory path. Path is a subpath of the current path.");
                return false;
            }

            //Path is a parent to the current path (not currently supported)
            if (PathUtils.ContainsOtherPath(CurrentPath, path))
            {
                UtilityLogger.LogWarning("Unable to set log directory path. Path is a parent of the current path.");
                return false;
            }

            if (!DirectoryUtils.ParentExists(path))
            {
                UtilityLogger.LogWarning("Unable to set log directory path. Part of the path cannot be found.");
                return false;
            }
            return true;
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
                    UtilityLogger.LogWarning("Unable to read path history file");
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
        /// The result of a log directory path search
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

        public enum EligibilityResult
        {
            Success,
            InsufficientPermissions,
            PathIneligible,
        }
    }
}
