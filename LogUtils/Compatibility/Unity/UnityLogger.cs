using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Requests;
using UnityEngine;

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
        /// Ensures that the maximum LogType value able to be processed by the Unity logger is at least the specified capacity value </br>
        /// </summary>
        /// <param name="capacity">The desired maximum FilterType value as an integer</param>
        internal static void EnsureLogTypeCapacity(int capacity)
        {
            LogType capacityWanted = (LogType)capacity;

            if (Debug.unityLogger.filterLogType < capacityWanted)
                Debug.unityLogger.filterLogType = capacityWanted;
        }

        public void Log(object data)
        {
            Debug.Log(data);
        }

        public void LogDebug(object data)
        {
            Debug.Log(data);
        }

        public void LogInfo(object data)
        {
            Log(LogCategory.Info, data);
        }

        public void LogImportant(object data)
        {
            Log(LogCategory.Important, data);
        }

        public void LogMessage(object data)
        {
            Log(LogCategory.Message, data);
        }

        public void LogWarning(object data)
        {
            Debug.LogWarning(data);
        }

        public void LogError(object data)
        {
            Debug.LogError(data);
        }

        public void LogFatal(object data)
        {
            Log(LogCategory.Fatal, data);
        }

        public void Log(LogType category, object data)
        {
            Debug.unityLogger.Log(category, data);
        }

        public void Log(LogLevel category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        public void Log(string category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        public void Log(LogCategory category, object data)
        {
            Debug.unityLogger.Log(category.UnityCategory, data);
        }

        private static void logEvent(string message, string stackTrace, LogType category)
        {
            using (UtilityCore.RequestHandler.BeginCriticalSection())
            {
                UtilityCore.RequestHandler.SanitizeCurrentRequest();

                LogRequest request = UtilityCore.RequestHandler.CurrentRequest;

                //TODO: Investigate the buggy behavior that requires us to compare by message here
                bool submitRequest = request == null; 
                if (request != null && request.Data.Message != message)
                {
                    UtilityLogger.Logger.LogDebug("Request in system does not match incoming Unity request");
                    submitRequest = true;
                }

                //This submission wont be able to be logged until Rain World can initialize
                if (submitRequest)
                {
                    if (LogCategory.IsErrorCategory(category))
                    {
                        //Handle Unity error logging similarly to how the game would handle it
                        ExceptionInfo exceptionInfo = new ExceptionInfo(message, stackTrace);

                        //Check that the last exception reported matches information stored
                        if (!RWInfo.CheckExceptionMatch(LogID.Exception, exceptionInfo))
                        {
                            RWInfo.ReportException(LogID.Exception, exceptionInfo);
                            UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogMessageEventArgs(LogID.Exception, exceptionInfo, category)), false);
                        }
                        return;
                    }
                    UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogMessageEventArgs(LogID.Unity, message, category)), false);
                }
            }
        }

        static UnityLogger()
        {
            UtilityCore.EnsureInitializedState();
        }
    }
}
