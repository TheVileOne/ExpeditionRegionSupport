using LogUtils.Events;
using System;

namespace LogUtils.Timers
{
    public class FrameTimer
    {
        /// <summary>
        /// Contains a reference to a ScheduledEvent for timers created by an EventScheduler
        /// </summary>
        internal ScheduledEvent Event;

        private int _frequency = 1;
        public virtual int Frequency
        {
            get => _frequency;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _frequency = value;
            }
        }

        /// <summary>
        /// Should this timer be synced with the current RainWorld process or attempt to update on every available frame
        /// </summary>
        public readonly bool IsSynchronous;

        /// <summary>
        /// The FrameTimer equivalent of a disposed flag
        /// </summary>
        public bool Released { get; private set; }

        /// <summary>
        /// Stores delegate information that will run when an synchronous event is handled
        /// </summary>
        internal EventHandler<MainLoopProcess, EventArgs> SyncHandler;

        /// <summary>
        /// Number of allowed frame updates since timer was last started
        /// </summary>
        public int ElapsedTicks;

        public event Action OnInterval;

        private bool canUpdate;

        public FrameTimer(int interval, bool syncToRainWorld = false)
        {
            if (interval <= 0)
                throw new ArgumentOutOfRangeException(nameof(interval));

            IsSynchronous = syncToRainWorld;
            Frequency = interval;
            UtilityCore.Scheduler.AddTimer(this);
        }

        /// <summary>
        /// Allows frame counter to update
        /// </summary>
        public virtual void Start()
        {
            canUpdate = true;
        }

        /// <summary>
        /// Prevents frame counter from updating
        /// </summary>
        public virtual void Stop()
        {
            canUpdate = false;
        }

        /// <summary>
        /// Activate timer dispose procedure
        /// </summary>
        public virtual void Release()
        {
            if (Released) return;

            Released = true;

            Event?.Cancel();
            Stop();
            OnRelease?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resets frame counter back to zero and resumes updating the frame counter
        /// </summary>
        public virtual void Restart()
        {
            ElapsedTicks = 0;
            Start();
        }

        public virtual void Update()
        {
            if (!canUpdate) return;

            ElapsedTicks++;

            if (ElapsedTicks > Frequency)
                ElapsedTicks = 0;

            bool intervalReached = ElapsedTicks == Frequency || Frequency == 1;

            if (intervalReached)
                OnInterval?.Invoke();
        }

        /// <summary>
        /// Invoked when a FrameTimer instance signals that it should no longer be updated by the scheduler
        /// </summary>
        public static EventHandler<FrameTimer, EventArgs> OnRelease;
    }
}
