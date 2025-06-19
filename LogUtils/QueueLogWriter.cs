using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Properties.Formatting;
using LogUtils.Requests;
using System;
using System.Collections.Generic;
using System.IO;

namespace LogUtils
{
    /// <summary>
    /// This writer class imitates logging functionality exhibited by JollyCoop logger mainly in that all log messages are
    /// placed in a queue and logged at the end of a Rain World frame
    /// </summary>
    public class QueueLogWriter : LogWriter, IFlushable
    {
        internal Queue<LogRequestEventArgs> LogCache = new Queue<LogRequestEventArgs>();

        public QueueLogWriter()
        {
            WriteHandler = WriteToBuffer;
        }

        public override string ApplyRules(LogRequestEventArgs messageData)
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

        protected void EnqueueMessage(LogRequestEventArgs messageData)
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

                fileLock.Acquire();
                fileLock.SetActivity(logEntry.ID, FileAction.Write);

                ProcessResult streamResult = TryAssignWriter(logEntry.ID, out StreamWriter writer);

                try
                {
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
                    OnWriteException(logEntry.ID, ex);
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

        protected override void OnWriteException(LogID logFile, Exception exception)
        {
            ExceptionInfo exceptionInfo = new ExceptionInfo(exception);

            if (!RWInfo.CheckExceptionMatch(logFile, exceptionInfo)) //Only log unreported exceptions
            {
                var errorEntry = new LogRequestEventArgs(logFile, exception, LogCategory.Error);

                RWInfo.ReportException(logFile, exceptionInfo);
                EnqueueMessage(errorEntry);
            }

            base.OnWriteException(logFile, exception);
        }
    }
}
