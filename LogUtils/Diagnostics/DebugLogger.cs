using BepInEx.Logging;
using LogUtils.Enums;
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
        void ILogger.Log(object data)
        {
            Logger.Log(data);
        }

        void ILogger.Log(LogType category, object data)
        {
            Logger.Log(category, data);
        }

        void ILogger.Log(LogLevel category, object data)
        {
            Logger.Log(category, data);
        }

        void ILogger.Log(string category, object data)
        {
            Logger.Log(category, data);
        }

        void ILogger.Log(LogCategory category, object data)
        {
            Logger.Log(category, data);
        }

        void ILogger.LogDebug(object data)
        {
            Logger.LogDebug(data);
        }

        void ILogger.LogError(object data)
        {
            Logger.LogError(data);
        }

        void ILogger.LogFatal(object data)
        {
            Logger.LogFatal(data);
        }

        void ILogger.LogImportant(object data)
        {
            Logger.LogImportant(data);
        }

        void ILogger.LogInfo(object data)
        {
            Logger.LogInfo(data);
        }

        void ILogger.LogMessage(object data)
        {
            Logger.LogMessage(data);
        }

        void ILogger.LogWarning(object data)
        {
            Logger.LogWarning(data);
        }
        #endregion

        public void Dispose()
        {
            ResetState();
        }
    }
}
