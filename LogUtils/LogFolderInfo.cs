using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using FileMoveRecord = (System.IO.FileSystemInfo Info, LogUtils.Enums.LogID LogID);
using System.Security;

namespace LogUtils
{
    /// <summary>
    /// Collects info about log groups and log files, and provides basic folder operation support
    /// </summary>
    public sealed class LogFolderInfo
    {
        private string folderPathCache;

        /// <summary>
        /// A fully qualified path that contains log groups or files
        /// </summary>
        public string FolderPath
        {
            get
            {
                if (folderPathCache == null)
                    folderPathCache = FolderPathInfo.FullName;
                return folderPathCache;
            }
        }

        /// <summary>
        /// Checks that folder path exists
        /// </summary>
        public bool Exists => FolderPathInfo.Exists;

        internal DirectoryInfo FolderPathInfo;

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

        /// <summary>
        /// Creates an object for manipulating folders containing log files
        /// </summary>
        /// <param name="folderPath">A path (absolute, relative, or partial) to a folder to access information from</param>
        /// <exception cref="SecurityException">Insufficient access to specified path</exception>
        /// <exception cref="PathTooLongException">Path exceeded allowed maximum character length</exception>
        public LogFolderInfo(string folderPath)
        {
            FolderPathInfo = new DirectoryInfo(LogProperties.GetContainingPath(folderPath));
        }

        /// <summary>
        /// Creates an object for manipulating folders containing log files
        /// </summary>
        /// <param name="folderInfo">A directory object pointing to a desired folder path</param>
        /// <exception cref="ArgumentNullException">Folder info was a null value</exception>
        /// <exception cref="SecurityException">Insufficient access to specified path</exception>
        /// <exception cref="PathTooLongException">Path exceeded allowed maximum character length</exception>
        public LogFolderInfo(DirectoryInfo folderInfo)
        {
            if (folderInfo == null)
                throw new ArgumentNullException(nameof(folderInfo));
            FolderPathInfo = folderInfo;
        }

        private LogFolderInfo(string folderPath, LogFolderInfo parentInfo) : this(folderPath)
        {
            if (!PathUtils.ContainsOtherPath(FolderPath, parentInfo.FolderPath))
                throw new ArgumentException("Path must be a subdirectory");

            RefreshInfoInternal(parentInfo.Groups.GetProperties(), parentInfo.FilesNotFromFolderGroups);
        }

        private LogFolderInfo(DirectoryInfo folderInfo, LogFolderInfo parentInfo) : this(folderInfo)
        {
            if (!PathUtils.ContainsOtherPath(FolderPath, parentInfo.FolderPath))
                throw new ArgumentException("Path must be a subdirectory");

            RefreshInfoInternal(parentInfo.Groups.GetProperties(), parentInfo.FilesNotFromFolderGroups);
        }

        public void RefreshInfo()
        {
            RefreshInfoInternal(LogProperties.PropertyManager.GroupProperties, LogID.GetEntries());
        }

        internal void RefreshInfoInternal(IEnumerable<LogGroupProperties> searchGroups, IEnumerable<LogID> searchFiles)
        {
            folderPathCache = null;
            Groups = createCollection(LogGroup.GroupsSharingThisPath(FolderPath, searchGroups).ToList());

            IEnumerable<LogGroupProperties> allOtherGroups = searchGroups
                             .Except(Groups.GetProperties());

            var nonGroupMembers = LogGroup.NonGroupMembersSharingThisPath(FolderPath, searchFiles)
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
        /// Gets a slice of folder information that applies to a specified subfolder
        /// </summary>
        /// <exception cref="ArgumentException">Folder path provided is not a part of the current folder path</exception>
        /// <exception cref="SecurityException">Insufficient access to specified path</exception>
        /// <exception cref="PathTooLongException">Path exceeded allowed maximum character length</exception>
        public LogFolderInfo GetSubFolderInfo(string folderPath)
        {
            return new LogFolderInfo(folderPath, this);
        }

        /// <summary>
        /// Gets a slice of folder information that applies to a specified subfolder
        /// </summary>
        /// <exception cref="ArgumentException">Folder path provided is not a part of the current folder path</exception>
        /// <exception cref="ArgumentNullException">Folder info is a null value</exception>
        /// <exception cref="SecurityException">Insufficient access to specified path</exception>
        public LogFolderInfo GetSubFolderInfo(DirectoryInfo folderInfo)
        {
            return new LogFolderInfo(folderInfo, this);
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
                moveFilesToPath(newPath);
                return;
            }

            bool canMoveFolderToPath = !Directory.Exists(newPath);

            if (canMoveFolderToPath)
            {
                moveFolderToPath(newPath);
            }
            else
            {
                mergeFolder(newPath);
            }
        }

