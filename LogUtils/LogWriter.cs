using LogUtils.Console;
using LogUtils.Diagnostics.Tools;
using LogUtils.Enums;
using LogUtils.Enums.FileSystem;
using LogUtils.Events;
using LogUtils.Formatting;
using LogUtils.Helpers;
using LogUtils.Requests;
using LogUtils.Threading;
using LogUtils.Timers;
using System;
using System.IO;
using System.Linq;

namespace LogUtils
{
    public class LogWriter : ILogWriter
    {
        /// <summary>
        /// Writer used by default for most Logger implementations
        /// </summary>
        public static ILogWriter Writer = new LogWriter();

        /// <summary>
        /// Writer used by default for JollyCoop
        /// </summary>
        public static QueueLogWriter JollyWriter = new QueueLogWriter();

        /// <summary>
        /// Is this writer recognized by the assembly to be available for any Logger implementation to use
        /// </summary>
        public static bool IsCachedWriter(ILogWriter writer)
        {
            return writer == Writer || writer == JollyWriter;
        }

        private LogMessageFormatter _formatter;

        public LogMessageFormatter Formatter
        {
            get => _formatter ?? LogMessageFormatter.Default;
            set => _formatter = value;
        }

        /// <summary>
        /// A flag that prevents StreamWriter from being closed
        /// </summary>
        protected bool ShouldCloseWriterAfterUse = true;

        /// <summary>
        /// Primary process delegate for handling a write request
        /// </summary>
        protected Action<LogRequest> WriteHandler;

        public LogWriter()
        {
            WriteHandler = WriteToFile;
        }

        /// <summary>
        /// Processes a write request
        /// </summary>
        public void WriteFrom(LogRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.WriteInProcess();

            if (!request.ThreadCanWrite) return;

            LogProfiler profiler = request.Data.Properties.Profiler;
            MessageBuffer writeBuffer = request.Data.Properties.WriteBuffer;

            //Assume that thread is allowed to write if we get to this point
            try
            {
                if (!LogFilter.IsAllowed(request.Data))
                {
                    request.Reject(RejectionReason.FilterMatch);
                    return;
                }

                if (!request.Data.ID.IsEnabled)
                {
                    request.Reject(RejectionReason.LogDisabled);
                    return;
                }

                if (request.Type == RequestType.Console)
                {
                    WriteToConsole(request);
                    return; //Buffering does not apply to console requests
                }

                if (request.Data.PendingConsoleIDs.Any())
                    WriteToConsole(request);

                if (!RainWorldInfo.IsShuttingDown)
                {
                    /*
                     * In order to get back into a write qualifying range, we run a new calculation with zero new accumulated messages until the average logging rate
                     * returns back into the acceptable range
                     */
                    if (writeBuffer.IsBuffering || profiler.IsReadyToAnalyze)
                    {
                        //Perform logging average calculations on message data
                        profiler.UpdateCalculations();

                        double averageWriteTime = profiler.AverageLogRate.TotalMilliseconds;

                        bool highVolumePeriod = averageWriteTime < profiler.LogRateThreshold; //Lower value means higher rate

                        if (highVolumePeriod && !writeBuffer.IsBuffering)
                        {
                            UtilityLogger.DebugLog($"Average logging time: {averageWriteTime} ms per message");

                            //High volume periods are only counted when not buffering
                            profiler.PeriodsUnderHighVolume++;
                        }

                        if (highVolumePeriod && profiler.ShouldUseBuffer)
                        {
                            if (!writeBuffer.IsEntered(BufferContext.HighVolume))
                            {
                                ApplyBufferContext(request.Data.ID, BufferContext.HighVolume);
                                UtilityLogger.DebugLog($"Activity listeners {writeBuffer.ActivityListeners.Count}");
                            }
                            profiler.BufferedFrameCount++;
                        }
                        else if (writeBuffer.IsEntered(BufferContext.HighVolume) && writeBuffer.SetState(false, BufferContext.HighVolume))
                        {
                            profiler.Restart();
                        }
                    }

                    if (writeBuffer.IsBuffering)
                    {
                        WriteToBuffer(request);
                        return;
                    }
                }
                else
                {
                    //Buffer should be handled immediately, while Rain World is shutting down
                    WriteFromBuffer(request.Data.ID, TimeSpan.Zero, respectBufferState: false);
                }

                WriteHandler.Invoke(request);
            }
            finally
            {
                if (!request.IsCompleteOrRejected) //Something horribly went wrong for this code to run
                    request.Reject(RejectionReason.FailedToWrite);

                if (!writeBuffer.IsBuffering || RainWorldInfo.IsShuttingDown)
                    profiler.MessagesSinceLastSampling++;
                UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
            }
        }

