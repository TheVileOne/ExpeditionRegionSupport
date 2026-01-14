using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Threading;
using System;
using System.IO;
using System.Threading;

namespace LogUtils
{
    public static class TempFolder
    {
        internal const int MAX_DELETE_ATTEMPTS = 10;

        private static int accessCount;
        private static readonly object folderLock = new object();

        /// <summary>
        /// Checks whether deletion of the temp folder minimizes risk of unwanted data loss
        /// </summary>
        public static bool SafeToDelete => accessCount == 0;

        /// <summary>
        /// Full path to LogUtils temporary folder
        /// </summary>
        public static string Path => Paths.GetTempDirectory();

        /// <summary>
        /// Signal that a process intends to access and use LogUtils defined temporary folder. While accessing, LogUtils guarantees that the folder wont be moved, or deleted
        /// through the <see cref="TempFolder"/> public API. Call <see cref="RevokeAccess"/> to signal that your process no longer needs to access the Temp folder.
        /// </summary>
        /// <remarks>For each time this method is called, a following <see cref="RevokeAccess"/> must also be called.</remarks>
        public static void Access()
        {
            Interlocked.Increment(ref accessCount);
        }

        /// <summary>
        /// Signal that a process no longer needs to access any data located inside of the LogUtils defined temporary folder. Do not call this unless your process
        /// already has access. Doing so may corrupt/remove data being used by other processes that require this temporary folder.
        /// </summary>
        public static void RevokeAccess()
        {
            if (accessCount == 0)
            {
                UtilityLogger.LogWarning("Abnormal amount of revoke access attempts made");
                return;
            }
            Interlocked.Decrement(ref accessCount);
        }

        public static void Create()
        {
            lock (folderLock)
            {
                if (accessCount == 0)
                    UtilityLogger.LogWarning($"Please invoke {nameof(Access)} before using this method");

                Directory.CreateDirectory(Path);
            }
        }

        private static Task scheduledTask;
        public static void ScheduleForDeletion()
        {
            lock (folderLock)
            {
                if (scheduledTask != null && scheduledTask.PossibleToRun) //Only allow one deletion task to run
                    return;

                int deleteAttempts = 0; //Amount of failed delete attempts since last access revocation
                scheduledTask = LogTasker.Schedule(new Task(scheduledAction, TimeSpan.FromMilliseconds(50)) //Not in a hurry to remove this folder
                {
                    Name = "TempFolder",
                    IsContinuous = true
                });

                void scheduledAction()
                {
                    if (!SafeToDelete)
                    {
                        deleteAttempts = 0; //A valid attempt is when nothing is trying to access the Temp folder
                        return;
                    }

                    bool deleted = TryDelete();
                    if (!deleted)
                    {
                        deleteAttempts++;
                        if (deleteAttempts >= MAX_DELETE_ATTEMPTS)
                        {
                            UtilityLogger.LogWarning("Failed to delete Temp folder. It may be used by another process.");
                            scheduledTask.Cancel();
                            return;
                        }
                    }
                    scheduledTask.Complete();
                }
            }
        }

        public static bool TryDelete()
        {
            lock (folderLock)
            {
                if (!SafeToDelete)
                    return false;

                return DirectoryUtils.DeletePermanently(Path, DirectoryDeletionScope.AllFilesAndFolders);
            }
        }
    }
}
