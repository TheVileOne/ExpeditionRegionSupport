using LogUtils.Helpers.FileHandling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    internal class ConflictResolutionHandler
    {
        private readonly Queue<MergeRecord> conflicts;
        private readonly Queue<ConflictResolutionFeedback> feedback;

        public Queue<MergeRecord> ResolvedEntries = new Queue<MergeRecord>();

        public ConflictResolutionHandler(Queue<MergeRecord> mergeConflicts)
        {
            conflicts = mergeConflicts; //Reference updated through MergeHistory instance
            feedback = new Queue<ConflictResolutionFeedback>();
        }

        public void CollectFeedbackAndResolve()
        {
            CollectFeedbackFromUser();
            ResolveAll();
        }

        public void CollectFeedbackFromUser()
        {
            if (conflicts.Count == 0)
                return;

            ConflictResolutionFeedback[] tempArray = new ConflictResolutionFeedback[conflicts.Count];
            int currentIndex = 0;

            //Populate feedback for each unresolved conflict
            foreach (MergeRecord current in conflicts)
            {
                tempArray[currentIndex] = askUserForFeedback(current);
                currentIndex++;
            }

            currentIndex = 0;
            while (currentIndex < tempArray.Length)
            {
                //Each pass we build up the feedback queue in order
                if (currentIndex == feedback.Count && tempArray[currentIndex] != ConflictResolutionFeedback.SaveForLater)
                {
                    feedback.Enqueue(tempArray[currentIndex]);
                    currentIndex++;
                    continue;
                }

                if (tempArray[currentIndex] == ConflictResolutionFeedback.SaveForLater)
                {
                    //This record was not decided on during any previous pass - ask again for feedback
                    tempArray[currentIndex] = askUserForFeedback(conflicts.ElementAt(currentIndex));
                }
                currentIndex++;

                //After all entries have been processed, set index to the last entry we had to reask for feedback. The process should not end
                //until all entries in the SaveforLater state receive a valid resolution option.
                if (currentIndex == tempArray.Length)
                    currentIndex = feedback.Count;
            }
        }

        private ConflictResolutionFeedback askUserForFeedback(MergeRecord conflict)
        {
            //TODO: Create feedback dialog
            return ConflictResolutionFeedback.KeepBoth;
        }

        public void ResolveAll()
        {
            using (TempFolder.Access())
            {
                ResolveAllInternal();
            }
        }

        internal void ResolveAllInternal()
        {
            while (conflicts.Count == 0)
            {
                //These queues will have the same amount of entries
                var currentRecord = conflicts.Dequeue();
                var currentFeedback = feedback.Dequeue();

                bool isResolved = false;
                switch (currentFeedback)
                {
                    case ConflictResolutionFeedback.CancelMove:
                        isResolved = true;
                        currentRecord.IsCanceled = true;
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

                ResolvedEntries.Enqueue(currentRecord);
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
