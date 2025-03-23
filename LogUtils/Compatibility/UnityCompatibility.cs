using LogUtils.Enums;
using System;
using System.Linq;
using UnityEngine;

namespace LogUtils
{
    public partial class Logger : UnityEngine.ILogger
    {
        /// <summary>
        /// Unity object - typically given to provide context to the log message
        /// </summary>
        protected UnityEngine.Object Context;

        /// <summary>
        /// Message context tag - provided as part of the UnityEngine.ILogger API
        /// </summary>
        protected string DataTag;

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

        bool UnityEngine.ILogger.IsLogTypeAllowed(LogType logType)
        {
            return LogCategory.GlobalFilter.IsAllowed(LogCategory.ToCategory(logType));
        }

        void UnityEngine.ILogger.Log(LogType logType, object message, UnityEngine.Object context)
        {
            LogData(logType, null, message, context);
        }

        void UnityEngine.ILogger.Log(LogType logType, string tag, object message)
        {
            LogData(logType, tag, message, null);
        }

        void UnityEngine.ILogger.Log(LogType logType, string tag, object message, UnityEngine.Object context)
        {
            LogData(logType, tag, message, context);
        }

        void UnityEngine.ILogger.Log(string tag, object message)
        {
            LogData(LogCategory.Default.UnityCategory, tag, message, null);
        }

        void UnityEngine.ILogger.Log(string tag, object message, UnityEngine.Object context)
        {
            LogData(LogCategory.Default.UnityCategory, tag, message, context);
        }

        void UnityEngine.ILogger.LogWarning(string tag, object message)
        {
            LogData(LogType.Warning, tag, message, null);
        }

        void UnityEngine.ILogger.LogWarning(string tag, object message, UnityEngine.Object context)
        {
            LogData(LogType.Warning, tag, message, context);
        }

        void UnityEngine.ILogger.LogError(string tag, object message)
        {
            LogData(LogType.Error, tag, message, null);
        }

        void UnityEngine.ILogger.LogError(string tag, object message, UnityEngine.Object context)
        {
            LogData(LogType.Error, tag, message, context);
        }

        void UnityEngine.ILogger.LogException(Exception exception)
        {
            LogError(exception);
        }

        void UnityEngine.ILogHandler.LogException(Exception exception, UnityEngine.Object context)
        {
            LogData(LogType.Exception, null, exception, context);
        }

        void UnityEngine.ILogger.LogFormat(LogType logType, string format, params object[] args)
        {
            Log(logType, string.Format(format, args));
        }

        void UnityEngine.ILogHandler.LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            LogData(logType, null, string.Format(format, args), context);
        }

        protected void LogData(LogType logType, string tag, object message, UnityEngine.Object context)
        {
            if (!LogTargets.Any())
            {
                UtilityLogger.LogWarning("Attempted to log message with no available log targets");
                return;
            }

            using (DataLock.Acquire())
            {
                Context = context;
                DataTag = tag;
                Log(logType, message);
            }
        }
    }
}
