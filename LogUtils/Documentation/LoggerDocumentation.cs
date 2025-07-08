using BepInEx.Logging;
using LogUtils.Enums;
using UnityEngine;

namespace LogUtils.Documentation
{
    internal static class LoggerDocumentation
    {
        internal interface Standard
        {
            /// <summary>
            /// Formats and writes a log message
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void Log(object data);

            /// <summary>
            /// Formats and writes a log message with a debug context
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogDebug(object data);

            /// <summary>
            /// Formats and writes a log message with an informational context (typically the default context)
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogInfo(object data);

            /// <summary>
            /// Formats and writes a log message with an important context
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogImportant(object data);

            /// <summary>
            /// Formats and writes a log message with a message context
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogMessage(object data);

            /// <summary>
            /// Formats and writes a log message with a warning context
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogWarning(object data);

            /// <summary>
            /// Formats and writes a log message with an error context
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogError(object data);

            /// <summary>
            /// Formats and writes a log message with a fatal context
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogFatal(object data);

            /// <summary>
            /// Formats and writes a log message with a specified UnityEngine logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            //void Log(LogType category, object data);

            /// <summary>
            /// Formats and writes a log message with a specified BepInEx logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            //void Log(LogLevel category, object data);

            /// <summary>
            /// Formats and writes a log message with a specified logging context as a string
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            //void Log(string category, object data);

            /// <summary>
            /// Formats and writes a log message with a specified logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void Log(LogCategory category, object data);
        }

        internal interface Game
        {
            /// <summary>
            /// Formats and writes a log message to BepInEx
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogBepEx(object data);

            /// <summary>
            /// Formats and writes a log message to BepInEx with a specified logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogBepEx(LogLevel category, object data);

            /// <summary>
            /// Formats and writes a log message to BepInEx with a specified logging context, and logging source
            /// </summary>
            /// <param name="source">The source of the logged message (usually a ManualLogSource in a modding context)</param>
            /// <param name="category">The specified logging context</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogBepEx(ILogSource source, LogLevel category, object data);

            /// <summary>
            /// Formats and writes a log message to BepInEx
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogUnity(object data);

            /// <summary>
            /// Formats and writes a log message to Unity with a specified logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogUnity(LogType category, object data);

            /// <summary>
            /// Formats and writes a log message to Expedition
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogExp(object data);

            /// <summary>
            /// Formats and writes a log message to Expedition with a specified logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogExp(LogCategory category, object data);

            /// <summary>
            /// Formats and writes a log message to JollyCoop
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogJolly(object data);

            /// <summary>
            /// Formats and writes a log message to JollyCoop with a specified logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogJolly(LogCategory category, object data);
        }
    }
}
