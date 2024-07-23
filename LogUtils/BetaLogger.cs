using BepInEx.Logging;
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

        private bool _allowRemoteLogging;

        /// <summary>
        /// A flag that allows/disallows handling of remote log requests through this logger
        /// </summary>
        public bool AllowRemoteLogging
        {
            get => _allowRemoteLogging;
            set
            {
                if (AllowRemoteLogging != value)
                {
                    List<BetaLogger> availableForRemoteLogging = UtilityCore.RequestHandler.AvailableLoggers;

                    if (value)
                        availableForRemoteLogging.Add(this);
                    else
                        availableForRemoteLogging.Remove(this);
                    _allowRemoteLogging = value;
                }
            }
        }

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

            UtilityCore.RequestHandler.AvailableLoggers.Add(this);
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

        /// <summary>
        /// Constructs a logger instance for a temporary log file
        /// </summary>
        /// <param name="logPath"> The full path to the log file (including filename)</param>
        /// <param name="allowLogging">Whether logger accepts logs by default, or has to be enabled first</param>
        public BetaLogger(string logPath, bool allowLogging = true)
        {
            AllowLogging = allowLogging;
            AllowRemoteLogging = true;

            string logName = Path.GetFileNameWithoutExtension(logPath);
            logPath = Path.GetDirectoryName(logPath);

            LogTargets.Add(new LogID(logName, logPath, false)); //Unregistered to avoid properties being saved for this temporary log file
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

            var bepLogger = ManagedLogSource ?? UtilityCore.BaseLogger;
            bepLogger.Log(category, data);
        }

        public void LogBepEx(LogCategory category, object data)
        {
            if (!AllowLogging || !LogID.BepInEx.IsEnabled) return;

            var bepLogger = ManagedLogSource ?? UtilityCore.BaseLogger;
            bepLogger.Log(category.BepInExCategory, data);
        }

        public void LogUnity(object data)
        {
            LogUnity(LogCategory.Default, data);
        }

        public void LogUnity(LogType category, object data)
        {
            if (!AllowLogging
            || (!LogID.Unity.IsEnabled && category != LogType.Error && category != LogType.Exception) //Non-error logging
            || (!LogID.Exception.IsEnabled && (category == LogType.Error || category == LogType.Exception))) //Error logging
                return;

            Debug.unityLogger.Log(category, data);
        }

        public void LogUnity(LogCategory category, object data)
        {
            if (!AllowLogging
            || (!LogID.Unity.IsEnabled && category.UnityCategory != LogType.Error && category.UnityCategory != LogType.Exception) //Non-error logging
            || (!LogID.Exception.IsEnabled && (category.UnityCategory == LogType.Error || category.UnityCategory == LogType.Exception))) //Error logging
                return;

            //TODO: Put this in a proper place
            Debug.unityLogger.filterLogType = (LogType)1000; //Allow space for custom LogTypes to be defined

            /*
            LogType unityLogCategory;

            //Convert translatable LogTypes first
            if (category == LogCategory.Info || category == LogCategory.Message)
                unityLogCategory = LogType.Log;
            else if (category == LogCategory.Warning)
                unityLogCategory = LogType.Warning;
            else if (category == LogCategory.Error || category == LogCategory.Fatal)
                unityLogCategory = LogType.Error;
            else if (category.index >= 0)
                unityLogCategory = (LogType)(150 + category.index); //Define a custom LogType when no other LogType will fit
            else
            {
                UtilityCore.BaseLogger.LogInfo("Unity logger requires custom log categories to be registered");
                unityLogCategory = LogType.Log;
            }
            */
            Debug.unityLogger.Log(category.UnityCategory, data);
        }

        public void LogExp(object data)
        {
            LogExp(LogCategory.Default, data);
        }

        public void LogExp(LogCategory category, object data)
        {
            if (!AllowLogging || !LogID.Expedition.IsEnabled) return;

            Expedition.ExpLog.Log(data?.ToString());
        }

        public void LogJolly(object data)
        {
            LogJolly(LogCategory.Default, data);
        }

        public void LogJolly(LogCategory category, object data)
        {
            if (!AllowLogging || !LogID.JollyCoop.IsEnabled) return;

            JollyCoop.JollyCustom.Log(data?.ToString());
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
            if (!AllowLogging || !target.IsEnabled || (target.Properties.ShowLogsAware && !RainWorld.ShowLogs)) return;

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
                    Writer.WriteToFile(target, data?.ToString());
                }
            }
            else
            {
                //TODO: Remote logging code here
            }
        }
    }
}
