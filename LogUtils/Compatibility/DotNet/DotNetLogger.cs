using LogUtils.Enums;
using LogUtils.Requests;
using Microsoft.Extensions.Logging;
using System;

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

        /// <summary>
        /// Logs a message using Microsoft's ILogger interface
        /// </summary>
        /// <typeparam name="TState">The type belonging to the provided message object state</typeparam>
        /// <param name="logLevel">The specified logging context</param>
        /// <param name="eventID">Extra identifying information associated with the log event</param>
        /// <param name="state">The message object</param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventID, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!AllowLogging || !IsEnabled(logLevel)) return; //Remote logging is not applicable here

            EventArgs extraData = new DotNetLoggerEventArgs(eventID);
            LogRequest addDataToRequest(ILogTarget target, LogCategory category, object data, bool shouldFilter)
            {
                LogRequest request = CreateRequest(target, category, data, shouldFilter);

                if (request != null)
                    request.Data.ExtraArgs.Add(extraData);
                return request;
            }

            object messageObj = formatter != null ? formatter.Invoke(state, exception) : state;

            LogData(Targets, LoggerUtils.GetEquivalentCategory(logLevel), messageObj, false, addDataToRequest);
        }
    }
}
