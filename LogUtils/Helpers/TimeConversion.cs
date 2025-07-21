using System;

namespace LogUtils.Helpers
{
    public static class TimeConversion
    {
        public static double ToMilliseconds(this DateTime date)
        {
            return date.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public static TimeSpan MultiplyBy(this TimeSpan span, double amount)
        {
            return new TimeSpan((long)(span.Ticks * amount));
        }

        public static TimeSpan DivideBy(this TimeSpan span, double amount)
        {
            return new TimeSpan((long)(span.Ticks / amount));
        }
    }
}
