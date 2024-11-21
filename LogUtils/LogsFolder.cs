using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.IO;

namespace LogUtils
{
    public static class LogsFolder
    {
        /// <summary>
        /// The folder that will store log files
        /// </summary>
        public const string LOGS_FOLDER_NAME = "Logs";

        /// <summary>
        /// StreamingAssets folder
        /// </summary>
        public static readonly string AlternativePath = System.IO.Path.Combine(Paths.StreamingAssetsPath, LOGS_FOLDER_NAME);

        /// <summary>
        /// Rain World root folder
        /// </summary>
        public static readonly string DefaultPath = System.IO.Path.Combine(Paths.GameRootPath, LOGS_FOLDER_NAME);

        /// <summary>
        /// The path to the Logs directory if it exists, otherwise null
        /// </summary>
        public static string Path { get; private set; }

        public static string InitialPath { get; private set; }

        public static string CustomPath { get; private set; }

        /// <summary>
        /// PathCycler is responsible for moving forward, or backward through a collection of valid paths
        /// </summary>
        public static PathCycler PathCycler;

        public static bool HasInitialized;

        /// <summary>
        /// Checks a path against the current Logs directory path
        /// </summary>
        public static bool IsCurrentPath(string path)
        {
            if (path == null)
                return false;

            UpdatePath();

            string basePath = Path;
            return PathUtils.PathsAreEqual(path, basePath);
        }

        /// <summary>
        /// Check that a path matches one of the two supported Logs directories
        /// </summary>
        public static bool IsLogsFolderPath(string path)
        {
            return PathUtils.PathsAreEqual(path, DefaultPath) || PathUtils.PathsAreEqual(path, AlternativePath);
        }

        /// <summary>
        /// Establishes the path for the Logs directory, creating it if it doesn't exist. This is not called by LogUtils directly
        /// </summary>
        public static void Initialize()
        {
            if (HasInitialized) return;

            string errorMsg = null;
            try
            {
                //SetPath creates the directory for us
                SetPath(FindLogsDirectory());

                if (IsLogsFolderPath(Path))
                {
                    //The alternative log path needs to be removed if it exists
                    string alternativeLogPath = IsCurrentPath(DefaultPath) ? AlternativePath : DefaultPath;

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
                        errorMsg = "Unable to delete log directory";
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(errorMsg ?? "Unable to create log directory", ex);
            }

            HasInitialized = true;
        }

        /// <summary>
        /// Finds the full path to the Logs directory if it exists, checking the default path first, otherwise returns null
        /// </summary>
        public static string FindExistingLogsDirectory()
        {
            if (Directory.Exists(DefaultPath))
                return DefaultPath;

            if (Directory.Exists(AlternativePath))
                return AlternativePath;

            return null;
        }

        /// <summary>
        /// Finds the full path to the Logs directory if it exists, otherwise returns the default path
        /// </summary>
        public static string FindLogsDirectory()
        {
            return FindExistingLogsDirectory() ?? DefaultPath;
        }

        /// <summary>
        /// Returns log file IDs that have the Logs folder as a current folder path
        /// </summary>
        public static IEnumerable<LogID> GetContainedLogFiles()
        {
            if (Path == null)
                return Array.Empty<LogID>();

            return LogID.FindAll(properties => PathUtils.PathsAreEqual(properties.CurrentFolderPath, Path));
        }

        public static void OnEligibilityChanged(LogEventArgs e)
        {
            LogProperties properties = e.Properties;

            if (!properties.IsNewInstance) return; //Eligibility only applies to newly created log properties

            if (properties.LogsFolderEligible && properties.LogsFolderAware)
                AddToFolder(properties);
            else
                RevokeDesignation(properties); //TODO: LogManager needs a way to ignore this when LogsFolderAware is set to false
        }

        internal static void OnPathChanged(PathChangedEventArgs e)
        {
            //TODO: Finish event handler
            string newPath = e.NewPath;
            string oldPath = e.OldPath;

            if (!Directory.Exists(newPath))
            {
                UtilityLogger.Log("Creating directory: " + newPath);
                Directory.CreateDirectory(newPath);
            }

            //Searches all LogProperties that contain the old path, and migrate them to the new path
            foreach (LogID logFile in LogID.FindAll(p => PathUtils.PathsAreEqual(p.CurrentFolderPath, Path)))
            {
                if (LogFile.Move(logFile, newPath) != FileStatus.MoveComplete)
                    UtilityLogger.LogWarning("Unable to move log file");
            }
        }

        /// <summary>
        /// Designates a log file to write to the Logs folder if it exists
        /// </summary>
        public static void AddToFolder(LogProperties properties)
        {
            if (Path == null) return;

            lock (properties.FileLock)
            {
                if (IsCurrentPath(properties.CurrentFolderPath)) return;

                string newPath = Path;

                if (properties.FileExists)
                {
                    properties.FileLock.SetActivity(properties.ID, FileAction.Move);

                    //This particular MoveLog overload doesn't update current path
                    FileStatus moveResult = LogFile.Move(properties.CurrentFolderPath, newPath);

                    if (moveResult != FileStatus.MoveComplete) //There was an issue moving this file
                        return;
                }
                properties.ChangePath(newPath);
            }
        }

        /// <summary>
        /// Removes association with the Logs folder
        /// </summary>
        internal static void RevokeDesignation(LogProperties properties)
        {
            lock (properties.FileLock)
            {
                if (!IsLogsFolderPath(properties.CurrentFolderPath)) //Check that the log file is currently designated
                    return;

                string newPath = properties.FolderPath;

                if (Path != null && properties.FileExists) //When Path is null, Logs folder does not exist
                {
                    properties.FileLock.SetActivity(properties.ID, FileAction.Move);

                    //This particular MoveLog overload doesn't update current path
                    FileStatus moveResult = LogFile.Move(properties.CurrentFolderPath, newPath);

                    if (moveResult != FileStatus.MoveComplete) //There was an issue moving this file
                        return;
                }
                properties.ChangePath(newPath);
            }
        }

        public static bool TryMove(LogsFolderAccessToken accessToken, string path)
        {
            try
            {
                Move(accessToken, path);
                return true;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Unable to move logs directory", ex);

                foreach (LogID logFile in GetContainedLogFiles())
                    logFile.Properties.NotifyPendingMoveAborted();
                return false;
            }
        }

        public static void Move(LogsFolderAccessToken accessToken, string path)
        {
            if (path == null)
                throw new ArgumentNullException("Path argument cannot be null");

            string basePath = Path;
            if (PathUtils.PathsAreEqual(path, basePath))
                return;

            bool canProceed = RequestAccess(accessToken, path);

            if (!canProceed)
                throw new InvalidOperationException("Unable to change path. Access is denied");

            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                List<StreamResumer> streamsToResume = new List<StreamResumer>();
                //TODO: Notify LogIDs of a pending move, shut down their Filestreams (reopen when finished)
                try
                {
                    //TODO: Code for shutting down Filestreams at the LogWriter level
                    foreach (LogID logFile in GetContainedLogFiles())
                    {
                        logFile.Properties.NotifyPendingMove(path);

                        foreach (PersistentFileHandle streamHandle in logFile.Properties.PersistentStreamHandles)
                            streamsToResume.Add(streamHandle.InterruptStream());
                    }

                    Directory.Move(basePath, path);
                }
                finally
                {
                    //Open streams
                    streamsToResume.ForEach(stream => stream.Resume());
                }
            }
        }

