using LogUtils.Enums;
using LogUtils.Properties;
using System;

namespace LogUtils.Events
{
    public class LogEventArgs : EventArgs, ICloneable
    {
        public LogID ID { get; protected set; }
        public LogProperties Properties => ID.Properties;

        public LogEventArgs(LogID logID) : this(logID.Properties)
        {
        }

        public LogEventArgs(LogProperties properties)
        {
            ID = properties?.ID;
        }

        public virtual LogEventArgs Clone(LogID newID)
        {
            LogEventArgs copy = (LogEventArgs)Clone();

            copy.ID = newID;
            return copy;
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
