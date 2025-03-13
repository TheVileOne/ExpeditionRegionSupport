using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Properties;
using LogUtils.Requests;
using System;
using System.IO;
using System.Linq;

namespace LogUtils
{
    public class LogWriter : ILogWriter
    {
        private static SharedField<ILogWriter> _writer;
        private static SharedField<QueueLogWriter> _jollyWriter;

        public static ILogWriter Writer
        {
            get
            {
                if (_writer == null)
                {
                    _writer = UtilityCore.DataHandler.GetField<ILogWriter>("logwriter");

                    if (_writer.Value == null)
                        _writer.Value = new LogWriter();
                }
                return _writer.Value;
            }
        }

        public static QueueLogWriter JollyWriter
        {
            get
            {
                if (_jollyWriter == null)
                {
                    _jollyWriter = UtilityCore.DataHandler.GetField<QueueLogWriter>("logwriter_jolly");

                    if (_jollyWriter.Value == null)
                        _jollyWriter.Value = new QueueLogWriter();
                }
                return _jollyWriter.Value;
            }
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

            LogID logFile = request.Data.ID;
            MessageBuffer buffer = request.Data.Properties.WriteBuffer;

            var fileLock = logFile.Properties.FileLock;

            //Lock is used to ensure that no messages end up added in the wrong order, but this isn't the best place to lock.
            //File lock should be applied to the entire batch in the case multiple requests need to be added to the buffer at the same time
            using (fileLock.Acquire())
            {
                fileLock.SetActivity(logFile, FileAction.Buffering);

                string message = ApplyRules(request.Data);
                buffer.AppendMessage(message);

                logFile.Properties.MessagesHandledThisSession++;

                //Message has been delivered to the write buffer, and will eventually be written to file - consider the request complete here
                request.Complete();
            }
        }

        public void WriteToFile(LogID logFile, string message)
        {
            RequestType requestType = logFile.IsGameControlled ? RequestType.Game : RequestType.Local;
            LogMessageEventArgs logEventData = new LogMessageEventArgs(logFile, message);

            WriteFrom(new LogRequest(requestType, logEventData));
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

            OnLogMessageReceived(request.Data);

            LogID logFile = request.Data.ID;
            string message = ApplyRules(request.Data);

            bool writeCompleted = false;
            var fileLock = logFile.Properties.FileLock;

            StreamWriter writer = null;
            try
            {
                fileLock.Acquire();
                fileLock.SetActivity(logFile, FileAction.Write);

                ProcessResult streamResult = AssignWriter(logFile, out writer);

                if (streamResult != ProcessResult.Success)
                    throw new IOException("Unable to create stream");

                writer.WriteLine(message);

                request.Complete();
                logFile.Properties.MessagesHandledThisSession++;
                writeCompleted = true;
            }
            catch (IOException writeException)
            {
                UtilityLogger.LogError("Log write error", writeException);
            }
            finally
            {
                if (ShouldCloseWriterAfterUse && writer != null)
                    writer.Close();

                if (!writeCompleted)
                {
                    request.Reject(RejectionReason.FailedToWrite);

                    OnFailedToWrite(request.Data);
                    logFile.Properties.MessagesHandledThisSession++;
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

        public virtual string ApplyRules(LogMessageEventArgs logEventData)
        {
            string message = logEventData.Message;
            var activeRules = logEventData.Properties.Rules.Where(r => r.IsEnabled);

            foreach (LogRule rule in activeRules)
                rule.Apply(ref message, logEventData);
            return message;
        }

        protected void OnFailedToWrite(LogMessageEventArgs messageData)
        {
            LogID logFile = messageData.ID;

            logFile.Properties.FileLock.SetActivity(logFile, FileAction.Buffering);

            string message = ApplyRules(messageData);
            logFile.Properties.WriteBuffer.AppendMessage(message);
        }

        protected virtual void OnLogMessageReceived(LogMessageEventArgs messageData)
        {
            UtilityEvents.OnMessageReceived?.Invoke(messageData);
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
        public string ApplyRules(LogMessageEventArgs logEventData);
        internal void WriteFrom(LogRequest request);
        internal void WriteToFile(LogID logFile, string message);
    }
}
