using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogUtils
{
    /// <summary>
    /// A class for handling game-controlled log content
    /// </summary>
    public class GameLogger
    {
        public void HandleRequest(LogRequest request)
        {
            LogID logFile = request.Data.ID;

            if (!logFile.IsGameControlled) return;

            LogCategory category = request.Data.Category;
            string message = request.Data.Message;

            if (logFile == LogID.Unity)
                LogUnity(category, message);
            else if (logFile == LogID.BepInEx)
                LogBepEx(request.Data.LogSource, category, message);
            else if (logFile == LogID.Expedition)
                LogExp(category, message);
            else if (logFile == LogID.JollyCoop)
                LogJolly(category, message);
            else if (logFile == LogID.Exception)
                LogUnity(LogType.Error, message);
        }

        public void HandleRequests(IEnumerable<LogRequest> requests)
        {
            foreach (LogRequest request in requests)
                HandleRequest(request);
        }

        public void LogBepEx(object data)
        {
            LogBepEx(null, LogLevel.Info, data);
        }

        public void LogBepEx(ManualLogSource source, LogLevel category, object data)
        {
            var bepLogger = source ?? UtilityCore.BaseLogger;
            bepLogger.Log(category, data);
        }

        public void LogBepEx(ManualLogSource source, LogCategory category, object data)
        {
            var bepLogger = source ?? UtilityCore.BaseLogger;
            bepLogger.Log(category.BepInExCategory, data);
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
            Debug.unityLogger.Log(category.UnityCategory, data);
        }

        public void LogExp(object data)
        {
            LogExp(LogCategory.Default, data);
        }

        public void LogExp(LogCategory category, object data)
        {
            string message = data?.ToString();

            if (UtilityCore.RequestHandler.CurrentRequest == null)
                UtilityCore.RequestHandler.Submit(new LogRequest(new LogEvents.LogMessageEventArgs(LogID.Expedition, message, category)));
            Expedition.ExpLog.Log(message);
        }

        public void LogJolly(object data)
        {
            LogJolly(LogCategory.Default, data);
        }

        public void LogJolly(LogCategory category, object data)
        {
            string message = data?.ToString();

            if (UtilityCore.RequestHandler.CurrentRequest == null)
                UtilityCore.RequestHandler.Submit(new LogRequest(new LogEvents.LogMessageEventArgs(LogID.JollyCoop, message, category)));
            JollyCoop.JollyCustom.Log(data?.ToString());
        }
    }
}
