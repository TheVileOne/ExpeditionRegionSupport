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

namespace LogUtils
{
    public partial class Logger : ILogger, ILogHandler, ILogWriterProvider, IDisposable
    {
        /// <summary>
        /// A flag that allows/disallows handling of log requests (local and remote) through this logger 
        /// </summary>
        public bool AllowLogging { get; set; }

        /// <summary>
        /// A flag that allows/disallows handling of remote log requests through this logger
        /// </summary>
        public virtual bool AllowRemoteLogging { get; set; } = true;

        public virtual bool AllowRegistration => !IsDisposed;

        IEnumerable<LogID> ILogFileHandler.AvailableTargets => LogTargets.ToArray();

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
        /// BepInEx logging source object
        /// </summary>
        public ManualLogSource ManagedLogSource;

        /// <summary>
        /// Contains a record of logger field values that can be restored on demand
        /// </summary>
        public LoggerRestorePoint RestorePoint;

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
        /// <param name="logSource">Assigns a BepInEx log source to this logger, targeting BepInEx log file</param>
        public Logger(ManualLogSource logSource) : this(LoggingMode.Inherit, true, LogID.BepInEx)
        {
            ManagedLogSource = logSource;
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
            RestorePoint = new LoggerRestorePoint(this);
        }

        public void RestoreState()
        {
            AllowLogging = RestorePoint.AllowLogging;
            AllowRemoteLogging = RestorePoint.AllowRemoteLogging;

            LogTargets.Clear();
            LogTargets.AddRange(RestorePoint.LogTargets);
        }

        #endregion

        #region Logging

        #region Log Overloads (object)

        public void Log(object data)
        {
            Log(LogCategory.Default, data);
        }

        public void LogOnce(object data)
        {
            LogOnce(LogCategory.Default, data);
        }

        public void LogDebug(object data)
        {
            Log(LogCategory.Debug, data);
        }

        public void LogInfo(object data)
        {
            Log(LogCategory.Info, data);
        }

        public void LogImportant(object data)
        {
            Log(LogCategory.Important, data);
        }

        public void LogMessage(object data)
        {
            Log(LogCategory.Message, data);
        }

        public void LogWarning(object data)
        {
            Log(LogCategory.Warning, data);
        }

        public void LogError(object data)
        {
            Log(LogCategory.Error, data);
        }

        public void LogFatal(object data)
        {
            Log(LogCategory.Fatal, data);
        }

        #region Base log overloads
        #region BepInEx
        public void LogBepEx(object data)
        {
            LogData(LogID.BepInEx, LogCategory.Default, data, false);
        }

        public void LogBepEx(LogLevel category, object data)
        {
            LogData(LogID.BepInEx, LogCategory.ToCategory(category), data, false);
        }

        public void LogBepEx(LogCategory category, object data)
        {
            LogData(LogID.BepInEx, category, data, false);
        }
        #endregion
        #region Unity
        public void LogUnity(object data)
        {
            LogData(LogID.Unity, LogCategory.Default, data, false);
        }

        public void LogUnity(LogType category, object data)
        {
            LogData(LogCategory.GetUnityLogID(category), LogCategory.ToCategory(category), data, false);
        }

        public void LogUnity(LogCategory category, object data)
        {
            LogData(LogCategory.GetUnityLogID(category.UnityCategory), category, data, false);
        }
        #endregion
        #region Expedition
        public void LogExp(object data)
        {
            LogData(LogID.Expedition, LogCategory.Default, data, false);
        }

        public void LogExp(LogCategory category, object data)
        {
            LogData(LogID.Expedition, category, data, false);
        }
        #endregion
        #region JollyCoop
        public void LogJolly(object data)
        {
            LogData(LogID.JollyCoop, LogCategory.Default, data, false);
        }

        public void LogJolly(LogCategory category, object data)
        {
            LogData(LogID.JollyCoop, category, data, false);
        }
        #endregion
        #endregion

        public void Log(LogType category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        public void Log(LogLevel category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        public void Log(string category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        public void Log(LogCategory category, object data)
        {
            LogData(Targets, category, data, false);
        }

        public void LogOnce(LogType category, object data)
        {
            LogOnce(LogCategory.ToCategory(category), data);
        }

        public void LogOnce(LogLevel category, object data)
        {
            LogOnce(LogCategory.ToCategory(category), data);
        }

        public void LogOnce(string category, object data)
        {
            LogOnce(LogCategory.ToCategory(category), data);
        }

        public void LogOnce(LogCategory category, object data)
        {
            LogData(Targets, category, data, true);
        }

        #endregion
        #region  Log Overloads (LogID, object)

        public void Log(ILogTarget target, object data)
        {
            Log(target, LogCategory.Default, data);
        }

        public void LogOnce(ILogTarget target, object data)
        {
            Log(target, LogCategory.Default, data);
        }

        public void LogDebug(ILogTarget target, object data)
        {
            Log(target, LogCategory.Debug, data);
        }

        public void LogInfo(ILogTarget target, object data)
        {
            Log(target, LogCategory.Info, data);
        }

        public void LogImportant(ILogTarget target, object data)
        {
            Log(target, LogCategory.Important, data);
        }

        public void LogMessage(ILogTarget target, object data)
        {
            Log(target, LogCategory.Message, data);
        }

        public void LogWarning(ILogTarget target, object data)
        {
            Log(target, LogCategory.Warning, data);
        }

        public void LogError(ILogTarget target, object data)
        {
            Log(target, LogCategory.Error, data);
        }

        public void LogFatal(ILogTarget target, object data)
        {
            Log(target, LogCategory.Fatal, data);
        }

        public void Log(ILogTarget target, LogLevel category, object data)
        {
            Log(target, LogCategory.ToCategory(category), data);
        }

        public void Log(ILogTarget target, string category, object data)
        {
            Log(target, LogCategory.ToCategory(category), data);
        }

        public void Log(ILogTarget target, LogCategory category, object data)
        {
            LogData(target, category, data, false);
        }

        public void LogOnce(ILogTarget target, LogCategory category, object data)
        {
            LogData(target, category, data, true);
        }

        #endregion
        #region  Log Overloads (IEnumerable<LogID>, object)

        public void Log(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Default, data);
        }

        public void LogOnce(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Default, data);
        }

        public void LogDebug(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Debug, data);
        }

