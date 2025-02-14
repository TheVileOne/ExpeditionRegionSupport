using BepInEx.Logging;
using LogUtils.Enums;
using UnityEngine;

namespace LogUtils.CompatibilityServices
{
    /// <summary>
    /// A logger that exclusively writes directly through Unity's logging API
    /// </summary>
    public class UnityLogger : ILogger
    {
        public void Log(object data)
        {
            Debug.Log(data);
        }

        public void Log(LogType category, object data)
        {
            Debug.unityLogger.Log(category, data);
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
            Debug.unityLogger.Log(category.UnityCategory, data);
        }

        public void LogDebug(object data)
        {
            Debug.Log(data);
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
            Debug.LogWarning(data);
        }

        public void LogError(object data)
        {
            Debug.LogError(data);
        }

        public void LogFatal(object data)
        {
            Log(LogCategory.Fatal, data);
        }
    }
}
