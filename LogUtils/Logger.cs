﻿using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers.Extensions;
using LogUtils.Requests;
using LogUtils.Requests.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CreateRequestCallback = LogUtils.Requests.LogRequest.Factory.Callback;

namespace LogUtils
{
    /// <summary>
    /// Allows you to log messages to file, or a console using the write implementation of your choosing
    /// </summary>
    public partial class Logger : ILogger, ILogHandler, ILogSourceProvider, ILogWriterProvider, IDisposable
    {
        /// <summary>
        /// A flag that allows/disallows handling of log requests (local and remote) through this logger 
        /// </summary>
        public bool AllowLogging { get; set; }

        /// <summary>
        /// A flag that allows/disallows handling of remote log requests through this logger
        /// </summary>
        public virtual bool AllowRemoteLogging { get; set; } = true;

        /// <inheritdoc/>
        public virtual bool AllowRegistration => !IsDisposed;

        IEnumerable<LogID> ILogFileHandler.AvailableTargets => LogTargets.ToArray();

        /// <summary>
        /// Contains all assigned log targets
        /// </summary>
        protected LogTargetCollection Targets = new LogTargetCollection();

        /// <summary>
        /// Contains a list of LogIDs (both local and remote) that will be handled in the case of an untargeted log request
        /// </summary>
        public List<LogID> LogTargets => Targets.LogIDs;

        /// <summary>
        /// Contains a list of ConsoleIDs that will be handled by any log request that can be handled by this logger
        /// </summary>
        public List<ConsoleID> ConsoleTargets => Targets.ConsoleIDs;

        /// <summary>
        /// A log source that will be applied by default to log requests that require such information
        /// </summary>
        public ILogSource LogSource { get; set; }

        /// <summary>
        /// Contains a record of logger field values that can be restored on demand
        /// </summary>
        public LogRestorePoint RestorePoint;

        public IRequestValidator Validator;

        /// <summary>
        /// Writer implementation (responsible for writing to file, or storing messages in the message buffer)
        /// </summary>
        public ILogWriter Writer = LogWriter.Writer;

        #region Initialization

