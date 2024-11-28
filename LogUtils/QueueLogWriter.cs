using LogUtils.Enums;
using LogUtils.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils
{
    /// <summary>
    /// This writer class imitates logging functionality exhibited by JollyCoop logger mainly in that all log messages are
    /// placed in a queue and logged at the end of a Rain World frame
    /// </summary>
    public class QueueLogWriter : LogWriter
    {
        internal Queue<LogMessageEventArgs> LogCache = new Queue<LogMessageEventArgs>();

        public override void WriteFrom(LogRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.WriteInProcess();

            if (request.ThreadCanWrite)
                WriteToBuffer(request);
        }

        protected override void WriteToBuffer(LogRequest request)
        {
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

                if (!InternalWriteToBuffer(request.Data))
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

        internal bool InternalWriteToBuffer(LogMessageEventArgs logEventData)
        {
            OnLogMessageReceived(logEventData);

            LogCache.Enqueue(logEventData);
            return true;
        }

        protected override void WriteToFile(LogRequest request)
        {
            throw new NotSupportedException();
        }

        protected override bool PrepareLogFile(LogID logFile)
        {
            //Does not attempt to create the file. Code is handled in a different place
            return logFile.Properties.CanBeAccessed;
        }

        /// <summary>
        /// Writes all messages in the queue to file
        /// </summary>
        public void Flush()
        {
            while (LogCache.Count > 0)
            {
                var logEntry = LogCache.Dequeue();

                StreamWriter writer = null;
                try
                {
                    var fileLock = logEntry.Properties.FileLock;

                    lock (fileLock)
                    {
                        fileLock.SetActivity(logEntry.ID, FileAction.Write);

                        ProcessResult streamResult = AssignWriter(logEntry.ID, out writer);

                        if (streamResult != ProcessResult.Success)
                            throw new IOException("Unable to create stream");

                        bool fileChanged;
                        do
                        {
                            string message = logEntry.Message;

                            //Behavior taken from JollyCoop, shouldn't be necessary if error category is already going to display
                            if (!logEntry.Properties.ShowCategories.IsEnabled && LogCategory.IsErrorCategory(logEntry.Category))
                                message = "[ERROR] " + message;

                            message = ApplyRules(logEntry);
                            writer.WriteLine(message);
                            logEntry.ID.Properties.MessagesLoggedThisSession++;

                            //Keep StreamWriter open while LogID remains unchanged
                            fileChanged = !LogCache.Any() || LogCache.Peek().ID != logEntry.ID;

                            if (!fileChanged)
                                logEntry = LogCache.Dequeue();
                        }
                        while (!fileChanged);
                    }
                }
                catch (IOException writeException)
                {
                    ExceptionInfo exceptionInfo = new ExceptionInfo(writeException);

                    if (!RWInfo.CheckExceptionMatch(logEntry.ID, exceptionInfo)) //Only log unreported exceptions
                    {
                        logEntry = new LogMessageEventArgs(logEntry.ID, writeException, LogCategory.Error);

                        RWInfo.ReportException(logEntry.ID, exceptionInfo);

                        OnLogMessageReceived(logEntry);
                        LogCache.Enqueue(logEntry);
                    }
                    break;
                }
                finally
                {
                    if (ShouldCloseWriterAfterUse && writer != null)
                        writer.Close();
                }
            }
        }
    }
}
