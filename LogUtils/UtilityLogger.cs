using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers.FileHandling;
using LogUtils.Requests;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LogUtils
{
    internal static class UtilityLogger
    {
        public static UtilityLogSource Logger;

        /// <summary>
        /// Activity logger is responsible for reporting file behavior associated with log related files
        /// </summary>
        private static Logger activityLogger;

        private static bool _receiveUnityLogEvents;

        internal static void Initialize()
        {
            var sources = BepInEx.Logging.Logger.Sources;

            Logger = sources.FirstOrDefault(l => l.SourceName == UtilityConsts.UTILITY_NAME) as UtilityLogSource;

            if (Logger == null)
            {
                Logger = new UtilityLogSource();
                sources.Add(Logger);
            }

            //TODO: Deprecate use of test.txt when utility is close to release
            File.Delete("test.txt");
        }

        internal static bool ReceiveUnityLogEvents
        {
            get => _receiveUnityLogEvents;
            set
            {
                if (_receiveUnityLogEvents == value) return;

                if (value)
                    Application.logMessageReceivedThreaded += handleUnityLog;
                else
                    Application.logMessageReceivedThreaded -= handleUnityLog;

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

        public static void DebugLog(object data)
        {
            FileUtils.WriteLine("test.txt", data?.ToString());
        }

        public static void Log(object data)
        {
            Logger.LogInfo(data);
        }

        public static void Log(LogCategory category, object data)
        {
            Logger.Log(category.BepInExCategory, data);
        }

        public static void LogActivity(FormattableString message)
        {
            if (activityLogger == null)
            {
                //Until LogIDs are first initialized, accessed LogIDs will be null
                if (LogID.FileActivity == null)
                {
                    //TODO: Use Activity log category for activity logging
                    Logger.LogInfo(message);
                    return;
                }
                activityLogger = new Logger(LogID.FileActivity);
            }
            activityLogger.Log(message);
        }

        public static void LogError(object data)
        {
            Logger.LogError(data);
        }

        public static void LogError(string errorMessage, Exception ex)
        {
            if (errorMessage != null)
                Logger.LogError(errorMessage);
            Logger.LogError(ex);
        }

        public static void LogFatal(object data)
        {
            Logger.LogFatal(data);
        }

        public static void LogFatal(string errorMessage, Exception ex)
        {
            if (errorMessage != null)
                Logger.LogFatal(errorMessage);
            Logger.LogFatal(ex);
        }

        public static void LogWarning(object data)
        {
            Logger.LogWarning(data);
        }

        private static void handleUnityLog(string message, string stackTrace, LogType category)
        {
            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                //This submission wont be able to be logged until Rain World can initialize
                if (UtilityCore.RequestHandler.CurrentRequest == null)
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

        static UtilityLogger()
        {
            UtilityCore.EnsureInitializedState();
        }
    }
}
