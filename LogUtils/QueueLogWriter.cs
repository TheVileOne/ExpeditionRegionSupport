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
                var fileLock = logEntry.Properties.FileLock;

                using (fileLock.Acquire())
                {
                    fileLock.SetActivity(logEntry.ID, FileAction.Write);

                    string message = null;
                    StreamWriter writer = null;
                    try
                    {
                        ProcessResult streamResult = AssignWriter(logEntry.ID, out writer);

                        if (streamResult != ProcessResult.Success)
                            throw new IOException("Unable to create stream");

                        bool fileChanged;
                        do
                        {
                            message = ApplyRules(logEntry);
                            writer.WriteLine(message);
                            logEntry.ID.Properties.MessagesHandledThisSession++;

                            //Keep StreamWriter open while LogID remains unchanged
                            fileChanged = !LogCache.Any() || LogCache.Peek().ID != logEntry.ID;

                            if (!fileChanged)
                                logEntry = LogCache.Dequeue();
                        }
                        while (!fileChanged);
                    }
                    catch (IOException writeException)
                    {
                        //Error happened before rules could be applied to the first entry
                        if (message == null)
                            message = ApplyRules(logEntry);

                        fileLock.SetActivity(logEntry.ID, FileAction.Buffering);

                        //Since we cannot add it back to the queue in the same place it started, use the buffer system from the other writers to prioritize
                        //it on the next write frame
                        logEntry.Properties.MessagesHandledThisSession++;
                        logEntry.Properties.WriteBuffer.AppendMessage(message);

                        ExceptionInfo exceptionInfo = new ExceptionInfo(writeException);

                        if (!RWInfo.CheckExceptionMatch(logEntry.ID, exceptionInfo)) //Only log unreported exceptions
                        {
                            var errorEntry = new LogMessageEventArgs(logEntry.ID, writeException, LogCategory.Error);

                            RWInfo.ReportException(errorEntry.ID, exceptionInfo);
                            EnqueueMessage(errorEntry);
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
}
