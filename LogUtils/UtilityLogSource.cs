using BepInEx.Logging;
using LogUtils.Compatibility.BepInEx;
using LogUtils.Enums;
using System;
using System.Threading;
using UnityEngine;

namespace LogUtils
{
    internal sealed class UtilityLogSource : IExtendedLogSource, IFormattableLogger
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
            LogBase(LogLevel.Info, messageObj);
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

        public void Log(string category, object messageObj)
        {
            LogBase(LogCategory.ToCategory(category).BepInExCategory, messageObj);
        }

        public void Log(LogCategory category, object messageObj)
        {
            LogBase(category.BepInExCategory, messageObj);
        }
        #endregion
        #region IFormattableLogger members

        public void Log(FormattableString messageObj)
        {
            LogBase(LogLevel.Info, messageObj);
        }

        public void LogDebug(FormattableString messageObj)
        {
            LogBase(LogLevel.Debug, messageObj);
        }

        public void LogInfo(FormattableString messageObj)
        {
            LogBase(LogLevel.Info, messageObj);
        }

        public void LogImportant(FormattableString messageObj)
        {
            LogBase(LogCategory.Important.BepInExCategory, messageObj);
        }

        public void LogMessage(FormattableString messageObj)
        {
            LogBase(LogLevel.Message, messageObj);
        }

        public void LogWarning(FormattableString messageObj)
        {
            LogBase(LogLevel.Warning, messageObj);
        }

        public void LogError(FormattableString messageObj)
        {
            LogBase(LogLevel.Error, messageObj);
        }

        public void LogFatal(FormattableString messageObj)
        {
            LogBase(LogLevel.Fatal, messageObj);
        }

        public void Log(LogType category, FormattableString messageObj)
        {
            LogBase(LogCategory.ToCategory(category).BepInExCategory, messageObj);
        }

        public void Log(LogLevel category, FormattableString messageObj)
        {
            LogBase(category, messageObj);
        }

        public void Log(string category, FormattableString messageObj)
        {
            LogBase(LogCategory.ToCategory(category).BepInExCategory, messageObj);
        }

        public void Log(LogCategory category, FormattableString messageObj)
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
