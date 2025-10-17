using Microsoft.Extensions.Logging;
using System;

namespace LogUtils.Compatibility.DotNet
{
    public class DotNetLoggerEventArgs : EventArgs
    {
        public readonly EventId EventID;

        public DotNetLoggerEventArgs(EventId eventID)
        {
            EventID = eventID;
        }
    }
}
