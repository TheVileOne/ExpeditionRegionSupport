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

        /// <summary>
        /// Clones the current <see cref="EventArgs"/> assigning it the provided <see cref="LogID"/> instance
        /// </summary>
        public virtual LogEventArgs Clone(LogID newID)
        {
            LogEventArgs copy = (LogEventArgs)Clone();

            copy.ID = newID;
            return copy;
        }

        /// <inheritdoc/>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
