using System;

namespace LogUtils.Timers
{
    public class FrameTimer
    {
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

        public int Ticks;

        public event Action OnInterval;

        private bool canUpdate;

        public FrameTimer(int interval)
        {
            if (interval <= 0)
                throw new ArgumentOutOfRangeException();

            Frequency = interval;
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

            Ticks = Ticks < Frequency ? Ticks + 1 : 0;

            bool intervalReached = Ticks == Frequency || Frequency == 1;

            if (intervalReached)
                OnInterval?.Invoke();
        }
    }
}
