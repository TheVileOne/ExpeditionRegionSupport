using BepInEx.Logging;
using LogUtils.Console;
using LogUtils.Enums;
using LogUtils.Formatting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using LoggerDocs = LogUtils.Documentation.LoggerDocumentation;
using LogLevel = BepInEx.Logging.LogLevel;

namespace LogUtils
{
    /// <summary>
    /// Allows you to log messages to file, or a console using the write implementation of your choosing
    /// </summary>
    public partial class Logger : LoggerBase, IFormatLogger
    {
        internal const int PRIORITY_MODERATE = 1;

        #region Constructors

        /// <inheritdoc/>
        public Logger(ILogSource logSource) : base(logSource)
        {
        }

        /// <inheritdoc/>
        public Logger(ILogTarget preset) : base(LoggingMode.Inherit, true, preset)
        {
        }

        /// <inheritdoc/>
        public Logger(bool allowLogging, ILogTarget preset) : base(LoggingMode.Inherit, allowLogging, preset)
        {
        }

        /// <inheritdoc/>
        public Logger(LoggingMode mode, ILogTarget preset) : base(mode, true, preset)
        {
        }

        /// <inheritdoc/>
        public Logger(params ILogTarget[] presets) : base(LoggingMode.Inherit, true, presets)
        {
        }

        /// <inheritdoc/>
        public Logger(bool allowLogging, params ILogTarget[] presets) : base(LoggingMode.Inherit, allowLogging, presets)
        {
        }

        /// <inheritdoc/>
        public Logger(LoggingMode mode, params ILogTarget[] presets) : base(mode, true, presets)
        {
        }

        /// <inheritdoc/>
        public Logger(LoggingMode mode, bool allowLogging, params ILogTarget[] presets) : base(mode, allowLogging, presets)
        {
        }
        #endregion

