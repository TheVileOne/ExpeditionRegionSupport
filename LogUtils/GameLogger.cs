using BepInEx.Logging;
using Expedition;
using JollyCoop;
using LogUtils.CompatibilityServices;
using LogUtils.Enums;
using LogUtils.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogUtils
{
    /// <summary>
    /// A class for handling game-controlled log content
    /// </summary>
    public class GameLogger
    {
        /// <summary>
        /// Set to the LogID of a request while it is being handled through an external logging API accessed by a GameLogger instance
        /// </summary>
        public LogID LogFileInProcess;
        public int GameLoggerRequestCounter;

        public void HandleRequest(LogRequest request)
        {
            request.ResetStatus(); //Ensure that processing request is handled in a consistent way

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

            if (!logFile.Properties.CanBeAccessed)
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

        public void LogBepEx(ILogSource source, LogLevel category, object data)
        {
            Process(LogID.BepInEx, processLog);

            void processLog()
            {
                if (source is ManualLogSource)
                {
                    ManualLogSource sourceLogger = (ManualLogSource)source;
                    sourceLogger.Log(category, data);
                    return;
                }
                else
                {
                    IExtendedLogSource sourceLogger = source as IExtendedLogSource;

                    if (sourceLogger == null)
                        sourceLogger = UtilityLogger.Logger;

                    sourceLogger.Log(category, data);
                }
            }
        }

        public void LogBepEx(ILogSource source, LogCategory category, object data)
        {
            Process(LogID.BepInEx, processLog);

            void processLog()
            {
                if (source is ManualLogSource)
                {
                    ManualLogSource sourceLogger = (ManualLogSource)source;
                    sourceLogger.Log(category.BepInExCategory, data);
                    return;
                }
                else
                {
                    IExtendedLogSource sourceLogger = source as IExtendedLogSource;

                    if (sourceLogger == null)
                        sourceLogger = UtilityLogger.Logger;

                    sourceLogger.Log(category.BepInExCategory, data);
                }
            }
        }

        public void LogUnity(object data)
        {
            LogUnity(LogCategory.Default, data);
        }

        public void LogUnity(LogType category, object data)
        {
            Process(LogCategory.GetUnityLogID(category), processLog);

            void processLog()
            {
                Debug.unityLogger.Log(category, data);
            }
        }

        public void LogUnity(LogCategory category, object data)
        {
            Process(LogCategory.GetUnityLogID(category.UnityCategory), processLog);

            void processLog()
            {
                Debug.unityLogger.Log(category.UnityCategory, data);
            }
        }

        public void LogExp(object data)
        {
            LogExp(LogCategory.Default, data);
        }

        public void LogExp(LogCategory category, object data)
        {
            Process(LogID.Expedition, processLog);

            void processLog()
            {
                string message = null;
                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                //CurrentRequest has already passed preprocess validation checks if this is not null
                if (request == null)
                {
                    request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogMessageEventArgs(LogID.Expedition, data, category)), false);

                    if (request.Status == RequestStatus.Rejected)
                        return;

                    message = request.Data.Message;
                }

                if (message == null)
                    message = data?.ToString();

                ExpLog.Log(message);
            }
        }

        public void LogJolly(object data)
        {
            LogJolly(LogCategory.Default, data);
        }

        public void LogJolly(LogCategory category, object data)
        {
            Process(LogID.JollyCoop, processLog);

            void processLog()
            {
                string message = null;
                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                //CurrentRequest has already passed preprocess validation checks if this is not null
                if (request == null)
                {
                    request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogMessageEventArgs(LogID.JollyCoop, data, category)), false);

                    if (request.Status == RequestStatus.Rejected)
                        return;

                    message = request.Data.Message;
                }

                if (message == null)
                    message = data?.ToString();

                JollyCustom.Log(message);
            }
        }

        protected void Process(LogID logFile, Action processLog)
        {
            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                //Check values to ensure that the same request going into an API is the same request coming out of it
                GameLoggerRequestCounter++;

                LogID lastProcessState = LogFileInProcess;

                LogFileInProcess = logFile;
                processLog();

                LogFileInProcess = lastProcessState;
                GameLoggerRequestCounter--;
            }
        }

        public delegate void LogHandler(LogCategory category, string message);
    }
}
