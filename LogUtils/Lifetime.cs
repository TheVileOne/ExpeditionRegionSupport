using LogUtils.Helpers;
using LogUtils.Threading;
using System;

namespace LogUtils
{
    public class Lifetime
    {
        public bool IsAlive => TimeRemaining == LifetimeDuration.Infinite || TimeRemaining > 0;

        /// <summary>
        /// A managed representation of the time remaining before filestream is disposed in milliseconds
        /// </summary>
        public int TimeRemaining { get; private set; }

        private TimeSpan lastCheckedTime = TimeSpan.Zero;

        /// <summary>
        /// Task assigned to update the life span for this object
        /// </summary>
        public Task UpdateTask;

        private Lifetime(int duration)
        {
            SetDuration(duration);
            UpdateTask = LogTasker.Schedule(new Task(Update, 0)
            {
                Name = "Lifetime",
                IsContinuous = true
            });
        }

        /// <summary>
        /// Constructs a representation of a Lifetime with a given duration in milliseconds
        /// </summary>
        public static Lifetime FromMilliseconds(int duration)
        {
            return new Lifetime(duration);
        }

        /// <summary>
        /// Set the lifetime remaining to a duration in milliseconds
        /// </summary>
        public void SetDuration(int duration)
        {
            //LifetimeStart is reset whenever duration is infinite, and assigned when a duration is changed to a finite duration from infinite
            if (duration == LifetimeDuration.Infinite)
            {
                lastCheckedTime = TimeSpan.Zero;
                TimeRemaining = LifetimeDuration.Infinite;
            }
            else if (duration != TimeRemaining)
            {
                if (TimeRemaining == LifetimeDuration.Infinite)
                    lastCheckedTime = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
                TimeRemaining = Math.Max(0, duration);
            }
        }

        private void calculateTimeRemaining()
        {
            TimeSpan currentTime = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);

            int timePassed = (int)(currentTime - lastCheckedTime).TotalMilliseconds;

            //Updating lastCheckedTime makes timePassed relative to last update time
            lastCheckedTime = currentTime;
            TimeRemaining = Math.Max(0, TimeRemaining - timePassed);
        }

        public void Update()
        {
            if (TimeRemaining == LifetimeDuration.Infinite) return;

            calculateTimeRemaining();

            //Once a lifetime has ended, stop running updates
            if (!IsAlive)
                UpdateTask.End();
        }
    }

    public static class LifetimeDuration
    {
        public const int Infinite = -1;
    }
}
