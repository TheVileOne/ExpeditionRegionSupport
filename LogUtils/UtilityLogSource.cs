using BepInEx.Logging;
using LogUtils.Compatibility.BepInEx;
using LogUtils.Enums;
using LogUtils.Formatting;
using System;
using System.Threading;
using UnityEngine;

namespace LogUtils
{
    internal sealed class UtilityLogSource : IExtendedLogSource, IFormatLogger
    {
        public event EventHandler<LogEventArgs> LogEvent;

        private bool recursiveAccessFlag;

        private readonly object sourceLock = new object();

        public string SourceName => UtilityConsts.UTILITY_NAME;

        internal bool IsAccessRecursive()
        {
            return recursiveAccessFlag;
        }

        #region ILogger members

        public void Log(object messageObj)
        {
            //This could potentially be accessed before LogCategory has initialized
            LogBase(LogCategory.LOG_LEVEL_DEFAULT, messageObj);
        }

        public void LogDebug(object messageObj)
        {
            LogBase(LogLevel.Debug, messageObj);
        }

        public void LogInfo(object messageObj)
        {
            LogBase(LogLevel.Info, messageObj);
        }

        public void LogImportant(object messageObj)
        {
            LogBase(LogCategory.Important.BepInExCategory, messageObj);
        }

        public void LogMessage(object messageObj)
        {
            LogBase(LogLevel.Message, messageObj);
        }

        public void LogWarning(object messageObj)
        {
            LogBase(LogLevel.Warning, messageObj);
        }

        public void LogError(object messageObj)
        {
            LogBase(LogLevel.Error, messageObj);
        }

        public void LogFatal(object messageObj)
        {
            LogBase(LogLevel.Fatal, messageObj);
        }

        public void Log(LogType category, object messageObj)
        {
            LogBase(LogCategory.ToCategory(category).BepInExCategory, messageObj);
        }

        public void Log(LogLevel category, object messageObj)
        {
            LogBase(category, messageObj);
        }

        public void Log(LogCategory category, object messageObj)
        {
            LogBase(category.BepInExCategory, messageObj);
        }
        #endregion
        #region IFormattableLogger members

        public void Log(InterpolatedStringHandler messageObj)
        {
            //This could potentially be accessed before LogCategory has initialized
            LogBase(LogCategory.LOG_LEVEL_DEFAULT, messageObj);
        }

        public void LogDebug(InterpolatedStringHandler messageObj)
        {
            LogBase(LogLevel.Debug, messageObj);
        }

        public void LogInfo(InterpolatedStringHandler messageObj)
        {
            LogBase(LogLevel.Info, messageObj);
        }

        public void LogImportant(InterpolatedStringHandler messageObj)
        {
            LogBase(LogCategory.Important.BepInExCategory, messageObj);
        }

        public void LogMessage(InterpolatedStringHandler messageObj)
        {
            LogBase(LogLevel.Message, messageObj);
        }

        public void LogWarning(InterpolatedStringHandler messageObj)
        {
            LogBase(LogLevel.Warning, messageObj);
        }

        public void LogError(InterpolatedStringHandler messageObj)
        {
            LogBase(LogLevel.Error, messageObj);
        }

        public void LogFatal(InterpolatedStringHandler messageObj)
        {
            LogBase(LogLevel.Fatal, messageObj);
        }

        public void Log(LogType category, InterpolatedStringHandler messageObj)
        {
            LogBase(LogCategory.ToCategory(category).BepInExCategory, messageObj);
        }

        public void Log(LogLevel category, InterpolatedStringHandler messageObj)
        {
            LogBase(category, messageObj);
        }

        public void Log(LogCategory category, InterpolatedStringHandler messageObj)
        {
            LogBase(category.BepInExCategory, messageObj);
        }
        #endregion

        internal void LogBase(LogLevel category, object messageObj)
        {
            Monitor.Enter(sourceLock);

            if (IsAccessRecursive()) //Game will be put into a bad state if we allow log event to execute
            {
                Monitor.Exit(sourceLock);
                return;
            }

            recursiveAccessFlag = true;
            try
            {
                LogEvent?.Invoke(this, new LogEventArgs(messageObj, category, this));
            }
            finally
            {
                recursiveAccessFlag = false;
                Monitor.Exit(sourceLock);
            }
        }

        /// <summary>
        /// Performs tasks for disposing a <see cref="UtilityLogSource"/>
        /// </summary>
        public void Dispose()
        {
            BepInEx.Logging.Logger.Sources.Remove(this);
        }
    }
}
