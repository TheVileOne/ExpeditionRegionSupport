using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Requests;
using Microsoft.Extensions.Logging;
using System;

namespace LogUtils.Compatibility.DotNet
{
    public class DotNetLogger : Logger, Microsoft.Extensions.Logging.ILogger
    {
        /// <summary>
        /// A pending EventID waiting for its request to be handled. The reason it is stored at the class level is because it cannot be passed into the base class
        /// </summary>
        protected EventId PendingEventID;

        /// <summary>
        /// Creates a new DotNetLogger instance
        /// </summary>
        public DotNetLogger() : base()
        {
            LogRequestEvents.OnSubmit += onNewRequest; 
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            //TODO: Implement temporary Logger state
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            //TODO: Implement
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventID, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!AllowLogging || !IsEnabled(logLevel)) return; //Remote logging is not applicable here

            LogCategory messageCategory = LoggerUtils.GetEquivalentCategory(logLevel);
            object messageData = formatter != null ? formatter.Invoke(state, exception) : state;

            using (DataLock.Acquire())
            {
                PendingEventID = eventID;
                Log(messageCategory, messageData);
            }
        }

        private void onNewRequest(LogRequest request)
        {
            if (request.Sender != this) return;

            LogMessageEventArgs messageData = request.Data;

            messageData.ExtraArgs.Add(new DotNetLoggerEventArgs(messageData.ID, PendingEventID));
            PendingEventID = default;
        }
    }
}
