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

        internal IEnumerable<ActivityRecord> ActiveMoves => _activeRecords.Values.SelectMany(list => list);
        internal IEnumerable<ActivityRecord> ActiveMovesThisThread => _activeRecords.Value;

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
        /// Removes an active record
        /// </summary>
        public bool RemoveRecord(ActivityRecord record)
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

        public ActivityRecord GetRecord(MergeHistory history)
        {
            if (history == null)
                return null;

            return ActiveMoves.FirstOrDefault(entry => entry.MergeHistory == history);
        }

        /// <summary>
        /// Gets any merge records merging into, or out of a provided path, or the parent directory containing the path
        /// </summary>
        public ActivityRecord[] GetMergeRecords(string path)
        {
            return ActiveMoves.GetMergeRecords()
                              .GetMatches(path)
                              .ToArray();
        }

        /// <summary>
        /// Searches for any move records that can interfere with the folder state during a move operation
        /// </summary>
        public ActivityRecord[] GetProblematicRecords(string path)
        {
            var problematicMerges
                = ActiveMoves.GetMergeRecords()
                             //Check that merge happened off this thread, or  whether this thread will have priority over it
                             .Where(entry => !ActiveMovesThisThread.Contains(entry) || entry.State == ActivityState.WaitingForConflictResolution);

            var exclusions = ActiveMoves.GetMergeRecords().Concat(ActiveMovesThisThread).Distinct();

            return ActiveMoves.Except(exclusions)                                  //Exclude
                              .Where(entry => entry.State < ActivityState.Faulted)
                              .Concat(problematicMerges)
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

        internal static IEnumerable<ActivityRecord> GetMergeRecords(this IEnumerable<ActivityRecord> self)
        {
            return self.Where(entry => entry.MergeHistory != null);
        }
    }
}
