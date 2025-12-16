using LogUtils.Diagnostics;
using LogUtils.Enums;
using LogUtils.Formatting;
using LogUtils.Helpers.FileHandling;
using LogUtils.Policy;
using LogUtils.Threading;
using System;
using System.Linq;

namespace LogUtils
{
    internal static class UtilityLogger
    {
        internal static DirectToFileLogger DebugLogger = new DirectToFileLogger(DirectToFileLogger.DEFAULT_LOG_NAME, false);

        public static UtilityLogSource Logger;

        /// <summary>
        /// Activity logger is responsible for reporting file behavior associated with log related files
        /// </summary>
        private static Logger activityLogger;

        /// <summary>
        /// Used to maintain the high performance write implementation
        /// </summary>
        private static Task writeTask;

        private static bool _performanceMode;

        /// <summary>
        /// Enables a write buffer that intercepts all debug logs and writes them to file off the main thread
        /// </summary>
        public static bool PerformanceMode
        {
            get => _performanceMode;
            set
            {
                if (_performanceMode == value)
                    return;

                _performanceMode = value;

                DebugPolicy.UpdateAllowConditions();

                //Enable the buffer when performance mode is enabled, and disable it when it is no longer necessary
                DebugLogger.WriteBuffer.SetState(value, BufferContext.Debug);

                if (value)
                {
                    Logger.LogDebug("Performance mode enabled");

                    writeTask = new Task(() => DebugLogger.TryFlush(), 2000);
                    writeTask.IsContinuous = true;
                    LogTasker.Schedule(writeTask);
                }
                else
                {
                    Logger.LogDebug("Performance mode disabled");
                    //We want to run one more time, and end the process
                    writeTask.RunOnceAndEnd(true);
                    writeTask = null;
                }
            }
        }

        internal static void Initialize()
        {
            if (Logger != null) return;

            var sources = BepInEx.Logging.Logger.Sources;

            Logger = sources.FirstOrDefault(l => l.SourceName == UtilityConsts.UTILITY_NAME) as UtilityLogSource;

            if (Logger == null)
            {
                Logger = new UtilityLogSource();
                sources.Add(Logger);
            }
        }

        internal static void DeleteInternalLogs()
        {
            FileUtils.TryDelete("LogActivity.log");
            DebugLogger.DeleteAll();
        }

        public static void DebugLog(object messageObj)
        {
            DebugLogger.Log(messageObj);
        }

        public static void Log(object messageObj)
        {
            Logger.LogInfo(messageObj);
        }

        public static void Log(LogCategory category, object messageObj)
        {
            Logger.Log(category.BepInExCategory, messageObj);
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

                activityLogger = new Logger(LogID.FileActivity)
                {
                    IsThreadSafe = false //This logger is used in very sensitive areas, and cannot be safe logging from other threads
                };
            }
            activityLogger.Log(message);
        }

        public static void LogError(object messageObj)
        {
            Logger.LogError(messageObj);
        }

        public static void LogError(string errorMessage, Exception error)
        {
            if (errorMessage != null)
                Logger.LogError(errorMessage);
            Logger.LogError(error);
        }

        public static void LogFatal(object messageObj)
        {
            Logger.LogFatal(messageObj);
        }

        public static void LogFatal(string errorMessage, Exception error)
        {
            if (errorMessage != null)
                Logger.LogFatal(errorMessage);
            Logger.LogFatal(error);
        }

        public static void LogWarning(object messageObj)
        {
            Logger.LogWarning(messageObj);
        }

        /// <summary>
        /// Creates a logger that LogUtils can use to log to files directly (without using LogIDs, or the log request system) - not intended for users of LogUtils
        /// </summary>
        internal static DebugLogger CreateLogger(string logName, StringProvider provider)
        {
            var logger = logName == DirectToFileLogger.DEFAULT_LOG_NAME ? DebugLogger : new DirectToFileLogger(logName, true);

            return new DebugLogger(logger, provider);
        }

        static UtilityLogger()
        {
            UtilityCore.EnsureInitializedState();
        }
    }
}