        private static readonly TimeSpan initialWaitInterval = TimeSpan.FromMilliseconds(25);
        private static readonly TimeSpan maxWaitInterval = TimeSpan.FromSeconds(5);

        void IBufferHandler.WriteFromBuffer(LogID logFile, TimeSpan waitTime, bool respectBufferState)
        {
            WriteFromBuffer(logFile, waitTime, respectBufferState);
        }

        protected void ApplyBufferContext(LogID logFile, BufferContext context)
        {
            LogProfiler profiler = logFile.Properties.Profiler;
            MessageBuffer writeBuffer = logFile.Properties.WriteBuffer;

            writeBuffer.SetState(true, context);

            PollingTimer listener = writeBuffer.ActivityListeners.FirstOrDefault(l => Equals(l.Tag, context));

            if (listener == null)
            {
                //TODO: This will run on a different thread. It needs to be thread-safe
                listener = writeBuffer.PollForActivity(null, (t, e) =>
                {
                    //Ensure that buffer will disable itself after a short period of time without needing extra log requests to clear the buffer
                    if (writeBuffer.SetState(false, context))
                    {
                        profiler.Restart();
                        WriteFromBuffer(logFile, TimeSpan.Zero);
                    }
                }, initialWaitInterval);
                listener.Tag = context;
            }
        }

        /// <inheritdoc cref="IBufferHandler.WriteFromBuffer(LogID, TimeSpan, bool)"/>
        /// <returns>A handle to the scheduled write task</returns>
        public Task WriteFromBuffer(LogID logFile, TimeSpan waitTime, bool respectBufferState = true)
        {
            if (!UtilityCore.IsControllingAssembly)
                return null;

            MessageBuffer writeBuffer = logFile.Properties.WriteBuffer;
            FileLock fileLock = logFile.Properties.FileLock;

            //TODO: Determine if this should always run through a Task
            if (waitTime <= TimeSpan.Zero)
            {
                //Write implementation will ignore this field - make sure we want that to happen
                if ((!respectBufferState || !writeBuffer.IsBuffering) && tryWrite())
                    return null;

                waitTime = initialWaitInterval;
            }

            Task writeTask = null;

            writeTask = new Task(() =>
            {
                bool taskCompleted = (!respectBufferState || !writeBuffer.IsBuffering) && tryWrite();

                if (taskCompleted)
                    writeTask.IsContinuous = false;
                else
                {
                    //Each failed attempt doubles the wait time for the next attempt up to a maximum of 5 seconds
                    TimeSpan waitInterval = writeTask.WaitTimeInterval.MultiplyBy(2);

                    if (waitInterval > maxWaitInterval)
                        waitInterval = maxWaitInterval;
                    writeTask.WaitTimeInterval = waitInterval;
                }

            }, waitTime);
            writeTask.Name = "BufferWriteTask";
            writeTask.IsContinuous = true;

            LogTasker.Schedule(writeTask);
            return writeTask;

            bool tryWrite()
            {
                if (!writeBuffer.HasContent)
                    return true;

                if (!PrepareLogFile(logFile))
                    return false;

                fileLock.Acquire();
                fileLock.SetActivity(FileAction.Write);

                ProcessResult result = TryAssignWriter(logFile, out StreamWriter writer);

                try
                {
                    if (result != ProcessResult.Success)
                        return false;

                    writer.WriteLine(writeBuffer);
                    writeBuffer.Clear();
                    return true;
                }
                catch (Exception ex)
                {
                    OnWriteException(logFile, ex);
                    return false;
                }
                finally
                {
                    if (ShouldCloseWriterAfterUse && writer != null)
                        writer.Close();

                    fileLock.Release();
                }
            }
        }

        protected virtual void WriteToBuffer(LogRequest request)
        {
            OnLogMessageReceived(request.Data);

            SendToBuffer(request.Data);
            request.Complete(); //LogRequest no longer needs to be processed once its message has been added to the write buffer
        }

        protected virtual void WriteToConsole(LogRequest request)
        {
            LogConsole.HandleRequest(request);
        }

        /// <summary>
        /// Attempts to write the most recently requested message to file
        /// </summary>
        protected virtual void WriteToFile(LogRequest request)
        {
            if (!PrepareLogFile(request.Data.ID))
            {
                request.Reject(RejectionReason.LogUnavailable);
                return;
            }

            LogID logFile = request.Data.ID;

            bool errorHandled = false;
            var fileLock = logFile.Properties.FileLock;

            fileLock.Acquire();
            fileLock.SetActivity(FileAction.Write);

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
                if (ShouldCloseWriterAfterUse && writer != null)
                    writer.Close();

                if (errorHandled)
                    request.Reject(RejectionReason.FailedToWrite);

                if (request.UnhandledReason == RejectionReason.FailedToWrite)
                    SendToBuffer(request.Data);

                fileLock.Release();
            }
        }

