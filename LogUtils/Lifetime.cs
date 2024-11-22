using System;

namespace LogUtils
{
    public struct Lifetime
    {
        public readonly bool IsAlive => TimeRemaining == Duration.Infinite || TimeRemaining > 0;

        /// <summary>
        /// A managed representation of the time remaining before filestream is disposed in milliseconds
        /// </summary>
        public int TimeRemaining { get; private set; }

        private int lastCheckedTime;

        /// <summary>
        /// Set the lifetime remaining to a duration in milliseconds
        /// </summary>
        public void SetDuration(int duration)
        {
            //LifetimeStart is reset whenever duration is infinite, and assigned when a duration is changed to a finite duration from infinite
            if (duration == Duration.Infinite)
            {
                lastCheckedTime = 0;
                TimeRemaining = Duration.Infinite;
            }
            else if (duration != TimeRemaining)
            {
                if (TimeRemaining == Duration.Infinite)
                    lastCheckedTime = DateTime.Now.Millisecond;
                TimeRemaining = Math.Max(0, duration);
            }
        }

        public void Update()
        {
            if (TimeRemaining == Duration.Infinite) return;

            int currentTime = DateTime.Now.Millisecond;
            int timePassed = currentTime - lastCheckedTime;

            //Updating lastCheckedTime makes timePassed relative to last update time
            lastCheckedTime = currentTime;
            TimeRemaining = Math.Max(0, TimeRemaining - timePassed);
        }

        public static class Duration
        {
            public const int Infinite = -1;
        }
    }
}
