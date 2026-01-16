using LogUtils.Helpers;
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
        private static int accessCount;
        private static readonly object folderLock = new object();

        private static readonly TempFolder folder = new TempFolder();

        internal static IAccessToken AccessToken => folder;

        private static readonly HashSet<string> _orphanedFiles = new HashSet<string>(ComparerUtils.PathComparer);
        /// <summary>
        /// Contains paths pertaining to files within the temp folder flagged as orphaned; usually an indication that the file was unable to be moved from the directory
        /// </summary>
        public static ICollection<string> OrphanedFiles => _orphanedFiles;

        /// <summary>
        /// Checks whether deletion of the temp folder minimizes risk of unwanted data loss
        /// </summary>
        public static bool SafeToDelete => UtilityCore.IsControllingAssembly && accessCount == 0 && OrphanedFiles.Count == 0;

        /// <summary>
        /// Full path to LogUtils temporary folder
        /// </summary>
        public static string Path => Paths.GetTempDirectory();

        private TempFolder()
        {
        }

        #region Static
        /// <summary>
        /// Signal that a process intends to access and use LogUtils defined temporary folder. While accessing, LogUtils guarantees that the folder wont be moved, or deleted
        /// through the <see cref="TempFolder"/> public API. Call <see cref="RevokeAccess"/> to signal that your process no longer needs to access the Temp folder.
        /// </summary>
        /// <remarks>For each time this method is called, a following <see cref="RevokeAccess"/> must also be called.</remarks>
        public static IAccessToken Access()
        {
            return AccessToken.Access();
        }

        /// <summary>
        /// Signal that a process no longer needs to access any data located inside of the LogUtils defined temporary folder. Do not call this unless your process
        /// already has access. Doing so may corrupt/remove data being used by other processes that require this temporary folder.
        /// </summary>
        public static void RevokeAccess()
        {
            AccessToken.RevokeAccess();
        }

        public static void Create()
        {
            folder.CreateInternal();
        }

        public static bool TryCreate()
        {
            try
            {
                folder.CreateInternal();
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                UtilityLogger.LogError(ex);
                return false;
            }
        }

        public static bool TryDelete()
        {
            try
            {
                folder.DeleteInternal();
                return true;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Unable to delete Temp folder", ex);
                return false;
            }
        }

        internal static void OnStartup()
        {
            if (!UtilityCore.IsControllingAssembly || !Directory.Exists(Path))

            OrphanAllFiles();
            if (OrphanedFiles.Count == 0) //There are no files - folder is safe for removal 
                folder.ScheduleDelete();
        }

        /// <summary>
        /// Does an inventory of all files inside the temp folder (when it exists), and marks each file as orphaned
        /// </summary>
        internal static void OrphanAllFiles()
        {
            string tempFolderPath = Path;
            try
            {
                string[] allFiles = Directory.GetFiles(tempFolderPath, "*", SearchOption.AllDirectories);
                _orphanedFiles.UnionWith(allFiles);
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Orphaned file check failed", ex);
            }
        }
        #endregion
        #region Internal
        IAccessToken IAccessToken.Access()
        {
            Interlocked.Increment(ref accessCount);
            return this;
        }

        void IAccessToken.RevokeAccess()
        {
            if (accessCount == 0)
            {
                UtilityLogger.LogWarning("Abnormal amount of revoke access attempts made");
                return;
            }
            Interlocked.Decrement(ref accessCount);
        }

        void IDisposable.Dispose()
        {
            //Not safe to rfevoke access and dispose token
            RevokeAccess();
        }

        internal void CreateInternal()
        {
            lock (folderLock)
            {
                if (accessCount == 0)
                    UtilityLogger.LogWarning($"Please invoke {nameof(Access)} before using this method");

                Directory.CreateDirectory(Path);
                ScheduleDelete();
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

                bool deleted = DirectoryUtils.DeletePermanently(Path, DirectoryDeletionScope.AllFilesAndFolders);

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
                DirectoryUtils.DeletePermanently(Path, DirectoryDeletionScope.AllFilesAndFolders);
            }
        }
        #endregion
    }
}
