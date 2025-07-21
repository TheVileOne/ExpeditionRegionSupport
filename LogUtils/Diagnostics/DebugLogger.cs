using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Formatting;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        #region ILogger implementation
        void ILogger.Log(object messageObj)
        {
            Logger.Log(messageObj);
        }

        void ILogger.Log(LogType category, object messageObj)
        {
            Logger.Log(category, messageObj);
        }

        void ILogger.Log(LogLevel category, object messageObj)
        {
            Logger.Log(category, messageObj);
        }

        void ILogger.Log(string category, object messageObj)
        {
            Logger.Log(category, messageObj);
        }

        void ILogger.Log(LogCategory category, object messageObj)
        {
            Logger.Log(category, messageObj);
        }

        void ILogger.LogDebug(object messageObj)
        {
            Logger.LogDebug(messageObj);
        }

        void ILogger.LogError(object messageObj)
        {
            Logger.LogError(messageObj);
        }

        void ILogger.LogFatal(object messageObj)
        {
            Logger.LogFatal(messageObj);
        }

        void ILogger.LogImportant(object messageObj)
        {
            Logger.LogImportant(messageObj);
        }

        void ILogger.LogInfo(object messageObj)
        {
            Logger.LogInfo(messageObj);
        }

        void ILogger.LogMessage(object messageObj)
        {
            Logger.LogMessage(messageObj);
        }

        void ILogger.LogWarning(object messageObj)
        {
            Logger.LogWarning(messageObj);
        }
        #endregion

        public void Dispose()
        {
            ResetState();
        }
    }
}
