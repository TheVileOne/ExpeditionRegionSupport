using LogUtils.Helpers.FileHandling;
using System;
using System.Collections.Generic;

namespace LogUtils
{
    internal class ConflictResolutionHandler
    {
        private readonly List<MergeRecord> conflicts;
        private readonly List<ConflictResolutionFeedback> feedback;

        private int skipIndex = -1;
        public Queue<MergeRecord> ResolvedEntries = new Queue<MergeRecord>();

        public ConflictResolutionHandler()
        {
            conflicts = new List<MergeRecord>();
            feedback = new List<ConflictResolutionFeedback>();
        }

        public void CollectFeedback(ConflictResolutionFeedback feedback, MergeRecord conflict)
        {
            UtilityLogger.Log("Received feedback");
            if (skipIndex != -1)
            {
                this.feedback[skipIndex] = feedback;
                this.conflicts[skipIndex] = conflict;
                return;
            }

            this.feedback.Add(feedback);
            this.conflicts.Add(conflict);
        }

        /// <summary>
        /// Returns an enumerator containing all conflicts the user decided to handler later
        /// </summary>
        public IEnumerator<MergeRecord> GetSkippedConflicts()
        {
            for (int i = 0; i < feedback.Count; i++)
            {
                if (feedback[i] == ConflictResolutionFeedback.SaveForLater)
                {
                    skipIndex = i;
                    yield return conflicts[i];
                }
            }
            yield break;
        }

        public void ResolveAll()
        {
            IAccessToken access = TempFolder.Access();
            int conflictsHandled = 0;
            try
            {
                for (int i = 0; i < conflicts.Count; i++)
                {
                    //These collections have the same amount of entries
                    var currentRecord = conflicts[i];
                    var currentFeedback = feedback[i];

                    bool isResolved = false;
                    switch (currentFeedback)
                    {
                        case ConflictResolutionFeedback.CancelMove:
                            isResolved = true;
                            currentRecord.IsCanceled = true;
                            break;
                        case ConflictResolutionFeedback.Overwrite:
                            isResolved = FileUtils.TryReplace(currentRecord.OriginalPath, currentRecord.CurrentPath);

                            if (isResolved)
                            {
                                FileReplaceRecord replaceRecord = new FileReplaceRecord
                                {
                                    OriginalPath = currentRecord.OriginalPath,
                                    CurrentPath = currentRecord.CurrentPath
                                };

                                if (currentRecord is FileMoveRecord moveRecord && moveRecord.Target != null)
                                {
                                    //Transfer log file and update path to reflect the new current path
                                    replaceRecord.Target = moveRecord.Target;
                                    replaceRecord.Target.Properties.ChangePath(replaceRecord.CurrentPath);
                                }
                                currentRecord = replaceRecord;
                            }
                            break;
                        case ConflictResolutionFeedback.KeepBoth:
                            isResolved = FileUtils.TryMove(currentRecord.OriginalPath, currentRecord.CurrentPath, FileMoveOption.RenameSourceIfNecessary, out string newCurrentPath);

                            if (isResolved)
                                currentRecord.CurrentPath = newCurrentPath;
                            break;
                        case ConflictResolutionFeedback.SaveForLater:
                            throw new InvalidOperationException("Conflict resolution state is not valid.");
                    }

                    if (!isResolved)
                        throw new OperationCanceledException("Unable to resolve conflict");

                    ResolvedEntries.Enqueue(currentRecord);
                    conflictsHandled = i;
                }
            }
            finally
            {
                access.Dispose();
                conflicts.RemoveRange(0, conflictsHandled);
                feedback.RemoveRange(0, conflictsHandled);
            }
        }
    }

    public enum ConflictResolutionFeedback
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
