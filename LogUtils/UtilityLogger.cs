using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using LogUtils.Threading;
using System;
using System.IO;
using System.Linq;

namespace LogUtils
{
    internal static class UtilityLogger
    {
        public static UtilityLogSource Logger;

#if DEBUG
        /// <summary>
        /// Activity logger is responsible for reporting file behavior associated with log related files
        /// </summary>
        private static Logger activityLogger;
#endif

        /// <summary>
        /// Used to maintain the high performance write implementation
        /// </summary>
        private static Task writeTask;

        /// <summary>
        /// Used to store pending messages waiting to be handled by the high performance write task
        /// </summary>
        private static MessageBuffer writeBuffer = new MessageBuffer();

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

#if DEBUG
                LogID.FileActivity.Properties.AllowLogging = !value;
#endif
                writeBuffer.SetState(value, BufferContext.Debug);

                if (value)
                {
                    Logger.LogDebug("Performance mode enabled");
                    writeTask = new Task(() =>
                    {
                        if (writeBuffer.HasContent)
                        {
                            try
                            {
                                writeMessage(writeBuffer.ToString());
                                writeBuffer.Clear();
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                //Race condition exception - ignore and retry on the next cycle
                            }
                        }
                    }, 2000);
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

            File.Delete("LogActivity.log");

            //TODO: Restrict Debug log to the Development build
            File.Delete("test.txt");
        }

        public static void DebugLog(object data)
        {
            string message = data?.ToString();

            if (PerformanceMode)
            {
                writeBuffer.AppendMessage(message);
                return;
            }
            writeMessage(message);
        }

        private static void writeMessage(string message)
        {
            FileUtils.WriteLine("test.txt", message);
        }

        public static void Log(object data)
        {
            Logger.LogInfo(data);
        }

        public static void Log(LogCategory category, object data)
        {
            Logger.Log(category.BepInExCategory, data);
        }

#if DEBUG
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
#endif

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

        static UtilityLogger()
        {
            UtilityCore.EnsureInitializedState();
        }
    }
}
