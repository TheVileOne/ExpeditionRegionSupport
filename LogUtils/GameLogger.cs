using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogUtils
{
    /// <summary>
    /// A class for handling game-controlled log content
    /// </summary>
    public class GameLogger
    {
        public void HandleRequest(LogRequest request)
        {
            LogID logFile = request.Data.ID;

            if (!logFile.IsGameControlled) return;

            //Check RainWorld.ShowLogs for logs that are restricted by it
            if (logFile.Properties.ShowLogsAware && !RainWorld.ShowLogs)
            {
                if (RWInfo.LatestSetupPeriodReached < RWInfo.SHOW_LOGS_ACTIVE_PERIOD)
                    request.Reject(RejectionReason.ShowLogsNotInitialized);
                else
                    request.Reject(RejectionReason.LogDisabled);
                return;
            }

            //Check that the log file can be initialized
            if (!logFile.Properties.LogSessionActive && RWInfo.LatestSetupPeriodReached < logFile.Properties.AccessPeriod)
                request.Reject(RejectionReason.LogUnavailable);

            if (request.Status == RequestStatus.Rejected) return;

            string message = request.Data.Message;

            if (logFile == LogID.BepInEx) //The only logger example that needs an extra parameter
            {
                LogBepEx(request.Data.LogSource, request.Data.BepInExCategory, message);
            }
            else if (logFile == LogID.Unity) //Unity, and Exception log requests are not guaranteed to have a defined LogCategory instance
            {
                LogUnity(request.Data.UnityCategory, message);
            }
            else if (logFile == LogID.Exception)
            {
                LogUnity(LogType.Error, message);
            }
            else
            {
                LogCategory category = request.Data.Category;
                LogHandler logMethod = LogUnity;

                if (logFile != LogID.Unity)
                {
                    if (logFile == LogID.Expedition)
                        logMethod = LogExp;
                    else if (logFile == LogID.JollyCoop)
                        logMethod = LogJolly;
                    else if (logFile == LogID.Exception)
                        logMethod = LogUnity;
                }

                logMethod(category, message);
            }
        }

        public void HandleRequests(IEnumerable<LogRequest> requests)
        {
            foreach (LogRequest request in requests)
                HandleRequest(request);
        }

        public void LogBepEx(object data)
        {
            LogBepEx(null, LogLevel.Info, data);
        }

        public void LogBepEx(ManualLogSource source, LogLevel category, object data)
        {
            var bepLogger = source ?? UtilityCore.BaseLogger;
            bepLogger.Log(category, data);
        }

        public void LogBepEx(ManualLogSource source, LogCategory category, object data)
        {
            var bepLogger = source ?? UtilityCore.BaseLogger;
            bepLogger.Log(category.BepInExCategory, data);
        }

        public void LogUnity(object data)
        {
            LogUnity(LogCategory.Default, data);
        }

        public void LogUnity(LogType category, object data)
        {
            Debug.unityLogger.Log(category, data);
        }

        public void LogUnity(LogCategory category, object data)
        {
            Debug.unityLogger.Log(category.UnityCategory, data);
        }

        public void LogExp(object data)
        {
            LogExp(LogCategory.Default, data);
        }

        public void LogExp(LogCategory category, object data)
        {
            string message = null;

            //CurrentRequest has already passed preprocess validation checks if this is not null
            if (UtilityCore.RequestHandler.CurrentRequest == null)
            {
                LogRequest request = UtilityCore.RequestHandler.Submit(new LogRequest(new LogEvents.LogMessageEventArgs(LogID.Expedition, data, category)), false);

                if (request.Status == RequestStatus.Rejected)
                    return;

                message = request.Data.Message;
            }

            if (message == null)
                message = data?.ToString();

            Expedition.ExpLog.Log(message);
        }

        public void LogJolly(object data)
        {
            LogJolly(LogCategory.Default, data);
        }

        public void LogJolly(LogCategory category, object data)
        {
            string message = null;

            //CurrentRequest has already passed preprocess validation checks if this is not null
            if (UtilityCore.RequestHandler.CurrentRequest == null)
            {
                LogRequest request = UtilityCore.RequestHandler.Submit(new LogRequest(new LogEvents.LogMessageEventArgs(LogID.JollyCoop, data, category)), false);

                if (request.Status == RequestStatus.Rejected)
                    return;

                message = request.Data.Message;
            }

            if (message == null)
                message = data?.ToString();

            JollyCoop.JollyCustom.Log(message);
        }

        public delegate void LogHandler(LogCategory category, string message);
    }
}
