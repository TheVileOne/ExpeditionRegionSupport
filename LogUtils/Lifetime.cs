using LogUtils.Helpers;
using System;

namespace LogUtils
{
    public struct Lifetime
    {
        public readonly bool IsAlive => TimeRemaining == LifetimeDuration.Infinite || TimeRemaining > 0;

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
            if (duration == LifetimeDuration.Infinite)
            {
                lastCheckedTime = 0;
                TimeRemaining = LifetimeDuration.Infinite;
            }
            else if (duration != TimeRemaining)
            {
                if (TimeRemaining == LifetimeDuration.Infinite)
                    lastCheckedTime = (int)TimeConversion.DateTimeInMilliseconds(DateTime.Now);
                TimeRemaining = Math.Max(0, duration);
            }
        }

        public void Update()
        {
            if (TimeRemaining == LifetimeDuration.Infinite) return;

            int currentTime = (int)TimeConversion.DateTimeInMilliseconds(DateTime.Now);
            int timePassed = currentTime - lastCheckedTime;

            //Updating lastCheckedTime makes timePassed relative to last update time
            lastCheckedTime = currentTime;
            TimeRemaining = Math.Max(0, TimeRemaining - timePassed);
        }
    }

    public static class LifetimeDuration
    {
        public const int Infinite = -1;
    }
}
