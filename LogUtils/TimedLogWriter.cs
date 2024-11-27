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

        protected bool IsDisposed;

        protected int WriteInterval;

        /// <summary>
        /// The flush interval in milliseconds
        /// </summary>
        public const int INTERVAL_DEFAULT = 2000;

        /// <summary>
        /// Construct a TimedLogWriter instance
        /// </summary>
        /// <param name="writeInterval">The flush interval in milliseconds</param>
        /// <exception cref="ArgumentOutOfRangeException">The flush interval is an invalid value</exception>
        public TimedLogWriter(int writeInterval = INTERVAL_DEFAULT)
        {
            if (writeInterval <= 0)
                throw new ArgumentOutOfRangeException("Write interval must be greater than zero");

            WriteInterval = writeInterval;

            FlushTimer = new Timer(delegate
            {
                Flush();
            }, null, WriteInterval, WriteInterval);

            UtilityCore.PersistenceManager.OnHandleDisposed += onHandleDisposed;
        }

        public void Flush()
        {
            if (IsDisposed)
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
                    ReleaseHandle(logFile, false);
            }
        }

        protected override void WriteToFile(LogRequest request)
        {
            //Get locally controlled LogIDs from the logger, and compare against the persistent file handles managed by the LogWriter
            if (request.Host != null)
            {
                IEnumerable<PersistentLogFileHandle> handlesToRelease = request.Host.GetUnusedHandles(LogFiles);

                foreach (PersistentLogFileHandle handle in handlesToRelease)
                    ReleaseHandle(handle, false);
            }

            LogID requestID = request.Data.ID;

            int handleIndex = LogFiles.FindIndex(handle => handle.FileID == requestID);
            bool handleExists = handleIndex >= 0;

            PersistentLogFileHandle writeHandle = handleExists ? LogFiles[handleIndex] : new PersistentLogFileHandle(requestID);

            //TODO: Check that temporarily closing the FileStream wont cause weird behavior here
            if (writeHandle.IsClosed)
                writeHandle = ReplaceHandle(writeHandle);
            else if (!handleExists)
            {
                AttachHandle(writeHandle);
                handleIndex = LogFiles.Count - 1;
            }

            //FileStream was unable to open for an unknown reason
            if (writeHandle.IsClosed)
            {
                request.Reject(RejectionReason.FailedToWrite);
                UtilityLogger.LogError("LogWriter was unable to handle request");
                return;
            }

            //TODO: Filter logic, applying rules, and other writer checks
            LogWriters[handleIndex].WriteLine(request.Data.Message);
        }

        protected PersistentLogFileHandle AttachHandle(PersistentLogFileHandle handle)
        {
            LogFiles.Add(handle);
            LogWriters.Add(new StreamWriter(handle.Stream, Utility.UTF8NoBom));
            return handle;
        }

        protected void ReleaseHandle(PersistentLogFileHandle handle, bool disposing)
        {
            if (!handle.IsAlive) return;

            //This needs to drop handle immediately if LogWriter is being disposed, or at the next write interval if it is being lazy disposed
            //Currently no way to tell when the next interval will be, so the full interval period is used here
            int disposalDelay = disposing ? 0 : WriteInterval;

            handle.Lifetime.SetDuration(disposalDelay);
        }

        protected PersistentLogFileHandle ReplaceHandle(PersistentLogFileHandle handle)
        {
            ReleaseHandle(handle, disposing: true); //Handle situation as if handle was being disposed
            onHandleDisposed(handle);

            return AttachHandle(new PersistentLogFileHandle(handle.FileID));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                FlushTimer.Dispose();
                FlushTimer = null;

                Flush();

                foreach (PersistentLogFileHandle handle in LogFiles)
                    ReleaseHandle(handle, disposing);
                LogFiles = null;
            }

            UtilityCore.PersistenceManager.OnHandleDisposed -= onHandleDisposed;
            IsDisposed = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void onHandleDisposed(PersistentFileHandle handle)
        {
            //Handle wont necessarily belong to this writer
            int handleIndex = LogFiles.IndexOf(handle as PersistentLogFileHandle);

            if (handleIndex != -1)
            {
                LogFiles.RemoveAt(handleIndex);
                LogWriters.RemoveAt(handleIndex);
            }
        }
    }
}