        private void moveFilesToPath(string newPath)
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

        private void moveFolderToPath(string newPath)
        {
            using (var scope = demandAllLocks())
            {
                bool moveCompleted = false;

                List<MessageBuffer> activeBuffers = new List<MessageBuffer>();
                List<StreamResumer> streamsToResume = new List<StreamResumer>();
                try
                {
                    int expectedFileMoves = AllFiles.Count;
                    int currentFile = 0;
                    UtilityLogger.Log($"Moving {expectedFileMoves} files");

                    UtilityCore.RequestHandler.BeginCriticalSection();
                    while (currentFile != expectedFileMoves)
                    {
                        LogID logFile = AllFiles[currentFile];

                        logFile.Properties.EnsureStartupHasRun();

                        MessageBuffer writeBuffer = logFile.Properties.WriteBuffer;

                        writeBuffer.SetState(true, BufferContext.CriticalArea);
                        activeBuffers.Add(writeBuffer);

                        logFile.Properties.FileLock.SetActivity(FileAction.Move); //Lock activated by ThreadSafeWorker
                        logFile.Properties.NotifyPendingMove(LogProperties.GetNewBasePath(logFile, FolderPath, newPath));

                        //The move operation requires that all persistent file activity be closed until move is complete
                        streamsToResume.AddRange(logFile.Properties.PersistentStreamHandles.InterruptAll());
                        currentFile++;
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

        private void mergeFolder(string newPath)
        {
            //TODO: Group paths are not updated
            MergeRecord record;
            mergeFolderRecursive(null, newPath, record = new MergeRecord());

            if (record.HasFailed)
            {
                UtilityLogger.LogError("Failed to merge folder", record.Exception);
                cancelMerge(newPath, record);
                return;
            }
            //Handle collisions here
            collectFeedbackFromUser(record);
        }

        private void cancelMerge(string mergePath, MergeRecord record)
        {
            foreach (FileMoveRecord moveRecord in record.FilesMoved)
            {
                if (moveRecord.LogID != null)
                {
                    LogFile.Move(moveRecord.LogID, moveRecord.LogID.Properties.LastKnownFilePath);
                    continue;
                }
                FileUtils.TryMove(moveRecord.Info.FullName, Path.Combine(FolderPath, moveRecord.Info.Name));
            }

            foreach (FileMoveRecord moveRecord in record.FoldersMoved)
            {
                //Slugg, but what if this folder had a collision problem???
                if (record.FolderConflicts.Contains(moveRecord))
                {
                    //var filesWithResolvedConflicts = record.FileConflicts.Where(r =>
                    //            PathUtils.ContainsOtherPath(PathUtils.PathWithoutFilename(r.Info.FullName), moveRecord.Info.FullName));
                    var filesToRestore = record.FilesMoved.Where(r =>
                                PathUtils.ContainsOtherPath(PathUtils.PathWithoutFilename(r.Info.FullName), moveRecord.Info.FullName));

                    //TODO: Introduce handle behavior for conflicted files (probably through a temp path)

                }

                //FileUtils.TryMove(moveRecord.Info.FullName, Path.Combine(FolderPath, moveRecord.Info.Name));
            }
        }

        private void mergeFolderRecursive(DirectoryInfo source, string destinationPath, MergeRecord record)
        {
            try
            {
                DirectoryInfo destination = new DirectoryInfo(destinationPath);
                if (source == null) //The toplevel directory is handled differently than the rest
                {
                    source = new DirectoryInfo(FolderPath);
                }
                else
                {
                    if (destination.Exists) //Collisions should not qualify as errors
                    {
                        record.FolderConflicts.Add(new FileMoveRecord(destination, null));
                        return;
                    }
                    source.MoveTo(destination.FullName);
                    record.FoldersMoved.Add(new FileMoveRecord(source, null));
                }
                mergeFileHelper(source, destination, record);

                if (record.HasFailed)
                    return;

                foreach (DirectoryInfo subFolder in source.GetDirectories())
                {
                    string newSubFolderPath = Path.Combine(destinationPath, subFolder.Name);
                    mergeFolderRecursive(subFolder, newSubFolderPath, record);
                }
            }
            catch (Exception ex) //Exceptional states will be handled by the caller
            {
                record.HasFailed = true;
                record.Exception = ex;
            }
        }

        private void mergeFileHelper(DirectoryInfo source, DirectoryInfo destination, MergeRecord record)
        {
            bool fileMoveError = false;

            //The folder at the destination should exist at this point
            foreach (FileInfo file in source.GetFiles())
            {
                LogID logFile = AllFiles.Find(file.Name, file.DirectoryName);

                string fileDestination = Path.Combine(destination.FullName, file.Name);

                if (File.Exists(fileDestination)) //Collisions should not qualify as errors
                {
                    record.FileConflicts.Add(new FileMoveRecord(file, logFile));
                    continue;
                }

                fileMoveError = false;
                if (logFile != null)
                {
                    FileStatus status = LogFile.Move(logFile, fileDestination); //File must necessarily exist here - a path only change wouldn't apply
                    if (status != FileStatus.MoveComplete)
                    {
                        UtilityLogger.DebugLog($"Encountered status code [{status}]");
                        fileMoveError = true;
                    }
                }
                else //This is an unrecognized file being moved
                {
                    fileMoveError = !FileUtils.TryMove(file.FullName, fileDestination);
                }

                if (fileMoveError) break;//There was an issue processing a log file - abort process

                record.FilesMoved.Add(new FileMoveRecord(new FileInfo(fileDestination), logFile));
            }

            if (fileMoveError)
                record.HasFailed = true;
        }

        private CollisionResolutionFeedback[] collectFeedbackFromUser(MergeRecord record)
        {
            CollisionResolutionFeedback[] feedback = new CollisionResolutionFeedback[record.FileConflicts.Count];
            for (int i = 0; i < feedback.Length; i++)
            {
                feedback[i] = askUserForFeedback(record.FileConflicts[i]);
            }

            bool hasUnresolvedConflicts = true;
            while (hasUnresolvedConflicts)
            {
                hasUnresolvedConflicts = false;
                for (int i = 0; i < feedback.Length; i++)
                {
                    if (feedback[i] == CollisionResolutionFeedback.SaveForLater)
                    {
                        feedback[i] = askUserForFeedback(record.FileConflicts[i]);

                        if (feedback[i] == CollisionResolutionFeedback.SaveForLater)
                            hasUnresolvedConflicts = true;
                    }
                }
            }
            return feedback;
            CollisionResolutionFeedback askUserForFeedback(FileMoveRecord conflict)
            {
                //TODO: Create feedback dialog
                return CollisionResolutionFeedback.KeepBoth;
            }
        }

        internal IEnumerable<KeyValuePair<string, LogID>> GroupByFolder(ICollection<LogID> logFiles)
        {
            string[] files = Directory.GetFiles(FolderPath);
            return files.Select(f =>
            {
                string path = PathUtils.PathWithoutFilename(f, out string filename);
                return new KeyValuePair<string, LogID>(f, logFiles.Find(filename, path));
            });
        }

        internal void UpdateAllPaths(string newPath)
        {
            string currentPath = FolderPath;
            foreach (LogGroupID groupEntry in Groups)
                LogGroup.ChangePath(groupEntry, currentPath, newPath, applyToMembers: true);

            LogGroup.ChangePath(FilesNotFromFolderGroups, currentPath, newPath);
            FolderPathInfo = new DirectoryInfo(newPath); //Info cache will be stale and should be refreshed before next access
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

        private class MergeRecord
        {
            public List<FileMoveRecord> FilesMoved,
                                        FoldersMoved,
                                        FileConflicts,
                                        FolderConflicts;
            public bool HasFailed;
            public Exception Exception;

            public MergeRecord()
            {
                FilesMoved = new List<FileMoveRecord>();
                FoldersMoved = new List<FileMoveRecord>();
                FileConflicts = new List<FileMoveRecord>();
                FolderConflicts = new List<FileMoveRecord>();
            }
        }
    }

    public enum CollisionResolutionFeedback
    {
        /// <summary>
        /// The file at destination will be replaced
        /// </summary>
        Overwrite,
        /// <summary>
        /// The new filename will be renamed to avoid filename collisions
        /// </summary>
        KeepBoth,
        /// <summary>
        /// File move will not be permitted
        /// </summary>
        CancelMove,
        /// <summary>
        /// Result will be ignored, and asked again at the end
        /// </summary>
        SaveForLater,
    }
}
