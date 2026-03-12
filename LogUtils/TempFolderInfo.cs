using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LogUtils
{
    public class TempFolderInfo : IAccessToken
    {
        private static readonly object folderLock = new object();

        private int accessCount;

        /// <summary>
        /// Fully qualified path to a temporary folder
        /// </summary>
        public readonly string FullPath;

        private readonly HashSet<string> _orphanedFiles = new HashSet<string>(ComparerUtils.PathComparer);
        /// <summary>
        /// Contains paths pertaining to files within the temp folder flagged as orphaned; usually an indication that the file was unable to be moved from the directory
        /// </summary>
        public ICollection<string> OrphanedFiles => _orphanedFiles;

        public PathResolver Resolver;

        /// <summary>
        /// Checks whether deletion of the temp folder minimizes risk of unwanted data loss
        /// </summary>
        public bool SafeToDelete => UtilityCore.IsControllingAssembly && accessCount == 0 && OrphanedFiles.Count == 0;

        /// <summary>
        /// Creates a new <see cref="TempFolderInfo"/> instance
        /// </summary>
        /// <param name="folderName">The name of the folder that should be created in the users Temp folder path</param>
        /// <exception cref="ArgumentException">
        /// folderName was null, or contains only whitespace - OR - contains an illegal path character
        /// </exception>
        public TempFolderInfo(string folderName)
        {
            if (PathUtils.IsEmpty(folderName))
                throw new ArgumentException(nameof(folderName), "Folder name cannot be empty.");

            FullPath = Path.Combine(Path.GetTempPath(), folderName);
            Resolver = new TempPathResolver(FullPath);
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


        IAccessToken IAccessToken.Access()
        {
            lock (folderLock)
            {
                UtilityLogger.DebugLog("Temporary folder accessed");
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
                UtilityLogger.DebugLog("Temporary folder access revoked");
                Interlocked.Decrement(ref accessCount);
            }
        }

        void IDisposable.Dispose()
        {
            IAccessToken token = this;
            token.RevokeAccess();
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
                UtilityLogger.DebugLog("Temporary folder cleanup successful");
            }
            catch
            {
                UtilityLogger.LogWarning("Temporary folder cleanup unable to complete successfully");
            }
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
                UtilityLogger.LogError("Unable to delete temporary folder", ex);
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

        internal void CreateInternal()
        {
            lock (folderLock)
            {
                Directory.CreateDirectory(FullPath);
                ScheduleDelete();
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
    }
}
