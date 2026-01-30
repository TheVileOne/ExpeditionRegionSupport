using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Enums.FileSystem;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.IO;

namespace LogUtils.Helpers
{
    public static class LogGroup
    {
        internal static void ChangePath(LogGroupID group, string currentPath, string newPath, bool applyToMembers = false)
        {
            string newFolderPath = LogProperties.GetNewBasePath(group, currentPath, newPath);
            group.Properties.ChangePath(newFolderPath, applyToMembers);
        }

        internal static void ChangePath(IEnumerable<LogID> logFilesInFolder, string currentPath, string newPath)
        {
            foreach (LogID logFile in logFilesInFolder)
            {
                logFile.Properties.ChangeBasePath(currentPath, newPath);
            }
        }

        public static void DeleteFolder(LogGroupID group)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            LogFolderInfo folderInfo = new LogFolderInfo(group.Properties.CurrentFolderPath);

            if (folderInfo.Exists)
                return;
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
                OnPermissionDenied(group.Properties.CurrentFolderPath, permission);
        }

        /// <summary>
        /// Throws an exception based on a permission violation
        /// </summary>
        /// <param name="folderPath">The path that caused the violation</param>
        /// <param name="permission">The permission violation</param>
        /// <exception cref="InvalidOperationException">Log group does not have a folder path specified</exception>
        /// <exception cref="IOException">Folder operation was unsafe, or attempted on a protected path</exception>
        /// <exception cref="PermissionDeniedException">Insufficient permission to complete folder operation</exception>
        internal static void OnPermissionDenied(string folderPath, FolderPermissions permission)
        {
            if (PathUtils.IsEmpty(folderPath))
                throw new InvalidOperationException("Group does not support folder operations");

            bool pathWasUnsafe = !DirectoryUtils.IsSafeToMove(folderPath);

            if (pathWasUnsafe)
            {
                string exceptionMessage = null;
                switch (permission)
                {
                    case FolderPermissions.Delete:
                        exceptionMessage = "Unable to delete folder at source path\n" +
                                           "REASON: Folder path is restricted";
                        break;
                    case FolderPermissions.Move:
                        exceptionMessage = "Unable to move folder at source path\n" +
                                           "REASON: Folder path is restricted";
                        break;
                    default:
                        exceptionMessage = "Unable to move folder at source path";
                        break;
                }
                throw new IOException(exceptionMessage);
            }

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
            throw new PermissionDeniedException($"Permission was not given to {action}. Check mod permissions.");
        }

        public static IEnumerable<LogGroupID> GroupsSharingThisPath(string path)
        {
            return GroupsSharingThisPath(path, LogProperties.PropertyManager.GroupProperties);
        }

        public static IEnumerable<LogGroupID> GroupsSharingThisPath(string path, IEnumerable<LogGroupProperties> searchEntries)
        {
            IEnumerable<LogGroupProperties> groupsSharingThisPath = searchEntries
                             .WithFolder()
                             .HasPath(LogProperties.GetContainingPath(path));
            return groupsSharingThisPath.GetIDs();
        }

        public static IEnumerable<LogID> NonGroupMembersSharingThisPath(string path)
        {
            return NonGroupMembersSharingThisPath(path, LogProperties.PropertyManager.Properties);
        }

        public static IEnumerable<LogID> NonGroupMembersSharingThisPath(string path, IEnumerable<LogID> searchEntries)
        {
            return NonGroupMembersSharingThisPath(path, searchEntries.GetProperties());
        }

        public static IEnumerable<LogID> NonGroupMembersSharingThisPath(string path, IEnumerable<LogProperties> searchEntries)
        {
            IEnumerable<LogProperties> nonGroupMembers =
                searchEntries.FindAll(properties => properties.Group == null).GetProperties();
            return nonGroupMembers.HasPath(LogProperties.GetContainingPath(path)).GetIDs();
        }
    }
}
