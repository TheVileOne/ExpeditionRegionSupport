using BepInEx.Logging;
using LogUtils.Enums;
using UnityEngine;
using LoggerDocs = LogUtils.Documentation.LoggerDocumentation;

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

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        public void Log(object data)
        {
            Source.LogInfo(data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        public void LogDebug(object data)
        {
            Source.LogDebug(data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        public void LogInfo(object data)
        {
            Source.LogInfo(data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        public void LogImportant(object data)
        {
            Log(LogCategory.Important, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        public void LogMessage(object data)
        {
            Source.LogMessage(data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        public void LogWarning(object data)
        {
            Source.LogWarning(data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        public void LogError(object data)
        {
            Source.LogError(data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        public void LogFatal(object data)
        {
            Source.LogFatal(data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogLevel category, object data)
        {
            Source.Log(category, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogType category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(string category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogCategory category, object data)
        {
            Source.Log(category.BepInExCategory, data);
        }
    }
}
