using Microsoft.Extensions.Logging;
using System;

namespace LogUtils.Compatibility.DotNet
{
    public class DotNetLoggerEventArgs : EventArgs
    {
        public EventId EventID { get; }

        public DotNetLoggerEventArgs(EventId eventID)
        {
            EventID = eventID;
        }
    }
}
