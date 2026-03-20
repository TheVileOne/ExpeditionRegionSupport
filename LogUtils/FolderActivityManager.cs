using LogUtils.Events;
using LogUtils.Helpers.FileHandling;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LogUtils
{
    internal class FolderActivityManager
    {
        private readonly ThreadLocal<List<ActivityRecord>> _activity = new ThreadLocal<List<ActivityRecord>>(true);

        internal IEnumerable<ActivityRecord> ActiveMoves => _activity.Values.SelectMany(list => list);
        internal IEnumerable<ActivityRecord> ActiveMovesThisThread => _activity.Value;

        internal ThreadSafeEvent<FolderActivityManager, ActivityRecord> OnRecordAdded = new ThreadSafeEvent<FolderActivityManager, ActivityRecord>();
        internal ThreadSafeEvent<FolderActivityManager, ActivityRecord> OnRecordRemoved = new ThreadSafeEvent<FolderActivityManager, ActivityRecord>();

        public FolderActivityManager()
        {
            _activity.Value = new List<ActivityRecord>();
        }

        public ActivityRecord AddRecord(string sourcePath, string destinationPath)
        {
            ActivityRecord record;
            _activity.Value.Add(record = new ActivityRecord()
            {
                SourcePath = sourcePath,
                DestinationPath = destinationPath
            });
            OnRecordAdded.Raise(this, record);
            return record;
        }

        public bool RemoveRecordAnyThread(ActivityRecord record)
        {
            bool removed = false;
            foreach (var entries in _activity.Values)
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
