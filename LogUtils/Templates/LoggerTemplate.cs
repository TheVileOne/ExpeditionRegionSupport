using BepInEx.Logging;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogUtils.Templates
{
    /// <summary>
    /// Illustrates the standardized method order implementation of the ILogger interface
    /// </summary>
    internal sealed class LoggerTemplate : ILogger
    {
        #region Implementation

        public List<LogID> LogTargets = new List<LogID>();

        public void Log(object messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogDebug(object messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogInfo(object messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogImportant(object messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogMessage(object messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(object messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogError(object messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogFatal(object messageObj)
        {
            throw new NotImplementedException();
        }

        public void Log(LogType category, object messageObj)
        {
            throw new NotImplementedException();
        }

        public void Log(LogLevel category, object messageObj)
        {
            throw new NotImplementedException();
        }

        public void Log(string category, object messageObj)
        {
            throw new NotImplementedException();
        }

        public void Log(LogCategory category, object messageObj)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
