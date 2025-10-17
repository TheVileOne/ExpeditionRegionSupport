using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Requests;
using Microsoft.Extensions.Logging;
using System;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace LogUtils.Compatibility.DotNet
{
    /// <summary>
    /// A logger type that implements <see cref="Microsoft.Extensions.Logging.ILogger"/> interface
    /// </summary>
    /// <remarks>Allows usage of all existing logging overloads in addition to those provided by the interface</remarks>
    public class DotNetLogger : Logger, Microsoft.Extensions.Logging.ILogger
    {
        /// <summary>
        /// Constructs a new <see cref="DotNetLogger"/> instance
        /// </summary>
        /// <inheritdoc select="params"/>
        public DotNetLogger(ILogSource logSource) : base(LoggingMode.Inherit, true, LogID.BepInEx)
        {
            LogSource = logSource;
        }

        /// <inheritdoc cref="DotNetLogger(ILogSource)"/>
        /// <inheritdoc select="params"/>
        public DotNetLogger(ILogTarget preset) : base(LoggingMode.Inherit, true, preset)
        {
        }

        /// <inheritdoc cref="DotNetLogger(ILogSource)"/>
        /// <inheritdoc select="params"/>
        public DotNetLogger(bool allowLogging, ILogTarget preset) : base(LoggingMode.Inherit, allowLogging, preset)
        {
        }

        /// <inheritdoc cref="DotNetLogger(ILogSource)"/>
        /// <inheritdoc select="params"/>
        public DotNetLogger(LoggingMode mode, ILogTarget preset) : base(mode, true, preset)
        {
        }

        /// <inheritdoc cref="DotNetLogger(ILogSource)"/>
        /// <inheritdoc select="params"/>
        public DotNetLogger(LoggingMode mode, bool allowLogging, ILogTarget preset) : base(mode, allowLogging, preset)
        {
        }

        /// <inheritdoc cref="DotNetLogger(ILogSource)"/>
        /// <inheritdoc select="params"/>
        public DotNetLogger(params ILogTarget[] presets) : base(LoggingMode.Inherit, true, presets)
        {
        }

        /// <inheritdoc cref="DotNetLogger(ILogSource)"/>
        /// <inheritdoc select="params"/>
        public DotNetLogger(bool allowLogging, params ILogTarget[] presets) : base(LoggingMode.Inherit, allowLogging, presets)
        {
        }

        /// <inheritdoc cref="DotNetLogger(ILogSource)"/>
        /// <inheritdoc select="params"/>
        public DotNetLogger(LoggingMode mode, params ILogTarget[] presets) : base(mode, true, presets)
        {
        }

        /// <inheritdoc cref="DotNetLogger(ILogSource)"/>
        /// <inheritdoc select="params"/>
        public DotNetLogger(LoggingMode mode, bool allowLogging, params ILogTarget[] presets) : base(mode, allowLogging, presets)
        {
        }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <remarks>Currently not implemented and will throw an exception</remarks>
        /// <param name="state">The identifier for the scope.</param>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <returns>An <see cref="IDisposable"/> that ends the logical operation scope on dispose.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            //TODO: Implement temporary Logger state
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the given <paramref name="category"/> is enabled.
        /// </summary>
        /// <param name="category">Logging context to be checked.</param>
        /// <returns><see langword="true" /> if enabled.</returns>
        public bool IsEnabled(LogLevel category)
        {
            return LogFilter.IsAllowed(LoggerUtils.GetEquivalentCategory(category));
        }

        /// <summary>
        /// Logs a message using the <see cref="Microsoft.Extensions.Logging.ILogger"/> interface, storing all provided event state to be accessed at a later stage.
        /// </summary>
        /// <param name="category">Entry will be written on this level.</param>
        /// <param name="eventID">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <see cref="string"/> message of the <paramref name="state"/> and <paramref name="exception"/>.</param>
        /// <typeparam name="TState">The type of the object to be written.</typeparam>
        public void Log<TState>(LogLevel category, EventId eventID, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!AllowLogging || !IsEnabled(category)) return; //Remote logging is not applicable here

            object messageObj = formatter != null ? formatter.Invoke(state, exception) : state;

            EventArgs extraData = new DotNetLoggerEventArgs(eventID);
            LogBase(LoggerUtils.GetEquivalentCategory(category), messageObj, false, LogRequest.Factory.CreateDataCallback(extraData));
        }
    }
}
