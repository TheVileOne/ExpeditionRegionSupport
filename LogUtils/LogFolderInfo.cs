using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using LogUtils.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace LogUtils
{
    /// <summary>
    /// Collects info about log groups and log files, and provides basic folder operation support
    /// </summary>
    public sealed class LogFolderInfo
    {
        /// <summary>
        /// A fully qualified path that contains log groups or files
        /// </summary>
        public string FolderPath { get; private set; }

        /// <summary>
        /// Checks that folder path exists
        /// </summary>
        public bool Exists => Directory.Exists(FolderPath);

        /// <summary>
        /// A snapshot of any groups that target this folder path
        /// </summary>
        public ReadOnlyCollection<LogGroupID> Groups { get; private set; }

        /// <summary>
        /// A snapshot of any files that target this folder path (as part of a group, or otherwise)
        /// </summary>
        public ReadOnlyCollection<LogID> AllFiles { get; private set; }

        /// <summary>
        /// A snapshot of any files not found in a folder group that targets this folder path
        /// (includes both non-folder group files, and files from folder groups that do not target this folder path)
        /// </summary>
        public ReadOnlyCollection<LogID> FilesNotFromFolderGroups { get; private set; }

        public LogFolderInfo(string folderPath)
        {
            FolderPath = LogProperties.GetContainingPath(folderPath);
        }

        public void RefreshInfo()
        {
            Groups = createCollection(LogGroup.GroupsSharingThisPath(FolderPath).ToList());

            IEnumerable<LogGroupProperties> allOtherGroups =
                LogProperties.PropertyManager.GroupProperties
                             .Except(Groups.GetProperties());

            var nonGroupMembers = LogGroup.NonGroupMembersSharingThisPath(FolderPath)
                                          .Concat(allOtherGroups.SelectMany(group =>
                                          {
                                              var membersToCheck = group.IsFolderGroup ? group.GetNonConformingMembers() : group.Members;
                                              return membersToCheck.Where(member => PathUtils.ContainsOtherPath(member.Properties.CurrentFolderPath, FolderPath));

                                          }));
            FilesNotFromFolderGroups = createCollection(nonGroupMembers.ToList());
            AllFiles = createCollection(Groups.SelectMany(group => group.Properties.GetFolderMembers())
                                              .Concat(FilesNotFromFolderGroups)
                                              .ToList());
        }

        public bool IsSafeToMove()
        {
            return UtilityCore.IsControllingAssembly && DirectoryUtils.IsSafeToMove(FolderPath);
        }

        /// <summary>
        /// Returns the folder permissions associated with this folder path
        /// </summary>
        public FolderPermissions GetPermissions()
        {
            //Check that we have groups with permission info. Assume there are no permissions before a group has been assigned.
            if (Groups.Count == 0 || !IsSafeToMove())
                return FolderPermissions.None;

            FolderPermissions permissions = FolderPermissions.All; //The permissions that are shared by all groups
            foreach (LogGroupID group in Groups)
                permissions &= group.Properties.FolderPermissions;
            return permissions;
        }

        /// <summary>
        /// Attempts to move this folder and its contents to a new path
        /// </summary>
        /// <param name="newPath">A fully qualified folder path, or path keyword</param>
        /// <param name="checkPermissions">Flag helps enhance safe folder operations. Keep value set to true (Recommended)</param>
        public void Move(string newPath, bool checkPermissions = true)
        {
            RefreshInfo();
            if (checkPermissions)
            {
                FolderPermissions permissions = GetPermissions();
                bool canMove = (permissions & FolderPermissions.Move) != 0;

                if (!canMove)
                    LogGroup.OnPermissionDenied(FolderPath, FolderPermissions.Move);
            }
            MoveInternal(newPath);
        }

        internal void MoveInternal(string newPath)
        {
            if (!UtilityCore.IsControllingAssembly) //TODO: Can we try to predict what the filepaths will be without moving any files/folders?
                return;

            newPath = LogProperties.GetContainingPath(newPath);
            bool canMoveFiles = Exists;

            if (!canMoveFiles)
            {
                MoveFilesToPath(newPath);
                return;
            }

            using (var scope = demandAllLocks())
            {
                bool moveCompleted = false;

                List<MessageBuffer> activeBuffers = new List<MessageBuffer>();
                List<StreamResumer> streamsToResume = new List<StreamResumer>();
                try
                {
                    UtilityCore.RequestHandler.BeginCriticalSection();
                    foreach (LogID logFile in AllFiles)
                    {
                        logFile.Properties.EnsureStartupHasRun();
                        MessageBuffer writeBuffer = logFile.Properties.WriteBuffer;

                        writeBuffer.SetState(true, BufferContext.CriticalArea);
                        activeBuffers.Add(writeBuffer);

                        logFile.Properties.FileLock.SetActivity(FileAction.Move); //Lock activated by ThreadSafeWorker
                        logFile.Properties.NotifyPendingMove(LogProperties.GetNewBasePath(logFile, FolderPath, newPath));

                        //The move operation requires that all persistent file activity be closed until move is complete
                        streamsToResume.AddRange(logFile.Properties.PersistentStreamHandles.InterruptAll());
                    }
                    string currentPath = FolderPath;

                    Directory.Move(currentPath, newPath);
                    moveCompleted = true;

                    UpdateAllPaths(newPath);
                }
                finally
                {
                    if (!moveCompleted)
                    {
                        //TODO: Sanity check the number of aborted entries
                        foreach (LogID logFile in AllFiles)
                            logFile.Properties.NotifyPendingMoveAborted();
                    }

                    //Reopen the streams
                    streamsToResume.ResumeAll();
                    activeBuffers.ForEach(buffer => buffer.SetState(false, BufferContext.CriticalArea));
                    UtilityCore.RequestHandler.EndCriticalSection();
                }
            }
        }

        internal void MoveFilesToPath(string newPath)
        {
            string currentPath = FolderPath;

            ThreadSafeWorker worker = new ThreadSafeWorker(Groups.GetLocks())
            {
                UseEnumerableWrapper = false
            };

            worker.DoWork(() =>
            {
                UpdateAllPaths(newPath);
            });
        }

        internal void UpdateAllPaths(string newPath)
        {
            string currentPath = FolderPath;
            foreach (LogGroupID groupEntry in Groups)
                LogGroup.ChangePath(groupEntry, currentPath, newPath, applyToMembers: true);

            LogGroup.ChangePath(FilesNotFromFolderGroups, currentPath, newPath);
            FolderPath = newPath; //Info cache will be stale and should be refreshed before next access
        }

        /// <summary>
        /// Acquire locks for all groups and files
        /// </summary>
        /// <returns>A lock scope containing the source of each lock object</returns>
        private IScopedCollection<LogID> demandAllLocks()
        {
            CombinationLock<LogID> locks = new CombinationLock<LogID>(Groups.Concat(AllFiles));
            return locks.Acquire();
        }

        private static ReadOnlyCollection<T> createCollection<T>(IList<T> list)
        {
            return new ReadOnlyCollection<T>(list);
        }
    }
}
