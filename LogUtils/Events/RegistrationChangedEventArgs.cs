using System;

namespace LogUtils.Events
{
    public class RegistrationChangedEventArgs : EventArgs
    {
        public readonly bool Current;

        public RegistrationChangedEventArgs(bool status)
        {
            Current = status;
        }
    }
}