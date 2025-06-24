using BepInEx.Logging;
using LogUtils.Enums;
using UnityEngine;

namespace LogUtils.Compatibility.BepInEx
{
    public class BepInExLogger : ILogger
    {
        /// <summary>
        /// BepInEx derived logging interface
        /// </summary>
        internal readonly IExtendedLogSource Source;

        public BepInExLogger(ManualLogSource source)
        {
            Source = AdapterServices.Convert(source);
        }

        public BepInExLogger(IExtendedLogSource source)
        {
            Source = source;
        }

        public void Log(object data)
        {
            Source.LogInfo(data);
        }

        public void LogDebug(object data)
        {
            Source.LogDebug(data);
        }

        public void LogInfo(object data)
        {
            Source.LogInfo(data);
        }

        public void LogImportant(object data)
        {
            Log(LogCategory.Important, data);
        }

        public void LogMessage(object data)
        {
            Source.LogMessage(data);
        }

        public void LogWarning(object data)
        {
            Source.LogWarning(data);
        }

        public void LogError(object data)
        {
            Source.LogError(data);
        }

        public void LogFatal(object data)
        {
            Source.LogFatal(data);
        }

        public void Log(LogLevel category, object data)
        {
            Source.Log(category, data);
        }

        public void Log(LogType category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        public void Log(string category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        public void Log(LogCategory category, object data)
        {
            Source.Log(category.BepInExCategory, data);
        }
    }
}
