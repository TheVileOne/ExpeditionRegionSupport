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
using LoggerDocs = LogUtils.Documentation.LoggerDocumentation;

namespace LogUtils
{
    /// <summary>
    /// A class for handling game-controlled log content
    /// </summary>
    public class GameLogger : ILogHandler, ILogWriterProvider
    {
        /// <summary>
        /// Set to the <see cref="LogID"/> of a request while it is being handled through an external logging API accessed by this instance
        /// </summary>
        public LogID LogFileInProcess;

        internal Dictionary<LogID, int> ExpectedRequestCounter;

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

        /// <summary>
        /// Checks that the provided <see cref="LogID"/> is game-controlled, the only type of <see cref="LogID"/> supported by this instance
        /// </summary>
        public bool CanHandle(LogID logFile) => logFile.IsGameControlled;

        ILogWriter ILogWriterProvider.GetWriter() => null; //Not associated with a particular writer implementation

        public GameLogger()
        {
            Validator = new GameRequestValidator(this);
            ExpectedRequestCounter = new Dictionary<LogID, int>();
        }

        /// <summary>
        /// Retrieves the current <see cref="ILogWriter"/> instance for a game-controlled log file 
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

        /// <inheritdoc/>
        public void HandleRequest(LogRequest request)
        {
            if (request.Submitted)
                UtilityCore.RequestHandler.CurrentRequest = request;

            request.ResetStatus(); //Ensure that processing request is handled in a consistent way
            request.Validate(Validator);

            if (request.Status == RequestStatus.Rejected)
                return;

            object message = request.Data.MessageObject;

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

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(object)"/>
        public void LogBepEx(object messageObj)
        {
            LogBepEx(null, LogCategory.Default.BepInExCategory, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(ILogSource, BepInEx.Logging.LogLevel, object)"/>
        public void LogBepEx(ILogSource source, LogCategory category, object messageObj)
        {
            LogBepEx(source, category.BepInExCategory, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(ILogSource, BepInEx.Logging.LogLevel, object)"/>
        public void LogBepEx(ILogSource source, LogLevel category, object messageObj)
        {
            Process(LogID.BepInEx, processLog);

            void processLog()
            {
                if (source is ManualLogSource)
                {
                    ManualLogSource sourceLogger = (ManualLogSource)source;
                    sourceLogger.Log(category, messageObj);
                    return;
                }
                else
                {
                    IExtendedLogSource sourceLogger = source as IExtendedLogSource;

                    if (sourceLogger == null)
                        sourceLogger = UtilityLogger.Logger;

                    sourceLogger.Log(category, messageObj);
                }
            }
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(object)"/>
        public void LogUnity(object messageObj)
        {
            LogUnity(LogCategory.Default.UnityCategory, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        public void LogUnity(LogCategory category, object messageObj)
        {
            LogUnity(category.UnityCategory, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        public void LogUnity(LogType category, object messageObj)
        {
            Process(LogCategory.GetUnityLogID(category), processLog);

            void processLog()
            {
                Debug.unityLogger.Log(category, messageObj);
            }
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(object)"/>
        public void LogExp(object messageObj)
        {
            LogExp(LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(LogCategory, object)"/>
        public void LogExp(LogCategory category, object messageObj)
        {
            Process(LogID.Expedition, processLog);

            void processLog()
            {
                UtilityCore.RequestHandler.SanitizeCurrentRequest();

                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                string message = null;
                if (request == null) //CurrentRequest has already passed preprocess validation checks if this is not null
                {
                    request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.Expedition, messageObj, category)), false);

                    if (request.Status == RequestStatus.Rejected)
                        return;

                    message = request.Data.Message;
                }

                if (message == null)
                    message = messageObj?.ToString();

                ExpLog.Log(message);
            }
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(object)"/>
        public void LogJolly(object messageObj)
        {
            LogJolly(LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(LogCategory, object)"/>
        public void LogJolly(LogCategory category, object messageObj)
        {
            Process(LogID.JollyCoop, processLog);

            void processLog()
            {
                UtilityCore.RequestHandler.SanitizeCurrentRequest();

                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                string message = null;
                if (request == null) //CurrentRequest has already passed preprocess validation checks if this is not null
                {
                    request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.JollyCoop, messageObj, category)), false);

                    if (request.Status == RequestStatus.Rejected)
                        return;

                    message = request.Data.Message;
                }

                if (message == null)
                    message = messageObj?.ToString();

                JollyCustom.Log(message);
            }
        }

        protected void Process(LogID logFile, Action processLog)
        {
            using (UtilityCore.RequestHandler.BeginCriticalSection())
            {
                //Check values to ensure that the same request going into an API is the same request coming out of it
                ExpectedRequestCounter[logFile]++;
                processLog();
            }
        }
    }
}
