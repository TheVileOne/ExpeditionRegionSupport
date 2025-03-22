using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Requests;
using UnityEngine;

namespace LogUtils
{
    public interface ILogger
    {
        /// <summary>
        /// Log file selected by the logger when no log target is provided
        /// </summary>
        public LogID[] AvailableTargets { get; }

        public void Log(object data);
        public void LogDebug(object data);
        public void LogInfo(object data);
        public void LogImportant(object data);
        public void LogMessage(object data);
        public void LogWarning(object data);
        public void LogError(object data);
        public void LogFatal(object data);

        public void Log(LogType category, object data);
        public void Log(LogLevel category, object data);
        public void Log(string category, object data);
        public void Log(LogCategory category, object data);
    }

    public interface ILoggerBase
    {
        /// <summary>
        /// Can this logger instance accept, and process a specific LogRequest instance
        /// </summary>
        bool CanHandle(LogRequest request, bool doPathCheck = false);

        /// <summary>
        /// Accepts and processes a LogRequest instance
        /// </summary>
        void HandleRequest(LogRequest request, bool skipAccessValidation = false);
    }
}
