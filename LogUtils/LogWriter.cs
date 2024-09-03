using LogUtils.Helpers;
using LogUtils.Properties;
using System.IO;
using System.Linq;

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
            logFile.Properties.BeginLogSession();
        }

        public void WriteFromRequest(LogRequest request)
        {
            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                //This shouldn't under normal circumstances be a rejected request
                UtilityCore.RequestHandler.CurrentRequest = request;
                WriteToFile();
            }
        }

        /// <summary>
        /// Attempts to write the most recently requested message to file
        /// </summary>
        public void WriteToFile()
        {
            //This shouldn't under normal circumstances be a rejected request
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

            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Local, logEventData), false);
                WriteToFile();
            }
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
                FileUtils.WriteLine(writePath, message);
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
