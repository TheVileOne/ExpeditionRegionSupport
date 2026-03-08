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

        /// <summary>
        /// Indicates whether move operation is part of a folder move
        /// </summary>
        public readonly bool IsPendingFolderMove;

        public bool IsRenamed => NewFilename != null;

        public LogMovePendingEventArgs(LogProperties properties, string pendingLogPath, string pendingFilename = null) : base(properties)
        {
            if (pendingLogPath == null)
                throw new ArgumentNullException(nameof(pendingLogPath), "Path must be specified");

            MovePath = pendingLogPath;
            NewFilename = pendingFilename;
        }

        public LogMovePendingEventArgs(LogProperties properties, string pendingLogPath, string pendingFilename, bool isPendingFolderMove = false) : this(properties, pendingLogPath, pendingFilename)
        {
            IsPendingFolderMove = isPendingFolderMove;
        }
    }

    public class LogMoveAbortedEventArgs : LogEventArgs
    {
        /// <summary>
        /// Indicates whether move operation was part of a folder move
        /// </summary>
        public readonly bool FolderMoveAborted;

        public LogMoveAbortedEventArgs(LogProperties properties) : base(properties)
        {
        }

        public LogMoveAbortedEventArgs(LogProperties properties, bool wasFolderMoveEvent = false) : base(properties)
        {
            FolderMoveAborted = wasFolderMoveEvent;
        }
    }
}
