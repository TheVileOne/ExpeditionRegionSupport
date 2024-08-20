﻿using BepInEx.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LogUtils
{
    public class BetaLogger
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

        #region Constructors

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        /// <param name="visibleToRemoteLoggers">Whether logger is able to handle remote log requests</param>
        /// <param name="presets">LogIDs that are handled by this logger for untargeted, and remote log requests</param>
        public BetaLogger(bool allowLogging, bool visibleToRemoteLoggers, params LogID[] presets)
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
        public BetaLogger(bool visibleToRemoteLoggers, params LogID[] presets) : this(true, visibleToRemoteLoggers, presets)
        {
        }

        /// <summary>
        /// Constructs a logger instance
        /// </summary>
        /// <param name="presets">LogIDs that are handled by this logger for untargeted, and remote log requests</param>
        public BetaLogger(params LogID[] presets) : this(true, true, presets)
        {
        }

        #endregion
        #region Log Overloads (object)

        public void Log(object data)
        {
            Log(LogCategory.Default, data);
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

        public void LogBepEx(object data)
        {
            LogBepEx(LogLevel.Info, data);
        }

        public void LogBepEx(LogLevel category, object data)
        {
            if (!AllowLogging || !LogID.BepInEx.IsEnabled) return;

            UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.BepInEx, data, category)
            {
                LogSource = ManagedLogSource
            }), true);
        }

        public void LogBepEx(LogCategory category, object data)
        {
            if (!AllowLogging || !LogID.BepInEx.IsEnabled) return;

            UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.BepInEx, data, category)
            {
                LogSource = ManagedLogSource
            }), true);
        }

        public void LogUnity(object data)
        {
            LogUnity(LogCategory.Default, data);
        }

        public void LogUnity(LogType category, object data)
        {
            if (!AllowLogging) return;

            LogID logFile = !LogCategory.IsUnityErrorCategory(category) ? LogID.Unity : LogID.Exception;

            if (logFile.IsEnabled)
                UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(logFile, data, category)), true);
        }

        public void LogUnity(LogCategory category, object data)
        {
            if (!AllowLogging) return;

            LogID logFile = !LogCategory.IsUnityErrorCategory(category.UnityCategory) ? LogID.Unity : LogID.Exception;

            if (logFile.IsEnabled)
                UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(logFile, data, category)), true);
        }

        public void LogExp(object data)
        {
            LogExp(LogCategory.Default, data);
        }

        public void LogExp(LogCategory category, object data)
        {
            if (!AllowLogging || !LogID.Expedition.IsEnabled) return;

            UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.Expedition, data, category)), true);
        }

        public void LogJolly(object data)
        {
            LogJolly(LogCategory.Default, data);
        }

        public void LogJolly(LogCategory category, object data)
        {
            if (!AllowLogging || !LogID.JollyCoop.IsEnabled) return;

            UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Game, new LogEvents.LogMessageEventArgs(LogID.JollyCoop, data, category)), true);
        }

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
            LogData(LogTargets, category, data);
        }

        #endregion
        #region  Log Overloads (LogID, object)

        public void Log(LogID target, object data)
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
            LogData(target, category, data);
        }

        #endregion
        #region  Log Overloads (IEnumerable<LogID>, object)

        public void Log(IEnumerable<LogID> targets, object data)
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
            LogData(targets, category, data);
        }

        #endregion

        protected virtual void LogData(IEnumerable<LogID> targets, LogCategory category, object data)
        {
            if (!targets.Any())
            {
                UtilityCore.BaseLogger.LogWarning("Attempted to log message with no available log targets");
                return;
            }

            //Log data for each targetted LogID
            foreach (LogID target in targets)
                LogData(target, category, data);
        }

        protected virtual void LogData(LogID target, LogCategory category, object data)
        {
            if (!AllowLogging || !target.IsEnabled) return;

            if (target.Access != LogAccess.RemoteAccessOnly)
            {
                if (target.IsGameControlled) //Game controlled LogIDs are always full access
                {
                    if (target == LogID.BepInEx)
                    {
                        LogBepEx(category, data);
                    }
                    else if (target == LogID.Unity)
                    {
                        LogUnity(category, data);
                    }
                    else if (target == LogID.Expedition)
                    {
                        LogExp(category, data);
                    }
                    else if (target == LogID.JollyCoop)
                    {
                        LogJolly(category, data);
                    }
                    else if (target == LogID.Exception)
                    {
                        LogUnity(LogType.Error, data);
                    }
                }
                else if (target.Access == LogAccess.FullAccess || target.Access == LogAccess.Private)
                {
                    UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Local, new LogEvents.LogMessageEventArgs(target, data, category)), false);
                    Writer.WriteToFile();
                }
            }
            else
            {
                UtilityCore.RequestHandler.Submit(new LogRequest(RequestType.Remote, new LogEvents.LogMessageEventArgs(target, data, category)), true);
            }
        }

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
                UtilityCore.BaseLogger.LogInfo("Request not handled, log paths do not match");

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

            Writer.WriteFromRequest(request);

            if (request.Status == RequestStatus.Complete)
                return RejectionReason.None;

            return request.UnhandledReason;
        }
    }
}
