using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;
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

        public static void OnEligibilityChanged(LogProperties properties)
        {
            if (!properties.IsNewInstance) return; //Eligibility only applies to newly created log properties

            if (properties.LogsFolderEligible && properties.LogsFolderAware)
                AddToFolder(properties);
            else
                RevokeDesignation(properties); //TODO: LogManager needs a way to ignore this when LogsFolderAware is set to false
        }

        internal static void OnPathChanged(string newPath)
        {
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

        /// <summary>
        /// This method controls the directory that eligible log files target instead of the normal log path. Log files will be moved when the path is set
        /// </summary>
        /// <param name="path">The path (including folder name). Path is assumed to be valid, resolve path before invoking this method if path resolution is required</param>
        public static void SetPath(string path)
        {
            if (IsLogsFolderPath(path) && !IsCurrentPath(path))
            {
                if (CustomPath == null) //Prioritize any custom paths over the initial path
                    OnPathChanged(path);

                Path = InitialPath = path;
            }
            else if (!PathUtils.PathsAreEqual(CustomPath, path))
            {
                if (path != null || InitialPath != null)
                    OnPathChanged(path);

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
