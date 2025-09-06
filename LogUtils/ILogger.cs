using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Requests;
using System;
using System.Collections.Generic;
using UnityEngine;
using LoggerDocs = LogUtils.Documentation.LoggerDocumentation;

namespace LogUtils
{
    /// <summary>
    /// Represents a type used to perform logging
    /// </summary>
    public interface ILogger : ILogger<object>;

    /// <summary>
    /// Represents a type used to perform logging that supports <see cref="FormattableString"/>
    /// </summary>
    public interface IFormattableLogger : ILogger, ILogger<FormattableString>;

    /// <summary>
    /// Represents a type used to perform logging
    /// </summary>
    /// <typeparam name="T">The type accepted as the message argument</typeparam>
    public interface ILogger<T>
    {
        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        void Log(T messageObj);

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        void LogDebug(T messageObj);

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        void LogInfo(T messageObj);

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        void LogImportant(T messageObj);

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        void LogMessage(T messageObj);

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        void LogWarning(T messageObj);

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        void LogError(T messageObj);

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        void LogFatal(T messageObj);

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        void Log(LogType category, T messageObj);

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        void Log(LogLevel category, T messageObj);

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        void Log(string category, T messageObj);

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        void Log(LogCategory category, T messageObj);
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