        protected virtual bool PrepareLogFile(LogID logFile)
        {
            return LogFile.TryCreate(logFile);
        }

        /// <summary>
        /// Assigns a writer instance for handling a specified log file
        /// </summary>
        protected ProcessResult TryAssignWriter(LogID logFile, out StreamWriter writer)
        {
            try
            {
                return AssignWriter(logFile, out writer);
            }
            catch //Exception should be handled, and reported by caller
            {
                writer = null;
                return ProcessResult.FailedToCreate;
            }
        }

        /// <summary>
        /// Assigns a writer instance for handling a specified log file
        /// </summary>
        protected virtual ProcessResult AssignWriter(LogID logFile, out StreamWriter writer)
        {
            FileStream stream = LogFile.Open(logFile);

            if (stream == null)
            {
                writer = null;
                return ProcessResult.FailedToCreate;
            }
            writer = new StreamWriter(stream);
            return ProcessResult.Success;
        }

        /// <inheritdoc/>
        public virtual string ApplyRules(LogRequestEventArgs messageData)
        {
            return Formatter.Format(messageData);
        }

        protected virtual void OnWriteException(LogID logFile, Exception exception)
        {
            //Do not attempt to log an error to BepInEx when the exception came from attempting to write to BepInEx
            if (logFile != LogID.BepInEx)
                UtilityLogger.LogError("Log write error", exception);

            //Use Unity's API to log the exception, unless that option has already failed
            if (logFile != LogID.Exception)
                UnityEngine.Debug.LogException(exception);
            else
            {
                //TODO: Add fallback process to ensure exceptions are logged
            }
        }

        protected virtual void OnLogMessageReceived(LogRequestEventArgs messageData)
        {
            UtilityEvents.OnMessageReceived?.Invoke(messageData);
        }

        /// <inheritdoc/>
        public void SendToBuffer(LogRequestEventArgs messageData)
        {
            LogID logFile = messageData.ID;

            var fileLock = logFile.Properties.FileLock;

            using (fileLock.Acquire())
            {
                fileLock.SetActivity(FileAction.Buffering);

                messageData.CacheMessageTotal();

                //Keep this inside a lock, we want to ensure that it remains in sync with the MessagesHandled count, which is used for this process
                string message = ApplyRules(messageData);

                logFile.Properties.WriteBuffer.AppendLine(message);
                logFile.Properties.MessagesHandledThisSession++;
            }
        }

        protected void SendToWriter(StreamWriter writer, LogRequestEventArgs messageData)
        {
            MessageBuffer writeBuffer = messageData.Properties.WriteBuffer;

            //The buffer always gets written to file before the request message
            if (writeBuffer.HasContent)
            {
                writer.WriteLine(writeBuffer);
                writeBuffer.Clear();
            }

            messageData.CacheMessageTotal();

            string message = ApplyRules(messageData);

            writer.WriteLine(message);

            if (RainWorldInfo.IsShuttingDown)
                writer.Flush();

            messageData.Properties.MessagesHandledThisSession++;
        }

        public static bool TryDispose(ILogWriter writer)
        {
            if (IsCachedWriter(writer)) //Avoid disposing a shared resource
                return false;

            IDisposable disposable = writer as IDisposable;

            if (disposable != null)
            {
                disposable.Dispose();
                return true;
            }
            return false;
        }

        protected enum ProcessResult
        {
            Success,
            WaitingToResume, //The FileStream is temporarily closed
            FailedToCreate,
        }
    }

    public interface ILogWriter : IBufferHandler
    {
        /// <summary>
        /// Applies rule-defined formatting to a message
        /// </summary>
        string ApplyRules(LogRequestEventArgs messageData);

        /// <summary>
        /// Provides a procedure for writing a message to file
        /// </summary>
        void WriteFrom(LogRequest request);
    }

    public interface IBufferHandler
    {
        /// <summary>
        /// Provides a procedure for adding a message to the WriteBuffer
        /// </summary>
        /// <remarks>Bypasses the log request system - intended to be used as a fallback message handling process</remarks>
        void SendToBuffer(LogRequestEventArgs messageData);

        /// <summary>
        /// Attempts to write content from the message buffer to file after a specified amount of time
        /// </summary>
        /// <remarks>Default behavior: Wait time will double on each failed attempt to write to file (up to a maximum of 5000 ms)</remarks>
        /// <param name="logFile">The file that contains the message buffer</param>
        /// <param name="waitTime">The initial time to wait before writing to file (when set to zero, write attempt will be immediate)</param>
        /// <param name="respectBufferState">Allow the buffer state to determine when to make a write attempt</param>
        void WriteFromBuffer(LogID logFile, TimeSpan waitTime, bool respectBufferState = true);
    }
}
