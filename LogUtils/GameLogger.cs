using BepInEx.Logging;
using Expedition;
using JollyCoop;
using LogUtils.Compatibility.BepInEx;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Requests;
using LogUtils.Requests.Validation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogUtils
{
    /// <summary>
    /// A class for handling game-controlled log content
    /// </summary>
    public class GameLogger : ILogHandler, ILogWriterProvider
    {
        /// <summary>
        /// Set to the LogID of a request while it is being handled through an external logging API accessed by a GameLogger instance
        /// </summary>
        public LogID LogFileInProcess;

        public int GameLoggerRequestCounter;

        public IRequestValidator Validator;

        bool ILogHandler.AllowLogging => true;

        bool ILogHandler.AllowRemoteLogging => true;

        bool ILogHandler.AllowRegistration => false;

        IEnumerable<LogID> ILogFileHandler.AvailableTargets => LogTargets;

        IEnumerable<LogID> ILogFileHandler.GetAccessibleTargets() => LogTargets;

        internal static LogID[] LogTargets
        {
            get
            {
                return new LogID[]
                {
                    LogID.BepInEx,
                    LogID.Unity,
                    LogID.Exception,
                    LogID.Expedition,
                    LogID.JollyCoop
                };
            }
        }

        bool ILogHandler.CanHandle(LogID logFile, RequestType requestType) => CanHandle(logFile);

        public bool CanHandle(LogID logFile) => logFile.IsGameControlled;

        ILogWriter ILogWriterProvider.GetWriter() => null; //Not associated with a particular writer implementation

        public GameLogger()
        {
            Validator = new GameRequestValidator(this);
        }

        /// <summary>
        /// Retrieves the current LogWriter for a game-controlled log file 
        /// </summary>
        public ILogWriter GetWriter(LogID logFile)
        {
            if (!logFile.IsGameControlled)
                return null;

            if (logFile.Equals(LogID.BepInEx))
                return BepInExInfo.LogListener.Writer;

            if (logFile.Equals(LogID.JollyCoop))
                return LogWriter.JollyWriter;

            //All others use this
            return LogWriter.Writer;
        }

        public void HandleRequest(LogRequest request)
        {
            if (request.Submitted)
                UtilityCore.RequestHandler.CurrentRequest = request;

            request.ResetStatus(); //Ensure that processing request is handled in a consistent way
            request.Validate(Validator);

            if (request.Status == RequestStatus.Rejected)
                return;

            string message = request.Data.Message;

            LogID logFile = request.Data.ID;

            if (logFile == LogID.BepInEx)
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
            else if (logFile == LogID.JollyCoop)
            {
                LogJolly(request.Data.Category, message);
            }
            else if (logFile == LogID.Expedition)
            {
                LogExp(request.Data.Category, message);
            }
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
                UtilityCore.RequestHandler.SanitizeCurrentRequest();

                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                string message = null;
                if (request == null) //CurrentRequest has already passed preprocess validation checks if this is not null
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
                UtilityCore.RequestHandler.SanitizeCurrentRequest();

                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                string message = null;
                if (request == null) //CurrentRequest has already passed preprocess validation checks if this is not null
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
            using (UtilityCore.RequestHandler.BeginCriticalSection())
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
    }
}
