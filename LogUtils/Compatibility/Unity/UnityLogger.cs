using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Requests;
using UnityEngine;
using LoggerDocs = LogUtils.Documentation.LoggerDocumentation;

namespace LogUtils.Compatibility.Unity
{
    /// <summary>
    /// A logger that exclusively writes directly through Unity's logging API
    /// </summary>
    public class UnityLogger : ILogger
    {
        /// <summary>
        /// Critical section flag for interacting with Unity's logging API
        /// </summary>
        internal static bool IsSafeToLogToUnity = true;

        private static bool _receiveUnityLogEvents;
        internal static bool ReceiveUnityLogEvents
        {
            get => _receiveUnityLogEvents;
            set
            {
                if (_receiveUnityLogEvents == value) return;

                if (value)
                    Application.logMessageReceivedThreaded += logEvent;
                else
                    Application.logMessageReceivedThreaded -= logEvent;

                _receiveUnityLogEvents = value;
            }
        }

        /// <summary>
        /// Ensures that the maximum LogType value able to be processed by the Unity logger is at least the specified capacity value
        /// </summary>
        /// <param name="capacity">The desired maximum FilterType value as an integer</param>
        internal static void EnsureLogTypeCapacity(int capacity)
        {
            LogType capacityWanted = (LogType)capacity;

            if (Debug.unityLogger.filterLogType < capacityWanted)
                Debug.unityLogger.filterLogType = capacityWanted;
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void Log(object messageObj)
        {
            if (LogCategory.Default != null)
            {
                Log(LogCategory.Default.UnityCategory, messageObj);
                return;
            }
            Log(LogCategory.LOG_TYPE_DEFAULT, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogDebug(object messageObj)
        {
            Log(LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogInfo(object messageObj)
        {
            Log(LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogImportant(object messageObj)
        {
            Log(LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogMessage(object messageObj)
        {
            Log(LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogWarning(object messageObj)
        {
            Debug.LogWarning(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogError(object messageObj)
        {
            Debug.LogError(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogFatal(object messageObj)
        {
            Log(LogCategory.Fatal, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void Log(LogType category, object messageObj)
        {
            Debug.unityLogger.Log(category, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void Log(LogLevel category, object messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void Log(LogCategory category, object messageObj)
        {
            Debug.unityLogger.Log(category.UnityCategory, messageObj);
        }

        private static void logEvent(string message, string stackTrace, LogType category)
        {
            IsSafeToLogToUnity = false;
            using (UtilityCore.RequestHandler.BeginCriticalSection())
            {
                UtilityCore.RequestHandler.SanitizeCurrentRequest();

                LogID logFile = LogCategory.GetUnityLogID(category);

                LogRequest request = UtilityCore.RequestHandler.GetRequestFromAPI(logFile);

                //This submission wont be able to be logged until Rain World can initialize
                if (request == null)
                {
                    try
                    {
                        UtilityCore.RequestHandler.RecursionCheckCounter++;
                        if (logFile == LogID.Exception)
                        {
                            //Handle Unity error logging similarly to how the game would handle it
                            ExceptionInfo exceptionInfo = new ExceptionInfo(message, stackTrace);

                            //Check that the last exception reported matches information stored
                            if (!RainWorldInfo.CheckExceptionMatch(LogID.Exception, exceptionInfo))
                            {
                                RainWorldInfo.ReportException(LogID.Exception, exceptionInfo);
                                request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.Exception, exceptionInfo, category)), false);
                            }
                            return;
                        }
                        request = UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.Unity, message, category)), false);
                    }
                    finally
                    {
                        if (request != null && request.Status == RequestStatus.Rejected)
                            UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(request);

                        UtilityCore.RequestHandler.RecursionCheckCounter--;
                        IsSafeToLogToUnity = true;
                    }
                }
            }
        }

        static UnityLogger()
        {
            UtilityCore.EnsureInitializedState();
        }
    }
}