        public static bool TryCycle(LogsFolderAccessToken accessToken, bool cycleForward = true)
        {
            try
            {
                Cycle(accessToken, cycleForward);
                return true;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Unable to cycle logs directory", ex);

                foreach (LogID logFile in GetContainedLogFiles())
                    logFile.Properties.NotifyPendingMoveAborted();
                return false;
            }
        }

        public static void Cycle(LogsFolderAccessToken accessToken, bool cycleForward = true)
        {
            if (cycleForward)
                PathCycler.CycleNext(accessToken);
            else
                PathCycler.CyclePrev(accessToken);

            try
            {
                Move(accessToken, PathCycler.Result);
            }
            finally
            {
                PathCycler.Result = null;
            }
        }

        /// <summary>
        /// Attempts to move log files back to where they were before the files were moved to a Logs folder
        /// </summary>
        public static void Restore()
        {
            //TODO: Immplement
            throw new NotImplementedException();
        }

        internal static bool RequestAccess(LogsFolderAccessToken accessToken, string requestPath)
        {
            if (accessToken.Access == FolderAccess.Unrestricted)
                return true;

            bool hasPermission = false;
            string basePath = Path;

            //We need to check access permissions before we can touch any folders
            FolderRelationship currentRelationship = GetAccessRelationship(accessToken, basePath);
            FolderRelationship incomingRelationship = GetAccessRelationship(accessToken, requestPath);

            if (currentRelationship != FolderRelationship.None)
            {
                switch (currentRelationship)
                {
                    case FolderRelationship.Familiar:
                        hasPermission = true;
                        break;
                    case FolderRelationship.Base:
                        hasPermission = accessToken.Access != FolderAccess.Strict;
                        break;
                    case FolderRelationship.Foreign:
                        hasPermission = false;
                        break;
                }

                if (hasPermission)
                {
                    switch (incomingRelationship)
                    {
                        case FolderRelationship.Familiar:
                            hasPermission = true;
                            break;
                        case FolderRelationship.Base:
                            hasPermission = accessToken.Access != FolderAccess.Strict;
                            break;
                        case FolderRelationship.Foreign:
                            hasPermission = false;
                            break;
                    }
                }
            }
            return hasPermission;
        }

        public static FolderRelationship GetAccessRelationship(LogsFolderAccessToken accessToken, string path)
        {
            if (path == null)
                return FolderRelationship.None;

            bool pathAllowed = Path.MatchAny(ComparerUtils.PathComparer, accessToken.AllowedPaths);

            if (pathAllowed)
                return FolderRelationship.Familiar;

            if (IsLogsFolderPath(path))
                return FolderRelationship.Base;

            return FolderRelationship.Foreign;
        }

        /// <summary>
        /// Sets the logs folder path to that of an existing directory
        /// <br>DO NOT set to any directory you don't want moved around</br>
        /// </summary>
        /// <param name="path">A valid directory path</param>
        public static void SetPath(string path)
        {
            //Mods are responsible for creating directory, if the utility is unable to do so
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(path);

            string currentPath = Path;
            if (IsLogsFolderPath(path) && !IsCurrentPath(path))
            {
                if (CustomPath == null) //Prioritize any custom paths over the initial path
                    OnPathChanged(new PathChangedEventArgs(path, currentPath));

                Path = InitialPath = path;
            }
            else if (!PathUtils.PathsAreEqual(CustomPath, path))
            {
                if (path != null || InitialPath != null)
                    OnPathChanged(new PathChangedEventArgs(path, currentPath));

                CustomPath = path;
                Path = CustomPath == null ? InitialPath : CustomPath;
            }
        }

        /// <summary>
        /// Gets, and stores the full path to the Logs directory if it exists, otherwise sets Path to null if it doesn't exist
        /// </summary>
        public static void UpdatePath()
        {
            Path ??= FindExistingLogsDirectory();
        }
    }
}
