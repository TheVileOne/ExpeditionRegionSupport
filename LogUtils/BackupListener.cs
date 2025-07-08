using LogUtils.Enums;
using System;
using System.Collections.Generic;

namespace LogUtils
{
    /// <summary>
    /// Provides a means for mods to listen for log backup opportunity signals
    /// </summary>
    public static class BackupListener
    {
        private static List<EventRecord> records = new List<EventRecord>();

        private static EventFeed eventHandle;

        /// <summary>
        /// Allows subscribers access to all recent, and future backup records
        /// </summary>
        public static event EventFeed Feed
        {
            add
            {
                if (value != null)
                {
                    records.ForEach(r => value.Invoke(r));
                    eventHandle += value;
                }
            }
            remove
            {
                eventHandle -= value;
            }
        }

        internal static void OnTempFileCreated(LogID logFile)
        {
            //Check for an existing backup record - only one record should be stored per log file
            int index = records.FindIndex(r => r.LogFile.Equals(logFile));

            if (index != -1)
                records.RemoveAt(index);

            EventRecord record = new EventRecord(logFile);

            records.Add(record);

            //Notify feed subscribers of the new temp file
            eventHandle?.Invoke(record);
        }

        public class EventRecord : EventArgs
        {
            /// <summary>
            /// Backup target
            /// </summary>
            public LogID LogFile;

            /// <summary>
            /// The primary source path to the temporary file
            /// </summary>
            public string SourcePath;

            /// <summary>
            /// A list of source paths for log backups created by other mods (use in case the file is no longer at the primary source path)
            /// </summary>
            public ICollection<string> BackupPaths = new List<string>();

            public EventRecord(LogID logFile)
            {
                LogFile = logFile;
                SourcePath = LogFile.Properties.ReplacementFilePath;
            }
        }

        public delegate void EventFeed(EventRecord record);
    }
}
