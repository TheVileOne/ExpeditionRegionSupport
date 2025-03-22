using BepInEx.Logging;
using LogUtils.Compatibility;
using LogUtils.Enums;
using System;
using System.Threading;
using UnityEngine;

namespace LogUtils
{
    internal class UtilityLogSource : IExtendedLogSource
    {
        private bool recursiveAccessFlag;

        private readonly object sourceLock = new object();

        public string SourceName => UtilityConsts.UTILITY_NAME;

        public event EventHandler<LogEventArgs> LogEvent;

        LogID[] ILogger.AvailableTargets => [LogID.BepInEx];

        internal bool IsAccessRecursive()
        {
            return recursiveAccessFlag;
        }

        #region Implementation
        public void Log(object data)
        {
            Log(LogLevel.Info, data);
        }

        public void LogDebug(object data)
        {
            Log(LogLevel.Debug, data);
        }

        public void LogInfo(object data)
        {
            Log(LogLevel.Info, data);
        }

        public void LogImportant(object data)
        {
            Log(LogCategory.Important.BepInExCategory, data);
        }

        public void LogMessage(object data)
        {
            Log(LogLevel.Message, data);
        }

        public void LogWarning(object data)
        {
            Log(LogLevel.Warning, data);
        }

        public void LogError(object data)
        {
            Log(LogLevel.Error, data);
        }

        public void LogFatal(object data)
        {
            Log(LogLevel.Fatal, data);
        }

        public void Log(LogType category, object data)
        {
            Log(LogCategory.ToCategory(category).BepInExCategory, data);
        }

        public void Log(LogLevel category, object data)
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
                LogEvent?.Invoke(this, new LogEventArgs(data, category, this));
            }
            finally
            {
                recursiveAccessFlag = false;
                Monitor.Exit(sourceLock);
            }
        }

        public void Log(string category, object data)
        {
            Log(LogCategory.ToCategory(category).BepInExCategory, data);
        }

        public void Log(LogCategory category, object data)
        {
            Log(category.BepInExCategory, data);
        }
        #endregion

        public void Dispose()
        {
            BepInEx.Logging.Logger.Sources.Remove(this);
        }
    }
}
