using System;

namespace LogUtils.Helpers
{
    public static class TimeConversion
    {
        public static double DateTimeInMilliseconds(DateTime date)
        {
            return new TimeSpan(date.Ticks).TotalMilliseconds;
        }
    }
}
