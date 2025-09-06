using BepInEx.Logging;
using LogUtils.Enums;
using System;
using System.ComponentModel;
using UnityEngine;
using LoggerDocs = LogUtils.Documentation.LoggerDocumentation;

namespace LogUtils.Compatibility.BepInEx
{
    public class BepInExLogger : IFormattableLogger
    {
        /// <summary>
        /// BepInEx derived logging interface
        /// </summary>
        internal readonly IExtendedLogSource Source;

        public BepInExLogger(ManualLogSource source)
        {
            Source = AdapterServices.Convert(source);
        }

        public BepInExLogger(IExtendedLogSource source)
        {
            Source = source;
        }

        #region ILogger members

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        public void Log(object messageObj)
        {
            Source.LogInfo(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        public void LogDebug(object messageObj)
        {
            Source.LogDebug(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        public void LogInfo(object messageObj)
        {
            Source.LogInfo(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        public void LogImportant(object messageObj)
        {
            Log(LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        public void LogMessage(object messageObj)
        {
            Source.LogMessage(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        public void LogWarning(object messageObj)
        {
            Source.LogWarning(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        public void LogError(object messageObj)
        {
            Source.LogError(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        public void LogFatal(object messageObj)
        {
            Source.LogFatal(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogLevel category, object messageObj)
        {
            Source.Log(category, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogType category, object messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(string category, object messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogCategory category, object messageObj)
        {
            Source.Log(category.BepInExCategory, messageObj);
        }
        #endregion
        #region IFormattableLogger implementation

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(FormattableString messageObj)
        {
            Source.LogInfo(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogDebug(FormattableString messageObj)
        {
            Source.LogDebug(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogInfo(FormattableString messageObj)
        {
            Source.LogInfo(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogImportant(FormattableString messageObj)
        {
            Log(LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogMessage(FormattableString messageObj)
        {
            Source.LogMessage(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogWarning(FormattableString messageObj)
        {
            Source.LogWarning(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogError(FormattableString messageObj)
        {
            Source.LogError(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogFatal(FormattableString messageObj)
        {
            Source.LogFatal(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogLevel category, FormattableString messageObj)
        {
            Source.Log(category, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogType category, FormattableString messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(string category, FormattableString messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogCategory category, FormattableString messageObj)
        {
            Source.Log(category.BepInExCategory, messageObj);
        }
        #endregion
    }
}
