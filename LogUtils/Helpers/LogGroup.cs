using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.IO;

namespace LogUtils.Helpers
{
    public static class LogGroup
    {
        internal static void ChangePath(LogGroupID group, string currentPath, string newPath)
        {
            string newFolderPath = getUpdatedPath(group, currentPath, newPath);
            group.Properties.ChangePath(newFolderPath, applyToMembers: false);
        }

        internal static void ChangePath(IEnumerable<LogID> logFilesInFolder, string currentPath, string newPath)
        {
            foreach (LogID logFile in logFilesInFolder)
            {
                string newFolderPath = getUpdatedPath(logFile, currentPath, newPath);
                logFile.Properties.ChangePath(newFolderPath);
            }
        }

        private static string getUpdatedPath(LogID logID, string currentPath, string newPath)
        {
            string currentFolderPath = logID.Properties.CurrentFolderPath;

            bool isTopLevel = currentFolderPath.Length == currentPath.Length;
            if (isTopLevel)
            {
                //Top-level files can directly be assigned the new path (most common case)
                return newPath;
            }
            else
            {
                //Take the subfolder part of the path and assign it a new root
                string subFolderPath = currentFolderPath.Substring(Math.Min(currentPath.Length, currentFolderPath.Length)).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return Path.Combine(newPath, subFolderPath);
            }
        }

        public static void DeleteFolder(LogGroupID group)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            Lock groupLock = group.Properties.GetLock();

            using (groupLock.Acquire())
            {
                ThrowOnValidationFailed(group, FolderPermissions.Delete);
                //TODO: Delete code here
            }
        }

        public static void MoveFolder(LogGroupID group, string newPath)
        {
            LogGroupMover groupMover = new LogGroupMover(newPath)
            {
                FailProtocol = FailProtocol.Throw
            };

            try
            {
                groupMover.MoveFolder(group);
            }
            catch (InvalidOperationException ex)
            {
                if (PathUtils.IsEmpty(newPath))
                    throw new ArgumentNullException(nameof(newPath), ex.Message);
                throw new ArgumentException(ex.Message);
            }
        }

        public static void MoveFiles(LogGroupID group, string newPath)
        {
            LogGroupMover groupMover = new LogGroupMover(newPath)
            {
                FailProtocol = FailProtocol.Throw
            };
            groupMover.Move(group);
        }

        /// <summary>
        /// Example showing how API can be used by a mod to move their group folder, or its contents around
        /// </summary>
        public static void MoveFolderExample()
        {
            LogGroupID myGroupID = null;
            bool hasTriedToMoveFolder = false;
        retry:
            try
            {
                //Define a new group path
                string folderName = Path.GetFileName(myGroupID.Properties.CurrentFolderPath);
                string newFolderPath = Path.Combine("new path", folderName);

                //Attempt to move entire folder, and if it fails, attempt to move only the files instead
                if (!hasTriedToMoveFolder)
                {
                    MoveFolder(myGroupID, newFolderPath);
                }
                else
                {
                    MoveFiles(myGroupID, newFolderPath);
                }
                //Confirm that we have a new path
                Assert.That(PathUtils.PathsAreEqual(newFolderPath, myGroupID.Properties.CurrentFolderPath));
            }
            catch (IOException)
            {
                if (!hasTriedToMoveFolder) //Ignore if this fails twice, but actual handling procesures may differ
                {
                    hasTriedToMoveFolder = true;
                    goto retry;
                }
            }
        }

        internal static void DemandPermission(LogGroupID group, FolderPermissions permission)
        {
            bool hasPermission = group.Properties.VerifyPermissions(permission);

            if (!hasPermission)
            {
                string action = string.Empty;
                switch (permission)
                {
                    case FolderPermissions.Delete:
                        action = "delete group folder";
                        break;
                    case FolderPermissions.Move:
                        action = "move group folder";
                        break;
                    default:
                        throw new PermissionDeniedException("Unknown permission error");
                }
                throw new PermissionDeniedException($"Permission was not given to {action}.");
            }
        }

        internal static void ThrowOnValidationFailed(LogGroupID group, FolderPermissions validationContext)
        {
            if (!group.Properties.IsFolderGroup)
                throw new ArgumentException("Group is not associated with a folder path");

            string groupPath = group.Properties.CurrentFolderPath;

            //The folder might be a game directory
            if (!DirectoryUtils.IsSafeToMove(groupPath))
            {
                string exceptionMessage = null;
                switch (validationContext)
                {
                    case FolderPermissions.Delete:
                        exceptionMessage = "Unable to delete folder at source path\n" +
                                           "REASON: Folder path is restricted";
                        break;
                    case FolderPermissions.Move:
                        exceptionMessage = "Unable to move folder at source path\n" +
                                           "REASON: Folder path is restricted";
                        break;
                }
                throw new IOException(exceptionMessage);
            }
        }

        public static IEnumerable<LogGroupID> GroupsSharingThisPath(string path)
        {
            LogGroupID groupID = LogGroupID.Factory.CreateID("LogUtils", path);
            return groupID.Properties.AllGroupsSharingMyFolder().GetIDs();
        }
    }
}
