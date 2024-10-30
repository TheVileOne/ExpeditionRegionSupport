using System;

namespace LogUtils.Events
{
    public class SetupPeriodEventArgs : EventArgs
    {
        public readonly SetupPeriod LastPeriod;
        public readonly SetupPeriod CurrentPeriod;

        public SetupPeriodEventArgs(SetupPeriod last, SetupPeriod current)
        {
            LastPeriod = last;
            CurrentPeriod = current;
        }
    }
}
