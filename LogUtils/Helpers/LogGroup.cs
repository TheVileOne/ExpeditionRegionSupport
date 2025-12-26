using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                string subFolderPath = currentFolderPath.Substring(currentPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return Path.Combine(newPath, subFolderPath);
            }
        }

        public static void DeleteFolder(LogGroupID group)
        {
        }

        public static void MoveFolder(LogGroupID group, string newPath)
        {
            //TODO: This code is not fully thread safe. It is vulnerable to new files being added to the group folder after entries are processed.
            //It is also vulnerable to the paths being retargeted before locks have been applied.
            UtilityLogger.Log("Attempting to move folder");
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            if (PathUtils.IsEmpty(newPath))
                throw new ArgumentNullException(nameof(newPath), "Path cannot be null, or empty");

            if (PathUtils.IsFilePath(newPath))
                throw new ArgumentException("Path cannot contain a filename");

            Lock groupLock = group.Properties.GetLock();

            using (groupLock.Acquire())
            {
                throwOnValidationFailed(group, FolderPermissions.Move);

                string currentPath = group.Properties.CurrentFolderPath;
                if (PathUtils.PathsAreEqual(currentPath, newPath))
                {
                    UtilityLogger.Log("No move necessary");
                    return;
                }

                using (var scope = group.Properties.DemandFolderAccess())
                {
                    IReadOnlyCollection<LogGroupProperties> groupsSharingThisFolder = scope.Items;

                    IEnumerable<LogGroupProperties> allOtherGroups =
                        LogProperties.PropertyManager.GroupProperties
                        .Except(groupsSharingThisFolder);

                    //All known log file entries located within the group path
                    IEnumerable<LogID> containedLogFiles = groupsSharingThisFolder
                        .SelectMany(group => group.GetFolderMembers()) //Members targeting a folder, or subfolder of the group path
                        .Union(allOtherGroups
                               .GetMembers()               //Members belonging to each of the other groups
                               .Concat(LogID.GetEntries()) //Individual log files (may or may not belong to a group)
                               .FindAll(p => PathUtils.ContainsOtherPath(p.CurrentFolderPath, currentPath))
                              );

                    foreach (LogGroupProperties entry in groupsSharingThisFolder)
                    {
                        //Make sure that all groups that are assigned to this folder allow it to be moved
                        DemandPermission((LogGroupID)entry.ID, FolderPermissions.Move);
                    }
                    MoveFolder(containedLogFiles.ToArray(), currentPath, newPath);

                    foreach (LogGroupProperties entry in groupsSharingThisFolder)
                    {
                        //Members were handled in the helper method above
                        ChangePath((LogGroupID)entry.ID, currentPath, newPath);
                    }
                }
            }
        }

        /// <summary>
        /// Process for moving a directory containing log files - assumes folder path is valid, and log files are located within the folder
        /// </summary>
        internal static void MoveFolder(IEnumerable<LogID> logFilesInFolder, string currentPath, string newPath)
        {
            ThreadSafeWorker worker = new ThreadSafeWorker(logFilesInFolder.GetLocks());

            worker.DoWork(() =>
            {
                bool moveCompleted = false;

                List<MessageBuffer> activeBuffers = new List<MessageBuffer>();
                List<StreamResumer> streamsToResume = new List<StreamResumer>();
                try
                {
                    UtilityCore.RequestHandler.BeginCriticalSection();
                    foreach (LogID logFile in logFilesInFolder)
                    {
                        MessageBuffer writeBuffer = logFile.Properties.WriteBuffer;

                        writeBuffer.SetState(true, BufferContext.CriticalArea);
                        activeBuffers.Add(writeBuffer);

                        logFile.Properties.FileLock.SetActivity(FileAction.Move); //Lock activated by ThreadSafeWorker
                        logFile.Properties.NotifyPendingMove(newPath);

                        //The move operation requires that all persistent file activity be closed until move is complete
                        streamsToResume.AddRange(logFile.Properties.PersistentStreamHandles.InterruptAll());
                    }
                    Directory.Move(currentPath, newPath);
                    moveCompleted = true;

                    ChangePath(logFilesInFolder, currentPath, newPath);
                }
                finally
                {
                    if (!moveCompleted)
                    {
                        foreach (LogID logFile in logFilesInFolder)
                            logFile.Properties.NotifyPendingMoveAborted();
                    }

                    //Reopen the streams
                    streamsToResume.ResumeAll();
                    activeBuffers.ForEach(buffer => buffer.SetState(false, BufferContext.CriticalArea));
                    UtilityCore.RequestHandler.EndCriticalSection();
                }
            });
        }

        public static void MoveFiles(LogGroupID group, string newPath)
        {
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

        private static void throwOnValidationFailed(LogGroupID group, FolderPermissions validationContext)
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

        public static IEnumerable<LogGroupID> GroupsSharingThisPath(string path)
        {
            LogGroupID groupID = LogGroupID.Factory.CreateID("LogUtils", path);
            return groupID.Properties.AllGroupsSharingMyFolder().GetIDs();
        }
    }
}
