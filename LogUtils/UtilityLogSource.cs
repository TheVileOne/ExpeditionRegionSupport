using BepInEx.Logging;
using LogUtils.CompatibilityServices;
using System;
using System.Threading;

namespace LogUtils
{
    internal class UtilityLogSource : IExtendedLogSource
    {
        private bool recursiveAccessFlag;

        private object sourceLock = new object();

        public string SourceName => UtilityConsts.UTILITY_NAME;

        public event EventHandler<LogEventArgs> LogEvent;

        internal bool IsAccessRecursive()
        {
            return recursiveAccessFlag;
        }

        public void Log(LogLevel level, object data)
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
                LogEvent?.Invoke(this, new LogEventArgs(data, level, this));
            }
            finally
            {
                recursiveAccessFlag = false;
                Monitor.Exit(sourceLock);
            }
        }

        public void LogFatal(object data)
        {
            Log(LogLevel.Fatal, data);
        }

        public void LogError(object data)
        {
            Log(LogLevel.Error, data);
        }

        public void LogWarning(object data)
        {
            Log(LogLevel.Warning, data);
        }

        public void LogMessage(object data)
        {
            Log(LogLevel.Message, data);
        }

        public void LogInfo(object data)
        {
            Log(LogLevel.Info, data);
        }

        public void LogDebug(object data)
        {
            Log(LogLevel.Debug, data);
        }

        public void Dispose()
        {
            BepInEx.Logging.Logger.Sources.Remove(this);
        }
    }
}
