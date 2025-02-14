using BepInEx.Logging;
using System;

namespace LogUtils.CompatibilityServices
{
    /// <summary>
    /// A class dedicated to translating a ManualLogSource to an IExtendedLogSource
    /// </summary>
    internal sealed class LogSourceAdapter : IExtendedLogSource
    {
        public readonly ManualLogSource Source;

        public string SourceName => Source.SourceName;

        public event EventHandler<LogEventArgs> LogEvent
        {
            add => Source.LogEvent += value;
            remove => Source.LogEvent -= value;
        }

        private LogSourceAdapter(ManualLogSource source)
        {
            Source = source;
        }

        #region Implementation
        public void Log(LogLevel level, object data)
        {
            Source.Log(level, data);
        }

        public void LogFatal(object data)
        {
            Source.LogFatal(data);
        }

        public void LogError(object data)
        {
            Source.LogError(data);
        }

        public void LogWarning(object data)
        {
            Source.LogWarning(data);
        }

        public void LogMessage(object data)
        {
            Source.LogMessage(data);
        }

        public void LogInfo(object data)
        {
            Source.LogInfo(data);
        }

        public void LogDebug(object data)
        {
            Source.LogDebug(data);
        }

        void IDisposable.Dispose()
        {
            //It is unclear whether cleaning up the underlying log source should be the responsibility of this adapter.
            //For this reason, it does nothing on dispose
        }
        #endregion

        internal static IExtendedLogSource FromManualSource(ManualLogSource source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return new LogSourceAdapter(source);
        }

        /// <summary>
        /// Attempts to convert a log source instance into a form that LogUtils can recognize
        /// </summary>
        /// <param name="source">A log source</param>
        /// <param name="conversion">Result parameter (set to null if conversion fails)</param>
        /// <returns>true if conversion was successful; otherwise false</returns>
        public static bool TryCreate(in ILogSource source, out IExtendedLogSource conversion)
        {
            if (source is ManualLogSource manualSource)
            {
                conversion = new LogSourceAdapter(manualSource);
                return true;
            }

            //No one has a good reason to adapt a logging source that is already compatible with LogUtils, but handle it anyways
            if (source is IExtendedLogSource extendedSource)
            {
                conversion = extendedSource;
                return true;
            }

            conversion = null;
            return false;
        }
    }
}
