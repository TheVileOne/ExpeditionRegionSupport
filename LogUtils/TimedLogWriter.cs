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
        protected List<PersistentLogFileWriter> LogWriters = new List<PersistentLogFileWriter>();

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

        /// <summary>
        /// Writes the log buffer to file for managed log files
        /// </summary>
        public void Flush()
        {
            try
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Cannot access a disposed LogWriter");

                foreach (var writer in LogWriters)
                    writer.Flush();
            }
            catch (ObjectDisposedException ex)
            {
                UtilityLogger.LogError(ex);
            }
        }

        protected override void WriteToFile(LogRequest request)
        {
            request.WriteInProcess();

            if (request.ThreadCanWrite)
            {
                //Assume that thread is allowed to write if we get past this point
                try
                {
                    LogID logFile = request.Data.ID;
                    string message = request.Data.Message;

                    if (LogFilter.CheckFilterMatch(logFile, message))
                    {
                        request.Reject(RejectionReason.FilterMatch);
                        return;
                    }

                    //Get locally controlled LogIDs from the logger, and compare against the persistent file handles managed by the LogWriter
                    if (request.Host != null)
                    {
                        IEnumerable<PersistentLogFileHandle> handlesToRelease = request.Host.GetUnusedHandles(LogWriters.Select(writer => writer.Handle));

                        foreach (PersistentLogFileHandle handle in handlesToRelease)
                            ReleaseHandle(handle, false);
                    }

                    ProcessResult streamResult = AssignWriter(logFile, out PersistentLogFileWriter writer);

                    switch (streamResult)
                    {
                        case ProcessResult.Success:
                            OnLogMessageReceived(request.Data);

                            try
                            {
                                var fileLock = logFile.Properties.FileLock;

                                lock (fileLock)
                                {
                                    fileLock.SetActivity(logFile, FileAction.Log);

                                    message = ApplyRules(request.Data);
                                    writer.WriteLine(message);
                                    logFile.Properties.MessagesLoggedThisSession++;
                                }
                            }
                            catch (IOException writeException)
                            {
                                request.Reject(RejectionReason.FailedToWrite);
                                UtilityLogger.LogError("Log write error", writeException);
                                return;
                            }
                            break;
                        case ProcessResult.WaitingToResume:
                            request.Reject(RejectionReason.LogUnavailable);
                            break;
                        case ProcessResult.FailedToCreate:
                            request.Reject(RejectionReason.FailedToWrite);
                            break;
                    }

                    //All checks passed is a complete request
                    request.Complete();
                }
                finally
                {
                    UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
                }
            }
        }

        protected ProcessResult AssignWriter(LogID logFile, out PersistentLogFileWriter writer)
        {
            writer = LogWriters.Find(writer => writer.Handle.FileID.Properties.HasID(logFile));

        retry:
            PersistentLogFileHandle writeHandle;
            if (writer == null)
            {
                writeHandle = new PersistentLogFileHandle(logFile);

                //An exception will be thrown if we try to create a StreamWriter with an invalid stream
                if (!writeHandle.IsClosed)
                {
                    writer = new PersistentLogFileWriter(writeHandle);
                    LogWriters.Add(writer);
                    return ProcessResult.Success;
                }
                return ProcessResult.FailedToCreate;
            }

            writeHandle = writer.Handle;

            if (writeHandle.IsClosed)
            {
                if (writeHandle.WaitingToResume)
                    return ProcessResult.WaitingToResume;

                //This writer is no longer useful - time to replace it with a new instance
                writer.Dispose();
                LogWriters.Remove(writer);

                writer = null;
                goto retry;
            }
            return ProcessResult.Success;
        }

        protected void ReleaseHandle(PersistentLogFileHandle handle, bool disposing)
        {
            if (!handle.IsAlive) return;

            //This needs to drop handle immediately if LogWriter is being disposed, or at the next write interval if it is being lazy disposed
            //Currently no way to tell when the next interval will be, so the full interval period is used here
            int disposalDelay = disposing ? 0 : WriteInterval;

            handle.Lifetime.SetDuration(disposalDelay);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                FlushTimer.Dispose();
                FlushTimer = null;

                Flush();

                foreach (PersistentLogFileHandle handle in LogWriters.Select(writer => writer.Handle))
                    ReleaseHandle(handle, disposing);
                LogWriters = null;
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
            int handleIndex = LogWriters.FindIndex(writer => writer.Handle == handle);

            if (handleIndex != -1)
                LogWriters.RemoveAt(handleIndex);
        }
    }
}
