using LogUtils.Events;
using LogUtils.Helpers.FileHandling;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        public FolderActivityManager()
        {
            _activeRecords.Value = new List<ActivityRecord>();
            _inactiveRecords.Value = new List<ActivityRecord>();
        }

        public ActivityRecord AddRecord(string sourcePath, string destinationPath)
        {
            ActivityRecord record;
            _activeRecords.Value.Add(record = new ActivityRecord()
            {
                SourcePath = sourcePath,
                DestinationPath = destinationPath
            });
            OnRecordAdded.Raise(this, record);
            return record;
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
        public void SetInactive(ActivityRecord record)
        {
            if (!RemoveRecordAnyThread(record))
                UtilityLogger.LogWarning("Expected active move record");
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

        public ActivityRecord[] GetProblematicRecords(string path)
        {
            return ActiveMoves.Except(ActiveMovesThisThread)
                              .Where(query)
                              .ToArray();
            bool query(ActivityRecord record)
            {
                bool hasProblematicSource, hasProblematicDestination;

                hasProblematicSource =
                    !PathUtils.IsEmpty(record.SourcePath)
                         && (PathUtils.ContainsOtherPath(record.SourcePath, path) || PathUtils.ContainsOtherPath(path, record.SourcePath));

                hasProblematicDestination =
                    !PathUtils.IsEmpty(record.DestinationPath)
                         && (PathUtils.ContainsOtherPath(record.DestinationPath, path) || PathUtils.ContainsOtherPath(path, record.DestinationPath));
                return hasProblematicSource || hasProblematicDestination;
            }
        }

        public record ActivityRecord
        {
            public MergeHistory MergeHistory;

            public string SourcePath;
            public string DestinationPath;
        }

        public enum ActivityContext
        {
            None,
            Merge,
        }
    }
}
