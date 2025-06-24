using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Requests;
using System.Collections.Generic;
using UnityEngine;

namespace LogUtils
{
    public interface ILogger
    {
        void Log(object data);
        void LogDebug(object data);
        void LogInfo(object data);
        void LogImportant(object data);
        void LogMessage(object data);
        void LogWarning(object data);
        void LogError(object data);
        void LogFatal(object data);

        void Log(LogType category, object data);
        void Log(LogLevel category, object data);
        void Log(string category, object data);
        void Log(LogCategory category, object data);
    }

    public interface ILogHandler : ILogFileHandler
    {
        /// <summary>
        /// Does this handler accept logging requests (local or remote)
        /// </summary>
        bool AllowLogging { get; }

        /// <summary>
        /// Does this handler accept remote logging requests
        /// </summary>
        bool AllowRemoteLogging { get; }

        /// <summary>
        /// Does this handler register with the LogRequest system
        /// </summary>
        bool AllowRegistration{ get; }

        /// <summary>
        /// Does this handler accept LogRequests of a specific log file and request type
        /// </summary>
        bool CanHandle(LogID logID, RequestType requestType);

        /// <summary>
        /// Accepts and processes a LogRequest instance
        /// </summary>
        void HandleRequest(LogRequest request);
    }

    public interface ILogFileHandler
    {
        /// <summary>
        /// Log files available for use by the handler
        /// </summary>
        IEnumerable<LogID> AvailableTargets { get; }

        /// <summary>
        /// Retrieves all log files that are accessible by the handler
        /// </summary>
        IEnumerable<LogID> GetAccessibleTargets();
    }

    public interface ILogWriterProvider
    {
        /// <summary>
        /// Gets the log writer managed by the provider
        /// </summary>
        ILogWriter GetWriter();

        /// <summary>
        /// Gets the log writer associated with the provider for a specific log file
        /// </summary>
        ILogWriter GetWriter(LogID logFile);
    }
}
