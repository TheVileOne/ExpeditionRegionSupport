using System;

namespace LogUtils.Helpers
{
    public struct AttemptCounter
    {
        private int initialAttemptCount;
        private int currentAttempt;

        /// <summary>
        /// The number of attempts remaining before maximum attempts is reached
        /// </summary>
        public readonly int Remaining => initialAttemptCount - currentAttempt;

        /// <summary>
        /// Indicates that this is the last attempt, or that there are no attempts remaining
        /// </summary>
        public readonly bool IsLast => Remaining <= 1;

        /// <summary>
        /// Indicates that this is the first attempt
        /// </summary>
        public readonly bool IsFirst => currentAttempt == 0;

        public AttemptCounter(int attemptCount)
        {
            initialAttemptCount = Math.Max(attemptCount, 0);
        }

        /// <summary>
        /// Increments the attempt count
        /// </summary>
        /// <returns>The new attempt count</returns>
        public int Increment()
        {
            if (currentAttempt < initialAttemptCount)
                currentAttempt++;
            return currentAttempt;
        }
    }
}
