using LogUtils.Events;
using LogUtils.Helpers.FileHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static LogUtils.FolderActivityManager;

namespace LogUtils
{
    internal class FolderActivityManager
    {
        private readonly ThreadLocal<List<ActivityRecord>> _activeRecords = new ThreadLocal<List<ActivityRecord>>(true);
        private readonly ThreadLocal<List<ActivityRecord>> _inactiveRecords = new ThreadLocal<List<ActivityRecord>>(true);

        internal IEnumerable<ActivityRecord> ActiveMoves => _activeRecords.Values.SelectMany(list => list);
        internal IEnumerable<ActivityRecord> ActiveMovesThisThread => _activeRecords.Value;
        internal IEnumerable<ActivityRecord> InactiveMoves => _inactiveRecords.Values.SelectMany(list => list);
        internal IEnumerable<ActivityRecord> InactiveMovesThisThread => _inactiveRecords.Value;

        internal ThreadSafeEvent<FolderActivityManager, ActivityRecord> OnRecordAdded = new ThreadSafeEvent<FolderActivityManager, ActivityRecord>();
        internal ThreadSafeEvent<FolderActivityManager, ActivityRecord> OnRecordRemoved = new ThreadSafeEvent<FolderActivityManager, ActivityRecord>();

        public void AddRecord(ActivityRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            record.State = ActivityState.Started;

            if (!_activeRecords.IsValueCreated)
                _activeRecords.Value = new List<ActivityRecord>();

            _activeRecords.Value.Add(record);
            OnRecordAdded.Raise(this, record);
        }

        /// <summary>
        /// Removes an active record associated with any thread
        /// </summary>
        public bool RemoveRecordAnyThread(ActivityRecord record)
        {
            bool removed = false;
            foreach (var entries in _activeRecords.Values)
            {
                removed = entries.Remove(record);
                if (removed)
                {
                    OnRecordRemoved.Raise(this, record);
                    break;
                }
            }
            return removed;
        }

        /// <summary>
        /// Changes an active record to an inactive record
        /// </summary>
        public void Deactivate(ActivityRecord record)
        {
            if (!RemoveRecordAnyThread(record))
                UtilityLogger.LogWarning("Expected active move record");

            if (!_inactiveRecords.IsValueCreated)
                _inactiveRecords.Value = new List<ActivityRecord>();

            _inactiveRecords.Value.Add(record);
        }


        /// <summary>
        /// Removes an inactive record associated with any thread
        /// </summary>
        public bool RemoveInactiveAnyThread(ActivityRecord record)
        {
            bool removed = false;
            foreach (var entries in _inactiveRecords.Values)
            {
                removed = entries.Remove(record);
                if (removed)
                {
                    break;
                }
            }
            return removed;
        }

        public ActivityRecord GetRecord(MergeHistory history)
        {
            var allRecords = ActiveMoves.Concat(InactiveMoves);
            return allRecords.FirstOrDefault(entry => entry.MergeHistory == history);
        }

        public ActivityRecord[] GetPendingMergeRecords(string path)
        {
            return InactiveMoves.Except(ActiveMovesThisThread)
                                .GetMatches(path)
                                .Where(entry => entry.MergeHistory != null)
                                .ToArray();
        }

        public ActivityRecord[] GetProblematicRecords(string path)
        {
            return ActiveMoves.Except(ActiveMovesThisThread)
                              .GetMatches(path)
                              .ToArray();
        }

        public record ActivityRecord
        {
            private ActivityState _state = ActivityState.NotStarted;
            public ActivityState State
            {
                get => _state;
                set
                {
                    //Not thread safe, but probably good enough
                    if (value < _state) return; //Record state cannot be set to an earlier state
                    _state = value;
                }
            }

            public MergeHistory MergeHistory;
            public MergeEventHandler Events;

            public string SourcePath;
            public string DestinationPath;
        }

        public enum ActivityState
        {
            NotStarted = 0,
            Started = 1,
            AcquiringLocks = 2,
            VerifyingFolderState = 3,
            VerificationCompleted = 4, //Ready to move
            WaitingForConflictResolution = 5,
            Faulted = 6, //Failed, or canceled
            Completed = 7
        }

        public enum ActivityContext
        {
            None,
            Merge,
        }
    }

    public static partial class ExtensionMethods
    {
        internal static IEnumerable<ActivityRecord> GetMatches(this IEnumerable<ActivityRecord> self, string path)
        {
            return self.Where(matchPredicate);

            bool matchPredicate(ActivityRecord record)
            {
                bool isSourceMatch, isDestinationMatch;

                isSourceMatch =
                    !PathUtils.IsEmpty(record.SourcePath)
                         && (PathUtils.ContainsOtherPath(record.SourcePath, path) || PathUtils.ContainsOtherPath(path, record.SourcePath));

                isDestinationMatch =
                    !PathUtils.IsEmpty(record.DestinationPath)
                         && (PathUtils.ContainsOtherPath(record.DestinationPath, path) || PathUtils.ContainsOtherPath(path, record.DestinationPath));
                return isSourceMatch || isDestinationMatch;
            }
        }
    }
}
