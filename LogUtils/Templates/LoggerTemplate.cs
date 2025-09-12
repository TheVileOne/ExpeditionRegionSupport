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
    internal sealed class LoggerTemplate : ILogger, IFormattableLogger
    {
        /*
         * IFormattableLogger is optional. Implement interface if you want to add support for color formatting features for your logger. Alternatively,
         * your logger class can use the FormattableLogWrapper class to suppose these features without implementing the IFormattableLogger interface.
         */

        #region ILogger members

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
        #region IFormattableLogger members

        public void Log(InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogDebug(InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogInfo(InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogImportant(InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogMessage(InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogError(InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }

        public void LogFatal(InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }

        public void Log(LogType category, InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }

        public void Log(LogLevel category, InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }

        public void Log(string category, InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }

        public void Log(LogCategory category, InterpolatedStringHandler messageObj)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
