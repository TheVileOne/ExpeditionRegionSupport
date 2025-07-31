using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Helpers.Extensions;
using LogUtils.Requests;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using DotNetTask = System.Threading.Tasks.Task;

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
                    throw new ObjectDisposedException(nameof(TimedLogWriter));

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
                UtilityLogger.DebugLog(ex);
                UtilityLogger.LogError(ex);
            }
        }

        public DotNetTask ScheduleFlush()
        {
            return DotNetTask.Run(() =>
            {
                if (!IsDisposed)
                    Flush();
            });
        }

        /// <inheritdoc/>
        protected override void WriteToFile(LogRequest request)
        {
            LogID logFile = request.Data.ID;

            //Get locally controlled LogIDs from the logger, and compare against the persistent file handles managed by the LogWriter
            if (request.Host != null)
            {
                IEnumerable<PersistentLogFileHandle> handlesToRelease = request.Host.GetUnusedHandles(LogWriters.Select(writer => writer.Handle));

                foreach (PersistentLogFileHandle handle in handlesToRelease)
                    ReleaseHandle(handle, false);
            }

            bool errorHandled = false;
            var fileLock = logFile.Properties.FileLock;

            fileLock.Acquire();

            ProcessResult streamResult = TryAssignWriter(logFile, out StreamWriter writer);

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

            try
            {
                if (canReceiveMessage)
                {
                    OnLogMessageReceived(request.Data);

                    if (streamResult != ProcessResult.Success)
                        throw new IOException("Unable to create stream");

                    SendToWriter(writer, request.Data);
                    request.Complete();
                }
            }
            catch (Exception ex)
            {
                errorHandled = true;
                OnWriteException(logFile, ex);
            }
            finally
            {
                if (errorHandled)
                    request.Reject(RejectionReason.FailedToWrite);

                if (request.UnhandledReason == RejectionReason.FailedToWrite)
                    SendToBuffer(request.Data);

                fileLock.Release();
            }
        }

        /// <inheritdoc/>
        protected override ProcessResult AssignWriter(LogID logFile, out StreamWriter writer)
        {
            PersistentLogFileWriter localWriter = FindWriter(logFile);

            try
            {
                //Ensure that the writer we found will not be disposed during assignment
                if (localWriter != null)
                {
                    if (!localWriter.CanWrite)
                    {
                        UtilityLogger.DebugLog("Writer rejected due to impermissible write state");
                        localWriter = null;
                    }
                }

            retry:
                PersistentLogFileHandle writeHandle;
                if (localWriter == null)
                {
                    writeHandle = new PersistentLogFileHandle(logFile);

                    //An exception will be thrown if we try to create a StreamWriter with an invalid stream
                    if (!writeHandle.IsClosed)
                    {
                        localWriter = new PersistentLogFileWriter(writeHandle)
                        {
                            AutoFlush = false
                        };
                        LogWriters.Add(localWriter);
                        return ProcessResult.Success;
                    }
                    return ProcessResult.FailedToCreate;
                }

                writeHandle = localWriter.Handle;

                if (writeHandle.IsClosed)
                {
                    if (writeHandle.WaitingToResume)
                        return ProcessResult.WaitingToResume;

                    //This writer is no longer useful - time to replace it with a new instance
                    localWriter.Dispose();
                    LogWriters.Remove(localWriter);

                    localWriter = null;
                    goto retry;
                }
                else if (writeHandle.Lifetime.TimeRemaining != LifetimeDuration.Infinite)
                {
                    UtilityLogger.Log("Lifetime of filestream has been extended");
                    writeHandle.Lifetime.SetDuration(LifetimeDuration.Infinite);
                }
                return ProcessResult.Success;
            }
            finally
            {
                writer = localWriter;
            }
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

            if (LogTasker.IsRunning)
            {
                try
                {
                    if (WriteTask.PossibleToRun)
                    {
                        //Get a handle to the active task, so we can await it later
                        using (TaskHandle waitHandle = WriteTask.GetAsyncHandle())
                        {
                            WriteTask.RunOnceAndEnd(true);

                            //If we don't block here, flush operation will happen too late, and the dispose state will forbid it
                            waitHandle.BlockUntilTaskEnds(frequency: 5, timeout: 50);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ICollection<Exception> exceptions = ExceptionUtils.ExtractAggregate(ex);

                    if (exceptions.Count == 0) //Exception is not an aggregate
                        exceptions.Add(ex);

                    if (exceptions.ContainsType<TimeoutException>())
                        logError("Task took too long to complete");
                    else
                        logError("Task attempt did not finish - cancelling task");
                    logError(ex);

                    try
                    {
                        WriteTask.Cancel();
                    }
                    catch (AggregateException ex2)
                    {
                        logError("Task failed to end");
                        logError(ex2);
                    }
                }

                static void logError(object messageObj)
                {
                    UtilityLogger.DebugLog(messageObj);
                    Debug.LogError(messageObj);
                }
            }
            else
            {
                Flush();
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

        /// <inheritdoc/>
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
