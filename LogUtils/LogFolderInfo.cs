using LogUtils.Enums;
using LogUtils.Enums.FileSystem;
using LogUtils.Helpers;
using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;

namespace LogUtils
{
    /// <summary>
    /// Collects info about log groups and log files, and provides basic folder operation support
    /// </summary>
    public sealed class LogFolderInfo
    {
        private bool hasInitialized;
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
            hasInitialized = true;
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
            hasInitialized = true;
        }

        private LogFolderInfo(string folderPath, LogFolderInfo parentInfo) : this(folderPath)
        {
            hasInitialized = false;
            if (!PathUtils.ContainsOtherPath(FolderPath, parentInfo.FolderPath))
                throw new ArgumentException("Path must be a subdirectory");

            RefreshInfoInternal(parentInfo.Groups.GetProperties(), parentInfo.FilesNotFromFolderGroups);
            hasInitialized = true;
        }

        private LogFolderInfo(DirectoryInfo folderInfo, LogFolderInfo parentInfo) : this(folderInfo)
        {
            hasInitialized = false;
            if (!PathUtils.ContainsOtherPath(FolderPath, parentInfo.FolderPath))
                throw new ArgumentException("Path must be a subdirectory");

            RefreshInfoInternal(parentInfo.Groups.GetProperties(), parentInfo.FilesNotFromFolderGroups);
            hasInitialized = true;
        }

        public void RefreshInfo()
        {
            RefreshInfoInternal(LogProperties.PropertyManager.GroupProperties, LogID.GetEntries());
        }

