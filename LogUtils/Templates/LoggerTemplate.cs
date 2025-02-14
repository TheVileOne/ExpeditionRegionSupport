using BepInEx.Logging;
using LogUtils.Enums;
using System;
using UnityEngine;

namespace LogUtils.Templates
{
    /// <summary>
    /// Illustrates the standardized method order implementation of the ILogger interface
    /// </summary>
    internal sealed class LoggerTemplate : ILogger
    {
        #region Implementation
        public void Log(object data)
        {
            throw new NotImplementedException();
        }

        public void LogDebug(object data)
        {
            throw new NotImplementedException();
        }

        public void LogInfo(object data)
        {
            throw new NotImplementedException();
        }

        public void LogImportant(object data)
        {
            throw new NotImplementedException();
        }

        public void LogMessage(object data)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(object data)
        {
            throw new NotImplementedException();
        }

        public void LogError(object data)
        {
            throw new NotImplementedException();
        }

        public void LogFatal(object data)
        {
            throw new NotImplementedException();
        }

        public void Log(LogType category, object data)
        {
            throw new NotImplementedException();
        }

        public void Log(LogLevel category, object data)
        {
            throw new NotImplementedException();
        }

        public void Log(string category, object data)
        {
            throw new NotImplementedException();
        }

        public void Log(LogCategory category, object data)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
