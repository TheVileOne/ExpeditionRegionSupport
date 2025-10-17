using System;

namespace LogUtils.Events
{
    public class TimeEventArgs : EventArgs
    {
        public readonly DateTime Time;

        public TimeEventArgs(DateTime time)
        {
            Time = time;
        }
    }
}
