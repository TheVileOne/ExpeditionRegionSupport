using LogUtils.Enums;
using System;
using UnityEngine;

namespace LogUtils.Compatibility.Unity
{
    internal sealed class UnityLogHandler : Logger
    {
        /// <summary>
        /// Flag indicates that not all requests processed are known to have made it to Unity's logging API through this handler
        /// </summary>
        private bool _hasPendingRequests;

        private UnityEngine.ILogHandler _logHandler;
        internal override UnityEngine.ILogHandler Handler
        {
            get => _logHandler;
            set => _logHandler = value;
        }

        public UnityLogHandler() : base(LogID.Unity)
        {
            _logHandler = Debug.unityLogger.logHandler;
        }

        internal override void LogException(Exception exception, UnityEngine.Object context)
        {
            UtilityLogger.LogWarning(exception);

            if (!UnityLogger.IsSafeToLogToUnity)
            {
                _hasPendingRequests = true;
                base.LogException(exception, context);
                return;
            }

            if (_hasPendingRequests) //Ensure that requests are handled in the order they were processed
            {
                _hasPendingRequests = false;
                UtilityCore.RequestHandler.ProcessRequests(LogID.Unity | LogID.Exception);
            }

            _logHandler.LogException(exception, context);
        }

        internal override void LogFormat(LogType category, UnityEngine.Object context, string format, params object[] formatArgs)
        {
            if (!UnityLogger.IsSafeToLogToUnity)
            {
                _hasPendingRequests = true;
                base.LogFormat(category, context, format, formatArgs);
                return;
            }

            if (_hasPendingRequests) //Ensure that requests are handled in the order they were processed
            {
                _hasPendingRequests = false;
                UtilityCore.RequestHandler.ProcessRequests(LogID.Unity | LogID.Exception);
            }
            _logHandler.LogFormat(category, context, format, formatArgs);
        }
    }
}
