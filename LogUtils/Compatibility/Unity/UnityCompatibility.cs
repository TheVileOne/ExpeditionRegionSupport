using LogUtils.Compatibility.Unity;
using LogUtils.Enums;
using LogUtils.Requests;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using CreateRequestCallback = LogUtils.Requests.LogRequest.Factory.Callback;

namespace LogUtils
{
    public partial class Logger : UnityEngine.ILogger
    {
        private LogCategoryFilter.ByCategory categoryFilter = new LogCategoryFilter.ByCategory(null, false);

        LogType UnityEngine.ILogger.filterLogType
        {
            get => categoryFilter.Flags.UnityCategory;
            set => categoryFilter = new LogCategoryFilter.ByCategory(LogCategory.ToCategory(value), false);
        }

        UnityEngine.ILogHandler UnityEngine.ILogger.logHandler
        {
            get => this;
            set => LogError(LogID.Exception, new NotSupportedException($"logHandler is readonly and cannot be changed"));
        }

        bool UnityEngine.ILogger.logEnabled
        {
            get => AllowLogging;
            set => AllowLogging = value;
        }

        bool UnityEngine.ILogger.IsLogTypeAllowed(LogType category)
        {
            return LogFilter.IsAllowed(LogCategory.ToCategory(category));
        }

        void UnityEngine.ILogger.Log(LogType category, object messageObj, UnityEngine.Object context)
        {
            LogBase(category, null, messageObj, context);
        }

        void UnityEngine.ILogger.Log(LogType category, string tag, object messageObj)
        {
            LogBase(category, tag, messageObj, null);
        }

        void UnityEngine.ILogger.Log(LogType category, string tag, object messageObj, UnityEngine.Object context)
        {
            LogBase(category, tag, messageObj, context);
        }

        void UnityEngine.ILogger.Log(string tag, object messageObj)
        {
            LogBase(LogCategory.Default.UnityCategory, tag, messageObj, null);
        }

        void UnityEngine.ILogger.Log(string tag, object messageObj, UnityEngine.Object context)
        {
            LogBase(LogCategory.Default.UnityCategory, tag, messageObj, context);
        }

        void UnityEngine.ILogger.LogWarning(string tag, object messageObj)
        {
            LogBase(LogType.Warning, tag, messageObj, null);
        }

        void UnityEngine.ILogger.LogWarning(string tag, object messageObj, UnityEngine.Object context)
        {
            LogBase(LogType.Warning, tag, messageObj, context);
        }

        void UnityEngine.ILogger.LogError(string tag, object messageObj)
        {
            LogBase(LogType.Error, tag, messageObj, null);
        }

        void UnityEngine.ILogger.LogError(string tag, object messageObj, UnityEngine.Object context)
        {
            LogBase(LogType.Error, tag, messageObj, context);
        }

        void UnityEngine.ILogger.LogException(Exception exception)
        {
            LogError(exception);
        }

        void UnityEngine.ILogHandler.LogException(Exception exception, UnityEngine.Object context)
        {
            LogBase(LogType.Exception, null, exception, context);
        }

        void UnityEngine.ILogger.LogFormat(LogType logType, string format, params object[] formatArgs)
        {
            Log(logType, FormattableStringFactory.Create(format, formatArgs));
        }

        void UnityEngine.ILogHandler.LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] formatArgs)
        {
            LogBase(logType, null, FormattableStringFactory.Create(format, formatArgs), context);
        }

        /// <summary>
        /// This method receives all log API calls that make use of Unity specific logging arguments for this logger
        /// </summary>
        protected void LogBase(LogType logType, string tag, object messageObj, UnityEngine.Object context)
        {
            bool shouldAddData = context != null || tag != null; //At least some of the data should be available to warrant storage in a data class

            CreateRequestCallback dataCallback = null;
            if (shouldAddData)
            {
                EventArgs extraData = new UnityLogEventArgs(context, tag);
                dataCallback = LogRequest.Factory.CreateDataCallback(extraData);
            }
            LogBase(LogCategory.ToCategory(logType), messageObj, false, dataCallback);
        }
    }

    public static partial class LoggerExtensions
    {
        public static void Log(this UnityEngine.ILogger logger, LogCategory category, object messageObj, UnityEngine.Object context)
        {
            if (category == null)
                category = LogCategory.Default;

            logger.Log(category.UnityCategory, messageObj, context);
        }

        public static void Log(this UnityEngine.ILogger logger, LogCategory category, string tag, object messageObj)
        {
            if (category == null)
                category = LogCategory.Default;

            logger.Log(category.UnityCategory, tag, messageObj);
        }

        public static void Log(this UnityEngine.ILogger logger, LogCategory category, string tag, object messageObj, UnityEngine.Object context)
        {
            if (category == null)
                category = LogCategory.Default;

            logger.Log(category.UnityCategory, tag, messageObj, context);
        }

        public static void Log(this UnityEngine.ILogger logger, LogCategory category, FormattableString messageObj, UnityEngine.Object context)
        {
            if (category == null)
                category = LogCategory.Default;

            logger.Log(category.UnityCategory, messageObj, context);
        }

        public static void Log(this UnityEngine.ILogger logger, LogCategory category, string tag, FormattableString messageObj)
        {
            if (category == null)
                category = LogCategory.Default;

            logger.Log(category.UnityCategory, tag, messageObj);
        }

        public static void Log(this UnityEngine.ILogger logger, LogCategory category, string tag, FormattableString messageObj, UnityEngine.Object context)
        {
            if (category == null)
                category = LogCategory.Default;

            logger.Log(category.UnityCategory, tag, messageObj, context);
        }
    }
}
