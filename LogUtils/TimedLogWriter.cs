using BepInEx;
using LogUtils.Enums;
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
        protected List<StreamWriter> LogWriters = new List<StreamWriter>();

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
                    ReleaseHandle(logFile);
            }
        }

        protected override void WriteToFile(LogRequest request)
        {
            //Get locally controlled LogIDs from the logger, and compare against the persistent file handles managed by the LogWriter
            if (request.Host != null)
            {
                IEnumerable<PersistentLogFileHandle> handlesToRelease = request.Host.GetUnusedHandles(LogFiles);

                foreach (PersistentLogFileHandle handle in handlesToRelease)
                    ReleaseHandle(handle);
            }
        }

        /// <summary>
        /// Ensures that a persistent filestream with infinite lifetime is stored in the LogWriter instance
        /// </summary>
        internal void UpdateStreamHandles(List<LogID> logFiles)
        {
            //TODO: Find infinite duration stream handles that aren't represented by the updated list and change them to temporary streams
            foreach (LogID logFile in logFiles)
            {
                PersistentLogFileHandle handle = LogFiles.Find(h => h.FileID == logFile);

                //TODO: Check that LogID is writeable, before opening a stream 
                if (handle != null)
                {
                    if (!handle.IsClosed)
                        handle.Lifetime.SetDuration(LifetimeDuration.Infinite);
                    else //Handling a disposed stream
                    {
                        ReleaseHandle(handle);
                        AttachHandle(new PersistentLogFileHandle(logFile));
                    }
                }
                else if (logFile.Properties.CanBeAccessed)
                {
                    AttachHandle(new PersistentLogFileHandle(logFile));
                }
            }
        }

        /// <summary>
        /// Opens a threadsafe filestream for writing messages
        /// </summary>
        public void OpenFileStream(LogID logFile)
        {
            //TODO: This wont prepare logfiles properly
            FileStream fileStream;
            while (!Utility.TryOpenFileStream(logFile.Properties.CurrentFilePath, FileMode.OpenOrCreate, out fileStream, FileAccess.Write, FileShare.ReadWrite))
            {
                if (num == max_open_attempts)
                {
                    //if (!hasBackup)
                    {
                        Debug.LogError("Couldn't open a log file for writing. Skipping log file creation");
                        return;
                    }

                    backupRequired = true;
                    break;
                }

                Debug.LogWarning("Couldn't open log file '" + LogName + "' for writing, trying another...");
                string logName = $"{LogName}.log.{num++}";
                LogFullPath = Path.Combine(logPath, logName);
            }

            if (!backupRequired)
            {
                LogWriter = TextWriter.Synchronized(new StreamWriter(fileStream, Utility.UTF8NoBom));
                FlushTimer = new Timer(delegate
                {
                    LogWriter?.Flush();
                }, null, 2000, 2000);
            }
        }

        protected void AttachHandle(PersistentLogFileHandle handle)
        {
            handle.FileID.Properties.PersistentStreamHandles.Add(handle);
            LogFiles.Add(handle);
        }

        protected void ReleaseHandle(PersistentLogFileHandle handle)
        {
            handle.FileID.Properties.PersistentStreamHandles.Remove(handle);
            LogFiles.Remove(handle);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                FlushTimer.Dispose();
                FlushTimer = null;

                Flush();

                foreach (PersistentLogFileHandle handle in LogFiles)
                    ReleaseHandle(handle);
                LogFiles = null;
            }
            isDisposed = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
