using LogUtils.Enums;
using LogUtils.Requests;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace LogUtils
{
    public class TimedLogWriter : LogWriter, IDisposable
    {
        protected bool IsDisposed;

        protected List<PersistentLogFileWriter> LogWriters = new List<PersistentLogFileWriter>();

        /// <summary>
        /// This task handles writing to file, on a fixed interval off the main thread
        /// </summary>
        public Task WriteTask;

        public TimeSpan WriteInterval
        {
            get => WriteTask.WaitTimeInterval;
            set => WriteTask.WaitTimeInterval = value;
        }

        /// <summary>
        /// The flush interval in milliseconds
        /// </summary>
        public const int INTERVAL_DEFAULT = 2000;

        /// <summary>
        /// Constructs a TimedLogWriter instance
        /// </summary>
        /// <param name="writeInterval">The flush interval in milliseconds</param>
        /// <exception cref="ArgumentOutOfRangeException">The flush interval is an invalid value</exception>
        public TimedLogWriter(int writeInterval = INTERVAL_DEFAULT)
        {
            if (writeInterval <= 0)
                throw new ArgumentOutOfRangeException("Write interval must be greater than zero");

            TimeSpan taskInterval = TimeSpan.FromMilliseconds(writeInterval);

            WriteTask = LogTasker.Schedule(new Task(ScheduleFlush, taskInterval)
            {
                Name = "LogWriter",
                IsContinuous = true
            });
            WriteInterval = taskInterval;
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
                    throw new ObjectDisposedException("Cannot access a disposed object");

                var activeWriters = LogWriters.Where(w => w.CanWrite);

                foreach (var writer in activeWriters)
                {
                    LogID handleID = writer.Handle.FileID;
                    var fileLock = handleID.Properties.FileLock;

                    using (fileLock.Acquire())
                    {
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(ex);
            }
        }

        public void ScheduleFlush()
        {
            ThreadPool.QueueUserWorkItem((object writerObj) =>
            {
                TimedLogWriter writer = (TimedLogWriter)writerObj;
                writer.Flush();
            }, this);
        }

        protected override void WriteToFile(LogRequest request)
        {
            LogID logFile = request.Data.ID;
            string message = ApplyRules(request.Data);

            //Get locally controlled LogIDs from the logger, and compare against the persistent file handles managed by the LogWriter
            if (request.Host != null)
            {
                IEnumerable<PersistentLogFileHandle> handlesToRelease = request.Host.GetUnusedHandles(LogWriters.Select(writer => writer.Handle));

                foreach (PersistentLogFileHandle handle in handlesToRelease)
                    ReleaseHandle(handle, false);
            }

            bool writeCompleted = false;
            var fileLock = logFile.Properties.FileLock;

            try
            {
                fileLock.Acquire();

                ProcessResult streamResult = AssignWriter(logFile, out PersistentLogFileWriter writer);

                //Handle request rejection, and message receive events
                bool canReceiveMessage = false;
                switch (streamResult)
                {
                    case ProcessResult.Success:
                        {
                            canReceiveMessage = true;
                            break;
                        }
                    case ProcessResult.FailedToCreate:
                        {
                            canReceiveMessage = true;
                            request.Reject(RejectionReason.FailedToWrite);
                            break;
                        }
                    case ProcessResult.WaitingToResume:
                        {
                            request.Reject(RejectionReason.LogUnavailable);
                            break;
                        }
                }

                if (canReceiveMessage)
                {
                    OnLogMessageReceived(request.Data);

                    if (streamResult != ProcessResult.Success)
                        throw new IOException("Unable to create stream");

                    MessageBuffer writeBuffer = request.Data.Properties.WriteBuffer;

                    //The buffer always gets written to file before the request message
                    if (writeBuffer.HasContent)
                        writer.WriteLine(writeBuffer);

                    //Stream is ready to write the message
                    writer.WriteLine(message);

                    request.Complete();
                    logFile.Properties.MessagesHandledThisSession++;

                    writeCompleted = true;
                }
            }
            catch (IOException writeException)
            {
                UtilityLogger.LogError("Log write error", writeException);
            }
            finally
            {
                //This should account for uncaught exceptions, but still allow temporary stream interrupted requests to be retried
                if (!writeCompleted)
                {
                    if (request.Status != RequestStatus.Rejected)
                        request.Reject(RejectionReason.FailedToWrite);

                    if (request.UnhandledReason == RejectionReason.FailedToWrite)
                    {
                        OnFailedToWrite(request.Data);
                        logFile.Properties.MessagesHandledThisSession++;
                    }
                }
                fileLock.Release();
            }
        }

        protected ProcessResult AssignWriter(LogID logFile, out PersistentLogFileWriter writer)
        {
            writer = FindWriter(logFile);

            //Ensure that the writer we found will not be disposed during assignment
            if (writer != null)
            {
                if (!writer.CanWrite)
                {
                    UtilityLogger.DebugLog("Writer rejected due to impermissible write state");
                    writer = null;
                }
            }

        retry:
            PersistentLogFileHandle writeHandle;
            if (writer == null)
            {
                writeHandle = new PersistentLogFileHandle(logFile);

                //An exception will be thrown if we try to create a StreamWriter with an invalid stream
                if (!writeHandle.IsClosed)
                {
                    writer = new PersistentLogFileWriter(writeHandle)
                    {
                        AutoFlush = false
                    };
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
            else if (writeHandle.Lifetime.TimeRemaining != LifetimeDuration.Infinite)
            {
                UtilityLogger.Log("Lifetime of filestream has been extended");
                writeHandle.Lifetime.SetDuration(LifetimeDuration.Infinite);
            }
            return ProcessResult.Success;
        }

        protected PersistentLogFileWriter FindWriter(LogID logFile)
        {
            return LogWriters.Find(writer => writer.Handle.FileID.Equals(logFile));
        }

        protected void ReleaseHandle(PersistentLogFileHandle handle, bool disposing)
        {
            if (!handle.IsAlive) return;

            //This needs to drop handle immediately if LogWriter is being disposed, or at the next write interval if it is being lazy disposed
            int disposalDelay = disposing ? 0 : (int)WriteTask.TimeUntilNextActivation().TotalMilliseconds;

            handle.Lifetime.SetDuration(disposalDelay);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            try
            {
                WriteTask.RunOnceAndEnd(true);
            }
            catch
            {
            }

            if (disposing)
            {
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

        ~TimedLogWriter()
        {
            Dispose(disposing: false);
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
