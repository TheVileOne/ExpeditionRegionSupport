using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Formatting;
using System;
using UnityEngine;

namespace LogUtils.Compatibility.BepInEx
{
    /// <summary>
    /// A class dedicated to translating a ManualLogSource to an IExtendedLogSource
    /// </summary>
    internal sealed class ManualLogSourceWrapper : IExtendedLogSource, IFormatLogger
    {
        public readonly ManualLogSource Source;

        public string SourceName => Source.SourceName;

        public event EventHandler<LogEventArgs> LogEvent
        {
            add => Source.LogEvent += value;
            remove => Source.LogEvent -= value;
        }

        private ManualLogSourceWrapper(ManualLogSource source)
        {
            Source = source;
        }

        #region ILogger members

        public void Log(object messageObj)
        {
            Source.LogInfo(messageObj);
        }

        public void LogDebug(object messageObj)
        {
            Source.LogDebug(messageObj);
        }

        public void LogInfo(object messageObj)
        {
            Source.LogInfo(messageObj);
        }

        public void LogImportant(object messageObj)
        {
            Source.Log(LogCategory.Important.BepInExCategory, messageObj);
        }

        public void LogMessage(object messageObj)
        {
            Source.LogMessage(messageObj);
        }

        public void LogWarning(object messageObj)
        {
            Source.LogWarning(messageObj);
        }

        public void LogError(object messageObj)
        {
            Source.LogError(messageObj);
        }

        public void LogFatal(object messageObj)
        {
            Source.LogFatal(messageObj);
        }

        public void Log(LogType category, object messageObj)
        {
            Source.Log(LogCategory.ToCategory(category).BepInExCategory, messageObj);
        }

        public void Log(LogLevel category, object messageObj)
        {
            Source.Log(category, messageObj);
        }

        public void Log(LogCategory category, object messageObj)
        {
            Source.Log(category.BepInExCategory, messageObj);
        }
        #endregion
        #region IFormattableLogger members

        public void Log(InterpolatedStringHandler messageObj)
        {
            Source.LogInfo(messageObj);
        }

        public void LogDebug(InterpolatedStringHandler messageObj)
        {
            Source.LogDebug(messageObj);
        }

        public void LogInfo(InterpolatedStringHandler messageObj)
        {
            Source.LogInfo(messageObj);
        }

        public void LogImportant(InterpolatedStringHandler messageObj)
        {
            Source.Log(LogCategory.Important.BepInExCategory, messageObj);
        }

        public void LogMessage(InterpolatedStringHandler messageObj)
        {
            Source.LogMessage(messageObj);
        }

        public void LogWarning(InterpolatedStringHandler messageObj)
        {
            Source.LogWarning(messageObj);
        }

        public void LogError(InterpolatedStringHandler messageObj)
        {
            Source.LogError(messageObj);
        }

        public void LogFatal(InterpolatedStringHandler messageObj)
        {
            Source.LogFatal(messageObj);
        }

        public void Log(LogType category, InterpolatedStringHandler messageObj)
        {
            Source.Log(LogCategory.ToCategory(category).BepInExCategory, messageObj);
        }

        public void Log(LogLevel category, InterpolatedStringHandler messageObj)
        {
            Source.Log(category, messageObj);
        }

        public void Log(LogCategory category, InterpolatedStringHandler messageObj)
        {
            Source.Log(category.BepInExCategory, messageObj);
        }
        #endregion

        void IDisposable.Dispose()
        {
            //It is unclear whether cleaning up the underlying log source should be the responsibility of this adapter.
            //For this reason, it does nothing on dispose
        }

        internal static ManualLogSourceWrapper FromSource(ManualLogSource source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return new ManualLogSourceWrapper(source);
        }
    }
}
