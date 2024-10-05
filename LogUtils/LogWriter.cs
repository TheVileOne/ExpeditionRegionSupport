using LogUtils.Properties;
using System;
using System.IO;
using System.Linq;
using static LogUtils.FileHandling.FileEnums;

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

        public void CreateFile(LogID logFile)
        {
            logFile.Properties.BeginLogSession();
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
                UtilityCore.LogError(null, new IOException("Unable to delete log file", ex));
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
            LogEvents.LogMessageEventArgs logEventData = new LogEvents.LogMessageEventArgs(logFile, message);

            WriteFrom(new LogRequest(requestType, logEventData));
        }

        protected virtual bool PrepareLogFile(LogID logFile)
        {
            if (logFile.Properties.LogSessionActive)
                return true;

            //Have you reached a point where this LogID can log to file
            if (RWInfo.LatestSetupPeriodReached < logFile.Properties.AccessPeriod)
                return false;

            //Start routine for a new log session
            CreateFile(logFile);
            return logFile.Properties.LogSessionActive;
        }

        internal bool InternalWriteToFile(LogEvents.LogMessageEventArgs logEventData)
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

                    string writePath = logFile.Properties.CurrentFilePath;
                    bool retryAttempt = false;

                retry:
                    using (FileStream stream = GetWriteStream(writePath, false))
                    {
                        if (stream == null)
                        {
                            if (!retryAttempt)
                            {
                                logFile.Properties.FileExists = false;
                                if (PrepareLogFile(logFile)) //Allow a single retry after creating the file once confirming session has been established
                                {
                                    retryAttempt = true;
                                    goto retry;
                                }
                            }
                            throw new IOException("Unable to create log file");
                        }

                        //Assume stream is fine here
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            message = ApplyRules(logFile, message);
                            writer.WriteLine(message);
                        }
                    }
                }
                return true;
            }
            catch (IOException ex)
            {
                UtilityCore.LogError("Log write error", ex);
                return false;
            }
        }

        public string ApplyRules(LogID logFile, string message)
        {
            message = message ?? string.Empty;

            foreach (LogRule rule in logFile.Properties.Rules.Where(r => r.IsEnabled))
                rule.Apply(ref message);
            return message;
        }

        protected virtual void OnLogMessageReceived(LogEvents.LogMessageEventArgs e)
        {
            LogEvents.OnMessageReceived?.Invoke(e);
        }

        /// <summary>
        /// Starts the process to write to a game-controlled log file
        /// </summary>
        internal static void BeginWriteProcess(LogRequest request)
        {
            LogProperties properties = request.Data.Properties;

            properties.BeginLogSession();

            if (!properties.LogSessionActive) //Unable to create log file for some reason
            {
                request.Reject(RejectionReason.LogUnavailable);
                return;
            }

            LogEvents.OnMessageReceived?.Invoke(request.Data);
        }

        /// <summary>
        /// Ends the process to write to a game-controlled log file
        /// </summary>
        internal static void FinishWriteProcess(LogRequest request)
        {
            if (request.Status != RequestStatus.Rejected)
                request.Complete();
            UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);
        }

        internal static FileStream GetWriteStream(string path, bool createFile)
        {
            try
            {
                FileMode mode = createFile ? FileMode.OpenOrCreate : FileMode.Open;

                //Accessing the write stream this way provides better control over file creation and write access
                FileStream stream = File.Open(path, mode, FileAccess.ReadWrite, FileShare.ReadWrite);
                stream.Seek(0, SeekOrigin.End);
                return stream;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                return null;
            }
        }
    }

    public interface ILogWriter
    {
        public void CreateFile(LogID logFile);
        public void ResetFile(LogID logFile);
        internal void WriteFrom(LogRequest request);
        internal void WriteToFile(LogID logFile, string message);
        internal string ApplyRules(LogID logFile, string message);
    }
}
