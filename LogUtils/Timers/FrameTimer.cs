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
                    throw new ArgumentOutOfRangeException(nameof(Frequency));
                _frequency = value;
            }
        }

        /// <summary>
        /// Should this timer be synced with the current RainWorld process or attempt to update on every available frame
        /// </summary>
        public readonly bool IsSynchronous;

        public int Ticks;

        public event Action OnInterval;

        private bool canUpdate;

        public FrameTimer(int interval)
        {
            if (interval <= 0)
                throw new ArgumentOutOfRangeException();

            Frequency = interval;
            UtilityCore.Scheduler.AddTimer(this);
        }

        public virtual void Start()
        {
            canUpdate = true;
        }

        public virtual void Stop()
        {
            canUpdate = false;
        }

        public virtual void Restart()
        {
            Ticks = 0;
            Start();
        }

        public virtual void Update()
        {
            if (!canUpdate) return;

            Ticks++;

            if (Ticks > Frequency)
                Ticks = 0;

            bool intervalReached = Ticks == Frequency || Frequency == 1;

            if (intervalReached)
                OnInterval?.Invoke();
        }
    }
}
