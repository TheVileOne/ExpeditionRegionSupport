using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Requests;
using System.Collections.Generic;
using UnityEngine;
using LoggerDocs = LogUtils.Documentation.LoggerDocumentation;

namespace LogUtils
{
    /// <summary>
    /// Represents a type used to perform logging
    /// </summary>
    public interface ILogger
    {
        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        void Log(object data);

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        void LogDebug(object data);

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        void LogInfo(object data);

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        void LogImportant(object data);

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        void LogMessage(object data);

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        void LogWarning(object data);

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        void LogError(object data);

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        void LogFatal(object data);

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        void Log(LogType category, object data);

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        void Log(LogLevel category, object data);

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        void Log(string category, object data);

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        void Log(LogCategory category, object data);
    }

    /// <summary>
    /// Represents a type used to process logging requests
    /// </summary>
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
        bool AllowRegistration { get; }

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

    /// <summary>
    /// Provides access to an ILogSource
    /// </summary>
    public interface ILogSourceProvider
    {
        /// <summary>
        /// The logging source associated with the provider
        /// </summary>
        ILogSource LogSource { get; set; }
    }

    /// <summary>
    /// Provides access to an ILogWriter
    /// </summary>
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
