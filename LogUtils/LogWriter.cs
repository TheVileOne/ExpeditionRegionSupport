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
                //Assume that thread is allowed to write if we get past this point
                try
                {
                    if (!LogFilter.IsAllowed(request.Data))
                    {
                        request.Reject(RejectionReason.FilterMatch);
                        return;
                    }

                    MessageBuffer buffer = request.Data.Properties.WriteBuffer;

                    if (buffer.IsBuffering)
                    {
                        WriteToBuffer(request);
                        return;
                    }

                    WriteHandler.Invoke(request);
                }
                finally
                {
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

            bool writeCompleted = false;
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

                    writeCompleted = true;
                }
            }
            catch (IOException writeException)
            {
                UtilityLogger.LogError("Log write error", writeException);
            }
            finally
            {
                if (ShouldCloseWriterAfterUse && writer != null)
                    writer.Close();

                //This should account for uncaught exceptions, but still allow temporary stream interrupted requests to be retried
                if (!writeCompleted)
                {
                    if (request.Status != RequestStatus.Rejected)
                        request.Reject(RejectionReason.FailedToWrite);

                    if (request.UnhandledReason == RejectionReason.FailedToWrite)
                        SendToBuffer(request.Data);
                }
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

            //Stream is ready to write the message
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
