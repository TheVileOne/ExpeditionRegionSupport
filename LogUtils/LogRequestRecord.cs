using System.Diagnostics;

namespace LogUtils
{
    public struct LogRequestRecord
    {
        /// <summary>
        /// The last time the reason was updated (defaults to 0 prior to the first update)
        /// </summary>
        public long LastUpdated { get; private set; }

        public RejectionReason Reason { get; private set; }

        public readonly bool Rejected => Reason != RejectionReason.None;

        /// <summary>
        /// Resets the state back to original values
        /// </summary>
        public void Reset()
        {
            Reason = RejectionReason.None;
            LastUpdated = default;
        }

        public void SetReason(RejectionReason reason)
        {
            Reason = reason;
            LastUpdated = Stopwatch.GetTimestamp();
        }
    }
}
