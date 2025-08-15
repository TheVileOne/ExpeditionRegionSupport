using LogUtils.Compatibility.Unity;
using LogUtils.Enums;
using LogUtils.Requests;
using System;
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

        bool UnityEngine.ILogger.IsLogTypeAllowed(LogType logType)
        {
            return LogFilter.IsAllowed(LogCategory.ToCategory(logType));
        }

        void UnityEngine.ILogger.Log(LogType logType, object message, UnityEngine.Object context)
        {
            LogBase(logType, null, message, context);
        }

        void UnityEngine.ILogger.Log(LogType logType, string tag, object message)
        {
            LogBase(logType, tag, message, null);
        }

        void UnityEngine.ILogger.Log(LogType logType, string tag, object message, UnityEngine.Object context)
        {
            LogBase(logType, tag, message, context);
        }

        void UnityEngine.ILogger.Log(string tag, object message)
        {
            LogBase(LogCategory.Default.UnityCategory, tag, message, null);
        }

        void UnityEngine.ILogger.Log(string tag, object message, UnityEngine.Object context)
        {
            LogBase(LogCategory.Default.UnityCategory, tag, message, context);
        }

        void UnityEngine.ILogger.LogWarning(string tag, object message)
        {
            LogBase(LogType.Warning, tag, message, null);
        }

        void UnityEngine.ILogger.LogWarning(string tag, object message, UnityEngine.Object context)
        {
            LogBase(LogType.Warning, tag, message, context);
        }

        void UnityEngine.ILogger.LogError(string tag, object message)
        {
            LogBase(LogType.Error, tag, message, null);
        }

        void UnityEngine.ILogger.LogError(string tag, object message, UnityEngine.Object context)
        {
            LogBase(LogType.Error, tag, message, context);
        }

        void UnityEngine.ILogger.LogException(Exception exception)
        {
            LogError(exception);
        }

        void UnityEngine.ILogHandler.LogException(Exception exception, UnityEngine.Object context)
        {
            LogBase(LogType.Exception, null, exception, context);
        }

        void UnityEngine.ILogger.LogFormat(LogType logType, string format, params object[] args)
        {
            Log(logType, string.Format(format, args));
        }

        void UnityEngine.ILogHandler.LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            LogBase(logType, null, string.Format(format, args), context);
        }

        /// <summary>
        /// This method receives all log API calls that make use of Unity specific logging arguments for this logger
        /// </summary>
        protected void LogBase(LogType logType, string tag, object message, UnityEngine.Object context)
        {
            bool shouldAddData = context != null || tag != null; //At least some of the data should be available to warrant storage in a data class

            CreateRequestCallback dataCallback = null;
            if (shouldAddData)
            {
                EventArgs extraData = new UnityLogEventArgs(context, tag);
                dataCallback = LogRequest.Factory.CreateDataCallback(extraData);
            }
            LogBase(Targets, LogCategory.ToCategory(logType), message, false, dataCallback);
        }
    }
}
