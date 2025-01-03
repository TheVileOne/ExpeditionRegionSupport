using System;
using System.Diagnostics;
using System.Timers;

namespace LogUtils.Diagnostics.Tools
{
    public class PollingTimer : Timer
    {
        /// <summary>
        /// Should the timer record the time of poll signals
        /// </summary>
        public bool TrackingPollTime;

        /// <summary>
        /// The last time the timer was signaled (in ticks)
        /// </summary>
        private long lastPollTime;

        public TimeSpan TimeSinceLastPoll => lastPollTime <= 0 ? TimeSpan.Zero : TimeSpan.FromTicks(Stopwatch.GetTimestamp() - lastPollTime);

        /// <summary>
        /// The poll state since the last elapsed time interval
        /// </summary>
        public bool PollFlagged { get; private set; }

        public event Action<ElapsedEventArgs> OnTimeout;

        /// <summary>
        /// Activates the timer mechanism
        /// </summary>
        /// <param name="checkInterval">The time window in which a polling flag must be set</param>
        public PollingTimer(double checkInterval) : base(checkInterval)
        {
            lastPollTime = -1;
            Elapsed += Timer_Elapsed;
        }

        /// <summary>
        /// Notifies the timer that it shouldn't raise an event on the next timed interval
        /// </summary>
        public void Signal()
        {
            PollFlagged = true;

            if (TrackingPollTime)
                lastPollTime = Stopwatch.GetTimestamp();
        }

        /// <summary>
        ///     Starts raising the System.Timers.Timer.Elapsed event by setting System.Timers.Timer.Enabled
        ///     to true
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The System.Timers.Timer is created with an interval equal to or greater than
        ///     System.Int32.MaxValue + 1, or set to an interval less than zero.</exception>
        public new void Start()
        {
            lastPollTime = -1;
            PollFlagged = false;
            base.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!PollFlagged)
                OnTimeout?.Invoke(e);
            PollFlagged = false;
        }
    }
}
