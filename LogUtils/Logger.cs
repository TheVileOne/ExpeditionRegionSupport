using BepInEx.Logging;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LogUtils
{
    public class Logger : IDisposable
    {
        public ILogWriter Writer = LogWriter.Writer;

        public ManualLogSource ManagedLogSource;

        /// <summary>
        /// Contains a list of LogIDs (both local and remote) that will be handled in the case of an untargeted log request
        /// </summary>
        public List<LogID> LogTargets = new List<LogID>();

        /// <summary>
        /// A flag that allows/disallows handling of log requests (local and remote) through this logger 
        /// </summary>
        public bool AllowLogging;

        /// <summary>
        /// A flag that allows/disallows handling of remote log requests through this logger
        /// </summary>
        public bool AllowRemoteLogging;

        /// <summary>
        /// Contains a record of logger field values that can be restored on demand
        /// </summary>
        public LoggerRestorePoint RestorePoint;

        static Logger()
        {
            //Initialize the utility when this class is accessed
            if (!UtilityCore.IsInitialized)
                UtilityCore.Initialize();
        }

        #region Constructors

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        /// <param name="visibleToRemoteLoggers">Whether logger is able to handle remote log requests</param>
        /// <param name="presets">LogIDs that are handled by this logger for untargeted, and remote log requests</param>
        public Logger(bool allowLogging, bool visibleToRemoteLoggers, params LogID[] presets)
        {
            AllowLogging = allowLogging;
            AllowRemoteLogging = visibleToRemoteLoggers;

            LogTargets.AddRange(presets);
            SetRestorePoint();

            UtilityCore.RequestHandler.Register(this);
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="visibleToRemoteLoggers">Whether logger is able to handle remote log requests</param>
        /// <param name="presets">LogIDs that are handled by this logger for untargeted, and remote log requests</param>
        public Logger(bool visibleToRemoteLoggers, params LogID[] presets) : this(true, visibleToRemoteLoggers, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="presets">LogIDs that are handled by this logger for untargeted, and remote log requests</param>
        public Logger(params LogID[] presets) : this(true, true, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="mode">Changes the technique used to write messages to file</param>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        /// <param name="visibleToRemoteLoggers">Whether logger is able to handle remote log requests</param>
        /// <param name="presets">LogIDs that are handled by this logger for untargeted, and remote log requests</param>
        public Logger(LoggingMode mode, bool allowLogging, bool visibleToRemoteLoggers, params LogID[] presets) : this(allowLogging, visibleToRemoteLoggers, presets)
        {
            SetWriter(mode);
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="mode">Changes the technique used to write messages to file</param>
        /// <param name="visibleToRemoteLoggers">Whether logger is able to handle remote log requests</param>
        /// <param name="presets">LogIDs that are handled by this logger for untargeted, and remote log requests</param>
        public Logger(LoggingMode mode, bool visibleToRemoteLoggers, params LogID[] presets) : this(true, visibleToRemoteLoggers, presets)
        {
            SetWriter(mode);
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="mode">Changes the technique used to write messages to file</param>
        /// <param name="presets">LogIDs that are handled by this logger for untargeted, and remote log requests</param>
        public Logger(LoggingMode mode, params LogID[] presets) : this(true, true, presets)
        {
            SetWriter(mode);
        }

        #endregion
        #region Restore Points

        public void SetRestorePoint()
        {
            RestorePoint = new LoggerRestorePoint(this);
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
                case LoggingMode.Buffer:
                    throw new NotImplementedException();
            }
        }

        public void RestoreState()
        {
            AllowLogging = RestorePoint.AllowLogging;
            AllowRemoteLogging = RestorePoint.AllowRemoteLogging;

            LogTargets.Clear();
            LogTargets.AddRange(RestorePoint.LogTargets);
        }

        #endregion

        /// <summary>
        /// Action is invoked when the Logs directory is going to be moved on the next Update frame. May get called multiple times if move fails.
        /// If your logger uses a FileStream for logging to the Logs folder, please make sure it is closed upon activation of this event.
        /// </summary>
        public static Action OnMovePending;

        /// <summary>
        /// Action is invoked when all move attempts have failed. Signal will return to Signal.None on the following frame.
        /// </summary>
        public static Action OnMoveAborted;

        /// <summary>
        /// Action is invoked immediately after the Logs directory is successfully moved. The new path is given as an argument.
        /// If your logger uses a FileStream for logging to the Logs folder, it is safe to reenable it here.
        /// </summary>
        public static Action<string> OnMoveComplete;

        /// <summary>
        /// Handles an event based on a provided signal word
        /// </summary>
        internal static void ProcessLogSignal(string signal)
        {
            if (signal == "Signal.None") return;

            string[] signalData = Regex.Split(signal, "\\.");
            string signalWord = signalData[1]; //Signal.Keyword.Other data

            //Remote loggers need to be informed of when the Logs folder is moved.
            //The folder cannot be moved if any log file has an open filestream active
            if (signalWord == "MovePending")
                OnMovePending?.Invoke();
            else if (signalWord == "MoveComplete")
            {
                string path = signalData[2];

                UtilityLogger.Log("Log directory changed to " + path);

                OnMoveComplete?.Invoke(path);
                LogsFolder.Path = path; //This gets updated last. It is needed for comparison purposes.
            }
            else if (signalWord == "MoveAborted")
                OnMoveAborted?.Invoke();
        }

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
            if (!targets.Any())
            {
                UtilityLogger.LogWarning("Attempted to log message with no available log targets");
                return;
            }

            //Log data for each targetted LogID
            foreach (LogID target in targets)
                LogData(target, category, data, shouldFilter);
        }

        protected virtual void LogData(LogID target, LogCategory category, object data, bool shouldFilter)
        {
            if (!AllowLogging || !target.IsEnabled) return;

            RequestType requestType;

            if (target.IsGameControlled)
            {
                requestType = RequestType.Game;
            }
            else if (target.Access == LogAccess.FullAccess || target.Access == LogAccess.Private)
            {
                requestType = RequestType.Local;
            }
            else
            {
                requestType = RequestType.Remote;
            }

            LogRequest request = new LogRequest(requestType, new LogEvents.LogMessageEventArgs(target, data, category)
            {
                LogSource = ManagedLogSource
            });

            if (shouldFilter)
            {
                request.Data.ShouldFilter = true;
                request.Data.FilterDuration = FilterDuration.OnClose;
            }

            lock (UtilityCore.RequestHandler.RequestProcessLock)
            {
                //Local requests are processed immediately by the logger, while other types of requests are handled through RequestHandler
                if (request.Type != RequestType.Local)
                {
                    UtilityCore.RequestHandler.Submit(request, true);
                }
                else
                {
                    UtilityCore.RequestHandler.Submit(request, false);
                    Writer.WriteFrom(request);
                }
            }
        }

        /// <summary>
        /// Returns whether logger instance is able to handle a specified LogID
        /// </summary>
        public bool CanAccess(LogID logID, RequestType requestType, bool doPathCheck)
        {
            if (logID.IsGameControlled) return false; //This check is here to prevent TryHandleRequest from potentially handling requests that should be handled by a GameLogger

            //Find the LogID equivalent accepted by the Logger instance - only one LogID with this value can be stored
            LogID loggerID = LogTargets.Find(log => log.Properties.IDMatch(logID));

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

        public void HandleRequests(IEnumerable<LogRequest> requests, bool skipValidation = false)
        {
            LogID loggerID = null;
            foreach (LogRequest request in requests.Where(req => skipValidation || CanAccess(req.Data.ID, req.Type, doPathCheck: true)))
                TryHandleRequest(request, ref loggerID);
        }

        public RejectionReason HandleRequest(LogRequest request, bool skipValidation = false)
        {
            if (!skipValidation && !CanAccess(request.Data.ID, request.Type, doPathCheck: true))
                return request.UnhandledReason;

            LogID loggerID = null;
            return TryHandleRequest(request, ref loggerID);
        }

        internal RejectionReason TryHandleRequest(LogRequest request, ref LogID loggerID)
        {
            LogID requestID = request.Data.ID;
            if (loggerID == null || (loggerID != requestID)) //ExtEnums are not compared by reference
            {
                //The local LogID stored in LogTargets will be a different instance to the one stored in a remote log request
                //It is important to check the local id instead of the remote id in certain situations
                loggerID = LogTargets.Find(id => id == requestID);
            }

            if (loggerID.Properties.CurrentFolderPath != requestID.Properties.CurrentFolderPath) //Same LogID, different log paths - do not handle
            {
                UtilityLogger.Log("Request not handled, log paths do not match");

                //This particular rejection reason has problematic support, and is not guaranteed to be recorded by the request
                request.Reject(RejectionReason.PathMismatch);
                return RejectionReason.PathMismatch;
            }

            request.ResetStatus(); //Ensure that processing request is handled in a consistent way

            if (!AllowLogging || !loggerID.IsEnabled)
            {
                request.Reject(RejectionReason.LogDisabled);
                return request.UnhandledReason;
            }

            if (loggerID.Properties.ShowLogsAware && !RainWorld.ShowLogs)
            {
                if (RWInfo.LatestSetupPeriodReached < RWInfo.SHOW_LOGS_ACTIVE_PERIOD)
                    request.Reject(RejectionReason.ShowLogsNotInitialized);
                else
                    request.Reject(RejectionReason.LogDisabled);
                return request.UnhandledReason;
            }

            if (request.Type == RequestType.Remote && (loggerID.Access == LogAccess.Private || !AllowRemoteLogging))
            {
                request.Reject(RejectionReason.AccessDenied);
                return request.UnhandledReason;
            }

            Writer.WriteFrom(request);

            if (request.Status == RequestStatus.Complete)
                return RejectionReason.None;

            return request.UnhandledReason;
        }

        public void Dispose()
        {
        }
    }

    public enum LoggingMode
    {
        Normal,
        Queue,
        Buffer
    }

    public static class ExtendedILogListener
    {
        /// <summary>
        /// Fetches signal data produced by a custom ILogListener
        /// </summary>
        public static string GetSignal(this ILogListener self)
        {
            string stringToProcess = self.ToString();

            return stringToProcess.StartsWith("Signal") ? stringToProcess : null;
        }
    }
}
