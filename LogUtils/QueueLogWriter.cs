using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Properties;
using LogUtils.Requests;
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
    public class QueueLogWriter : LogWriter, IFlushable
    {
        internal Queue<LogMessageEventArgs> LogCache = new Queue<LogMessageEventArgs>();

        public override string ApplyRules(LogMessageEventArgs logEventData)
        {
            LogID logFile = logEventData.ID;
            LogRule headerRule = logFile.Properties.ShowCategories;

            //All requests that are handled by this type of LogWriter shall display an error header even if the LogID doesn't have all headers enabled
            if (!headerRule.IsEnabled && LogCategory.IsErrorCategory(logEventData.Category))
            {
                headerRule = new ErrorsOnlyHeaderRule(true);
                logFile.Properties.Rules.SetTemporaryRule(headerRule);
            }

            try
            {
                return base.ApplyRules(logEventData);
            }
            finally
            {
                //The rule that we added gets removed after application
                if (headerRule.IsTemporary && headerRule is ErrorsOnlyHeaderRule)
                    logFile.Properties.Rules.RemoveTemporaryRule(headerRule);
            }
        }

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

                    using (fileLock.Acquire())
                    {
                        fileLock.SetActivity(logEntry.ID, FileAction.Write);

                        ProcessResult streamResult = AssignWriter(logEntry.ID, out writer);

                        if (streamResult != ProcessResult.Success)
                            throw new IOException("Unable to create stream");

                        string message;
                        bool fileChanged;
                        do
                        {
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
