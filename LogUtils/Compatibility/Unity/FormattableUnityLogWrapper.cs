using LogUtils.Enums;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LogUtils.Compatibility.Unity
{
    internal sealed class FormattableUnityLogWrapper : IUnityLogger
    {
        public ILogger Value;
        public UnityEngine.ILogger ValueConverted => Value as UnityEngine.ILogger;

        UnityEngine.ILogHandler UnityEngine.ILogger.logHandler
        {
            get => ValueConverted?.logHandler;
            set
            {
                var conversion = ValueConverted;

                if (conversion != null)
                    conversion.logHandler = value;
            }
        }

        bool UnityEngine.ILogger.logEnabled
        {
            get
            {
                var conversion = ValueConverted;
                return conversion == null || conversion.logEnabled;
            }
            set
            {
                var conversion = ValueConverted;

                if (conversion != null)
                    conversion.logEnabled = value;
            }
        }

        LogType UnityEngine.ILogger.filterLogType
        {
            get
            {
                var conversion = ValueConverted;

                if (conversion != null)
                    return conversion.filterLogType;

                return LogType.Log; //TODO: Check this
            }
            set
            {
                var conversion = ValueConverted;

                if (conversion != null)
                    conversion.filterLogType = value;
            }
        }

        public FormattableUnityLogWrapper(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            Value = logger;
        }

        #region UnityEngine.ILogger members

        public bool IsLogTypeAllowed(LogType category)
        {
            var conversion = ValueConverted;
            return conversion == null || conversion.IsLogTypeAllowed(category);
        }

        public void Log(object messageObj)
        {
            Value.Log(messageObj);
        }

        public void Log(LogType category, object messageObj)
        {
            Value.Log(category, messageObj);
        }

        public void Log(LogType category, object messageObj, UnityEngine.Object context)
        {
            LogBase(category, null, messageObj, context);
        }

        public void Log(LogType category, string tag, object messageObj)
        {
            LogBase(category, tag, messageObj, null);
        }

        public void Log(LogType category, string tag, object messageObj, UnityEngine.Object context)
        {
            LogBase(category, tag, messageObj, context);
        }

        public void Log(string tag, object messageObj)
        {
            LogBase(LogCategory.Default.UnityCategory, tag, messageObj, null);
        }

        public void Log(string tag, object messageObj, UnityEngine.Object context)
        {
            LogBase(LogCategory.Default.UnityCategory, tag, messageObj, context);
        }

        public void LogWarning(string tag, object messageObj)
        {
            LogBase(LogType.Warning, tag, messageObj, null);
        }

        public void LogWarning(string tag, object messageObj, UnityEngine.Object context)
        {
            LogBase(LogType.Warning, tag, messageObj, context);
        }

        public void LogError(string tag, object messageObj)
        {
            LogBase(LogType.Error, tag, messageObj, null);
        }

        public void LogError(string tag, object messageObj, UnityEngine.Object context)
        {
            LogBase(LogType.Error, tag, messageObj, context);
        }

        public void LogException(Exception exception)
        {
            Value.LogError(exception);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            LogBase(LogType.Exception, null, exception, context);
        }

        public void LogFormat(LogType category, string format, params object[] formatArgs)
        {
            Value.Log(category, FormattableStringFactory.Create(format, formatArgs));
        }

        public void LogFormat(LogType category, UnityEngine.Object context, string format, params object[] formatArgs)
        {
            LogBase(category, null, FormattableStringFactory.Create(format, formatArgs), context);
        }
        #endregion
        #region IFormattableLogger members

        public void Log(LogType category, FormattableString messageObj, UnityEngine.Object context)
        {
            LogBase(category, null, messageObj, context);
        }

        public void Log(LogType category, string tag, FormattableString messageObj)
        {
            LogBase(category, tag, messageObj, null);
        }

        public void Log(LogType category, string tag, FormattableString messageObj, UnityEngine.Object context)
        {
            LogBase(category, tag, messageObj, context);
        }

        public void Log(string tag, FormattableString messageObj)
        {
            LogBase(LogCategory.Default.UnityCategory, tag, messageObj, null);
        }

        public void Log(string tag, FormattableString messageObj, UnityEngine.Object context)
        {
            LogBase(LogCategory.Default.UnityCategory, tag, messageObj, context);
        }

        public void LogWarning(string tag, FormattableString messageObj)
        {
            LogBase(LogType.Warning, tag, messageObj, null);
        }

        public void LogWarning(string tag, FormattableString messageObj, UnityEngine.Object context)
        {
            LogBase(LogType.Warning, tag, messageObj, context);
        }

        public void LogError(string tag, FormattableString messageObj)
        {
            LogBase(LogType.Error, tag, messageObj, null);
        }

        public void LogError(string tag, FormattableString messageObj, UnityEngine.Object context)
        {
            LogBase(LogType.Error, tag, messageObj, context);
        }
        #endregion

        internal void LogBase(LogType category, string tag, object messageObj, UnityEngine.Object context)
        {
            var conversion = ValueConverted;

            if (conversion != null)
            {
                conversion.Log(category, tag, messageObj, context);
                return;
            }
            Value.Log(category, messageObj);
        }
    }
}
