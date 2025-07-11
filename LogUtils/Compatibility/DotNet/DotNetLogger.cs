using LogUtils.Enums;
using LogUtils.Requests;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace LogUtils.Compatibility.DotNet
{
    /// <summary>
    /// A logger type that implements Microsoft's ILogger interface 
    /// </summary>
    /// <remarks>Allows you to use Microsoft's logging interface in addition to existing Logger provided functions</remarks>
    public class DotNetLogger : Logger, Microsoft.Extensions.Logging.ILogger
    {
        /// <summary>
        /// Creates a new DotNetLogger instance
        /// </summary>
        public DotNetLogger() : base()
        {
        }

        /// <summary>
        /// Currently not implemented
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            //TODO: Implement temporary Logger state
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return LogFilter.IsAllowed(LoggerUtils.GetEquivalentCategory(logLevel));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventID, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!AllowLogging || !IsEnabled(logLevel)) return; //Remote logging is not applicable here

            LogCategory messageCategory = LoggerUtils.GetEquivalentCategory(logLevel);
            object messageData = formatter != null ? formatter.Invoke(state, exception) : state;

            if (dataCache == null)
                dataCache = new ThreadLocal<EventArgs>();

            dataCache.Value = new DotNetLoggerEventArgs(eventID);
            Log(messageCategory, messageData);
        }

        protected override void ClearEventData()
        {
            base.ClearEventData();

            if (dataCache?.IsValueCreated == true)
                dataCache.Value = null;
        }

        protected override void OnNewRequest(LogRequest request)
        {
            if (request.Sender != this) return;

            base.OnNewRequest(request);

            //DotNetLogger exclusive data
            if (dataCache?.IsValueCreated == true)
                request.Data.ExtraArgs.Add(dataCache.Value);
        }

        /// <summary>
        /// Contains event data exclusive to logging through a DotNetLogger
        /// </summary>
        private ThreadLocal<EventArgs> dataCache;
    }
}
