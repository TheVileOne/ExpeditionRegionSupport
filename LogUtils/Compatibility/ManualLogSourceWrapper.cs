using BepInEx.Logging;
using LogUtils.Enums;
using System;
using UnityEngine;

namespace LogUtils.Compatibility
{
    /// <summary>
    /// A class dedicated to translating a ManualLogSource to an IExtendedLogSource
    /// </summary>
    internal sealed class ManualLogSourceWrapper : IExtendedLogSource
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

        #region Implementation
        public void Log(object data)
        {
            Source.LogInfo(data);
        }

        public void LogDebug(object data)
        {
            Source.LogDebug(data);
        }

        public void LogInfo(object data)
        {
            Source.LogInfo(data);
        }

        public void LogImportant(object data)
        {
            Source.Log(LogCategory.Important.BepInExCategory, data);
        }

        public void LogMessage(object data)
        {
            Source.LogMessage(data);
        }

        public void LogWarning(object data)
        {
            Source.LogWarning(data);
        }

        public void LogError(object data)
        {
            Source.LogError(data);
        }

        public void LogFatal(object data)
        {
            Source.LogFatal(data);
        }

        public void Log(LogType category, object data)
        {
            Source.Log(LogCategory.ToCategory(category).BepInExCategory, data);
        }

        public void Log(LogLevel category, object data)
        {
            Source.Log(category, data);
        }

        public void Log(string category, object data)
        {
            Source.Log(LogCategory.ToCategory(category).BepInExCategory, data);
        }

        public void Log(LogCategory category, object data)
        {
            Source.Log(category.BepInExCategory, data);
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
