using LogUtils.Enums;
using LogUtils.Events;
using UnityEngine;

namespace LogUtils.Compatibility
{
    public class UnityLogEventArgs : LogEventArgs
    {
        /// <summary>
        /// Unity object - typically given to provide context to the log message
        /// </summary>
        public Object Context { get; }

        /// <summary>
        /// Unity tag - typically given to provide context to the log message
        /// </summary>
        public string Tag { get; }

        public UnityLogEventArgs(LogID logID, Object context, string tag) : base(logID)
        {
            Context = context;
            Tag = tag;
        }
    }
}
