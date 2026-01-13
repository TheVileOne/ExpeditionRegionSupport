using LogUtils.Helpers.FileHandling;
using System;
using System.Collections.Generic;

namespace LogUtils
{
    internal class ConflictResolutionHandler
    {
        private readonly List<MergeRecord> conflicts;
        private ConflictResolutionFeedback[] feedback;

        public ConflictResolutionHandler(List<MergeRecord> mergeConflicts)
        {
            conflicts = mergeConflicts;
        }

        public void CollectFeedbackFromUser()
        {
            feedback = new ConflictResolutionFeedback[conflicts.Count];

            if (conflicts.Count == 0)
                return;

            for (int i = 0; i < feedback.Length; i++)
                feedback[i] = askUserForFeedback(conflicts[i]);

            bool hasUnresolvedConflicts = true;
            while (hasUnresolvedConflicts)
            {
                hasUnresolvedConflicts = false;
                for (int i = 0; i < feedback.Length; i++)
                {
                    if (feedback[i] == ConflictResolutionFeedback.SaveForLater)
                    {
                        feedback[i] = askUserForFeedback(conflicts[i]);

                        if (feedback[i] == ConflictResolutionFeedback.SaveForLater)
                            hasUnresolvedConflicts = true;
                    }
                }
            }
        }

        private ConflictResolutionFeedback askUserForFeedback(MergeRecord conflict)
        {
            //TODO: Create feedback dialog
            return ConflictResolutionFeedback.KeepBoth;
        }

        public void ResolveAll()
        {
            foreach (var currentFeedback in feedback)
            {
                MergeRecord currentRecord = conflicts[0]; //Each successful resolve removes the resolved item from the collection

                bool isResolved = false;
                switch (currentFeedback)
                {
                    case ConflictResolutionFeedback.CancelMove:
                        isResolved = true;
                        break;
                    case ConflictResolutionFeedback.Overwrite:
                        isResolved = FileUtils.TryReplace(currentRecord.OriginalPath, currentRecord.CurrentPath);
                        break;
                    case ConflictResolutionFeedback.KeepBoth:
                        isResolved = FileUtils.TryMove(currentRecord.OriginalPath, currentRecord.CurrentPath, 1, FileMoveOption.RenameSourceIfNecessary);
                        break;
                    case ConflictResolutionFeedback.SaveForLater:
                        throw new InvalidOperationException("Conflict resolution state is not valid.");
                }

                if (!isResolved)
                    throw new OperationCanceledException("Unable to resolve conflict");

                conflicts.RemoveAt(0);
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
