using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Formatting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using LoggerDocs = LogUtils.Documentation.LoggerDocumentation;

namespace LogUtils.Diagnostics
{
    public class DebugLogger : ILogger, IDisposable
    {
        public ILogger Logger;

        public StringProvider Provider;

        protected IEnumerator<string> Enumerator;

        public DebugLogger(ILogger logger, StringProvider provider)
        {
            Logger = logger;
            Provider = provider;

            BeginEnumeration();
        }

        /// <summary>
        /// Enumerates through a string provider if available
        /// </summary>
        /// <exception cref="NullReferenceException">Provider is null</exception>
        public void BeginEnumeration()
        {
            Enumerator = Provider.GetEnumerator();
        }

        /// <summary>
        /// Resets state to before enumerating process began
        /// </summary>
        public void ResetState()
        {
            Enumerator.Dispose();
            Enumerator = default;
        }

        /// <summary>
        /// Attempts to log currently provided string
        /// </summary>
        public void LogCurrent()
        {
            Logger.Log(Enumerator.Current);
        }

        /// <summary>
        /// Moves to next provided string, and attempts to log it
        /// </summary>
        public void LogNext()
        {
            Enumerator.MoveNext();
            Logger.Log(Enumerator.Current);
        }

        #region ILogger members

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void Log(object messageObj)
        {
            Logger.Log(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void LogDebug(object messageObj)
        {
            Logger.LogDebug(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void LogInfo(object messageObj)
        {
            Logger.LogInfo(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void LogImportant(object messageObj)
        {
            Logger.LogImportant(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void LogMessage(object messageObj)
        {
            Logger.LogMessage(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void LogWarning(object messageObj)
        {
            Logger.LogWarning(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        void ILogger<object>.LogError(object messageObj)
        {
            Logger.LogError(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void LogFatal(object messageObj)
        {
            Logger.LogFatal(messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void Log(LogType category, object messageObj)
        {
            Logger.Log(category, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void Log(LogLevel category, object messageObj)
        {
            Logger.Log(category, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void Log(LogCategory category, object messageObj)
        {
            Logger.Log(category, messageObj);
        }
        #endregion

        /// <summary>
        /// Resets enumerator to a default state
        /// </summary>
        public void Dispose()
        {
            ResetState();
        }
    }
}
