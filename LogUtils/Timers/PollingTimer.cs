﻿using LogUtils.Events;
using System;
using System.Diagnostics;
using System.Timers;

namespace LogUtils.Timers
{
    public class PollingTimer : Timer
    {
        private bool _started;
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
        public event EventHandler<Timer, ElapsedEventArgs> OnTimeout;

        /// <summary>
        /// Constructs a PollingTimer
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
            OnSignal?.Invoke(this);
        }

        /// <summary>
        /// Starts raising the System.Timers.Timer.Elapsed event by setting System.Timers.Timer.Enabled to true
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The System.Timers.Timer is created with an interval equal to or greater than
        /// System.Int32.MaxValue + 1, or set to an interval less than zero.</exception>
        public new void Start()
        {
            _started = true;
            lastPollTime = -1;

            Enabled = true;
            PollFlagged = false;
            base.Start();
        }

        public new void Stop()
        {
            _started = false;
            base.Stop();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!PollFlagged)
                OnTimeout?.Invoke(this, e);
            PollFlagged = false;
        }
    }

    public delegate void SignalEventHandler(Timer timer);
}
