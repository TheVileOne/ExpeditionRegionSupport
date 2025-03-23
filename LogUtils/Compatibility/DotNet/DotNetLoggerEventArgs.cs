using LogUtils.Enums;
using LogUtils.Events;
using Microsoft.Extensions.Logging;

namespace LogUtils.Compatibility.DotNet
{
    public class DotNetLoggerEventArgs : LogEventArgs
    {
        public EventId EventID { get; }

        public DotNetLoggerEventArgs(LogID logID, EventId eventID) : base(logID)
        {
            EventID = eventID;
        }
    }
}
