using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LogUtils
{
    public class TempFolder : IAccessToken
    {
        private static readonly object folderLock = new object();
        private static IAccessToken accessToken => UtilityCore.TempFolder;

        private int accessCount;

        private readonly HashSet<string> _orphanedFiles = new HashSet<string>(ComparerUtils.PathComparer);
        /// <summary>
        /// Contains paths pertaining to files within the temp folder flagged as orphaned; usually an indication that the file was unable to be moved from the directory
        /// </summary>
        public ICollection<string> OrphanedFiles => _orphanedFiles;

        /// <summary>
        /// Checks whether deletion of the temp folder minimizes risk of unwanted data loss
        /// </summary>
        public bool SafeToDelete => UtilityCore.IsControllingAssembly && accessCount == 0 && OrphanedFiles.Count == 0;

        /// <summary>
        /// Creates a new <see cref="TempFolder"/> instance
        /// </summary>
        /// <param name="folderName">The name of the folder that should be created in the users Temp folder path</param>
        /// <exception cref="ArgumentException">
        /// folderName was null, or contains only whitespace - OR - contains an illegal path character
        /// </exception>
        public TempFolder(string folderName)
        {
            if (PathUtils.IsEmpty(folderName))
                throw new ArgumentException(nameof(folderName), "Folder name cannot be empty.");

            FullPath = Path.Combine(Path.GetTempPath(), folderName);
        }

        /// <summary>
        /// Completes initialization tasks that should be handled when a temporary folder is established. 
        /// </summary>
        public void Initialize()
        {
            if (!UtilityCore.IsControllingAssembly || !Directory.Exists(FullPath))
                return;

            OrphanAllFiles();
            if (OrphanedFiles.Count == 0) //There are no files - folder is safe for removal 
                ScheduleDelete();
        }

        #region Static
        /// <inheritdoc cref="IAccessToken.Access"/>
        public static IAccessToken Access()
        {
            return accessToken.Access();
        }

        /// <inheritdoc cref="IAccessToken.RevokeAccess"/>
        public static void RevokeAccess()
        {
            accessToken.RevokeAccess();
        }

        /// <summary>
        /// Creates the directory structure for a given file, or directory path
        /// </summary>
        /// <param name="path">A file, or directory path</param>
        /// <returns>The created directory path, or null if path could not be created</returns>
        public string CreateDirectoryFor(string path)
        {
            string targetPath = getTargetPath(path);
            try
            {
                Directory.CreateDirectory(targetPath);
                return targetPath;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Unable to create directory", ex);
                return null;
            }

            string getTargetPath(string input)
            {
                string targetPath = MapPathToFolder(input); //Does not require path separator trimming

                bool isTargetingTempFolder = PathUtils.PathsAreEqual(targetPath, FullPath);

                if (isTargetingTempFolder)
                    return FullPath;

                //Targets the parent directory of the filename, or directory path provided
                return System.IO.Path.GetDirectoryName(targetPath);
            }
        }

        /// <summary>
        /// Maps a path, filename, or directory to a location within the Temp folder, and returns the resulting path string
        /// </summary>
        /// <param name="path">A path, filename, or directory name to locate</param>
        /// <returns>A fully qualified path inside the Temp folder</returns>
        /// <remarks>No attempt is made to ensure path exists within the Temp folder</remarks>
        public string MapPathToFolder(string path)
        {
            TempPathResolver pathResolver = new TempPathResolver(FullPath);
            return pathResolver.Resolve(path);
        }

        /// <summary>
        /// Create a temporary directory if it doesn't already exist
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="DirectoryNotFoundException">Folder is part of an unmapped drive</exception>
        /// <exception cref="UnauthorizedAccessException">Not allowed to access directory, or directory path</exception>
        public void Create()
        {
            CreateInternal();
        }

        /// <summary>
        /// Attempt to create a temporary directory
        /// </summary>
        /// <returns><see langword="true"/>, if directory was created, or already exists; otherwise <see langword="false"/></returns>
        public bool TryCreate()
        {
            try
            {
                CreateInternal();
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                UtilityLogger.LogError(ex);
                return false;
            }
        }

        public bool TryDelete()
        {
            try
            {
                DeleteInternal();
                return true;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Unable to delete Temp folder", ex);
                return false;
            }
        }

        /// <summary>
        /// Does an inventory of all files inside a temporary folder, and marks each file as orphaned
        /// </summary>
        public void OrphanAllFiles()
        {
            try
            {
                string[] allFiles = Directory.GetFiles(FullPath, "*", SearchOption.AllDirectories);
                _orphanedFiles.UnionWith(allFiles);
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Orphaned file check failed", ex);
            }
        }
        #endregion
        #region Internal
        /// <summary>
        /// Fully qualified path to a temporary folder
        /// </summary>
        public readonly string FullPath;

        IAccessToken IAccessToken.Access()
        {
            lock (folderLock)
            {
                Interlocked.Increment(ref accessCount);
                ScheduleDelete();
            }
            return this;
        }

        void IAccessToken.RevokeAccess()
        {
            lock (folderLock)
            {
                if (accessCount == 0)
                {
                    UtilityLogger.LogWarning("Abnormal amount of revoke access attempts made");
                    return;
                }
                Interlocked.Decrement(ref accessCount);
            }
        }

        void IDisposable.Dispose()
        {
            //Not safe to revoke access and dispose token
            RevokeAccess();
        }

        internal void CreateInternal()
        {
            lock (folderLock)
            {
                if (accessCount == 0)
                    UtilityLogger.LogWarning($"Please invoke {nameof(Access)} before using this method");

                Directory.CreateDirectory(FullPath);
                ScheduleDelete();
            }
        }

        /// <summary>
        /// Initiates a cleanup process on the temp folder
        /// </summary>
        /// <remarks>The current cleanup behavior is deletion of the folder when it is safe to do so</remarks>
        public void Cleanup()
        {
            try
            {
                if (RainWorldInfo.IsShuttingDown) //No other cleanup tasks need to run
                {
                    scheduledTask?.Cancel();
                    scheduledTask = null;
                }
                DeleteInternal();
            }
            catch
            {
                UtilityLogger.LogWarning("Temp folder cleanup unable to complete successfully");
            }
        }

        private Task scheduledTask;
        internal void ScheduleDelete()
        {
            if (scheduledTask != null && scheduledTask.PossibleToRun) //Only allow one deletion task to run
                return;

            scheduledTask = LogTasker.Schedule(new Task(scheduledAction, TimeSpan.FromSeconds(5)) //Not in a hurry to remove this folder
            {
                Name = "TempFolder",
                IsContinuous = true
            });

            void scheduledAction()
            {
                if (!SafeToDelete) return;

                bool deleted = DirectoryUtils.DeletePermanently(FullPath, DirectoryDeletionScope.AllFilesAndFolders);

                if (deleted)
                {
                    scheduledTask.Complete();
                    scheduledTask = null;
                }
            }
        }

        internal void DeleteInternal()
        {
            lock (folderLock)
            {
                if (!SafeToDelete)
                    throw new InvalidOperationException("Delete operation is unsafe.");

                //TODO: This needs to throw here
                DirectoryUtils.DeletePermanently(FullPath, DirectoryDeletionScope.AllFilesAndFolders);
            }
        }
        #endregion
    }
}