        internal void RefreshInfoInternal(IEnumerable<LogGroupProperties> searchGroups, IEnumerable<LogID> searchFiles)
        {
            if (hasInitialized)
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

            //TODO: Determine if there is a decent way to keep file paths in sync across multiple Rain World processes
            if (!UtilityCore.IsControllingAssembly)
                return;

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
            newPath = LogProperties.GetContainingPath(newPath);

            using (var scope = demandAllLocks())
            {
                bool canMoveFiles = Exists;

                if (!canMoveFiles)
                {
                    changePathOfNonExistingFilesAndFolders(newPath);
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
        }

        private void moveFolderToPath(string newPath)
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

        private void mergeFolder(string newPath)
        {
            MergeFolderState mergeInfo = new MergeFolderState
            {
                CurrentSource = new DirectoryInfo(FolderPath),
                DestinationPath = newPath,
                History = new MergeHistory()
            };

            mergeCurrentFolder(mergeInfo);

            MergeHistory history = mergeInfo.History;
            if (history.HasFailed)
            {
                onMergeFailed(history);
                return;
            }

            history.ResolveConflicts();
            if (history.HasFailed)
            {
                onMergeFailed(history);
                return;
            }
            UtilityLogger.Log("Merge operation on all files successful");
        }

        /// <summary>
        /// The contents of the current folder path will be merged into a destination path of the same name
        /// </summary>
        private void mergeCurrentFolder(MergeFolderState mergeInfo)
        {
            try
            {
                mergeInfo.CurrentSource = FolderPathInfo;
                mergeInfo.DestinationPath = Path.Combine(mergeInfo.DestinationPath, FolderPathInfo.Name);

                //Any LogID, or LogGroupIDs pointing to non-existing file, or folder paths are handled here
                var records = changePathOfNonExistingFilesAndFolders(mergeInfo.DestinationPath);
                mergeInfo.History.AddRecords(records);

                if (mergeInfo.FolderDepth == 0 || Directory.Exists(mergeInfo.DestinationPath)) //Folder will need be merged with an existing folder at the destination
                {
                    moveFilesDuringMerge(mergeInfo);
                }
                else
                {
                    moveFolderDuringMerge(mergeInfo);
                }

                if (mergeInfo.History.HasFailed)
                    return;

                //Merge current subfolders
                DirectoryInfo[] subFolders = mergeInfo.CurrentSource.GetDirectories();
                foreach (LogFolderInfo subFolderInfo in subFolders.Select(GetSubFolderInfo))
                {
                    subFolderInfo.mergeCurrentFolder(mergeInfo);

                    if (mergeInfo.History.HasFailed)
                        break;
                }
            }
            catch (Exception ex) //Exceptional states will be handled by the caller
            {
                mergeInfo.History.HasFailed = true;
                mergeInfo.History.Exception = ex;
            }
        }

        private void onMergeFailed(MergeHistory history)
        {
            UtilityLogger.LogError("Failed to merge folder", history.Exception);
            cancelMerge(history);
        }

        private void cancelMerge(MergeHistory history)
        {
            history.Restore();
        }

        private IEnumerable<MergeRecord> changePathOfNonExistingFilesAndFolders(string destinationPath)
        {
            if (!Exists)
            {
                //All folders, subfolders, and file paths may be assumed to not exist in this case
                return rebaseAllPaths(destinationPath, renameFolder: true);
            }

            List<LogGroupID> handledGroups = new List<LogGroupID>();

            IEnumerable<MergeRecord> records = [];
            processNonExistingFolderGroups();
            processNonExistingLogFiles();

            void processNonExistingFolderGroups()
            {
                var pathComparer = new LogPathComparer();

                IEnumerable<LogGroupID> groups = Groups.Where(isNonExistingFolderGroup);
                foreach (LogGroupID logGroup in groups)
                {
                    if (handledGroups.Contains(logGroup, pathComparer))
                        continue;

                    LogFolderInfo folderInfo = GetSubFolderInfo(logGroup.Properties.CurrentFolderPath);

                    records.Concat(folderInfo.rebaseAllPaths(destinationPath, renameFolder: false));
                    handledGroups.Add(logGroup);
                }
            }

            void processNonExistingLogFiles()
            {
                IEnumerable<LogID> files = FilesNotFromFolderGroups.Where(isNonExistingLogFile);
                foreach (LogID logFile in files)
                {
                    MergeRecord record = MergeRecordFactory.Create(logFile, canHandle: false);

                    bool locatedInCurrentFolder = logFile.Properties.CurrentFolderPath.Length == FolderPath.Length;

                    if (locatedInCurrentFolder)
                        logFile.Properties.ChangePath(destinationPath);
                    else
                        logFile.Properties.ChangeBasePath(FolderPath, destinationPath);

                    record.CurrentPath = logFile.Properties.CurrentFolderPath;
                    records.Append(record);
                }
            }
            return records;

            bool isNonExistingLogFile(LogID logFile)
            {
                if (logFile.Properties.FileExists) //This field shouldn't be stale in the context of a merge
                    return false;

                bool fileAlreadyHandled = handledGroups.Any(entry => PathUtils.ContainsOtherPath(logFile.Properties.CurrentFolderPath, entry.Properties.CurrentFolderPath));

                if (fileAlreadyHandled)
                    return false;

                //The folder must not be associated with an existing subpath - those cases will be handled with the subfolder is handled
                return !PathUtils.SubPathExists(logFile.Properties.CurrentFolderPath, FolderPath.Length);
            }

            bool isNonExistingFolderGroup(LogGroupID logGroup)
            {
                return !PathUtils.SubPathExists(logGroup.Properties.CurrentFolderPath, FolderPath.Length);
            }
        }

        /// <summary>
        /// Changes the base paths of all groups and files associated with this folder path and any subfolders
        /// </summary>
        private MergeRecord[] rebaseAllPaths(string newBasePath, bool renameFolder)
        {
            if (!renameFolder)
            {
                //The folder path must be included here, or it will be trimmed out of the new path
                newBasePath = Path.Combine(newBasePath, Path.GetFileName(FolderPath));
            }

            int totalRecordsExpected = Groups.Count + AllFiles.Count;

            UtilityLogger.Log($"Expecting {totalRecordsExpected} records");
            MergeRecord[] records = new MergeRecord[totalRecordsExpected];

            int currentIndex = -1;
            foreach (LogGroupID target in Groups)
            {
                MergeRecord record = records[++currentIndex] = MergeRecordFactory.Create(target);

                target.Properties.ChangeBasePath(currentBasePath: FolderPath, newBasePath);
                record.CurrentPath = target.Properties.CurrentFolderPath;
            }

            foreach (LogID target in AllFiles)
            {
                MergeRecord record = records[++currentIndex] = MergeRecordFactory.Create(target, canHandle: false);

                target.Properties.ChangeBasePath(currentBasePath: FolderPath, newBasePath);
                record.CurrentPath = target.Properties.CurrentFolderPath;
            }
            return records;
        }

        private void moveFilesDuringMerge(MergeFolderState mergeInfo)
        {
            bool fileMoveError = false;
            foreach (FileInfo file in mergeInfo.CurrentSource.GetFiles())
            {
                //Search for a LogID with the same name, and path as this file
                LogID logFile = AllFiles.Find(file);

                MergeRecord record;
                if (logFile != null)
                {
                    UtilityLogger.Log("Log file detected");
                    record = MergeRecordFactory.Create(logFile, canHandle: true);
                }
                else
                {
                    record = new FileMoveRecord()
                    {
                        OriginalPath = file.FullName,
                        CanHandleFile = true, //We know it must exist
                    };
                }

                string fileDestination = Path.Combine(mergeInfo.DestinationPath, file.Name);
                bool hasConflict = File.Exists(fileDestination);

                if (hasConflict)
                {
                    bool conflictResolved = tryQuickResolve(logFile, fileDestination);

                    if (!conflictResolved) //Unable to resolve conflict
                    {
                        record.CurrentPath = fileDestination; //Not actual current path, but we need a reference point
                        mergeInfo.History.Conflicts.Enqueue(record);
                        continue;
                    }
                    record.CurrentPath = logFile.Properties.CurrentFilePath;
                }
                else
                {
                    string pathResult = moveFileAndGetPathResult(logFile, fileDestination);

                    if (pathResult == null) //There was an issue processing a log file - abort process
                    {
                        fileMoveError = true;
                        break;
                    }
                    record.CurrentPath = pathResult;

                    string moveFileAndGetPathResult(LogID logFile, string destinationPath)
                    {
                        if (logFile != null)
                        {
                            FileStatus status = LogFile.Move(logFile, destinationPath);

                            if (status == FileStatus.MoveComplete)
                                return logFile.Properties.CurrentFilePath;
                        }
                        else if (FileUtils.TryMove(file.FullName, destinationPath)) //This is an unrecognized file being moved
                        {
                            return destinationPath;
                        }
                        return null;
                    }
                }
                mergeInfo.History.AddRecord(record);
            }

            if (fileMoveError)
                mergeInfo.History.HasFailed = true;
        }

        private bool tryQuickResolve(LogID logFile, string conflictingFilePath)
        {
            if (logFile == null) return false;

            LogID[] results = LogID.FindAll(properties =>
            {
                return ComparerUtils.PathComparer.CompareFilenameAndPath(properties.CurrentFilePath, conflictingFilePath, true) == 0;
            }).ToArray();

            if (results.Length == 0) //Unrecognized file - might be able to resolve conflict by renaming log file, but LogUtils would have to support it
            {
                //Currently not necessary as this method is only used in situations where a file must exist
                //if (!logFile.Properties.FileExists)
                //{
                //    logFile.Properties.ChangePath(conflictingFilePath);
                //    return true;
                //}
                return false;
            }

            LogID destinationLogFile = results[0]; //There shouldn't be more than one result

            if (destinationLogFile.Equals(logFile)) //Unusual situation, but shouldn't cause issues
            {
                UtilityLogger.LogWarning("Attempting to move log file to its own path");
                return true;
            }

            //There appears to be another log file at this destination - it is safe to attempt a move operation
            FileStatus status = LogFile.Move(logFile, conflictingFilePath);
            if (status != FileStatus.MoveComplete)
            {
                UtilityLogger.DebugLog($"File move incomplete [{status}]");
                return false;
            }
            return true;
        }

        private void moveFolderDuringMerge(MergeFolderState mergeInfo)
        {
            //Maintain a record of folder operations performed in order to make it possible to revert changes later 
            FolderMoveRecord record = new FolderMoveRecord
            {
                OriginalPath = FolderPath,
                CurrentPath = mergeInfo.DestinationPath,
                RestoreMode = FolderRestoreMode.EntireFolder, //Indicate that the entire folder has been moved instead of a merge of two folders
            };

            mergeInfo.CurrentSource.MoveTo(mergeInfo.DestinationPath);

            //Groups must have their path updated to reflect the destination path
            foreach (LogGroupID logGroup in Groups)
            {
                MergeRecord logGroupRecord = record.AddRecord(logGroup); //This must be defined before ChangePath is called

                logGroup.Properties.ChangePath(LogProperties.GetNewBasePath(logGroup, record.OriginalPath, record.CurrentPath), applyToMembers: false);
                logGroupRecord.CurrentPath = logGroup.Properties.CurrentFolderPath;
            }

            //Files must have their path updated to reflect the destination path
            foreach (LogID logFile in AllFiles)
            {
                MergeRecord logFileRecord = record.AddRecord(logFile, canHandle: false); //This must be defined before ChangePath is called

                logFile.Properties.ChangePath(LogProperties.GetNewBasePath(logFile, record.OriginalPath, record.CurrentPath));
                logFileRecord.CurrentPath = logFile.Properties.CurrentFilePath;
            }
            mergeInfo.History.AddRecord(record);
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
    }
}
