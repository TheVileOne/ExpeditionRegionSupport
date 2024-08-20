using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// The name of the combined mod log file in the Logs directory. Only produced with LogManager plugin.
        /// </summary>
        public static readonly string OUTPUT_NAME = "mods";

        /// <summary>
        /// The folder name that will store log files. Do not change this. It is case-sensitive.
        /// </summary>
        public static readonly string LOGS_FOLDER_NAME = "Logs";

        /// <summary>
        /// The default directory where logs are stored. This is managed by the mod.
        /// </summary>
        public static string BaseDirectory;

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

        /// <summary>
        /// Rain World root folder
        /// Application.dataPath is RainWorld_data folder
        /// </summary>
        public static readonly string DefaultLogPath = Path.Combine(Path.GetDirectoryName(Application.dataPath)/*Directory.GetParent(Application.dataPath).FullName*/, LOGS_FOLDER_NAME);

        /// <summary>
        /// StreamingAssets folder
        /// </summary>
        public static readonly string AlternativeLogPath = Path.Combine(Application.streamingAssetsPath, LOGS_FOLDER_NAME);

        public static bool HasInitialized;

        #region Static Methods

        public static void InitializeLogDirectory()
        {
            if (HasInitialized) return;

            BaseDirectory = FindLogsDirectory();
            try
            {
                //The found directory needs to be created if it doesn't yet exist, and the alternative directory removed
                if (!Directory.Exists(BaseDirectory))
                {
                    UtilityCore.BaseLogger.LogInfo("Creating directory: " + BaseDirectory);
                    Directory.CreateDirectory(BaseDirectory);
                }

                string alternativeLogPath = string.Equals(BaseDirectory, DefaultLogPath) ? AlternativeLogPath : DefaultLogPath;

                try
                {
                    if (Directory.Exists(alternativeLogPath))
                    {
                        UtilityCore.BaseLogger.LogInfo("Removing directory: " + alternativeLogPath);
                        Directory.Delete(alternativeLogPath, true);
                    }
                }
                catch (Exception ex)
                {
                    UtilityCore.BaseLogger.LogError("Unable to delete log directory");
                    UtilityCore.BaseLogger.LogError(ex);
                }
            }
            catch (Exception ex)
            {
                UtilityCore.BaseLogger.LogError("Unable to create log directory");
                UtilityCore.BaseLogger.LogError(ex);
            }

            HasInitialized = true;
        }

        public static string FindExistingLogsDirectory()
        {
            if (Directory.Exists(DefaultLogPath))
                return DefaultLogPath;

            if (Directory.Exists(AlternativeLogPath))
                return AlternativeLogPath;

            return null;
        }

        public static string FindLogsDirectory()
        {
            return FindExistingLogsDirectory() ?? DefaultLogPath;
        }

        /// <summary>
        /// Takes a filename and attaches the path stored in BaseDirectory
        /// </summary>
        /// <param name="useLogExt">A flag to convert extension to .log</param>
        public static string ApplyLogPathToFilename(string filename, bool useLogExt = true)
        {
            if (useLogExt)
                filename = FormatLogFile(filename);

            return Path.Combine(BaseDirectory ?? FindLogsDirectory(), filename);
        }

        /// <summary>
        /// Change the extension of any filename to .log
        /// </summary>
        public static string FormatLogFile(string filename)
        {
            return Path.ChangeExtension(filename, ".log");
        }

        /// <summary>
        /// Check that a path matches one of the two supported Logs directories.
        /// </summary>
        public static bool IsValidLogPath(string path)
        {
            if (path == null) return false;

            path = Path.GetFullPath(path).TrimEnd('\\');

            return string.Equals(path, DefaultLogPath, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(path, AlternativeLogPath, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Checks a path against the BaseDirectory
        /// </summary>
        public static bool IsBaseLogPath(string path)
        {
            if (path == null) return false;

            //Strip the filename if one exists
            if (Path.HasExtension(path))
                path = Path.GetDirectoryName(path);
            else
                path = Path.GetFullPath(path).TrimEnd('\\');

            string basePath = Path.GetFullPath(BaseDirectory).TrimEnd('\\');

            UtilityCore.BaseLogger.LogInfo("Comparing " + path + " to  base " + basePath);

            return string.Equals(path, basePath, StringComparison.InvariantCultureIgnoreCase);
        }

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

                UtilityCore.BaseLogger.LogInfo("Log directory changed to " + path);

                OnMoveComplete?.Invoke(path);
                BaseDirectory = path; //This gets updated last. It is needed for comparison purposes.
            }
            else if (signalWord == "MoveAborted")
                OnMoveAborted?.Invoke();
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

        public void Dispose()
        {
        }
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
