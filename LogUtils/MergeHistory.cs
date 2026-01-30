using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using System;
using System.Collections.Generic;
using System.IO;

namespace LogUtils
{
    public sealed class MergeHistory
    {
        internal Queue<MergeRecord> Entries = new Queue<MergeRecord>();

        /// <summary>
        /// A queue containing merge conflicts that require input from the user to resolve
        /// </summary>
        public Queue<MergeRecord> Conflicts = new Queue<MergeRecord>();

        /// <summary>
        /// Handles all merge conflicts
        /// </summary>
        internal ConflictResolutionHandler ConflictHandler;

        /// <summary>
        /// Indicates whether merge process ran to completion
        /// </summary>
        public bool HasFailed;

        /// <summary>
        /// Inclues the exception resulting in a failed merge execution. Not all exceptions are able to be exposed through this field yet.
        /// </summary>
        public Exception Exception;

        public MergeHistory()
        {
            ConflictHandler = new ConflictResolutionHandler(Conflicts);
        }

        public void AddRecord(MergeRecord record)
        {
            Entries.Enqueue(record);
        }

        public void AddRecords(IEnumerable<MergeRecord> records)
        {
            foreach (MergeRecord record in records)
                Entries.Enqueue(record);
        }

        public void ResolveConflicts()
        {
            //Handle unresolved file conflicts
            ConflictResolutionHandler handler = ConflictHandler;
            try
            {
                handler.CollectFeedbackAndResolve();
            }
            catch (OperationCanceledException ex) //User chose to cancel merge, or there was a failure to resolve
            {
                HasFailed = true;
                Exception = ex;
            }
            finally
            {
                while (handler.ResolvedEntries.Count > 0)
                {
                    MergeRecord current = handler.ResolvedEntries.Dequeue();

                    if (!current.IsCanceled)
                        AddRecord(current);
                }
            }
        }

        /// <summary>
        /// Restores all merged files, and folders back to their original states
        /// </summary>
        public void Restore()
        {
            while (Entries.Count > 0)
            {
                MergeRecord current = Entries.Dequeue();

                try
                {
                    current.Restore();
                }
                catch (Exception ex)
                {
                    //An exception thrown here will be very rare. Hopefully it wont cause any weird state issues with mods.
                    UtilityLogger.LogError("Failed to restore back to initial state", ex);
                }
            }
        }
    }

    /// <summary>
    /// Contains state required to reverse a folder merge operation
    /// </summary>
    public abstract class MergeRecord
    {
        /// <summary>
        /// Indicates that operation was never completed
        /// </summary>
        public bool IsCanceled;

        /// <summary>
        /// The path of a file, or folder which has been moved
        /// </summary>
        public string CurrentPath;

        /// <summary>
        /// The original path of a file, or folder which has been moved
        /// </summary>
        public string OriginalPath;

        /// <summary>
        /// Contains logic for restoring a file, or folder back to its original path
        /// </summary>
        public abstract void Restore();
    }

    internal class FolderMoveRecord : MergeRecord
    {
        /// <summary>
        /// All groups affected by a folder move
        /// </summary>
        public Stack<MergeRecord> GroupRecords = new Stack<MergeRecord>();

        /// <summary>
        /// All files affected by a folder move
        /// </summary>
        public Stack<MergeRecord> FileRecords = new Stack<MergeRecord>();

        /// <summary>
        /// Determines how the current folder should be restored to the original path
        /// </summary>
        public FolderRestoreMode RestoreMode;

        public MergeRecord AddRecord(LogID logFile, bool canHandle)
        {
            MergeRecord record = MergeRecordFactory.Create(logFile, canHandle);

            FileRecords.Push(record);
            return record;
        }

        public MergeRecord AddRecord(LogGroupID logGroup)
        {
            MergeRecord record = MergeRecordFactory.Create(logGroup);

            GroupRecords.Push(record);
            return record;
        }

        public override void Restore()
        {
            if (Directory.Exists(CurrentPath))
            {
                if (RestoreMode == FolderRestoreMode.EntireFolder)
                {
                    Directory.Move(CurrentPath, OriginalPath);
                }
                else if (RestoreMode == FolderRestoreMode.FilesOnly)
                {
                    //I have no logic for this particular case yet
                }
            }

            while (GroupRecords.Count > 0)
            {
                MergeRecord record = GroupRecords.Pop();
                record.Restore();
            }

            while (FileRecords.Count > 0)
            {
                MergeRecord record = FileRecords.Pop();
                record.Restore();
            }
        }
    }

    internal class LogGroupMoveRecord : MergeRecord
    {
        /// <summary>
        /// Field identifies a changed log group
        /// </summary>
        public LogGroupID Target;

        public override void Restore()
        {
            Target.Properties.ChangePath(OriginalPath, applyToMembers: false);
        }
    }

    internal class FileMoveRecord : MergeRecord
    {
        /// <summary>
        /// Optional field that identifies path as a log file
        /// </summary>
        public LogID Target;

        private bool _handleFile = true;

        /// <summary>
        /// Defines whether this record applies to an actual file, or only a targeted path
        /// </summary>
        public bool CanHandleFile
        {
            get => _handleFile && File.Exists(CurrentPath);
            set => _handleFile = value;
        }

        public override void Restore()
        {
            AttemptRestore();
        }

        internal bool AttemptRestore()
        {
            //How do we know this is the actual log file?
            if (CanHandleFile && !FileUtils.TryMove(CurrentPath, OriginalPath))
                return false;
            Target?.Properties.ChangePath(OriginalPath);
            return true;
        }
    }

    internal class FileReplaceRecord : FileMoveRecord
    {
        public override void Restore()
        {
            string replacedFilePath = TempFolder.MapPathToFolder(CurrentPath);

            bool fileRestored = AttemptRestore() && FileUtils.TryMove(replacedFilePath, CurrentPath);
            if (!fileRestored)
            {
                UtilityLogger.LogWarning("Restore operation could not be completed. File is now orphaned.");
                TempFolder.OrphanedFiles.Add(replacedFilePath);
            }
        }
    }

    internal enum FolderRestoreMode
    {
        /// <summary>
        /// Restoration will involve moving individual files to their original location
        /// </summary>
        FilesOnly,
        /// <summary>
        /// Restoration will involve moving folder back to its original location
        /// </summary>
        EntireFolder,
    }
}
