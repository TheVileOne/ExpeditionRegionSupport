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
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void Log(object messageObj);

            /// <summary>
            /// Formats and writes a log message with a specified logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void Log(LogCategory category, object messageObj);

            /// <summary>
            /// Formats and writes a log message
            /// </summary>
            /// <remarks>Accepts multiple targets through the use of bitflag operators (to combine multiple log targets into one)</remarks>
            /// <param name="target">The specified log file, or console identifier to target</param>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void Log(ILogTarget target, object messageObj);

            /// <inheritdoc cref="Log(LogCategory, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void Log(ILogTarget target, LogCategory category, object messageObj);

            /// <summary>
            /// Formats and writes a log message to multiple log targets
            /// </summary>
            /// <param name="targets">The specified log file, or console identifiers to target</param>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void Log(IEnumerable<ILogTarget> targets, object messageObj);

            /// <inheritdoc cref="Log(LogCategory, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void Log(IEnumerable<ILogTarget> targets, LogCategory category, object messageObj);
            #endregion
            #region Contextual

            /// <summary>
            /// Formats and writes a log message with a debug context
            /// </summary>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogDebug(object messageObj);

            /// <summary>
            /// Formats and writes a log message with an informational context (typically the default context)
            /// </summary>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogInfo(object messageObj);

            /// <summary>
            /// Formats and writes a log message with an important context
            /// </summary>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogImportant(object messageObj);

            /// <summary>
            /// Formats and writes a log message with a message context
            /// </summary>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogMessage(object messageObj);

            /// <summary>
            /// Formats and writes a log message with a warning context
            /// </summary>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogWarning(object messageObj);

            /// <summary>
            /// Formats and writes a log message with an error context
            /// </summary>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogError(object messageObj);

            /// <summary>
            /// Formats and writes a log message with a fatal context
            /// </summary>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogFatal(object messageObj);

            /// <inheritdoc cref="LogDebug(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogDebug(ILogTarget target, object messageObj);

            /// <inheritdoc cref="LogInfo(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogInfo(ILogTarget target, object messageObj);

            /// <inheritdoc cref="LogImportant(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogImportant(ILogTarget target, object messageObj);

            /// <inheritdoc cref="LogMessage(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogMessage(ILogTarget target, object messageObj);

            /// <inheritdoc cref="LogWarning(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogWarning(ILogTarget target, object messageObj);

            /// <inheritdoc cref="LogError(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogError(ILogTarget target, object messageObj);

            /// <inheritdoc cref="LogFatal(object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogFatal(ILogTarget target, object messageObj);

            /// <inheritdoc cref="LogDebug(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogDebug(IEnumerable<ILogTarget> targets, object messageObj);

            /// <inheritdoc cref="LogInfo(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogInfo(IEnumerable<ILogTarget> targets, object messageObj);

            /// <inheritdoc cref="LogImportant(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogImportant(IEnumerable<ILogTarget> targets, object messageObj);

            /// <inheritdoc cref="LogMessage(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogMessage(IEnumerable<ILogTarget> targets, object messageObj);

            /// <inheritdoc cref="LogWarning(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogWarning(IEnumerable<ILogTarget> targets, object messageObj);

            /// <inheritdoc cref="LogError(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogError(IEnumerable<ILogTarget> targets, object messageObj);

            /// <inheritdoc cref="LogFatal(object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogFatal(IEnumerable<ILogTarget> targets, object messageObj);
            #endregion
            #region LogOnce

            /// <summary>
            /// Formats and writes a log message only once
            /// </summary>
            /// <remarks>Prevents logging a message more than once</remarks>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogOnce(object messageObj);

            /// <summary>
            /// Formats and writes a log message only once with a specified logging context
            /// </summary>
            /// <inheritdoc cref="LogOnce(object)" select="remarks"/>
            /// <param name="category">The specified logging context</param>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogOnce(LogCategory category, object messageObj);

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
            void LogOnce(ILogTarget target, object messageObj);

            /// <summary>
            /// Formats and writes a log message only once with a specified logging context
            /// </summary>
            /// <inheritdoc cref="LogOnce(ILogTarget, object)" select="param, remarks"/>
            void LogOnce(ILogTarget target, LogCategory category, object messageObj);

            /// <summary>
            /// Formats and writes a log message to multiple log targets only once
            /// </summary>
            /// <inheritdoc cref="LogOnce(object)" select="remarks"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogOnce(IEnumerable<ILogTarget> targets, object messageObj);

            /// <inheritdoc cref="LogOnce(LogCategory, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)" select="param"/>
            void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, object messageObj);
            #endregion
            #endregion
            #region Colors
            #region Base

            /// <inheritdoc cref="Log(object)"/>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            /// <param name="messageColor">The text color to apply to a message</param>
            void Log(object messageObj, Color messageColor);

            /// <inheritdoc cref="Log(LogCategory, object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void Log(LogCategory category, object messageObj, Color messageColor);

            /// <inheritdoc cref="Log(ILogTarget, object)"/>
            /// <param name="target">The specified log file, or console identifier to target</param>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            /// <param name="messageColor">The text color to apply to a message</param>
            void Log(ILogTarget target, object messageObj, Color messageColor);

            /// <inheritdoc cref="Log(ILogTarget, LogCategory, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void Log(ILogTarget target, LogCategory category, object messageObj, Color messageColor);

            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object)"/>
            /// <param name="targets">The specified log file, or console identifiers to target</param>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            /// <param name="messageColor">The text color to apply to a message</param>
            void Log(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor);

            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, LogCategory, object, Color)" select="param"/>
            void Log(IEnumerable<ILogTarget> targets, LogCategory category, object messageObj, Color messageColor);
            #endregion
            #region Contextual

            /// <inheritdoc cref="LogDebug(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogDebug(object messageObj, Color messageColor);

            /// <inheritdoc cref="LogInfo(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogInfo(object messageObj, Color messageColor);

            /// <inheritdoc cref="LogImportant(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogImportant(object messageObj, Color messageColor);

            /// <inheritdoc cref="LogMessage(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogMessage(object messageObj, Color messageColor);

            /// <inheritdoc cref="LogWarning(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogWarning(object messageObj, Color messageColor);

            /// <inheritdoc cref="LogError(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogError(object messageObj, Color messageColor);

            /// <inheritdoc cref="LogFatal(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogFatal(object messageObj, Color messageColor);

            /// <inheritdoc cref="LogDebug(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogDebug(ILogTarget target, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogInfo(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogInfo(ILogTarget target, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogImportant(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogImportant(ILogTarget target, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogMessage(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogMessage(ILogTarget target, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogWarning(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object)" select="param"/>
            void LogWarning(ILogTarget target, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogError(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogError(ILogTarget target, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogFatal(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(ILogTarget, object, Color)" select="param"/>
            void LogFatal(ILogTarget target, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogDebug(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogDebug(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogInfo(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogInfo(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogImportant(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogImportant(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogMessage(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogMessage(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogWarning(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogWarning(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogError(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogError(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogFatal(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogFatal(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor);
            #endregion
            #region LogOnce

            /// <inheritdoc cref="LogOnce(object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogOnce(object messageObj, Color messageColor);

            /// <inheritdoc cref="LogOnce(LogCategory, object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogOnce(LogCategory category, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogOnce(ILogTarget, object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogOnce(ILogTarget target, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogOnce(ILogTarget, LogCategory, object)"/>
            /// <inheritdoc cref="Log(object, Color)" select="param"/>
            void LogOnce(ILogTarget target, LogCategory category, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogOnce(IEnumerable{ILogTarget}, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, object, Color)" select="param"/>
            void LogOnce(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogOnce(IEnumerable{ILogTarget}, LogCategory, object)"/>
            /// <inheritdoc cref="Log(IEnumerable{ILogTarget}, LogCategory, object, Color)" select="param"/>
            void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, object messageObj, Color messageColor);
            #endregion
            #endregion
        }

        internal interface Game
        {
            #region General

            /// <summary>
            /// Formats and writes a log message to BepInEx
            /// </summary>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogBepEx(object messageObj);

            /// <summary>
            /// Formats and writes a log message to BepInEx with a specified logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogBepEx(LogLevel category, object messageObj);

            /// <summary>
            /// Formats and writes a log message to BepInEx with a specified logging context, and logging source
            /// </summary>
            /// <param name="source">The source of the logged message (usually a ManualLogSource in a modding context)</param>
            /// <param name="category">The specified logging context</param>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogBepEx(ILogSource source, LogLevel category, object messageObj);

            /// <summary>
            /// Formats and writes a log message to BepInEx
            /// </summary>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogUnity(object messageObj);

            /// <summary>
            /// Formats and writes a log message to Unity with a specified logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogUnity(LogType category, object messageObj);

            /// <summary>
            /// Formats and writes a log message to Expedition
            /// </summary>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogExp(object messageObj);

            /// <summary>
            /// Formats and writes a log message to Expedition with a specified logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogExp(LogCategory category, object messageObj);

            /// <summary>
            /// Formats and writes a log message to JollyCoop
            /// </summary>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogJolly(object messageObj);

            /// <summary>
            /// Formats and writes a log message to JollyCoop with a specified logging context
            /// </summary>
            /// <param name="category">The specified logging context</param>
            /// <param name="messageObj">The object you want to log (string, interpolated string, object, etc.)</param>
            void LogJolly(LogCategory category, object messageObj);
            #endregion
            #region Colors

            /// <inheritdoc cref="LogBepEx(object)"/>
            /// <inheritdoc cref="Standard.Log(object, Color)" select="param"/>
            void LogBepEx(object messageObj, Color messageColor);

            /// <inheritdoc cref="LogBepEx(LogLevel, object)"/>
            /// <inheritdoc cref="Standard.Log(object, Color)" select="param"/>
            void LogBepEx(LogLevel category, object messageObj, Color messageColor);

            /// <inheritdoc cref="LogBepEx(ILogSource, LogLevel, object)"/>
            /// <inheritdoc cref="Standard.Log(object, Color)" select="param"/>
            void LogBepEx(ILogSource source, LogLevel category, object messageObj, Color messageColor);
            #endregion
        }
    }
}
