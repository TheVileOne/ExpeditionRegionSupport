using BepInEx.Logging;
using LogUtils.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace LogUtils.Documentation
{
    internal static class LoggerDocumentation
    {
        internal interface Standard
        {
            #region General
            #region Base

            /// <summary>
            /// Formats and writes a log message
            /// </summary>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void Log(object data);

            /// <summary>
            /// Formats and writes a log message with a specified logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void Log(LogCategory category, object data);

            /// <summary>
            /// Formats and writes a log message
            /// </summary>
            /// <remarks>Accepts multiple targets through the use of bitflag operators (to combine multiple log targets into one)</remarks>
            /// <param name="target">The specified log file, or console identifier to target</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void Log(ILogTarget target, object data);

            /// <inheritdoc cref="Log(LogCategory, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void Log(ILogTarget target, LogCategory category, object data);

            /// <summary>
            /// Formats and writes a log message to multiple log targets
            /// </summary>
            /// <param name="targets">The specified log file, or console identifiers to target</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void Log(IEnumerable<ILogTarget> targets, object data);

            /// <inheritdoc cref="Log(LogCategory, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void Log(IEnumerable<ILogTarget> targets, LogCategory category, object data);
            #endregion
            #region Contextual

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

            /// <inheritdoc cref="LogDebug(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogDebug(ILogTarget target, object data);

            /// <inheritdoc cref="LogInfo(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogInfo(ILogTarget target, object data);

            /// <inheritdoc cref="LogImportant(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogImportant(ILogTarget target, object data);

            /// <inheritdoc cref="LogMessage(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogMessage(ILogTarget target, object data);

            /// <inheritdoc cref="LogWarning(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogWarning(ILogTarget target, object data);

            /// <inheritdoc cref="LogError(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogError(ILogTarget target, object data);

            /// <inheritdoc cref="LogFatal(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogFatal(ILogTarget target, object data);

            /// <inheritdoc cref="LogDebug(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogDebug(IEnumerable<ILogTarget> targets, object data);

            /// <inheritdoc cref="LogInfo(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogInfo(IEnumerable<ILogTarget> targets, object data);

            /// <inheritdoc cref="LogImportant(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogImportant(IEnumerable<ILogTarget> targets, object data);

            /// <inheritdoc cref="LogMessage(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogMessage(IEnumerable<ILogTarget> targets, object data);

            /// <inheritdoc cref="LogWarning(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogWarning(IEnumerable<ILogTarget> targets, object data);

            /// <inheritdoc cref="LogError(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogError(IEnumerable<ILogTarget> targets, object data);

            /// <inheritdoc cref="LogFatal(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogFatal(IEnumerable<ILogTarget> targets, object data);
            #endregion
            #region LogOnce

            /// <summary>
            /// Formats and writes a log message only once
            /// </summary>
            /// <remarks>Prevents logging a message more than once</remarks>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogOnce(object data);

            /// <summary>
            /// Formats and writes a log message only once with a specified logging context
            /// </summary>
            /// <inheritdoc cref="LogOnce(object)" select="remarks"/>
            /// <param name="category">The specified logging context</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogOnce(LogCategory category, object data);

            /// <summary>
            /// Formats and writes a log message only once
            /// </summary>
            /// <remarks>
            /// <inheritdoc cref="LogOnce(object)" select="remarks"/>
            /// <para>
            /// Accepts multiple targets through the use of bitflag operators (to combine multiple log targets into one) <br/>
            /// When multiple targets are selected, it will log once to each specific target </para>
            /// </remarks>
            /// <inheritdoc cref="Log(ILogTarget, LogCategory,  object)" select="param"/>
            void LogOnce(ILogTarget target, object data);

            /// <summary>
            /// Formats and writes a log message only once with a specified logging context
            /// </summary>
            /// <inheritdoc cref="LogOnce(ILogTarget, object)" select="param, remarks"/>
            void LogOnce(ILogTarget target, LogCategory category, object data);

            /// <summary>
            /// Formats and writes a log message to multiple log targets only once
            /// </summary>
            /// <inheritdoc cref="LogOnce(object)" select="remarks"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogOnce(IEnumerable<ILogTarget> targets, object data);

            /// <inheritdoc cref="LogOnce(LogCategory, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, object data);
            #endregion
            #endregion
            #region Colors
            #region Base

            /// <inheritdoc cref="Log(object)"/>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            /// <param name="messageColor">The text color to apply to a message</param>
            void Log(object data, Color messageColor);

            /// <inheritdoc cref="Log(LogCategory, object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void Log(LogCategory category, object data, Color messageColor);

            /// <inheritdoc cref="Log(ILogTarget, object)"/>
            /// <param name="target">The specified log file, or console identifier to target</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            /// <param name="messageColor">The text color to apply to a message</param>
            void Log(ILogTarget target, object data, Color messageColor);

            /// <inheritdoc cref="Log(ILogTarget, LogCategory, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void Log(ILogTarget target, LogCategory category, object data, Color messageColor);

            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)"/>
            /// <param name="targets">The specified log file, or console identifiers to target</param>
            /// <param name="data">The object you want to log (string, interpolated string, object, etc.)</param>
            /// <param name="messageColor">The text color to apply to a message</param>
            void Log(IEnumerable<ILogTarget> targets, object data, Color messageColor);

            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, LogCategory, object, Color)" select="param"/>
            void Log(IEnumerable<ILogTarget> targets, LogCategory category, object data, Color messageColor);
            #endregion
            #region Contextual

            /// <inheritdoc cref="LogDebug(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogDebug(object data, Color messageColor);

            /// <inheritdoc cref="LogInfo(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogInfo(object data, Color messageColor);

            /// <inheritdoc cref="LogImportant(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogImportant(object data, Color messageColor);

            /// <inheritdoc cref="LogMessage(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogMessage(object data, Color messageColor);

            /// <inheritdoc cref="LogWarning(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogWarning(object data, Color messageColor);

            /// <inheritdoc cref="LogError(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogError(object data, Color messageColor);

            /// <inheritdoc cref="LogFatal(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogFatal(object data, Color messageColor);

            /// <inheritdoc cref="LogDebug(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogDebug(ILogTarget target, object data, Color messageColor);

            /// <inheritdoc cref="LogInfo(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogInfo(ILogTarget target, object data, Color messageColor);

            /// <inheritdoc cref="LogImportant(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogImportant(ILogTarget target, object data, Color messageColor);

            /// <inheritdoc cref="LogMessage(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogMessage(ILogTarget target, object data, Color messageColor);

            /// <inheritdoc cref="LogWarning(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogWarning(ILogTarget target, object data, Color messageColor);

            /// <inheritdoc cref="LogError(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogError(ILogTarget target, object data, Color messageColor);

            /// <inheritdoc cref="LogFatal(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogFatal(ILogTarget target, object data, Color messageColor);

            /// <inheritdoc cref="LogDebug(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogDebug(IEnumerable<ILogTarget> targets, object data, Color messageColor);

            /// <inheritdoc cref="LogInfo(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogInfo(IEnumerable<ILogTarget> targets, object data, Color messageColor);

            /// <inheritdoc cref="LogImportant(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogImportant(IEnumerable<ILogTarget> targets, object data, Color messageColor);

            /// <inheritdoc cref="LogMessage(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogMessage(IEnumerable<ILogTarget> targets, object data, Color messageColor);

            /// <inheritdoc cref="LogWarning(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogWarning(IEnumerable<ILogTarget> targets, object data, Color messageColor);

            /// <inheritdoc cref="LogError(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogError(IEnumerable<ILogTarget> targets, object data, Color messageColor);

            /// <inheritdoc cref="LogFatal(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogFatal(IEnumerable<ILogTarget> targets, object data, Color messageColor);
            #endregion
            #region LogOnce

            /// <inheritdoc cref="LogOnce(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogOnce(object data, Color messageColor);

            /// <inheritdoc cref="LogOnce(LogCategory, object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogOnce(LogCategory category, object data, Color messageColor);

            /// <inheritdoc cref="LogOnce(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogOnce(ILogTarget target, object data, Color messageColor);

            /// <inheritdoc cref="LogOnce(ILogTarget, LogCategory, object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogOnce(ILogTarget target, LogCategory category, object data, Color messageColor);

            /// <inheritdoc cref="LogOnce(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogOnce(IEnumerable<ILogTarget> targets, object data, Color messageColor);

            /// <inheritdoc cref="LogOnce(IEnumerable{ILogTarget}, LogCategory, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, LogCategory, object, Color)" select="param"/>
            void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, object data, Color messageColor);
            #endregion
            #endregion
        }

        internal interface Game
        {
            #region General

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
            #endregion
            #region Colors

            /// <inheritdoc cref="LogBepEx(object)"/>
            /// <inheritdoc cref="Standard.Log(object, Color)" select="param"/>
            void LogBepEx(object data, Color messageColor);

            /// <inheritdoc cref="LogBepEx(LogLevel, object)"/>
            /// <inheritdoc cref="Standard.Log(object, Color)" select="param"/>
            void LogBepEx(LogLevel category, object data, Color messageColor);

            /// <inheritdoc cref="LogBepEx(ILogSource, LogLevel, object)"/>
            /// <inheritdoc cref="Standard.Log(object, Color)" select="param"/>
            void LogBepEx(ILogSource source, LogLevel category, object data, Color messageColor);
            #endregion
        }
    }
}
