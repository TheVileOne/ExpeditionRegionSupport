using LogUtils.Helpers;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogUtils
{
    public static class LogsFolder
    {
        /// <summary>
        /// The folder name that will store log files. Do not change this. It is case-sensitive.
        /// </summary>
        public static readonly string LOGS_FOLDER_NAME = "Logs";

        /// <summary>
        /// Rain World root folder
        /// Application.dataPath is RainWorld_data folder
        /// </summary>
        public static readonly string DefaultPath = System.IO.Path.Combine(Paths.GameRootPath, LOGS_FOLDER_NAME);

        /// <summary>
        /// StreamingAssets folder
        /// </summary>
        public static readonly string AlternativePath = System.IO.Path.Combine(Paths.StreamingAssetsPath, LOGS_FOLDER_NAME);

        /// <summary>
        /// The path to the Logs directory if it exists, otherwise null
        /// </summary>
        public static string Path;

        public static bool HasInitialized;

        /// <summary>
        /// Check that a path matches one of the two supported Logs directories.
        /// </summary>
        public static bool ContainsPath(string path)
        {
            if (path == null) return false;

            path = System.IO.Path.GetFullPath(path);

            return IsLogsFolderPath(path);
        }

        public static bool IsLogsFolderPath(string path)
        {
            return PathUtils.PathsAreEqual(path, DefaultPath) || PathUtils.PathsAreEqual(path, AlternativePath);
        }

        /// <summary>
        /// Checks a path against the current Logs directory path
        /// </summary>
        public static bool IsBaseLogPath(string path)
        {
            if (path == null)
                return false;

            UpdatePath();

            string basePath = Path;
            return PathUtils.PathsAreEqual(path, basePath);
        }

        /// <summary>
        /// Establishes the path for the Logs directory, creating it if it doesn't exist. This is not called by LogUtils directly
        /// </summary>
        public static void Initialize()
        {
            if (HasInitialized) return;

            Path = FindLogsDirectory();

            string errorMsg = null;
            try
            {
                //The found directory needs to be created if it doesn't yet exist, and the alternative directory removed
                if (!Directory.Exists(Path))
                {
                    UtilityCore.BaseLogger.LogInfo("Creating directory: " + Path);
                    Directory.CreateDirectory(Path);
                }

                string alternativeLogPath = PathUtils.PathsAreEqual(Path, DefaultPath) ? AlternativePath : DefaultPath;

                try
                {
                    if (Directory.Exists(alternativeLogPath))
                    {
                        UtilityCore.BaseLogger.LogInfo("Removing directory: " + alternativeLogPath);
                        Directory.Delete(alternativeLogPath, true);
                    }
                }
                catch (Exception ex)
                {
                    errorMsg = "Unable to delete log directory";
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                UtilityCore.LogError(errorMsg ?? "Unable to create log directory", ex);
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
            if (!properties.PropertiesCreatedRecently) return; //Eligibility only applies to recently created log properties

            if (properties.LogsFolderEligible && properties.LogsFolderAware)
                AddToFolder(properties);
            else
                RevokeDesignation(properties); //TODO: LogManager needs a way to ignore this when LogsFolderAware is set to false
        }

        /// <summary>
        /// Designates a log file to write to the Logs folder if it exists
        /// </summary>
        public static void AddToFolder(LogProperties properties)
        {
            if (Path == null || IsLogsFolderPath(properties.CurrentFolderPath)) return;

            string newPath = Path;

            if (properties.FileExists)
            {
                //This particular MoveLog overload doesn't update current path
                FileStatus moveResult = Helpers.LogUtils.MoveLog(properties.CurrentFolderPath, newPath);

                if (moveResult != FileStatus.MoveComplete) //There was an issue moving this file
                    return;
            }
            properties.ChangePath(newPath);
        }

        /// <summary>
        /// Removes association with the Logs folder
        /// </summary>
        internal static void RevokeDesignation(LogProperties properties)
        {
            if (!IsLogsFolderPath(properties.CurrentFolderPath)) //Check that the log file is currently designated
                return;

            string newPath = properties.FolderPath;

            if (Path != null && properties.FileExists) //When Path is null, Logs folder does not exist
            {
                //This particular MoveLog overload doesn't update current path
                FileStatus moveResult = Helpers.LogUtils.MoveLog(properties.CurrentFolderPath, newPath);

                if (moveResult != FileStatus.MoveComplete) //There was an issue moving this file
                    return;
            }
            properties.ChangePath(newPath);
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
