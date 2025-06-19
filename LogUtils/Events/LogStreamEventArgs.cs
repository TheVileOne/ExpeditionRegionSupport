using LogUtils.Enums;
using System.IO;

namespace LogUtils.Events
{
    public class LogStreamEventArgs : LogEventArgs
    {
        public StreamWriter Writer { get; }

        public LogStreamEventArgs(LogID logID, StreamWriter writer) : base(logID)
        {
            Writer = writer;
        }
    }
}
