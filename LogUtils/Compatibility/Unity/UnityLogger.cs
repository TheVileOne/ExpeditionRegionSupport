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
        public void Log(object data)
        {
            Debug.Log(data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogDebug(object data)
        {
            Log(LogCategory.Debug, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogInfo(object data)
        {
            Log(LogCategory.Info, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogImportant(object data)
        {
            Log(LogCategory.Important, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogMessage(object data)
        {
            Log(LogCategory.Message, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogWarning(object data)
        {
            Debug.LogWarning(data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogError(object data)
        {
            Debug.LogError(data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void LogFatal(object data)
        {
            Log(LogCategory.Fatal, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void Log(LogType category, object data)
        {
            Debug.unityLogger.Log(category, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void Log(LogLevel category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void Log(string category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        /// <remarks>Utilizes Unity's logging API</remarks>
        public void Log(LogCategory category, object data)
        {
            Debug.unityLogger.Log(category.UnityCategory, data);
        }

        private static void logEvent(string message, string stackTrace, LogType category)
        {
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
                            if (!RWInfo.CheckExceptionMatch(LogID.Exception, exceptionInfo))
                            {
                                RWInfo.ReportException(LogID.Exception, exceptionInfo);
                                UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.Exception, exceptionInfo, category)), false);
                            }
                            return;
                        }
                        UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogRequestEventArgs(LogID.Unity, message, category)), false);
                    }
                    finally
                    {
                        UtilityCore.RequestHandler.RecursionCheckCounter--;
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
