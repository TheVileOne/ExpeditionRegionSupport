using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public class LogWriter : ILogWriter
    {
        private static SharedField<ILogWriter> _writer;

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

        public void CreateFile(LogID logFile)
        {
            try
            {
                File.Create(logFile.Properties.CurrentFilePath);
                logFile.Properties.LogStartProcess();
            }
            catch (IOException e)
            {
                UtilityCore.BaseLogger.LogError($"Unable to create file {logFile.Properties.CurrentFilename}.log");
                UtilityCore.BaseLogger.LogError(e);
            }
        }

        public void WriteFromRequest(LogRequest request)
        {
            UtilityCore.RequestHandler.CurrentRequest = request;
            WriteToFile();
        }

        /// <summary>
        /// Attempts to write the most recently requested message to file
        /// </summary>
        public void WriteToFile()
        {
            LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

            if (request == null || request.Status == RequestStatus.Rejected) return;

            try
            {
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

        public void WriteToFile(LogID logFile, string message)
        {
            LogEvents.LogMessageEventArgs logEventData = new LogEvents.LogMessageEventArgs(logFile, message);
            UtilityCore.RequestHandler.Submit(new LogRequest(logEventData), false);

            WriteToFile();
        }

        internal bool PrepareLogFile(LogID logFile)
        {
            if (logFile.Properties.LogSessionActive)
                return true;

            //Have you reached a point where this LogID can log to file
            if (RWInfo.LatestSetupPeriodReached < logFile.Properties.AccessPeriod)
                return false;

            //Start routine for a new log session
            CreateFile(logFile);
            return true;
        }

        internal bool InternalWriteToFile(LogEvents.LogMessageEventArgs logEventData)
        {
            OnLogMessageReceived(logEventData);

            LogID logFile = logEventData.ID;
            string message = logEventData.Message;

            string writePath = logFile.Properties.CurrentFilePath;

            try
            {
                message = ApplyRules(logFile, message);
                File.AppendAllText(writePath, message);
                return true;
            }
            catch (IOException ex)
            {
                UtilityCore.BaseLogger.LogError("Log write error");
                UtilityCore.BaseLogger.LogError(ex);
                UtilityCore.BaseLogger.LogError(ex.StackTrace);
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
    }

    public interface ILogWriter
    {
        public void CreateFile(LogID logFile);
        public void WriteFromRequest(LogRequest request);
        public void WriteToFile();
        public void WriteToFile(LogID logFile, string message);
        public string ApplyRules(LogID logFile, string message);
    }
}
