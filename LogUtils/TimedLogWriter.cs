using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace LogUtils
{
    public class TimedLogWriter : LogWriter, IDisposable
    {
        protected List<PersistentLogFileHandle> LogFiles = new List<PersistentLogFileHandle>();

        public Timer FlushTimer;

        /// <summary>
        /// The flush interval in milliseconds
        /// </summary>
        public const int INTERVAL_DEFAULT = 2000;

        private bool isDisposed;

        /// <summary>
        /// Construct a TimedLogWriter instance
        /// </summary>
        /// <param name="writeInterval">The flush interval in milliseconds</param>
        /// <exception cref="ArgumentOutOfRangeException">The flush interval is an invalid value</exception>
        public TimedLogWriter(int writeInterval = INTERVAL_DEFAULT)
        {
            if (writeInterval <= 0)
                throw new ArgumentOutOfRangeException("Write interval must be greater than zero");

            FlushTimer = new Timer(delegate
            {
                Flush();
            }, null, writeInterval, writeInterval);
        }

        public void Flush()
        {
            if (isDisposed)
                throw new ObjectDisposedException("Cannot access a disposed LogWriter");

            int logFilesProcessed = 0;
            foreach (PersistentLogFileHandle logFile in LogFiles.Where(file => !file.IsClosed))
            {
                logFilesProcessed++;
                try
                {
                    logFile.Stream.Flush();
                }
                catch (IOException)
                {
                    //TODO: Error logging
                }
            }

            bool hasClosedLogFiles = logFilesProcessed != LogFiles.Count;

            if (hasClosedLogFiles)
            {
                List<PersistentLogFileHandle> closedLogFiles = LogFiles.FindAll(file => file.IsClosed); 
                
                foreach (PersistentLogFileHandle logFile in closedLogFiles)
                {
                    logFile.FileID.Properties.PersistentStreamHandles.Remove(logFile);
                    LogFiles.Remove(logFile);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    FlushTimer.Dispose();
                    FlushTimer = null;

                    Flush();

                    LogFiles.ForEach(logFile => logFile.FileID.Properties.PersistentStreamHandles.Remove(logFile));
                    LogFiles = null;
                }
                isDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TimedLogWriter()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
