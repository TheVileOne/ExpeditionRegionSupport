using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Formatting;
using System;
using UnityEngine;
using LoggerDocs = LogUtils.Documentation.LoggerDocumentation;

namespace LogUtils
{
    internal sealed class FormattableLogWrapper : IFormattableLogger, IEquatable<ILogger>
    {
        public ILogger Value;

        public FormattableLogWrapper(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            Value = logger;
        }

        #region ILogger members

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        public void Log(object messageObj)
        {
            Value.Log(LogLevel.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        public void LogDebug(object messageObj)
        {
            Value.Log(LogLevel.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        public void LogInfo(object messageObj)
        {
            Value.Log(LogLevel.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        public void LogImportant(object messageObj)
        {
            Value.Log(LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        public void LogMessage(object messageObj)
        {
            Value.Log(LogLevel.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        public void LogWarning(object messageObj)
        {
            Value.Log(LogLevel.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        public void LogError(object messageObj)
        {
            Value.Log(LogLevel.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        public void LogFatal(object messageObj)
        {
            Value.Log(LogLevel.Fatal, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogType category, object messageObj)
        {
            Value.Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogLevel category, object messageObj)
        {
            Value.Log(category, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogCategory category, object messageObj)
        {
            Value.Log(category, messageObj);
        }
        #endregion
        #region IFormattableLogger members

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        public void Log(InterpolatedStringHandler messageObj)
        {
            Value.Log(LogLevel.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        public void LogDebug(InterpolatedStringHandler messageObj)
        {
            Value.Log(LogLevel.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        public void LogInfo(InterpolatedStringHandler messageObj)
        {
            Value.Log(LogLevel.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        public void LogImportant(InterpolatedStringHandler messageObj)
        {
            Value.Log(LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        public void LogMessage(InterpolatedStringHandler messageObj)
        {
            Value.Log(LogLevel.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        public void LogWarning(InterpolatedStringHandler messageObj)
        {
            Value.Log(LogLevel.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        public void LogError(InterpolatedStringHandler messageObj)
        {
            Value.Log(LogLevel.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        public void LogFatal(InterpolatedStringHandler messageObj)
        {
            Value.Log(LogLevel.Fatal, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogType category, InterpolatedStringHandler messageObj)
        {
            Value.Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogLevel category, InterpolatedStringHandler messageObj)
        {
            Value.Log(category, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogCategory category, InterpolatedStringHandler messageObj)
        {
            Value.Log(category, messageObj);
        }
        #endregion
        #region IEquality members

        /// <inheritdoc/>
        /// <remarks>Both instance, and value of instances are evaluated for equality</remarks>
        public bool Equals(ILogger other)
        {
            return other != null && (Value.Equals(other) || this == other);
        }
        #endregion
    }
}
