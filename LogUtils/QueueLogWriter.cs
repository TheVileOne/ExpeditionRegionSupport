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

        public QueueLogWriter()
        {
            WriteHandler = WriteToBuffer;
        }

        public override string ApplyRules(LogMessageEventArgs messageData)
        {
            LogID logFile = messageData.ID;
            LogRule headerRule = logFile.Properties.ShowCategories;

            //All requests that are handled by this type of LogWriter shall display an error header even if the LogID doesn't have all headers enabled
            if (!headerRule.IsEnabled && LogCategory.IsErrorCategory(messageData.Category))
            {
                headerRule = new ErrorsOnlyHeaderRule(true);
                logFile.Properties.Rules.SetTemporaryRule(headerRule);
            }

            try
            {
                return base.ApplyRules(messageData);
            }
            finally
            {
                //The rule that we added gets removed after application
                if (headerRule.IsTemporary && headerRule is ErrorsOnlyHeaderRule)
                    logFile.Properties.Rules.RemoveTemporaryRule(headerRule);
            }
        }

        protected override void WriteToBuffer(LogRequest request)
        {
            if (!PrepareLogFile(request.Data.ID))
            {
                request.Reject(RejectionReason.LogUnavailable);
                return;
            }

            EnqueueMessage(request.Data);
            request.Complete();
        }

        protected void EnqueueMessage(LogMessageEventArgs messageData)
        {
            OnLogMessageReceived(messageData);
            LogCache.Enqueue(messageData);
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

                bool errorHandled = false;
                var fileLock = logEntry.Properties.FileLock;

                StreamWriter writer = null;
                try
                {
                    fileLock.Acquire();
                    fileLock.SetActivity(logEntry.ID, FileAction.Write);

                    ProcessResult streamResult = AssignWriter(logEntry.ID, out writer);

                    if (streamResult != ProcessResult.Success)
                        throw new IOException("Unable to create stream");

                    bool fileChanged;
                    do
                    {
                        SendToWriter(writer, logEntry);

                        //Keep StreamWriter open while LogID remains unchanged
                        fileChanged = LogCache.Count == 0 || LogCache.Peek().ID != logEntry.ID;

                        if (!fileChanged)
                            logEntry = LogCache.Dequeue();
                    }
                    while (!fileChanged);
                }
                catch (Exception ex)
                {
                    errorHandled = true;
                    OnWriteException(ex, logEntry);
                    break; //Break out of process loop when an exception occurs
                }
                finally
                {
                    if (ShouldCloseWriterAfterUse && writer != null)
                        writer.Close();

                    if (errorHandled)
                        SendToBuffer(logEntry);

                    fileLock.Release();
                }
            }
        }

        protected override void OnWriteException(Exception exception, LogMessageEventArgs messageData)
        {
            ExceptionInfo exceptionInfo = new ExceptionInfo(exception);

            if (!RWInfo.CheckExceptionMatch(messageData.ID, exceptionInfo)) //Only log unreported exceptions
            {
                var errorEntry = new LogMessageEventArgs(messageData.ID, exception, LogCategory.Error);

                RWInfo.ReportException(errorEntry.ID, exceptionInfo);
                EnqueueMessage(errorEntry);
            }

            base.OnWriteException(exception, messageData);
        }
    }
}
