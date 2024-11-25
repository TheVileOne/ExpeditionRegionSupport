using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers;
using LogUtils.Properties;
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
        /// Ends the current log session, and prepares a new one
        /// </summary>
        public void ResetFile(LogID logFile)
        {
            logFile.Properties.EndLogSession();

            try
            {
                File.Delete(logFile.Properties.CurrentFilePath);
                logFile.Properties.FileExists = false;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(null, new IOException("Unable to delete log file", ex));
            }
            PrepareLogFile(logFile);
        }

        public virtual void WriteFrom(LogRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.WriteInProcess();

            if (request.ThreadCanWrite)
                WriteToFile(request);
        }

        protected virtual void WriteToBuffer(LogRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to write the most recently requested message to file
        /// </summary>
        protected virtual void WriteToFile(LogRequest request)
        {
            request.WriteInProcess();

            if (request.ThreadCanWrite)
            {
                //Assume that thread is allowed to write if we get past this point
                try
                {
                    if (LogFilter.CheckFilterMatch(request.Data.ID, request.Data.Message))
                    {
                        request.Reject(RejectionReason.FilterMatch);
                        return;
                    }

                    if (!PrepareLogFile(request.Data.ID))
                    {
                        request.Reject(RejectionReason.LogUnavailable);
                        return;
                    }

                    if (!InternalWriteToFile(request.Data))
                    {
                        request.Reject(RejectionReason.FailedToWrite);
                        return;
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

        public void WriteToFile(LogID logFile, string message)
        {
            RequestType requestType = logFile.IsGameControlled ? RequestType.Game : RequestType.Local;
            LogMessageEventArgs logEventData = new LogMessageEventArgs(logFile, message);

            WriteFrom(new LogRequest(requestType, logEventData));
        }

        protected virtual bool PrepareLogFile(LogID logFile)
        {
            return LogFile.TryCreate(logFile);
        }

        internal bool InternalWriteToFile(LogMessageEventArgs logEventData)
        {
            OnLogMessageReceived(logEventData);

            LogID logFile = logEventData.ID;
            string message = logEventData.Message;

            try
            {
                var fileLock = logFile.Properties.FileLock;

                lock (fileLock)
                {
                    fileLock.SetActivity(logFile, FileAction.Log);

                    using (FileStream stream = LogFile.Open(logFile))
                    {
                        //if (!logFile.Properties.FileExists)
                        //    throw new IOException("Unable to create log file");

                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            message = ApplyRules(logEventData);
                            writer.WriteLine(message);
                            logFile.Properties.MessagesLoggedThisSession++;
                        }
                    }
                }
                return true;
            }
            catch (IOException ex)
            {
                UtilityLogger.LogError("Log write error", ex);
                return false;
            }
        }

        public string ApplyRules(LogMessageEventArgs logEventData)
        {
            string message = logEventData.Message;
            var activeRules = logEventData.Properties.Rules.Where(r => r.IsEnabled);

            foreach (LogRule rule in activeRules)
                rule.Apply(ref message, logEventData);
            return message;
        }

        protected virtual void OnLogMessageReceived(LogMessageEventArgs e)
        {
            UtilityEvents.OnMessageReceived?.Invoke(e);
        }
    }

    public interface ILogWriter
    {
        public void ResetFile(LogID logFile);
        internal void WriteFrom(LogRequest request);
        internal void WriteToFile(LogID logFile, string message);
        internal string ApplyRules(LogMessageEventArgs logEventData);
    }
}
