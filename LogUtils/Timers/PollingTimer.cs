using LogUtils.Events;
using System;
using System.Diagnostics;
using System.Timers;

namespace LogUtils.Timers
{
    /// <summary>
    /// Class hides base methods. It is recommended to not use this class while casted by the <see cref="Timer"/> base class
    /// </summary>
    public class PollingTimer : Timer
    {
        private bool _started;

        /// <summary>
        /// The timer is available and is actively running
        /// </summary>
        public bool Started => Enabled && _started;

        /// <summary>
        /// Used to attach identifying information
        /// </summary>
        public object Tag;

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

        /// <summary>
        /// Activated when timer is signaled
        /// </summary>
        public event SignalEventHandler OnSignal;

        /// <summary>
        /// Activated when an entire polling interval passes without receiving a poll signal
        /// </summary>
        public event EventHandler<PollingTimer, ElapsedEventArgs> OnTimeout;

        /// <summary>
        /// Constructs a new <see cref="PollingTimer"/> instance
        /// </summary>
        /// <param name="checkInterval">The time window in which a polling flag must be set</param>
        public PollingTimer(double checkInterval) : base(checkInterval)
        {
            lastPollTime = -1;
            Elapsed += onIntervalElapsed;
        }

        /// <summary>
        /// Notifies the timer that it shouldn't raise an event on the next timed interval
        /// </summary>
        public void Signal()
        {
            PollFlagged = true;

            if (TrackingPollTime)
                lastPollTime = Stopwatch.GetTimestamp();
            OnSignal?.Invoke(this);
        }

        /// <inheritdoc cref="Timer.Start"/>
        /// <exception cref="ObjectDisposedException">Attempted to access object after it was disposed</exception>
        public new void Start()
        {
            //Though this state will be in an abnormal state if an exception were to occur, it should not affect standard operation.
            //No events will be fired in the event of an exception. This cannot throw an exception in any place that LogUtils utilizes this instance
            //without user intervention.
            _started = true;
            lastPollTime = -1;

            PollFlagged = false;
            base.Start();
        }

        /// <inheritdoc cref="Timer.Stop"/>
        public new void Stop()
        {
            _started = false;
            base.Stop();
        }

        private void onIntervalElapsed(object sender, ElapsedEventArgs e)
        {
            if (!PollFlagged)
                OnTimeout?.Invoke(this, e);
            PollFlagged = false;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _started = false;
            base.Dispose(disposing);
        }
    }

    public delegate void SignalEventHandler(PollingTimer timer);
}