        static Logger()
        {
            UtilityCore.EnsureInitializedState();
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="logSource">The BepInEx log source (typically a ManualLogsource) to assign to this logger</param>
        public Logger(ILogSource logSource) : this(LoggingMode.Inherit, true, LogID.BepInEx)
        {
            LogSource = logSource;
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="preset">The LogID, or ConsoleID to target, or handle by request by this logger</param>
        public Logger(ILogTarget preset) : this(LoggingMode.Inherit, true, preset)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        /// <param name="preset">The LogID, or ConsoleID to target, or handle by request by this logger</param>
        public Logger(bool allowLogging, ILogTarget preset) : this(LoggingMode.Inherit, allowLogging, preset)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="mode">Changes the technique used to write messages to file</param>
        /// <param name="preset">The LogID, or ConsoleID to target, or handle by request by this logger</param>
        public Logger(LoggingMode mode, ILogTarget preset) : this(mode, true, preset)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="presets">Include any LogIDs, or ConsoleIDs that this logger targets, or handles on request</param>
        public Logger(params ILogTarget[] presets) : this(LoggingMode.Inherit, true, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        /// <param name="presets">Include any LogIDs, or ConsoleIDs that this logger targets, or handles on request</param>
        public Logger(bool allowLogging, params ILogTarget[] presets) : this(LoggingMode.Inherit, allowLogging, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="mode">Changes the technique used to write messages to file</param>
        /// <param name="presets">Include any LogIDs, or ConsoleIDs that this logger targets, or handles on request</param>
        public Logger(LoggingMode mode, params ILogTarget[] presets) : this(mode, true, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="mode">Changes the technique used to write messages to file</param>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        /// <param name="presets">Include any LogIDs, or ConsoleIDs that this logger targets, or handles on request</param>
        public Logger(LoggingMode mode, bool allowLogging, params ILogTarget[] presets)
        {
            if (UtilitySetup.CurrentStep < UtilitySetup.InitializationStep.INITIALIZE_ENUMS)
                throw new EarlyInitializationException("Logger created too early");

            AllowLogging = allowLogging;

            Targets.AddRange(presets);

            Validator = new RequestValidator(this);

            if (mode != LoggingMode.Inherit)
                SetWriter(mode);

            SetEvents();
            SetRestorePoint();

            if (AllowRegistration)
                UtilityCore.RequestHandler.Register(this);
        }

        public void SetWriter(LoggingMode writeMode)
        {
            switch (writeMode)
            {
                case LoggingMode.Normal:
                    if (Writer is not LogWriter || Writer.GetType().IsSubclassOf(typeof(LogWriter))) //This logging mode asks for a vanilla LogWriter instance
                        Writer = new LogWriter();
                    break;
                case LoggingMode.Queue:
                    if (Writer is not QueueLogWriter)
                        Writer = new QueueLogWriter();
                    break;
                case LoggingMode.Timed:
                    if (Writer is not TimedLogWriter)
                        Writer = new TimedLogWriter();
                    break;
                case LoggingMode.Inherit:
                    Writer = LogWriter.Writer;
                    break;
            }
        }

        #endregion
        #region Restore Points

        public void SetRestorePoint()
        {
            RestorePoint = new LogRestorePoint(this);
        }

        public void RestoreState()
        {
            AllowLogging = RestorePoint.AllowLogging;
            AllowRemoteLogging = RestorePoint.AllowRemoteLogging;

            Targets.Clear();
            Targets.AddRange(RestorePoint.LogTargets);
        }
        #endregion

        #region Logging
#pragma warning disable CS1591 //Missing XML comment for publicly visible type or member

        protected void LogData(ILogTarget target, LogCategory category, object messageObj, bool shouldFilter, Color messageColor)
        {
            EventArgs extraData = new ColorEventArgs(messageColor);
            LogData(target, category, messageObj, shouldFilter, LogRequest.Factory.CreateDataCallback(extraData));
        }

        protected void LogData(CompositeLogTarget target, LogCategory category, object messageObj, bool shouldFilter, Color messageColor)
        {
            LogData(target.ToCollection(), category, messageObj, shouldFilter, messageColor);
        }

        protected void LogData(LogTargetCollection targets, LogCategory category, object messageObj, bool shouldFilter, Color messageColor)
        {
            EventArgs extraData = new ColorEventArgs(messageColor);
            LogData(targets, category, messageObj, shouldFilter, LogRequest.Factory.CreateDataCallback(extraData));
        }

        protected virtual void LogData(ILogTarget target, LogCategory category, object messageObj, bool shouldFilter, CreateRequestCallback createRequest = null)
        {
            if (!AllowLogging || !target.IsEnabled) return;

            if (createRequest == null)
                createRequest = LogRequest.Factory.Create;

            LogRequest request = createRequest.Invoke(target.GetRequestType(this), target, category, messageObj, shouldFilter);

            if (request != null)
                LogData(request);
        }

        protected void LogData(CompositeLogTarget target, LogCategory category, object messageObj, bool shouldFilter, CreateRequestCallback createRequest = null)
        {
            LogData(target.ToCollection(), category, messageObj, shouldFilter, createRequest);
        }

        protected virtual void LogData(LogTargetCollection targets, LogCategory category, object messageObj, bool shouldFilter, CreateRequestCallback createRequest = null)
        {
            if (!AllowLogging) return;

            if (targets.Count == 0)
            {
                UtilityLogger.LogWarning("Attempted to log message with no available log targets");
                return;
            }

            if (createRequest == null)
                createRequest = LogRequest.Factory.Create;

            IEnumerable<ILogTarget> enabledTargets = targets.LogIDs.Where(t => t.IsEnabled);

            LogRequest lastRequest = null;
            foreach (LogID target in enabledTargets.Cast<LogID>())
            {
                LogRequest currentRequest = createRequest.Invoke(target.GetRequestType(this), target, category, messageObj, shouldFilter);

                if (currentRequest != null)
                {
                    //Avoids the possibility of messages being processed by the console more than once
                    currentRequest.InheritHandledConsoleTargets(lastRequest);

                    lastRequest = currentRequest;
                    LogData(currentRequest);
                }
            }

            enabledTargets = targets.ConsoleIDs.Where(t => t.IsEnabled);

            if (lastRequest != null) //Possible to be null if all of the requests were rejected
            {
                var consoleMessageData = lastRequest.Data.GetConsoleData();

                //Exclude any ConsoleIDs that were already handled when the LogIDs were processed
                if (consoleMessageData != null)
                    enabledTargets = enabledTargets.Except(consoleMessageData.Handled);
            }

            foreach (ConsoleID target in enabledTargets.Cast<ConsoleID>())
            {
                LogRequest currentRequest = createRequest.Invoke(RequestType.Console, target, category, messageObj, shouldFilter);

                if (currentRequest != null)
                {
                    //Avoids the possibility of messages being processed by the console more than once
                    currentRequest.InheritHandledConsoleTargets(lastRequest);

                    lastRequest = currentRequest;
                    LogData(currentRequest);
                }
            }
        }

        protected virtual void LogData(LogRequest request)
        {
            try
            {
                UtilityCore.RequestHandler.RecursionCheckCounter++;

                request.Sender = this;
                request.Data.LogSource = LogSource;

                //Local requests are processed immediately by the logger, while other types of requests are handled through RequestHandler
                if (request.Type != RequestType.Local)
                {
                    UtilityCore.RequestHandler.Submit(request, true);
                }
                else
                {
                    using (UtilityCore.RequestHandler.BeginCriticalSection())
                    {
                        UtilityCore.RequestHandler.Submit(request, false);

                        if (request.Status != RequestStatus.Rejected)
                            SendToWriter(request);
                    }
                }
            }
            finally
            {
                UtilityCore.RequestHandler.RecursionCheckCounter--;
            }
        }
#pragma warning restore CS1591 //Missing XML comment for publicly visible type or member

        /// <summary>
        /// Determines if the specified LogID can be handled by this logger with the specified RequestType
        /// </summary>
        public bool CanHandle(LogID logID, RequestType requestType)
        {
            if (logID.IsGameControlled) return false;

            LogID loggerID = this.FindEquivalentTarget(logID);

            if (loggerID == null || loggerID.Access == LogAccess.RemoteAccessOnly) //Logger can only send remote requests for this LogID
                return false;

            //TODO: Enabled status is currently not evaluated here - It is unclear whether it should be part of the access check
            return requestType == RequestType.Local || loggerID.Access != LogAccess.Private;
        }

        IEnumerable<LogID> ILogFileHandler.GetAccessibleTargets()
        {
            return LogTargets.Where(log => !log.IsGameControlled && log.Access != LogAccess.RemoteAccessOnly);
        }

        ILogWriter ILogWriterProvider.GetWriter()
        {
            return Writer;
        }

        ILogWriter ILogWriterProvider.GetWriter(LogID logFile)
        {
            return Writer;
        }

        /// <inheritdoc/>
        public void HandleRequest(LogRequest request)
        {
            if (request.Submitted)
                UtilityCore.RequestHandler.CurrentRequest = request;

            request.ResetStatus(); //Ensure that processing request is handled in a consistent way
            request.Validate(Validator);

            if (request.Status == RequestStatus.Rejected)
                return;

            SendToWriter(request);
        }

        internal void SendToWriter(LogRequest request)
        {
            var writer = Writer;

            if (writer == null) //This is possible when the Logger gets disposed - Ideally the logger should not be referenced after disposal
            {
                UtilityLogger.LogWarning("Log writer unavailable to handle this request - deploying fallback writer");
                writer = LogWriter.Writer;
            }

            request.Host = this;
            writer.WriteFrom(request);
        }
        #endregion

        #region Dispose pattern

        protected bool IsDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            LogWriter.TryDispose(Writer);

            if (AllowRegistration)
            {
                UtilityCore.RequestHandler.Unregister(this);
                UtilityEvents.OnRegistrationChanged -= registrationChangedHandler;
            }

            Writer = null;
            IsDisposed = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            //Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~Logger()
        {
            Dispose(disposing: false);
        }

        #endregion

        /// <summary>
        /// A snapshot of the logger's state at a certain point in time
        /// </summary>
        public struct LogRestorePoint
        {
            public bool AllowLogging;
            public bool AllowRemoteLogging;
            public LogTargetCollection LogTargets;

            public LogRestorePoint(Logger logger)
            {
                AllowLogging = logger.AllowLogging;
                AllowRemoteLogging = logger.AllowRemoteLogging;
                LogTargets = logger.Targets.AllTargets;
            }
        }

        public class EarlyInitializationException(string message) : InvalidOperationException(message);
    }

    public enum LoggingMode
    {
        Inherit = 0,
        Normal,
        Queue,
        Timed
    }
}