        #region Log Overloads (object)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        public void Log(object messageObj)
        {
            Log(LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object)"/>
        public void LogOnce(object messageObj)
        {
            LogOnce(LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        public void LogDebug(object messageObj)
        {
            Log(LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        public void LogInfo(object messageObj)
        {
            Log(LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        public void LogImportant(object messageObj)
        {
            Log(LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        public void LogMessage(object messageObj)
        {
            Log(LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        public void LogWarning(object messageObj)
        {
            Log(LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        public void LogError(object messageObj)
        {
            Log(LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        public void LogFatal(object messageObj)
        {
            Log(LogCategory.Fatal, messageObj);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object, Color)"/>
        public void Log(object messageObj, Color messageColor)
        {
            Log(LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object, Color)"/>
        public void LogOnce(object messageObj, Color messageColor)
        {
            LogOnce(LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object, Color)"/>
        public void LogDebug(object messageObj, Color messageColor)
        {
            Log(LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object, Color)"/>
        public void LogInfo(object messageObj, Color messageColor)
        {
            Log(LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object, Color)"/>
        public void LogImportant(object messageObj, Color messageColor)
        {
            Log(LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object, Color)"/>
        public void LogMessage(object messageObj, Color messageColor)
        {
            Log(LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object, Color)"/>
        public void LogWarning(object messageObj, Color messageColor)
        {
            Log(LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object, Color)"/>
        public void LogError(object messageObj, Color messageColor)
        {
            Log(LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object, Color)"/>
        public void LogFatal(object messageObj, Color messageColor)
        {
            Log(LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object, Color)"/>
        public void Log(object messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object, Color)"/>
        public void LogOnce(object messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object, Color)"/>
        public void LogDebug(object messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Debug, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object, Color)"/>
        public void LogInfo(object messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Info, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object, Color)"/>
        public void LogImportant(object messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Important, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object, Color)"/>
        public void LogMessage(object messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Message, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object, Color)"/>
        public void LogWarning(object messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Warning, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object, Color)"/>
        public void LogError(object messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Error, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object, Color)"/>
        public void LogFatal(object messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Fatal, messageObj, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogType category, object messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogLevel category, object messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogCategory category, object messageObj)
        {
            LogBase(category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        public void LogOnce(LogType category, object messageObj)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        public void LogOnce(LogLevel category, object messageObj)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        public void LogOnce(LogCategory category, object messageObj)
        {
            LogBase(category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogType category, object messageObj, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogLevel category, object messageObj, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogCategory category, object messageObj, Color messageColor)
        {
            LogBase(category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void LogOnce(LogType category, object messageObj, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogLevel category, object messageObj, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogCategory category, object messageObj, Color messageColor)
        {
            LogBase(category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogType category, object messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogLevel category, object messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogCategory category, object messageObj, ConsoleColor messageColor)
        {
            LogBase(category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void LogOnce(LogType category, object messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogLevel category, object messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogCategory category, object messageObj, ConsoleColor messageColor)
        {
            LogBase(category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #region Rain World Overloads
        #region BepInEx

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(object)"/>
        public void LogBepEx(object messageObj)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object)"/>
        public void LogBepEx(LogLevel category, object messageObj)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object)"/>
        public void LogBepEx(LogCategory category, object messageObj)
        {
            LogBase(LogID.BepInEx, category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(object messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogLevel category, object messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogCategory category, object messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(object messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogLevel category, object messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogCategory category, object messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #region Unity

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(object)"/>
        public void LogUnity(object messageObj)
        {
            LogBase(LogID.Unity, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        public void LogUnity(LogType category, object messageObj)
        {
            LogBase(LogCategory.GetUnityLogID(category), LogCategory.ToCategory(category), messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        public void LogUnity(LogCategory category, object messageObj)
        {
            LogBase(LogCategory.GetUnityLogID(category.UnityCategory), category, messageObj, false);
        }
        #endregion
        #region Expedition

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(object)"/>
        public void LogExp(object messageObj)
        {
            LogBase(LogID.Expedition, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(LogCategory, object)"/>
        public void LogExp(LogCategory category, object messageObj)
        {
            LogBase(LogID.Expedition, category, messageObj, false);
        }
        #endregion
        #region JollyCoop

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(object)"/>
        public void LogJolly(object messageObj)
        {
            LogBase(LogID.JollyCoop, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(LogCategory, object)"/>
        public void LogJolly(LogCategory category, object messageObj)
        {
            LogBase(LogID.JollyCoop, category, messageObj, false);
        }
        #endregion
        #endregion
        #endregion
        #region  Log Overloads (ILogTarget, object)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object)"/>
        public void Log(ILogTarget target, object messageObj)
        {
            Log(target, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object)"/>
        public void LogOnce(ILogTarget target, object messageObj)
        {
            Log(target, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object)"/>
        public void LogDebug(ILogTarget target, object messageObj)
        {
            Log(target, LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object)"/>
        public void LogInfo(ILogTarget target, object messageObj)
        {
            Log(target, LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object)"/>
        public void LogImportant(ILogTarget target, object messageObj)
        {
            Log(target, LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object)"/>
        public void LogMessage(ILogTarget target, object messageObj)
        {
            Log(target, LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object)"/>
        public void LogWarning(ILogTarget target, object messageObj)
        {
            Log(target, LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object)"/>
        public void LogError(ILogTarget target, object messageObj)
        {
            Log(target, LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object)"/>
        public void LogFatal(ILogTarget target, object messageObj)
        {
            Log(target, LogCategory.Fatal, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        public void Log(ILogTarget target, LogLevel category, object messageObj)
        {
            Log(target, LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        public void Log(ILogTarget target, LogCategory category, object messageObj)
        {
            LogUnresolvedTarget(target, category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object)"/>
        public void LogOnce(ILogTarget target, LogCategory category, object messageObj)
        {
            LogUnresolvedTarget(target, category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object, Color)"/>
        public void Log(ILogTarget target, object messageObj, Color messageColor)
        {
            Log(target, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object, Color)"/>
        public void LogOnce(ILogTarget target, object messageObj, Color messageColor)
        {
            Log(target, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object, Color)"/>
        public void LogDebug(ILogTarget target, object messageObj, Color messageColor)
        {
            Log(target, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object, Color)"/>
        public void LogInfo(ILogTarget target, object messageObj, Color messageColor)
        {
            Log(target, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object, Color)"/>
        public void LogImportant(ILogTarget target, object messageObj, Color messageColor)
        {
            Log(target, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object, Color)"/>
        public void LogMessage(ILogTarget target, object messageObj, Color messageColor)
        {
            Log(target, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object, Color)"/>
        public void LogWarning(ILogTarget target, object messageObj, Color messageColor)
        {
            Log(target, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object, Color)"/>
        public void LogError(ILogTarget target, object messageObj, Color messageColor)
        {
            Log(target, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object, Color)"/>
        public void LogFatal(ILogTarget target, object messageObj, Color messageColor)
        {
            Log(target, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, LogLevel category, object messageObj, Color messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, LogCategory category, object messageObj, Color messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        public void LogOnce(ILogTarget target, LogCategory category, object messageObj, Color messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object, Color)"/>
        public void Log(ILogTarget target, object messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object, Color)"/>
        public void LogOnce(ILogTarget target, object messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object, Color)"/>
        public void LogDebug(ILogTarget target, object messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Debug, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object, Color)"/>
        public void LogInfo(ILogTarget target, object messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Info, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object, Color)"/>
        public void LogImportant(ILogTarget target, object messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Important, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object, Color)"/>
        public void LogMessage(ILogTarget target, object messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Message, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object, Color)"/>
        public void LogWarning(ILogTarget target, object messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Warning, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object, Color)"/>
        public void LogError(ILogTarget target, object messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Error, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object, Color)"/>
        public void LogFatal(ILogTarget target, object messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Fatal, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, LogLevel category, object messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, LogCategory category, object messageObj, ConsoleColor messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        public void LogOnce(ILogTarget target, LogCategory category, object messageObj, ConsoleColor messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #endregion
        #region  Log Overloads (IEnumerable<ILogTarget>, object)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object)"/>
        public void Log(IEnumerable<ILogTarget> targets, object messageObj)
        {
            Log(targets, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, object messageObj)
        {
            Log(targets, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object)"/>
        public void LogDebug(IEnumerable<ILogTarget> targets, object messageObj)
        {
            Log(targets, LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object)"/>
        public void LogInfo(IEnumerable<ILogTarget> targets, object messageObj)
        {
            Log(targets, LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object)"/>
        public void LogImportant(IEnumerable<ILogTarget> targets, object messageObj)
        {
            Log(targets, LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object)"/>
        public void LogMessage(IEnumerable<ILogTarget> targets, object messageObj)
        {
            Log(targets, LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object)"/>
        public void LogWarning(IEnumerable<ILogTarget> targets, object messageObj)
        {
            Log(targets, LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object)"/>
        public void LogError(IEnumerable<ILogTarget> targets, object messageObj)
        {
            Log(targets, LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object)"/>
        public void LogFatal(IEnumerable<ILogTarget> targets, object messageObj)
        {
            Log(targets, LogCategory.Fatal, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, object messageObj)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, object messageObj)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, object messageObj)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogDebug(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogInfo(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogImportant(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogMessage(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogWarning(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogError(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogFatal(IEnumerable<ILogTarget> targets, object messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, object messageObj, Color messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, object messageObj, Color messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, object messageObj, Color messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, object messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, object messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogDebug(IEnumerable<ILogTarget> targets, object messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogInfo(IEnumerable<ILogTarget> targets, object messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogImportant(IEnumerable<ILogTarget> targets, object messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogMessage(IEnumerable<ILogTarget> targets, object messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogWarning(IEnumerable<ILogTarget> targets, object messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogError(IEnumerable<ILogTarget> targets, object messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogFatal(IEnumerable<ILogTarget> targets, object messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, object messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, object messageObj, ConsoleColor messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, object messageObj, ConsoleColor messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #endregion

        #region Log Overloads (string)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(string messageObj)
        {
            Log(LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(string messageObj)
        {
            LogOnce(LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(string messageObj)
        {
            Log(LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(string messageObj)
        {
            Log(LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(string messageObj)
        {
            Log(LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(string messageObj)
        {
            Log(LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(string messageObj)
        {
            Log(LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(string messageObj)
        {
            Log(LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(string messageObj)
        {
            Log(LogCategory.Fatal, messageObj);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(string messageObj, Color messageColor)
        {
            Log(LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(string messageObj, Color messageColor)
        {
            LogOnce(LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(string messageObj, Color messageColor)
        {
            Log(LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(string messageObj, Color messageColor)
        {
            Log(LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(string messageObj, Color messageColor)
        {
            Log(LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(string messageObj, Color messageColor)
        {
            Log(LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(string messageObj, Color messageColor)
        {
            Log(LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(string messageObj, Color messageColor)
        {
            Log(LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(string messageObj, Color messageColor)
        {
            Log(LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(string messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(string messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(string messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Debug, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(string messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Info, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(string messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Important, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(string messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Message, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(string messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Warning, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(string messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Error, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(string messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Fatal, messageObj, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogType category, string messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogLevel category, string messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogCategory category, string messageObj)
        {
            LogBase(category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogType category, string messageObj)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogLevel category, string messageObj)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogCategory category, string messageObj)
        {
            LogBase(category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogType category, string messageObj, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogLevel category, string messageObj, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogCategory category, string messageObj, Color messageColor)
        {
            LogBase(category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void LogOnce(LogType category, string messageObj, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogLevel category, string messageObj, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogCategory category, string messageObj, Color messageColor)
        {
            LogBase(category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogType category, string messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogLevel category, string messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogCategory category, string messageObj, ConsoleColor messageColor)
        {
            LogBase(category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void LogOnce(LogType category, string messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogLevel category, string messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogCategory category, string messageObj, ConsoleColor messageColor)
        {
            LogBase(category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #region Rain World Overloads
        #region BepInEx

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(object)"/>
        public void LogBepEx(string messageObj)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object)"/>
        public void LogBepEx(LogLevel category, string messageObj)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object)"/>
        public void LogBepEx(LogCategory category, string messageObj)
        {
            LogBase(LogID.BepInEx, category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(string messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogLevel category, string messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogCategory category, string messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(string messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogLevel category, string messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogCategory category, string messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #region Unity

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(object)"/>
        public void LogUnity(string messageObj)
        {
            LogBase(LogID.Unity, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        public void LogUnity(LogType category, string messageObj)
        {
            LogBase(LogCategory.GetUnityLogID(category), LogCategory.ToCategory(category), messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        public void LogUnity(LogCategory category, string messageObj)
        {
            LogBase(LogCategory.GetUnityLogID(category.UnityCategory), category, messageObj, false);
        }
        #endregion
        #region Expedition

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(object)"/>
        public void LogExp(string messageObj)
        {
            LogBase(LogID.Expedition, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(LogCategory, object)"/>
        public void LogExp(LogCategory category, string messageObj)
        {
            LogBase(LogID.Expedition, category, messageObj, false);
        }
        #endregion
        #region JollyCoop

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(object)"/>
        public void LogJolly(string messageObj)
        {
            LogBase(LogID.JollyCoop, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(LogCategory, object)"/>
        public void LogJolly(LogCategory category, string messageObj)
        {
            LogBase(LogID.JollyCoop, category, messageObj, false);
        }
        #endregion
        #endregion
        #endregion
        #region  Log Overloads (ILogTarget, string)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, string messageObj)
        {
            Log(target, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, string messageObj)
        {
            Log(target, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(ILogTarget target, string messageObj)
        {
            Log(target, LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(ILogTarget target, string messageObj)
        {
            Log(target, LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(ILogTarget target, string messageObj)
        {
            Log(target, LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(ILogTarget target, string messageObj)
        {
            Log(target, LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(ILogTarget target, string messageObj)
        {
            Log(target, LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(ILogTarget target, string messageObj)
        {
            Log(target, LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(ILogTarget target, string messageObj)
        {
            Log(target, LogCategory.Fatal, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogLevel category, string messageObj)
        {
            Log(target, LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogCategory category, string messageObj)
        {
            LogUnresolvedTarget(target, category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, LogCategory category, string messageObj)
        {
            LogUnresolvedTarget(target, category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, string messageObj, Color messageColor)
        {
            Log(target, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, string messageObj, Color messageColor)
        {
            Log(target, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(ILogTarget target, string messageObj, Color messageColor)
        {
            Log(target, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(ILogTarget target, string messageObj, Color messageColor)
        {
            Log(target, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(ILogTarget target, string messageObj, Color messageColor)
        {
            Log(target, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(ILogTarget target, string messageObj, Color messageColor)
        {
            Log(target, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(ILogTarget target, string messageObj, Color messageColor)
        {
            Log(target, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(ILogTarget target, string messageObj, Color messageColor)
        {
            Log(target, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(ILogTarget target, string messageObj, Color messageColor)
        {
            Log(target, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogLevel category, string messageObj, Color messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogCategory category, string messageObj, Color messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, LogCategory category, string messageObj, Color messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, string messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, string messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(ILogTarget target, string messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Debug, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(ILogTarget target, string messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Info, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(ILogTarget target, string messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Important, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(ILogTarget target, string messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Message, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(ILogTarget target, string messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Warning, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(ILogTarget target, string messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Error, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(ILogTarget target, string messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Fatal, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogLevel category, string messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogCategory category, string messageObj, ConsoleColor messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, LogCategory category, string messageObj, ConsoleColor messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #endregion
        #region  Log Overloads (IEnumerable<ILogTarget>, string)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, string messageObj)
        {
            Log(targets, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, string messageObj)
        {
            Log(targets, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(IEnumerable<ILogTarget> targets, string messageObj)
        {
            Log(targets, LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(IEnumerable<ILogTarget> targets, string messageObj)
        {
            Log(targets, LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(IEnumerable<ILogTarget> targets, string messageObj)
        {
            Log(targets, LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(IEnumerable<ILogTarget> targets, string messageObj)
        {
            Log(targets, LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(IEnumerable<ILogTarget> targets, string messageObj)
        {
            Log(targets, LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(IEnumerable<ILogTarget> targets, string messageObj)
        {
            Log(targets, LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(IEnumerable<ILogTarget> targets, string messageObj)
        {
            Log(targets, LogCategory.Fatal, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, string messageObj)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, string messageObj)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, string messageObj)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, string messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, string messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(IEnumerable<ILogTarget> targets, string messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(IEnumerable<ILogTarget> targets, string messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(IEnumerable<ILogTarget> targets, string messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(IEnumerable<ILogTarget> targets, string messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(IEnumerable<ILogTarget> targets, string messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(IEnumerable<ILogTarget> targets, string messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(IEnumerable<ILogTarget> targets, string messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, string messageObj, Color messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, string messageObj, Color messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, string messageObj, Color messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, string messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, string messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(IEnumerable<ILogTarget> targets, string messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(IEnumerable<ILogTarget> targets, string messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(IEnumerable<ILogTarget> targets, string messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(IEnumerable<ILogTarget> targets, string messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(IEnumerable<ILogTarget> targets, string messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(IEnumerable<ILogTarget> targets, string messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(IEnumerable<ILogTarget> targets, string messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, string messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, string messageObj, ConsoleColor messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, string messageObj, ConsoleColor messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #endregion

        #region Log Overloads (InterpolatedStringHandler)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(InterpolatedStringHandler messageObj)
        {
            Log(LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(InterpolatedStringHandler messageObj)
        {
            LogOnce(LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(InterpolatedStringHandler messageObj)
        {
            Log(LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(InterpolatedStringHandler messageObj)
        {
            Log(LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(InterpolatedStringHandler messageObj)
        {
            Log(LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(InterpolatedStringHandler messageObj)
        {
            Log(LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(InterpolatedStringHandler messageObj)
        {
            Log(LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(InterpolatedStringHandler messageObj)
        {
            Log(LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(InterpolatedStringHandler messageObj)
        {
            Log(LogCategory.Fatal, messageObj);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogOnce(LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Debug, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Info, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Important, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Message, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Warning, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Error, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Fatal, messageObj, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogType category, InterpolatedStringHandler messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogLevel category, InterpolatedStringHandler messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogCategory category, InterpolatedStringHandler messageObj)
        {
            LogBase(category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogType category, InterpolatedStringHandler messageObj)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogLevel category, InterpolatedStringHandler messageObj)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogCategory category, InterpolatedStringHandler messageObj)
        {
            LogBase(category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogType category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogLevel category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogCategory category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogBase(category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogType category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogLevel category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogCategory category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogBase(category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogType category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogLevel category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogCategory category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogBase(category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogType category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogLevel category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogCategory category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogBase(category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #region Rain World Overloads
        #region BepInEx

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(InterpolatedStringHandler messageObj)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(LogLevel category, InterpolatedStringHandler messageObj)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(LogCategory category, InterpolatedStringHandler messageObj)
        {
            LogBase(LogID.BepInEx, category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(LogLevel category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(LogCategory category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(LogLevel category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(LogCategory category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #region Unity

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogUnity(InterpolatedStringHandler messageObj)
        {
            LogBase(LogID.Unity, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogUnity(LogType category, InterpolatedStringHandler messageObj)
        {
            LogBase(LogCategory.GetUnityLogID(category), LogCategory.ToCategory(category), messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogUnity(LogCategory category, InterpolatedStringHandler messageObj)
        {
            LogBase(LogCategory.GetUnityLogID(category.UnityCategory), category, messageObj, false);
        }
        #endregion
        #region Expedition

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogExp(InterpolatedStringHandler messageObj)
        {
            LogBase(LogID.Expedition, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogExp(LogCategory category, InterpolatedStringHandler messageObj)
        {
            LogBase(LogID.Expedition, category, messageObj, false);
        }
        #endregion
        #region JollyCoop

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogJolly(InterpolatedStringHandler messageObj)
        {
            LogBase(LogID.JollyCoop, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogJolly(LogCategory category, InterpolatedStringHandler messageObj)
        {
            LogBase(LogID.JollyCoop, category, messageObj, false);
        }
        #endregion
        #endregion
        #endregion
        #region  Log Overloads (ILogTarget, InterpolatedStringHandler)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, InterpolatedStringHandler messageObj)
        {
            Log(target, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, InterpolatedStringHandler messageObj)
        {
            Log(target, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(ILogTarget target, InterpolatedStringHandler messageObj)
        {
            Log(target, LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(ILogTarget target, InterpolatedStringHandler messageObj)
        {
            Log(target, LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(ILogTarget target, InterpolatedStringHandler messageObj)
        {
            Log(target, LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(ILogTarget target, InterpolatedStringHandler messageObj)
        {
            Log(target, LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(ILogTarget target, InterpolatedStringHandler messageObj)
        {
            Log(target, LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(ILogTarget target, InterpolatedStringHandler messageObj)
        {
            Log(target, LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(ILogTarget target, InterpolatedStringHandler messageObj)
        {
            Log(target, LogCategory.Fatal, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogLevel category, InterpolatedStringHandler messageObj)
        {
            Log(target, LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogCategory category, InterpolatedStringHandler messageObj)
        {
            LogUnresolvedTarget(target, category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, LogCategory category, InterpolatedStringHandler messageObj)
        {
            LogUnresolvedTarget(target, category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(target, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(target, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(ILogTarget target, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(target, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(ILogTarget target, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(target, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(ILogTarget target, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(target, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(ILogTarget target, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(target, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(ILogTarget target, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(target, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(ILogTarget target, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(target, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(ILogTarget target, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(target, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogLevel category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogCategory category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, LogCategory category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(ILogTarget target, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Debug, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(ILogTarget target, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Info, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(ILogTarget target, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Important, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(ILogTarget target, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Message, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(ILogTarget target, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Warning, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(ILogTarget target, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Error, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(ILogTarget target, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Fatal, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogLevel category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogCategory category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, LogCategory category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #endregion
        #region  Log Overloads (IEnumerable<ILogTarget>, InterpolatedStringHandler)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj)
        {
            Log(targets, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj)
        {
            Log(targets, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj)
        {
            Log(targets, LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj)
        {
            Log(targets, LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj)
        {
            Log(targets, LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj)
        {
            Log(targets, LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj)
        {
            Log(targets, LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj)
        {
            Log(targets, LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj)
        {
            Log(targets, LogCategory.Fatal, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, InterpolatedStringHandler messageObj)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, InterpolatedStringHandler messageObj)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, InterpolatedStringHandler messageObj)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, InterpolatedStringHandler messageObj, Color messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(IEnumerable<ILogTarget> targets, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, InterpolatedStringHandler messageObj, ConsoleColor messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #endregion

        #region Log Overloads (InterpolatedStringHandler, object[])

        /// <inheritdoc cref="LoggerDocs.Standard.Log(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(LogCategory.Default, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogOnce(LogCategory.Default, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(LogCategory.Debug, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(LogCategory.Info, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(LogCategory.Important, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(LogCategory.Message, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(LogCategory.Warning, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(LogCategory.Error, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(LogCategory.Fatal, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogType category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(LogCategory.ToCategory(category), format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogLevel category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(LogCategory.ToCategory(category), format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(LogCategory category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(category, format, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogType category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogOnce(LogCategory.ToCategory(category), format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogLevel category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogOnce(LogCategory.ToCategory(category), format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(LogCategory category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(category, format, true);
        }
        #region Rain World Overloads
        #region BepInEx

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(LogID.BepInEx, LogCategory.Default, format, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(LogLevel category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), format, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogBepEx(LogCategory category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(LogID.BepInEx, category, format, false);
        }
        #endregion
        #region Unity

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogUnity(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(LogID.Unity, LogCategory.Default, format, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogUnity(LogType category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(LogCategory.GetUnityLogID(category), LogCategory.ToCategory(category), format, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogUnity(LogCategory category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(LogCategory.GetUnityLogID(category.UnityCategory), category, format, false);
        }
        #endregion
        #region Expedition

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogExp(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(LogID.Expedition, LogCategory.Default, format, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogExp(LogCategory category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(LogID.Expedition, category, format, false);
        }
        #endregion
        #region JollyCoop

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogJolly(InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(LogID.JollyCoop, LogCategory.Default, format, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogJolly(LogCategory category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(LogID.JollyCoop, category, format, false);
        }
        #endregion
        #endregion
        #endregion
        #region  Log Overloads (ILogTarget, InterpolatedStringHandler, object[])

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(target, LogCategory.Default, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(target, LogCategory.Default, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(ILogTarget target, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(target, LogCategory.Debug, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(ILogTarget target, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(target, LogCategory.Info, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(ILogTarget target, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(target, LogCategory.Important, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(ILogTarget target, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(target, LogCategory.Message, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(ILogTarget target, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(target, LogCategory.Warning, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(ILogTarget target, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(target, LogCategory.Error, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(ILogTarget target, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(target, LogCategory.Fatal, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogLevel category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(target, LogCategory.ToCategory(category), format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(ILogTarget target, LogCategory category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogUnresolvedTarget(target, category, format, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(ILogTarget target, LogCategory category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogUnresolvedTarget(target, category, format, true);
        }
        #endregion
        #region  Log Overloads (IEnumerable<ILogTarget>, InterpolatedStringHandler, object[])

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(targets, LogCategory.Default, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(targets, LogCategory.Default, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogDebug(IEnumerable<ILogTarget> targets, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(targets, LogCategory.Debug, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogInfo(IEnumerable<ILogTarget> targets, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(targets, LogCategory.Info, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogImportant(IEnumerable<ILogTarget> targets, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(targets, LogCategory.Important, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogMessage(IEnumerable<ILogTarget> targets, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(targets, LogCategory.Message, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogWarning(IEnumerable<ILogTarget> targets, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(targets, LogCategory.Warning, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogError(IEnumerable<ILogTarget> targets, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(targets, LogCategory.Error, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogFatal(IEnumerable<ILogTarget> targets, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(targets, LogCategory.Fatal, format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            Log(targets, LogCategory.ToCategory(category), format);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(new LogTargetCollection(targets), category, format, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, string, object[])"/>
        [OverloadResolutionPriority(PRIORITY_MODERATE)]
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, InterpolatedStringHandler format, params object[] formatArgs)
        {
            format.AppendFormattedRange(formatArgs);
            LogBase(new LogTargetCollection(targets), category, format, true);
        }
        #endregion
        
        #region Log Overloads (string, object[])

        /// <inheritdoc cref="LoggerDocs.Standard.Log(string, object[])"/>
        public void Log(string format, params object[] formatArgs)
        {
            Log(LogCategory.Default, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(string, object[])"/>
        public void LogOnce(string format, params object[] formatArgs)
        {
            LogOnce(LogCategory.Default, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(string, object[])"/>
        public void LogDebug(string format, params object[] formatArgs)
        {
            Log(LogCategory.Debug, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(string, object[])"/>
        public void LogInfo(string format, params object[] formatArgs)
        {
            Log(LogCategory.Info, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(string, object[])"/>
        public void LogImportant(string format, params object[] formatArgs)
        {
            Log(LogCategory.Important, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(string, object[])"/>
        public void LogMessage(string format, params object[] formatArgs)
        {
            Log(LogCategory.Message, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(string, object[])"/>
        public void LogWarning(string format, params object[] formatArgs)
        {
            Log(LogCategory.Warning, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(string, object[])"/>
        public void LogError(string format, params object[] formatArgs)
        {
            Log(LogCategory.Error, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(string, object[])"/>
        public void LogFatal(string format, params object[] formatArgs)
        {
            Log(LogCategory.Fatal, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, object[])"/>
        public void Log(LogType category, string format, params object[] formatArgs)
        {
            Log(LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, object[])"/>
        public void Log(LogLevel category, string format, params object[] formatArgs)
        {
            Log(LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, object[])"/>
        public void Log(LogCategory category, string format, params object[] formatArgs)
        {
            LogBase(category, FormattableStringFactory.Create(format, formatArgs), false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, string, object[])"/>
        public void LogOnce(LogType category, string format, params object[] formatArgs)
        {
            LogOnce(LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, string, object[])"/>
        public void LogOnce(LogLevel category, string format, params object[] formatArgs)
        {
            LogOnce(LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, string, object[])"/>
        public void LogOnce(LogCategory category, string format, params object[] formatArgs)
        {
            LogBase(category, FormattableStringFactory.Create(format, formatArgs), true);
        }
        #region Rain World Overloads
        #region BepInEx

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(string, object[])"/>
        public void LogBepEx(string format, params object[] formatArgs)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, FormattableStringFactory.Create(format, formatArgs), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, string, object[])"/>
        public void LogBepEx(LogLevel category, string format, params object[] formatArgs)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArgs), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, string, object[])"/>
        public void LogBepEx(LogCategory category, string format, params object[] formatArgs)
        {
            LogBase(LogID.BepInEx, category, FormattableStringFactory.Create(format, formatArgs), false);
        }
        #endregion
        #region Unity

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(string, object[])"/>
        public void LogUnity(string format, params object[] formatArgs)
        {
            LogBase(LogID.Unity, LogCategory.Default, FormattableStringFactory.Create(format, formatArgs), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, string, object[])"/>
        public void LogUnity(LogType category, string format, params object[] formatArgs)
        {
            LogBase(LogCategory.GetUnityLogID(category), LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArgs), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, string, object[])"/>
        public void LogUnity(LogCategory category, string format, params object[] formatArgs)
        {
            LogBase(LogCategory.GetUnityLogID(category.UnityCategory), category, FormattableStringFactory.Create(format, formatArgs), false);
        }
        #endregion
        #region Expedition

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(string, object[])"/>
        public void LogExp(string format, params object[] formatArgs)
        {
            LogBase(LogID.Expedition, LogCategory.Default, FormattableStringFactory.Create(format, formatArgs), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(LogCategory, string, object[])"/>
        public void LogExp(LogCategory category, string format, params object[] formatArgs)
        {
            LogBase(LogID.Expedition, category, FormattableStringFactory.Create(format, formatArgs), false);
        }
        #endregion
        #region JollyCoop

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(string, object[])"/>
        public void LogJolly(string format, params object[] formatArgs)
        {
            LogBase(LogID.JollyCoop, LogCategory.Default, FormattableStringFactory.Create(format, formatArgs), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(LogCategory, string, object[])"/>
        public void LogJolly(LogCategory category, string format, params object[] formatArgs)
        {
            LogBase(LogID.JollyCoop, category, FormattableStringFactory.Create(format, formatArgs), false);
        }
        #endregion
        #endregion
        #endregion
        #region  Log Overloads (ILogTarget, string, object[])

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, string, object[])"/>
        public void Log(ILogTarget target, string format, params object[] formatArgs)
        {
            Log(target, LogCategory.Default, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, string, object[])"/>
        public void LogOnce(ILogTarget target, string format, params object[] formatArgs)
        {
            Log(target, LogCategory.Default, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, string, object[])"/>
        public void LogDebug(ILogTarget target, string format, params object[] formatArgs)
        {
            Log(target, LogCategory.Debug, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, string, object[])"/>
        public void LogInfo(ILogTarget target, string format, params object[] formatArgs)
        {
            Log(target, LogCategory.Info, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, string, object[])"/>
        public void LogImportant(ILogTarget target, string format, params object[] formatArgs)
        {
            Log(target, LogCategory.Important, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, string, object[])"/>
        public void LogMessage(ILogTarget target, string format, params object[] formatArgs)
        {
            Log(target, LogCategory.Message, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, string, object[])"/>
        public void LogWarning(ILogTarget target, string format, params object[] formatArgs)
        {
            Log(target, LogCategory.Warning, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, string, object[])"/>
        public void LogError(ILogTarget target, string format, params object[] formatArgs)
        {
            Log(target, LogCategory.Error, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, string, object[])"/>
        public void LogFatal(ILogTarget target, string format, params object[] formatArgs)
        {
            Log(target, LogCategory.Fatal, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, string, object[])"/>
        public void Log(ILogTarget target, LogLevel category, string format, params object[] formatArgs)
        {
            Log(target, LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, string, object[])"/>
        public void Log(ILogTarget target, LogCategory category, string format, params object[] formatArgs)
        {
            LogUnresolvedTarget(target, category, FormattableStringFactory.Create(format, formatArgs), false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, string, object[])"/>
        public void LogOnce(ILogTarget target, LogCategory category, string format, params object[] formatArgs)
        {
            LogUnresolvedTarget(target, category, FormattableStringFactory.Create(format, formatArgs), true);
        }
        #endregion
        #region  Log Overloads (IEnumerable<ILogTarget>, string, object[])

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, string, object[])"/>
        public void Log(IEnumerable<ILogTarget> targets, string format, params object[] formatArgs)
        {
            Log(targets, LogCategory.Default, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, string, object[])"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, string format, params object[] formatArgs)
        {
            Log(targets, LogCategory.Default, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, string, object[])"/>
        public void LogDebug(IEnumerable<ILogTarget> targets, string format, params object[] formatArgs)
        {
            Log(targets, LogCategory.Debug, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, string, object[])"/>
        public void LogInfo(IEnumerable<ILogTarget> targets, string format, params object[] formatArgs)
        {
            Log(targets, LogCategory.Info, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, string, object[])"/>
        public void LogImportant(IEnumerable<ILogTarget> targets, string format, params object[] formatArgs)
        {
            Log(targets, LogCategory.Important, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, string, object[])"/>
        public void LogMessage(IEnumerable<ILogTarget> targets, string format, params object[] formatArgs)
        {
            Log(targets, LogCategory.Message, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, string, object[])"/>
        public void LogWarning(IEnumerable<ILogTarget> targets, string format, params object[] formatArgs)
        {
            Log(targets, LogCategory.Warning, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, string, object[])"/>
        public void LogError(IEnumerable<ILogTarget> targets, string format, params object[] formatArgs)
        {
            Log(targets, LogCategory.Error, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, string, object[])"/>
        public void LogFatal(IEnumerable<ILogTarget> targets, string format, params object[] formatArgs)
        {
            Log(targets, LogCategory.Fatal, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, string, object[])"/>
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, string format, params object[] formatArgs)
        {
            Log(targets, LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, string, object[])"/>
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, string format, params object[] formatArgs)
        {
            LogBase(new LogTargetCollection(targets), category, FormattableStringFactory.Create(format, formatArgs), false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, string, object[])"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, string format, params object[] formatArgs)
        {
            LogBase(new LogTargetCollection(targets), category, FormattableStringFactory.Create(format, formatArgs), true);
        }
        #endregion

        //These overloads exist to circumvent a null being passed as the params array
        #region Log Overloads (string, string)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(string, string)"/>
        public void Log(string format, string formatArg)
        {
            Log(LogCategory.Default, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(string, string)"/>
        public void LogOnce(string format, string formatArg)
        {
            LogOnce(LogCategory.Default, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(string, string)"/>
        public void LogDebug(string format, string formatArg)
        {
            Log(LogCategory.Debug, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(string, string)"/>
        public void LogInfo(string format, string formatArg)
        {
            Log(LogCategory.Info, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(string, string)"/>
        public void LogImportant(string format, string formatArg)
        {
            Log(LogCategory.Important, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(string, string)"/>
        public void LogMessage(string format, string formatArg)
        {
            Log(LogCategory.Message, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(string, string)"/>
        public void LogWarning(string format, string formatArg)
        {
            Log(LogCategory.Warning, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(string, string)"/>
        public void LogError(string format, string formatArg)
        {
            Log(LogCategory.Error, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(string, string)"/>
        public void LogFatal(string format, string formatArg)
        {
            Log(LogCategory.Fatal, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, string)"/>
        public void Log(LogType category, string format, string formatArg)
        {
            Log(LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, string)"/>
        public void Log(LogLevel category, string format, string formatArg)
        {
            Log(LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, string)"/>
        public void Log(LogCategory category, string format, string formatArg)
        {
            LogBase(category, FormattableStringFactory.Create(format, formatArg), false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, string, string)"/>
        public void LogOnce(LogType category, string format, string formatArg)
        {
            LogOnce(LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, string, string)"/>
        public void LogOnce(LogLevel category, string format, string formatArg)
        {
            LogOnce(LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, string, string)"/>
        public void LogOnce(LogCategory category, string format, string formatArg)
        {
            LogBase(category, FormattableStringFactory.Create(format, formatArg), true);
        }
        #region Rain World Overloads
        #region BepInEx

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(string, string)"/>
        public void LogBepEx(string format, string formatArg)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, FormattableStringFactory.Create(format, formatArg), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, string, string)"/>
        public void LogBepEx(LogLevel category, string format, string formatArg)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArg), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, string, string)"/>
        public void LogBepEx(LogCategory category, string format, string formatArg)
        {
            LogBase(LogID.BepInEx, category, FormattableStringFactory.Create(format, formatArg), false);
        }
        #endregion
        #region Unity

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(string, string)"/>
        public void LogUnity(string format, string formatArg)
        {
            LogBase(LogID.Unity, LogCategory.Default, FormattableStringFactory.Create(format, formatArg), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, string, string)"/>
        public void LogUnity(LogType category, string format, string formatArg)
        {
            LogBase(LogCategory.GetUnityLogID(category), LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArg), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, string, string)"/>
        public void LogUnity(LogCategory category, string format, string formatArg)
        {
            LogBase(LogCategory.GetUnityLogID(category.UnityCategory), category, FormattableStringFactory.Create(format, formatArg), false);
        }
        #endregion
        #region Expedition

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(string, string)"/>
        public void LogExp(string format, string formatArg)
        {
            LogBase(LogID.Expedition, LogCategory.Default, FormattableStringFactory.Create(format, formatArg), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(LogCategory, string, string)"/>
        public void LogExp(LogCategory category, string format, string formatArg)
        {
            LogBase(LogID.Expedition, category, FormattableStringFactory.Create(format, formatArg), false);
        }
        #endregion
        #region JollyCoop

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(string, string)"/>
        public void LogJolly(string format, string formatArg)
        {
            LogBase(LogID.JollyCoop, LogCategory.Default, FormattableStringFactory.Create(format, formatArg), false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(LogCategory, string, string)"/>
        public void LogJolly(LogCategory category, string format, string formatArg)
        {
            LogBase(LogID.JollyCoop, category, FormattableStringFactory.Create(format, formatArg), false);
        }
        #endregion
        #endregion
        #endregion
        #region  Log Overloads (ILogTarget, string, string)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, string, string)"/>
        public void Log(ILogTarget target, string format, string formatArg)
        {
            Log(target, LogCategory.Default, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, string, string)"/>
        public void LogOnce(ILogTarget target, string format, string formatArg)
        {
            Log(target, LogCategory.Default, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, string, string)"/>
        public void LogDebug(ILogTarget target, string format, string formatArg)
        {
            Log(target, LogCategory.Debug, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, string, string)"/>
        public void LogInfo(ILogTarget target, string format, string formatArg)
        {
            Log(target, LogCategory.Info, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, string, string)"/>
        public void LogImportant(ILogTarget target, string format, string formatArg)
        {
            Log(target, LogCategory.Important, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, string, string)"/>
        public void LogMessage(ILogTarget target, string format, string formatArg)
        {
            Log(target, LogCategory.Message, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, string, string)"/>
        public void LogWarning(ILogTarget target, string format, string formatArg)
        {
            Log(target, LogCategory.Warning, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, string, string)"/>
        public void LogError(ILogTarget target, string format, string formatArg)
        {
            Log(target, LogCategory.Error, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, string, string)"/>
        public void LogFatal(ILogTarget target, string format, string formatArg)
        {
            Log(target, LogCategory.Fatal, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, string, string)"/>
        public void Log(ILogTarget target, LogLevel category, string format, string formatArg)
        {
            Log(target, LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, string, string)"/>
        public void Log(ILogTarget target, LogCategory category, string format, string formatArg)
        {
            LogUnresolvedTarget(target, category, FormattableStringFactory.Create(format, formatArg), false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, string, string)"/>
        public void LogOnce(ILogTarget target, LogCategory category, string format, string formatArg)
        {
            LogUnresolvedTarget(target, category, FormattableStringFactory.Create(format, formatArg), true);
        }
        #endregion
        #region  Log Overloads (IEnumerable<ILogTarget>, string, string)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, string, string)"/>
        public void Log(IEnumerable<ILogTarget> targets, string format, string formatArg)
        {
            Log(targets, LogCategory.Default, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, string, string)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, string format, string formatArg)
        {
            Log(targets, LogCategory.Default, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, string, string)"/>
        public void LogDebug(IEnumerable<ILogTarget> targets, string format, string formatArg)
        {
            Log(targets, LogCategory.Debug, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, string, string)"/>
        public void LogInfo(IEnumerable<ILogTarget> targets, string format, string formatArg)
        {
            Log(targets, LogCategory.Info, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, string, string)"/>
        public void LogImportant(IEnumerable<ILogTarget> targets, string format, string formatArg)
        {
            Log(targets, LogCategory.Important, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, string, string)"/>
        public void LogMessage(IEnumerable<ILogTarget> targets, string format, string formatArg)
        {
            Log(targets, LogCategory.Message, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, string, string)"/>
        public void LogWarning(IEnumerable<ILogTarget> targets, string format, string formatArg)
        {
            Log(targets, LogCategory.Warning, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, string, string)"/>
        public void LogError(IEnumerable<ILogTarget> targets, string format, string formatArg)
        {
            Log(targets, LogCategory.Error, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, string, string)"/>
        public void LogFatal(IEnumerable<ILogTarget> targets, string format, string formatArg)
        {
            Log(targets, LogCategory.Fatal, FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, string, string)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, string format, string formatArg)
        {
            Log(targets, LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArg));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, string, string)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, string format, string formatArg)
        {
            LogBase(new LogTargetCollection(targets), category, FormattableStringFactory.Create(format, formatArg), false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, string, string)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, string format, string formatArg)
        {
            LogBase(new LogTargetCollection(targets), category, FormattableStringFactory.Create(format, formatArg), true);
        }
        #endregion
    }

    public static class LoggerExtensions
    {
        /// <inheritdoc cref="LoggerDocs.Standard.Log(string, object[])"/>
        public static void Log(this IFormatLogger logger, string format, params object[] formatArgs)
        {
            logger.Log(LogCategory.Default, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(string, object[])"/>
        public static void LogDebug(this IFormatLogger logger, string format, params object[] formatArgs)
        {
            logger.Log(LogCategory.Debug, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(string, object[])"/>
        public static void LogInfo(this IFormatLogger logger, string format, params object[] formatArgs)
        {
            logger.Log(LogCategory.Info, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(string, object[])"/>
        public static void LogImportant(this IFormatLogger logger, string format, params object[] formatArgs)
        {
            logger.Log(LogCategory.Important, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(string, object[])"/>
        public static void LogMessage(this IFormatLogger logger, string format, params object[] formatArgs)
        {
            logger.Log(LogCategory.Message, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(string, object[])"/>
        public static void LogWarning(this IFormatLogger logger, string format, params object[] formatArgs)
        {
            logger.Log(LogCategory.Warning, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(string, object[])"/>
        public static void LogError(this IFormatLogger logger, string format, params object[] formatArgs)
        {
            logger.Log(LogCategory.Error, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(string, object[])"/>
        public static void LogFatal(this IFormatLogger logger, string format, params object[] formatArgs)
        {
            logger.Log(LogCategory.Fatal, FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, object[])"/>
        public static void Log(this IFormatLogger logger, LogType category, string format, params object[] formatArgs)
        {
            logger.Log(LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, object[])"/>
        public static void Log(this IFormatLogger logger, LogLevel category, string format, params object[] formatArgs)
        {
            logger.Log(LogCategory.ToCategory(category), FormattableStringFactory.Create(format, formatArgs));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, string, object[])"/>
        public static void Log(this IFormatLogger logger, LogCategory category, string format, params object[] formatArgs)
        {
            logger.Log(category, FormattableStringFactory.Create(format, formatArgs));
        }
    }
}
