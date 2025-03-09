using LogUtils.Enums;
using LogUtils.Helpers.FileHandling;
using System;
using System.IO;
using System.Linq;

namespace LogUtils
{
    internal static class UtilityLogger
    {
        public static UtilityLogSource Logger;

        /// <summary>
        /// Activity logger is responsible for reporting file behavior associated with log related files
        /// </summary>
        private static Logger activityLogger;

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

            //TODO: Deprecate use of test.txt when utility is close to release
            File.Delete("test.txt");
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

        static UtilityLogger()
        {
            UtilityCore.EnsureInitializedState();
        }
    }
}
