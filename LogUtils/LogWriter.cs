using LogUtils.Diagnostics;
using LogUtils.Diagnostics.Tools;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Requests;
using System;
using System.IO;

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

            if (request.ThreadCanWrite)
            {
                LogProfiler profiler = request.Data.Properties.Profiler;
                MessageBuffer buffer = request.Data.Properties.WriteBuffer;

                //Assume that thread is allowed to write if we get past this point
                try
                {
                    if (!LogFilter.IsAllowed(request.Data))
                    {
                        request.Reject(RejectionReason.FilterMatch);
                        return;
                    }

                    /*
                     * In order to get back into a write qualifying range, we run a new calculation with zero new accumulated messages until the average logging rate
                     * returns back into the acceptable range
                     */
                    if (buffer.IsBuffering || profiler.IsReadyToAnalyze)
                    {
                        //Perform logging average calculations on message data
                        profiler.UpdateCalculations();

                        double averageWriteTime = profiler.AverageLogRate.TotalMilliseconds;

                        bool highVolumePeriod = averageWriteTime < profiler.LogRateThreshold; //Lower value means higher rate

                        if (highVolumePeriod && !buffer.IsBuffering)
                        {
                            Debug.TestBuffer.AppendLine($"Average logging time: {averageWriteTime} ms per message");

                            //High volume periods are only counted when not buffering
                            profiler.PeriodsUnderHighVolume++;
                        }

                        //TODO: This doesn't account for buffered state being set for some alternative reason
                        if (highVolumePeriod && profiler.ShouldUseBuffer)
                        {
                            buffer.SetState(true, BufferContext.HighVolume);
                            profiler.BufferedFrameCount++;
                        }
                        else if (buffer.SetState(false, BufferContext.HighVolume))
                        {
                            if (profiler.BufferedFrameCount > 0)
                                Debug.TestBuffer.AppendLine($"Buffered {profiler.BufferedFrameCount} messages");

                            profiler.BufferedFrameCount = 0;
                            profiler.PeriodsUnderHighVolume = 0;
                        }
                    }

                    if (buffer.IsBuffering)
                    {
                        WriteToBuffer(request);
                        return;
                    }

                    WriteHandler.Invoke(request);
                }
                finally
                {
                    if (!buffer.IsBuffering)
                        profiler.MessagesSinceLastSampling++;
                    UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
                }
            }
        }

        protected virtual void WriteToBuffer(LogRequest request)
        {
            OnLogMessageReceived(request.Data);

            SendToBuffer(request.Data);
            request.Complete(); //LogRequest no longer needs to be processed once its message has been added to the write buffer
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

            StreamWriter writer = null;
            try
            {
                fileLock.Acquire();
                fileLock.SetActivity(logFile, FileAction.Write);

                ProcessResult streamResult = AssignWriter(logFile, out writer);

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

                    SendToWriter(writer, request.Data);
                    request.Complete();
                }
            }
            catch (Exception ex)
            {
                errorHandled = true;
                OnWriteException(ex, request.Data);
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
        protected ProcessResult AssignWriter(LogID logFile, out StreamWriter writer)
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

        public virtual string ApplyRules(LogMessageEventArgs messageData)
        {
            return Formatter.Format(messageData);
        }

        protected virtual void OnWriteException(Exception exception, LogMessageEventArgs messageData)
        {
            LogID logFile = messageData.ID;

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

        protected virtual void OnLogMessageReceived(LogMessageEventArgs messageData)
        {
            UtilityEvents.OnMessageReceived?.Invoke(messageData);
        }

        public void SendToBuffer(LogMessageEventArgs messageData)
        {
            LogID logFile = messageData.ID;

            var fileLock = logFile.Properties.FileLock;

            using (fileLock.Acquire())
            {
                fileLock.SetActivity(logFile, FileAction.Buffering);

                //Keep this inside a lock, we want to ensure that it remains in sync with the MessagesHandled count, which is used for this process
                string message = ApplyRules(messageData);

                logFile.Properties.WriteBuffer.AppendMessage(message);
                logFile.Properties.MessagesHandledThisSession++;
            }
        }

        protected void SendToWriter(StreamWriter writer, LogMessageEventArgs messageData)
        {
            MessageBuffer writeBuffer = messageData.Properties.WriteBuffer;

            //The buffer always gets written to file before the request message
            if (writeBuffer.HasContent)
            {
                writer.WriteLine(writeBuffer);
                writeBuffer.Clear();
            }

            string message = ApplyRules(messageData);

            writer.WriteLine(message);
            messageData.Properties.MessagesHandledThisSession++;
        }

        protected enum ProcessResult
        {
            Success,
            WaitingToResume, //The FileStream is temporarily closed
            FailedToCreate,
        }
    }

    public interface ILogWriter
    {
        /// <summary>
        /// Applies rule-defined formatting to a message
        /// </summary>
        string ApplyRules(LogMessageEventArgs messageData);

        /// <summary>
        /// Provides a procedure for adding a message to the WriteBuffer
        /// <br><remarks>
        /// Bypasses the LogRequest system - intended to be used as a fallback message handling process
        /// </remarks></br>
        /// </summary>
        void SendToBuffer(LogMessageEventArgs messageData);

        /// <summary>
        /// Provides a procedure for writing a message to file
        /// </summary>
        void WriteFrom(LogRequest request);
    }
}
