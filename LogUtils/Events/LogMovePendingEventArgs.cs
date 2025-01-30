using LogUtils.Properties;
using System;

namespace LogUtils.Events
{
    public class LogMovePendingEventArgs : LogEventArgs
    {
        /// <summary>
        /// The new path, may be the same path if the file was only renamed
        /// </summary>
        public readonly string MovePath;

        /// <summary>
        /// The pending filename to use when it is different from the current filename, otherwise null
        /// </summary>
        public readonly string NewFilename;

        public bool IsRenamed => NewFilename != null;

        public LogMovePendingEventArgs(LogProperties properties, string pendingLogPath, string pendingFilename = null) : base(properties)
        {
            if (pendingLogPath == null)
                throw new ArgumentNullException("Path must be specified");

            MovePath = pendingLogPath;
            NewFilename = pendingFilename;
        }
    }
}
