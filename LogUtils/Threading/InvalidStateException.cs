using System;

namespace LogUtils.Threading
{
    public class InvalidStateException : Exception
    {
        public InvalidStateException() : base() { }

        public InvalidStateException(string name) : base($"{name} is not in a valid state")
        {
        }
    }
}
