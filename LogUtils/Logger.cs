using BepInEx.Logging;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public bool AllowRemoteLogging { get; set; }

        LogID[] ILogFileHandler.AvailableTargets => LogTargets.ToArray();

        /// <summary>
        /// Contains a list of LogIDs (both local and remote) that will be handled in the case of an untargeted log request
        /// </summary>
        public List<LogID> LogTargets = new List<LogID>();

        /// <summary>
        /// Contains a list of ConsoleIDs that will be handled by any log request that can be handled by this logger
        /// </summary>
        public List<ConsoleID> ConsoleTargets = new List<ConsoleID>();

        /// <summary>
        /// BepInEx logging source object
        /// </summary>
        public ManualLogSource ManagedLogSource;

        /// <summary>
        /// Contains a record of logger field values that can be restored on demand
        /// </summary>
        public LoggerRestorePoint RestorePoint;

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
        /// <param name="preset">The LogID to target, or handle by request by this logger</param>
        public Logger(LogID preset) : this(LoggingMode.Inherit, true, preset)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        /// <param name="preset">The LogID to target, or handle by request by this logger</param>
        public Logger(bool allowLogging, LogID preset) : this(LoggingMode.Inherit, allowLogging, preset)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="mode">Changes the technique used to write messages to file</param>
        /// <param name="preset">The LogID to target, or handle by request by this logger</param>
        public Logger(LoggingMode mode, LogID preset) : this(mode, true, preset)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="presets">Include any LogIDs that this logger targets, or handles on request</param>
        public Logger(params LogID[] presets) : this(LoggingMode.Inherit, true, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        /// <param name="presets">Include any LogIDs that this logger targets, or handles on request</param>
        public Logger(bool allowLogging, params LogID[] presets) : this(LoggingMode.Inherit, allowLogging, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="mode">Changes the technique used to write messages to file</param>
        /// <param name="presets">Include any LogIDs that this logger targets, or handles on request</param>
        public Logger(LoggingMode mode, params LogID[] presets) : this(mode, true, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="mode">Changes the technique used to write messages to file</param>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        /// <param name="presets">Include any LogIDs that this logger targets, or handles on request</param>
        public Logger(LoggingMode mode, bool allowLogging, params LogID[] presets)
        {
            if (UtilitySetup.CurrentStep < UtilitySetup.InitializationStep.INITIALIZE_ENUMS)
                throw new EarlyInitializationException("Logger created too early");

            AllowLogging = allowLogging;

            LogTargets.AddRange(presets);

            if (mode != LoggingMode.Inherit)
                SetWriter(mode);

            UtilityEvents.OnRegister += OnRegister;
            UtilityEvents.OnUnregister += OnUnregister;
            FinalizeConstruction();
        }

        protected virtual void FinalizeConstruction()
        {
            SetRestorePoint();
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
            LogData(LogTargets, category, data, false);
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
            LogData(LogTargets, category, data, true);
        }

        #endregion
        #region  Log Overloads (LogID, object)

        public void Log(LogID target, object data)
        {
            Log(target, LogCategory.Default, data);
        }

        public void LogOnce(LogID target, object data)
        {
            Log(target, LogCategory.Default, data);
        }

        public void LogDebug(LogID target, object data)
        {
            Log(target, LogCategory.Debug, data);
        }

        public void LogInfo(LogID target, object data)
        {
            Log(target, LogCategory.Info, data);
        }

        public void LogImportant(LogID target, object data)
        {
            Log(target, LogCategory.Important, data);
        }

        public void LogMessage(LogID target, object data)
        {
            Log(target, LogCategory.Message, data);
        }

        public void LogWarning(LogID target, object data)
        {
            Log(target, LogCategory.Warning, data);
        }

        public void LogError(LogID target, object data)
        {
            Log(target, LogCategory.Error, data);
        }

        public void LogFatal(LogID target, object data)
        {
            Log(target, LogCategory.Fatal, data);
        }

        public void Log(LogID target, LogLevel category, object data)
        {
            Log(target, LogCategory.ToCategory(category), data);
        }

        public void Log(LogID target, string category, object data)
        {
            Log(target, LogCategory.ToCategory(category), data);
        }

        public void Log(LogID target, LogCategory category, object data)
        {
            LogData(target, category, data, false);
        }

        public void LogOnce(LogID target, LogCategory category, object data)
        {
            LogData(target, category, data, true);
        }

        public void Log(LogID target1, LogID target2, object data)
        {
            Log(new LogID[] { target1, target2 }, LogCategory.Default, data);
        }

        public void LogOnce(LogID target1, LogID target2, object data)
        {
            Log(new LogID[] { target1, target2 }, LogCategory.Default, data);
        }

        public void LogDebug(LogID target1, LogID target2, object data)
        {
            Log(new LogID[] { target1, target2 }, LogCategory.Debug, data);
        }

        public void LogInfo(LogID target1, LogID target2, object data)
        {
            Log(new LogID[] { target1, target2 }, LogCategory.Info, data);
        }

        public void LogImportant(LogID target1, LogID target2, object data)
        {
            Log(new LogID[] { target1, target2 }, LogCategory.Important, data);
        }

        public void LogMessage(LogID target1, LogID target2, object data)
        {
            Log(new LogID[] { target1, target2 }, LogCategory.Message, data);
        }

        public void LogWarning(LogID target1, LogID target2, object data)
        {
            Log(new LogID[] { target1, target2 }, LogCategory.Warning, data);
        }

        public void LogError(LogID target1, LogID target2, object data)
        {
            Log(new LogID[] { target1, target2 }, LogCategory.Error, data);
        }

        public void LogFatal(LogID target1, LogID target2, object data)
        {
            Log(new LogID[] { target1, target2 }, LogCategory.Fatal, data);
        }

        public void Log(LogID target1, LogID target2, LogLevel category, object data)
        {
            Log(new LogID[] { target1, target2 }, LogCategory.ToCategory(category), data);
        }

        public void Log(LogID target1, LogID target2, string category, object data)
        {
            Log(new LogID[] { target1, target2 }, LogCategory.ToCategory(category), data);
        }

        public void Log(LogID target1, LogID target2, LogCategory category, object data)
        {
            LogData(new LogID[] { target1, target2 }, category, data, false);
        }

        public void LogOnce(LogID target1, LogID target2, LogCategory category, object data)
        {
            LogData(new LogID[] { target1, target2 }, category, data, true);
        }

        public void Log(LogID target1, LogID target2, LogID target3, object data)
        {
            Log(new LogID[] { target1, target2, target3 }, LogCategory.Default, data);
        }

        public void LogOnce(LogID target1, LogID target2, LogID target3, object data)
        {
            Log(new LogID[] { target1, target2, target3 }, LogCategory.Default, data);
        }

        public void LogDebug(LogID target1, LogID target2, LogID target3, object data)
        {
            Log(new LogID[] { target1, target2, target3 }, LogCategory.Debug, data);
        }

        public void LogInfo(LogID target1, LogID target2, LogID target3, object data)
        {
            Log(new LogID[] { target1, target2, target3 }, LogCategory.Info, data);
        }

        public void LogImportant(LogID target1, LogID target2, LogID target3, object data)
        {
            Log(new LogID[] { target1, target2, target3 }, LogCategory.Important, data);
        }

        public void LogMessage(LogID target1, LogID target2, LogID target3, object data)
        {
            Log(new LogID[] { target1, target2, target3 }, LogCategory.Message, data);
        }

        public void LogWarning(LogID target1, LogID target2, LogID target3, object data)
        {
            Log(new LogID[] { target1, target2, target3 }, LogCategory.Warning, data);
        }

        public void LogError(LogID target1, LogID target2, LogID target3, object data)
        {
            Log(new LogID[] { target1, target2, target3 }, LogCategory.Error, data);
        }

        public void LogFatal(LogID target1, LogID target2, LogID target3, object data)
        {
            Log(new LogID[] { target1, target2, target3 }, LogCategory.Fatal, data);
        }

        public void Log(LogID target1, LogID target2, LogID target3, LogLevel category, object data)
        {
            Log(new LogID[] { target1, target2, target3 }, LogCategory.ToCategory(category), data);
        }

        public void Log(LogID target1, LogID target2, LogID target3, string category, object data)
        {
            Log(new LogID[] { target1, target2, target3 }, LogCategory.ToCategory(category), data);
        }

        public void Log(LogID target1, LogID target2, LogID target3, LogCategory category, object data)
        {
            LogData(new LogID[] { target1, target2, target3 }, category, data, false);
        }

        public void LogOnce(LogID target1, LogID target2, LogID target3, LogCategory category, object data)
        {
            LogData(new LogID[] { target1, target2, target3 }, category, data, true);
        }

        #endregion
        #region  Log Overloads (IEnumerable<LogID>, object)

        public void Log(IEnumerable<LogID> targets, object data)
        {
            Log(targets, LogCategory.Default, data);
        }

        public void LogOnce(IEnumerable<LogID> targets, object data)
        {
            Log(targets, LogCategory.Default, data);
        }

        public void LogDebug(IEnumerable<LogID> targets, object data)
        {
            Log(targets, LogCategory.Debug, data);
        }

        public void LogInfo(IEnumerable<LogID> targets, object data)
        {
            Log(targets, LogCategory.Info, data);
        }

        public void LogImportant(IEnumerable<LogID> targets, object data)
        {
            Log(targets, LogCategory.Important, data);
        }

        public void LogMessage(IEnumerable<LogID> targets, object data)
        {
            Log(targets, LogCategory.Message, data);
        }

        public void LogWarning(IEnumerable<LogID> targets, object data)
        {
            Log(targets, LogCategory.Warning, data);
        }

        public void LogError(IEnumerable<LogID> targets, object data)
        {
            Log(targets, LogCategory.Error, data);
        }

        public void LogFatal(IEnumerable<LogID> targets, object data)
        {
            Log(targets, LogCategory.Fatal, data);
        }

        public void Log(IEnumerable<LogID> targets, LogLevel category, object data)
        {
            Log(targets, LogCategory.ToCategory(category), data);
        }

        public void Log(IEnumerable<LogID> targets, string category, object data)
        {
            Log(targets, LogCategory.ToCategory(category), data);
        }

        public void Log(IEnumerable<LogID> targets, LogCategory category, object data)
        {
            LogData(targets, category, data, false);
        }

        public void LogOnce(IEnumerable<LogID> targets, LogCategory category, object data)
        {
            LogData(targets, category, data, true);
        }

        #endregion

        protected virtual void LogData(IEnumerable<LogID> targets, LogCategory category, object data, bool shouldFilter)
        {
            try
            {
                if (!targets.Any())
                {
                    UtilityLogger.LogWarning("Attempted to log message with no available log targets");
                    return;
                }

                //Log data for each targetted LogID
                foreach (LogID target in targets)
                    LogData(target, category, data, shouldFilter, LoggingContext.Batching);
            }
            finally
            {
                ClearEventData();
            }
        }

        protected virtual void LogData(LogID target, LogCategory category, object data, bool shouldFilter, LoggingContext context = LoggingContext.SingleRequest)
        {
            try
            {
                if (!AllowLogging || !target.IsEnabled) return;

                RequestType requestType;

                if (target.IsGameControlled)
                {
                    requestType = RequestType.Game;
                }
                else
                {
                    LogID loggerID = FindEquivalentTarget(target);

                    if (loggerID != null)
                    {
                        requestType = RequestType.Local;
                    }
                    else
                    {
                        requestType = RequestType.Remote;
                    }
                }

                LogRequest request = new LogRequest(requestType, new LogMessageEventArgs(target, data, category)
                {
                    LogSource = ManagedLogSource
                });

                if (shouldFilter)
                {
                    request.Data.ShouldFilter = true;
                    request.Data.FilterDuration = FilterDuration.OnClose;
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
                        SendToWriter(request);
                    }
                }
            }
            finally
            {
                if (context == LoggingContext.SingleRequest)
                    ClearEventData();
            }
        }

        bool ILogHandler.CanHandle(LogRequest request, bool doPathCheck)
        {
            return CanHandle(request.Data.ID, request.Type, doPathCheck);
        }

        /// <summary>
        /// Returns whether logger instance is able to handle a specified LogID
        /// </summary>
        bool ILogHandler.CanHandle(LogID logFile, RequestType requestType, bool doPathCheck)
        {
            return CanHandle(logFile, requestType, doPathCheck);
        }

        internal bool CanHandle(LogID logID, RequestType requestType, bool doPathCheck)
        {
            if (logID.IsGameControlled) return false; //This check is here to prevent TryHandleRequest from potentially handling requests that should be handled by a GameLogger

            LogID loggerID = FindEquivalentTarget(logID);

            //Enabled status is currently not evaluated here - It is unclear whether it should be part of the access check
            if (loggerID != null)
            {
                if (loggerID.Access == LogAccess.RemoteAccessOnly) //Logger can only send remote requests for this LogID
                    return false;

                if (doPathCheck && loggerID.Properties.CurrentFolderPath != logID.Properties.CurrentFolderPath) //It is possible for a LogID to associate with more than one path
                    return false;

                return requestType == RequestType.Local || loggerID.Access != LogAccess.Private;
            }
            return false;
        }

        protected LogID FindEquivalentTarget(LogID logID)
        {
            return LogTargets.Find(log => log.Equals(logID));
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

        public void HandleRequest(LogRequest request, bool skipAccessValidation = false)
        {
            if (request.Submitted)
                UtilityCore.RequestHandler.CurrentRequest = request;

            request.ResetStatus(); //Ensure that processing request is handled in a consistent way

            LogID requestID = request.Data.ID;

            //A log request must have a compatible LogID that is consistent with the access fields of a LogID targeted by the logger.
            //Generally skipping this check means access has already been verified. 
            bool requestCanBeHandled = skipAccessValidation || CanHandle(requestID, request.Type, doPathCheck: true);

            //Check that there is a target with the same filename and path as the request ID
            LogID loggerID = null;
            if (requestCanBeHandled)
            {
                loggerID = LogTargets.Find(log => log.BaseEquals(requestID));
                requestCanBeHandled = loggerID?.Equals(requestID) == true;
            }

            if (!requestCanBeHandled)
            {
                if (loggerID != null)
                {
                    UtilityLogger.Log("Request not handled, log paths do not match");
                    request.Reject(RejectionReason.PathMismatch);
                }
                else
                {
                    //TODO: CanHandle should be replaced with a function that returns an AccessViolation enum that tells us which specific reason to reject the request
                    UtilityLogger.LogWarning("Request sent to a logger that cannot handle it");
                    request.Reject(RejectionReason.NotAllowedToHandle);
                }
                return;
            }

            if (!AllowLogging || !loggerID.IsEnabled)
            {
                request.Reject(RejectionReason.LogDisabled);
                return;
            }

            if (loggerID.Properties.ShowLogsAware && !RainWorld.ShowLogs)
            {
                if (RWInfo.LatestSetupPeriodReached < RWInfo.SHOW_LOGS_ACTIVE_PERIOD)
                    request.Reject(RejectionReason.ShowLogsNotInitialized);
                else
                    request.Reject(RejectionReason.LogDisabled);
                return;
            }

            if (request.Type == RequestType.Remote && (loggerID.Access == LogAccess.Private || !AllowRemoteLogging))
            {
                request.Reject(RejectionReason.AccessDenied);
                return;
            }
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

        /// <summary>
        /// Contains event data pertaining to Unity context objects (if applicable)
        /// </summary>
        private ThreadLocal<EventArgs> unityDataCache;

        /// <summary>
        /// Contains event data exclusive to logging to a console (only once per request batch)
        /// </summary>
        private ThreadLocal<EventArgs> consoleDataCache;

        protected virtual void ClearEventData()
        {
            if (consoleDataCache?.IsValueCreated == true)
                consoleDataCache.Value = null;

            if (unityDataCache?.IsValueCreated == true)
                unityDataCache.Value = null;
        }

        protected virtual void OnRegister(ILogHandler handler)
        {
            if (handler != this) return;

            LogRequestEvents.OnSubmit += OnNewRequest;
        }

        protected virtual void OnUnregister(ILogHandler handler)
        {
            if (handler != this) return;

            LogRequestEvents.OnSubmit -= OnNewRequest;
        }

        protected virtual void OnNewRequest(LogRequest request)
        {
            if (request.Sender != this) return;

            //Console data
            if (request.Type == RequestType.Local && ConsoleTargets.Count > 0)
            {
                if (consoleDataCache == null)
                    consoleDataCache = new ThreadLocal<EventArgs>();

                var data = consoleDataCache.Value;

                if (data == null)
                {
                    data = new ConsoleRequestEventArgs(ConsoleTargets);
                    consoleDataCache.Value = data;
                }
                request.Data.ExtraArgs.Add(data);
            }

            //Unity exclusive data
            if (unityDataCache != null)
            {
                var data = unityDataCache.Value;

                if (data != null)
                    request.Data.ExtraArgs.Add(data);
            }
        }
        #endregion

        #region Dispose pattern

        protected bool IsDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            LogWriter.TryDispose(Writer);
            UtilityCore.RequestHandler.Unregister(this);

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
