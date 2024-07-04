using BepInEx.Logging;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogUtils
{
    public class BetaLogger : ILogger
    {
        /// <summary>
        /// A dictionary of loggers available to remote mod requests
        /// Stores mod_id as the key
        /// </summary>
        public static Dictionary<string, BetaLogger> VisibleLoggers = new Dictionary<string, BetaLogger>();

        /// <summary>
        /// A list of LogIDs that a Logger instance has access to by default
        /// </summary>
        public static List<LogID> LocalIDs = new List<LogID>()
        {
            LogID.BepInEx,
            LogID.Unity,
            LogID.Exception,
            LogID.Expedition,
            LogID.JollyCoop
        };

        private List<LogID> managedIDs = new List<LogID>();

        /// <summary>
        /// Contains local LogIDs that this logger instance has permission to handle arbitrary log requests for
        /// </summary>
        public List<LogID> ManagedIDs
        {
            get
            {
                if (managedIDs.Count == 0) //When no managed ids are specified, use the local LodID pool
                    return LocalIDs;
                return managedIDs;
            }
        }

        public ManualLogSource ManagedLogSource;

        /// <summary>
        /// Contains a list of LogIDs (both local and remote) that will be handled in the case of an untargeted log request
        /// </summary>
        public List<LogID> LogTargets = new List<LogID>();

        public BetaLogger(string modID, bool visibleToRemoteLoggers = true)
        {
            if (visibleToRemoteLoggers)
                VisibleLoggers[modID] = this;
        }

        public BetaLogger(LogID logID)
        {
            LogTargets.Add(logID);

            bool isLocal = LocalIDs.Contains(logID);
            if (!isLocal)
            {
                //Check that a foreign LogID doesn't exist
                if (!IsForeign(logID))
                {
                    isLocal = true;
                    LocalIDs.Add(logID);
                }
            }
        }

        public BetaLogger(string logName, string log)
        {

        }

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
            var bepLogger = ManagedLogSource ?? UtilityCore.BaseLogger;
            bepLogger.Log(category, data);
        }

        public void LogBepEx(LogCategory category, object data)
        {
            var bepLogger = ManagedLogSource ?? UtilityCore.BaseLogger;
            bepLogger.Log(category, data);
        }

        public void LogUnity(object data)
        {
            LogUnity(LogCategory.Default, data);
        }

        public void LogUnity(LogType category, object data)
        {
            Debug.unityLogger.Log(category, data);
        }

        public void LogUnity(LogCategory category, object data)
        {
            //TODO: Put this in a proper place
            Debug.unityLogger.filterLogType = (LogType)1000; //Allow space for custom LogTypes to be defined

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

            Debug.unityLogger.Log(unityLogCategory, data);
        }

        public void LogExp(object data)
        {
            LogExp(LogCategory.Default, data);
        }

        public void LogExp(LogCategory category, object data)
        {
            Expedition.ExpLog.Log(data?.ToString());
        }

        public void LogJolly(object data)
        {
            LogJolly(LogCategory.Default, data);
        }

        public void LogJolly(LogCategory category, object data)
        {
            //TODO: Headers????
            Debug.Log(data);
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
            //TODO: Implement
        }
    }
}