        public void LogInfo(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Info, data);
        }

        public void LogImportant(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Important, data);
        }

        public void LogMessage(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Message, data);
        }

        public void LogWarning(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Warning, data);
        }

        public void LogError(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Error, data);
        }

        public void LogFatal(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Fatal, data);
        }

        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, object data)
        {
            Log(targets, LogCategory.ToCategory(category), data);
        }

        public void Log(IEnumerable<ILogTarget> targets, string category, object data)
        {
            Log(targets, LogCategory.ToCategory(category), data);
        }

        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, object data)
        {
            LogData(new LogTargetCollection(targets), category, data, false);
        }

        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, object data)
        {
            LogData(new LogTargetCollection(targets), category, data, true);
        }

        #endregion
        protected void LogData(ILogTarget target, LogCategory category, object data, bool shouldFilter)
        {
            LogData(target, category, data, shouldFilter, null, LoggingContext.SingleRequest);
        }

        protected void LogData(CompositeLogTarget target, LogCategory category, object data, bool shouldFilter)
        {
            LogData(target.ToCollection(), category, data, shouldFilter);
        }

        protected virtual void LogData(LogTargetCollection targets, LogCategory category, object data, bool shouldFilter)
        {
            try
            {
                if (targets.Count == 0)
                {
                    UtilityLogger.LogWarning("Attempted to log message with no available log targets");
                    return;
                }

                LogRequest lastRequest = null;
                foreach (LogID target in targets.LogIDs)
                    lastRequest = LogData(target, category, data, shouldFilter, lastRequest, LoggingContext.Batching);

                IEnumerable<ConsoleID> consoleTargets = targets.ConsoleIDs;

                if (lastRequest != null) //Possible to be null if all of the requests were rejected
                {
                    var consoleMessageData = lastRequest.Data.GetConsoleData();

                    //Exclude any ConsoleIDs that were already handled when the LogIDs were processed
                    if (consoleMessageData != null)
                        consoleTargets = consoleTargets.Except(consoleMessageData.Handled);
                }

                foreach (ConsoleID target in consoleTargets)
                    lastRequest = LogData(target, category, data, shouldFilter, lastRequest, LoggingContext.Batching);
            }
            finally
            {
                ClearEventData();
            }
        }

        protected virtual LogRequest LogData(ILogTarget target, LogCategory category, object data, bool shouldFilter, LogRequest lastRequest, LoggingContext context)
        {
            try
            {
                UtilityCore.RequestHandler.RecursionCheckCounter++;

                if (target is CompositeLogTarget)
                    UtilityLogger.LogWarning("Composite processing not supported");

                if (!AllowLogging || !target.IsEnabled) return null;

                RequestType requestType = target.GetRequestType(this);

                if (requestType == RequestType.Invalid)
                {
                    UtilityLogger.LogWarning("Processed an invalid log request");
                    return null;
                }

                LogRequest request = null;
                LogRequestEventArgs requestData = null;

                if (requestType == RequestType.Console)
                {
                    ConsoleID consoleTarget = (ConsoleID)target;
                    requestData = new LogRequestEventArgs(consoleTarget, data, category);
                }
                else
                {
                    LogID fileTarget = (LogID)target;
                    requestData = new LogRequestEventArgs(fileTarget, data, category);
                }

                requestData.LogSource = ManagedLogSource;
                request = new LogRequest(requestType, requestData);

                if (shouldFilter)
                {
                    request.Data.ShouldFilter = true;
                    request.Data.FilterDuration = FilterDuration.OnClose;
                }

                if (lastRequest != null)
                {
                    //ConsoleIDs that were processed by an earlier request need to be transferred to the current request to avoid messages being logged to console more than once
                    var consoleMessageData = lastRequest.Data.GetConsoleData();

                    if (consoleMessageData != null)
                        request.NotifyComplete(consoleMessageData.Handled);
                }

                request.Sender = this;

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
                return request;
            }
            finally
            {
                UtilityCore.RequestHandler.RecursionCheckCounter--;
                if (context == LoggingContext.SingleRequest)
                    ClearEventData();
            }
        }

        public bool CanHandle(LogID logID, RequestType requestType)
        {
            if (logID.IsGameControlled) return false;

            LogID loggerID = this.FindEquivalentTarget(logID);

            if (loggerID == null || loggerID.Access == LogAccess.RemoteAccessOnly) //Logger can only send remote requests for this LogID
                return false;

            //TODO: Enabled status is currently not evaluated here - It is unclear whether it should be part of the access check
            return requestType == RequestType.Local || loggerID.Access != LogAccess.Private;
        }

        /// <summary>
        /// Retrieves all log files accessible by the logger
        /// </summary>
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

        public class EarlyInitializationException(string message) : InvalidOperationException(message)
        {
        }
    }

    public enum LoggingContext
    {
        SingleRequest,
        Batching
    }

    public enum LoggingMode
    {
        Inherit = 0,
        Normal,
        Queue,
        Timed
    }
}
