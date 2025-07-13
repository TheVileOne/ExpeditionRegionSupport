using LogUtils.Console;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using LogLevel = BepInEx.Logging.LogLevel;
using LoggerDocs = LogUtils.Documentation.LoggerDocumentation;

namespace LogUtils
{
    public partial class Logger
    {
        #region Log Overloads (object)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        public void Log(object data)
        {
            Log(LogCategory.Default, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object)"/>
        public void LogOnce(object data)
        {
            LogOnce(LogCategory.Default, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        public void LogDebug(object data)
        {
            Log(LogCategory.Debug, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        public void LogInfo(object data)
        {
            Log(LogCategory.Info, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        public void LogImportant(object data)
        {
            Log(LogCategory.Important, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        public void LogMessage(object data)
        {
            Log(LogCategory.Message, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        public void LogWarning(object data)
        {
            Log(LogCategory.Warning, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        public void LogError(object data)
        {
            Log(LogCategory.Error, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        public void LogFatal(object data)
        {
            Log(LogCategory.Fatal, data);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object, Color)"/>
        public void Log(object data, Color messageColor)
        {
            Log(LogCategory.Default, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object, Color)"/>
        public void LogOnce(object data, Color messageColor)
        {
            LogOnce(LogCategory.Default, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object, Color)"/>
        public void LogDebug(object data, Color messageColor)
        {
            Log(LogCategory.Debug, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object, Color)"/>
        public void LogInfo(object data, Color messageColor)
        {
            Log(LogCategory.Info, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object, Color)"/>
        public void LogImportant(object data, Color messageColor)
        {
            Log(LogCategory.Important, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object, Color)"/>
        public void LogMessage(object data, Color messageColor)
        {
            Log(LogCategory.Message, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object, Color)"/>
        public void LogWarning(object data, Color messageColor)
        {
            Log(LogCategory.Warning, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object, Color)"/>
        public void LogError(object data, Color messageColor)
        {
            Log(LogCategory.Error, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object, Color)"/>
        public void LogFatal(object data, Color messageColor)
        {
            Log(LogCategory.Fatal, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object, Color)"/>
        public void Log(object data, ConsoleColor messageColor)
        {
            Log(LogCategory.Default, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object, Color)"/>
        public void LogOnce(object data, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.Default, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object, Color)"/>
        public void LogDebug(object data, ConsoleColor messageColor)
        {
            Log(LogCategory.Debug, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object, Color)"/>
        public void LogInfo(object data, ConsoleColor messageColor)
        {
            Log(LogCategory.Info, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object, Color)"/>
        public void LogImportant(object data, ConsoleColor messageColor)
        {
            Log(LogCategory.Important, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object, Color)"/>
        public void LogMessage(object data, ConsoleColor messageColor)
        {
            Log(LogCategory.Message, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object, Color)"/>
        public void LogWarning(object data, ConsoleColor messageColor)
        {
            Log(LogCategory.Warning, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object, Color)"/>
        public void LogError(object data, ConsoleColor messageColor)
        {
            Log(LogCategory.Error, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object, Color)"/>
        public void LogFatal(object data, ConsoleColor messageColor)
        {
            Log(LogCategory.Fatal, data, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogType category, object data)
        {
            Log(LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogLevel category, object data)
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
            LogData(Targets, category, data, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        public void LogOnce(LogType category, object data)
        {
            LogOnce(LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        public void LogOnce(LogLevel category, object data)
        {
            LogOnce(LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        public void LogOnce(string category, object data)
        {
            LogOnce(LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        public void LogOnce(LogCategory category, object data)
        {
            LogData(Targets, category, data, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogType category, object data, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogLevel category, object data, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(string category, object data, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogCategory category, object data, Color messageColor)
        {
            LogData(Targets, category, data, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void LogOnce(LogType category, object data, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogLevel category, object data, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(string category, object data, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogCategory category, object data, Color messageColor)
        {
            LogData(Targets, category, data, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogType category, object data, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogLevel category, object data, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(string category, object data, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogCategory category, object data, ConsoleColor messageColor)
        {
            LogData(Targets, category, data, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void LogOnce(LogType category, object data, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogLevel category, object data, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(string category, object data, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogCategory category, object data, ConsoleColor messageColor)
        {
            LogData(Targets, category, data, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #region Rain World Overloads
        #region BepInEx

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(object)"/>
        public void LogBepEx(object data)
        {
            LogData(LogID.BepInEx, LogCategory.Default, data, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object)"/>
        public void LogBepEx(LogLevel category, object data)
        {
            LogData(LogID.BepInEx, LogCategory.ToCategory(category), data, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object)"/>
        public void LogBepEx(LogCategory category, object data)
        {
            LogData(LogID.BepInEx, category, data, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(object data, Color messageColor)
        {
            LogData(LogID.BepInEx, LogCategory.Default, data, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogLevel category, object data, Color messageColor)
        {
            LogData(LogID.BepInEx, LogCategory.ToCategory(category), data, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogCategory category, object data, Color messageColor)
        {
            LogData(LogID.BepInEx, category, data, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(object data, ConsoleColor messageColor)
        {
            LogData(LogID.BepInEx, LogCategory.Default, data, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogLevel category, object data, ConsoleColor messageColor)
        {
            LogData(LogID.BepInEx, LogCategory.ToCategory(category), data, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        public void LogBepEx(LogCategory category, object data, ConsoleColor messageColor)
        {
            LogData(LogID.BepInEx, category, data, false, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #region Unity

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(object)"/>
        public void LogUnity(object data)
        {
            LogData(LogID.Unity, LogCategory.Default, data, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        public void LogUnity(LogType category, object data)
        {
            LogData(LogCategory.GetUnityLogID(category), LogCategory.ToCategory(category), data, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        public void LogUnity(LogCategory category, object data)
        {
            LogData(LogCategory.GetUnityLogID(category.UnityCategory), category, data, false);
        }
        #endregion
        #region Expedition

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(object)"/>
        public void LogExp(object data)
        {
            LogData(LogID.Expedition, LogCategory.Default, data, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(LogCategory, object)"/>
        public void LogExp(LogCategory category, object data)
        {
            LogData(LogID.Expedition, category, data, false);
        }
        #endregion
        #region JollyCoop

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(object)"/>
        public void LogJolly(object data)
        {
            LogData(LogID.JollyCoop, LogCategory.Default, data, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(LogCategory, object)"/>
        public void LogJolly(LogCategory category, object data)
        {
            LogData(LogID.JollyCoop, category, data, false);
        }
        #endregion
        #endregion
        #endregion
        #region  Log Overloads (ILogTarget, object)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object)"/>
        public void Log(ILogTarget target, object data)
        {
            Log(target, LogCategory.Default, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object)"/>
        public void LogOnce(ILogTarget target, object data)
        {
            Log(target, LogCategory.Default, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object)"/>
        public void LogDebug(ILogTarget target, object data)
        {
            Log(target, LogCategory.Debug, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object)"/>
        public void LogInfo(ILogTarget target, object data)
        {
            Log(target, LogCategory.Info, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object)"/>
        public void LogImportant(ILogTarget target, object data)
        {
            Log(target, LogCategory.Important, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object)"/>
        public void LogMessage(ILogTarget target, object data)
        {
            Log(target, LogCategory.Message, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object)"/>
        public void LogWarning(ILogTarget target, object data)
        {
            Log(target, LogCategory.Warning, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object)"/>
        public void LogError(ILogTarget target, object data)
        {
            Log(target, LogCategory.Error, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object)"/>
        public void LogFatal(ILogTarget target, object data)
        {
            Log(target, LogCategory.Fatal, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        public void Log(ILogTarget target, LogLevel category, object data)
        {
            Log(target, LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        public void Log(ILogTarget target, string category, object data)
        {
            Log(target, LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        public void Log(ILogTarget target, LogCategory category, object data)
        {
            LogData(target, category, data, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object)"/>
        public void LogOnce(ILogTarget target, LogCategory category, object data)
        {
            LogData(target, category, data, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object, Color)"/>
        public void Log(ILogTarget target, object data, Color messageColor)
        {
            Log(target, LogCategory.Default, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object, Color)"/>
        public void LogOnce(ILogTarget target, object data, Color messageColor)
        {
            Log(target, LogCategory.Default, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object, Color)"/>
        public void LogDebug(ILogTarget target, object data, Color messageColor)
        {
            Log(target, LogCategory.Debug, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object, Color)"/>
        public void LogInfo(ILogTarget target, object data, Color messageColor)
        {
            Log(target, LogCategory.Info, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object, Color)"/>
        public void LogImportant(ILogTarget target, object data, Color messageColor)
        {
            Log(target, LogCategory.Important, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object, Color)"/>
        public void LogMessage(ILogTarget target, object data, Color messageColor)
        {
            Log(target, LogCategory.Message, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object, Color)"/>
        public void LogWarning(ILogTarget target, object data, Color messageColor)
        {
            Log(target, LogCategory.Warning, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object, Color)"/>
        public void LogError(ILogTarget target, object data, Color messageColor)
        {
            Log(target, LogCategory.Error, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object, Color)"/>
        public void LogFatal(ILogTarget target, object data, Color messageColor)
        {
            Log(target, LogCategory.Fatal, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, LogLevel category, object data, Color messageColor)
        {
            Log(target, LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, string category, object data, Color messageColor)
        {
            Log(target, LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, LogCategory category, object data, Color messageColor)
        {
            LogData(target, category, data, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        public void LogOnce(ILogTarget target, LogCategory category, object data, Color messageColor)
        {
            LogData(target, category, data, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object, Color)"/>
        public void Log(ILogTarget target, object data, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Default, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object, Color)"/>
        public void LogOnce(ILogTarget target, object data, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Default, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object, Color)"/>
        public void LogDebug(ILogTarget target, object data, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Debug, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object, Color)"/>
        public void LogInfo(ILogTarget target, object data, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Info, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object, Color)"/>
        public void LogImportant(ILogTarget target, object data, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Important, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object, Color)"/>
        public void LogMessage(ILogTarget target, object data, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Message, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object, Color)"/>
        public void LogWarning(ILogTarget target, object data, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Warning, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object, Color)"/>
        public void LogError(ILogTarget target, object data, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Error, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object, Color)"/>
        public void LogFatal(ILogTarget target, object data, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Fatal, data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, LogLevel category, object data, ConsoleColor messageColor)
        {
            Log(target, LogCategory.ToCategory(category), data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, string category, object data, ConsoleColor messageColor)
        {
            Log(target, LogCategory.ToCategory(category), data, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, LogCategory category, object data, ConsoleColor messageColor)
        {
            LogData(target, category, data, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        public void LogOnce(ILogTarget target, LogCategory category, object data, ConsoleColor messageColor)
        {
            LogData(target, category, data, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #endregion
        #region  Log Overloads (IEnumerable<ILogTarget>, object)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object)"/>
        public void Log(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Default, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Default, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object)"/>
        public void LogDebug(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Debug, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object)"/>
        public void LogInfo(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Info, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object)"/>
        public void LogImportant(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Important, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object)"/>
        public void LogMessage(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Message, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object)"/>
        public void LogWarning(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Warning, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object)"/>
        public void LogError(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Error, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object)"/>
        public void LogFatal(IEnumerable<ILogTarget> targets, object data)
        {
            Log(targets, LogCategory.Fatal, data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, object data)
        {
            Log(targets, LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        public void Log(IEnumerable<ILogTarget> targets, string category, object data)
        {
            Log(targets, LogCategory.ToCategory(category), data);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, object data)
        {
            LogData(new LogTargetCollection(targets), category, data, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, object data)
        {
            LogData(new LogTargetCollection(targets), category, data, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, object data, Color messageColor)
        {
            Log(targets, LogCategory.Default, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, object data, Color messageColor)
        {
            Log(targets, LogCategory.Default, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogDebug(IEnumerable<ILogTarget> targets, object data, Color messageColor)
        {
            Log(targets, LogCategory.Debug, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogInfo(IEnumerable<ILogTarget> targets, object data, Color messageColor)
        {
            Log(targets, LogCategory.Info, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogImportant(IEnumerable<ILogTarget> targets, object data, Color messageColor)
        {
            Log(targets, LogCategory.Important, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogMessage(IEnumerable<ILogTarget> targets, object data, Color messageColor)
        {
            Log(targets, LogCategory.Message, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogWarning(IEnumerable<ILogTarget> targets, object data, Color messageColor)
        {
            Log(targets, LogCategory.Warning, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogError(IEnumerable<ILogTarget> targets, object data, Color messageColor)
        {
            Log(targets, LogCategory.Error, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogFatal(IEnumerable<ILogTarget> targets, object data, Color messageColor)
        {
            Log(targets, LogCategory.Fatal, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, object data, Color messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, string category, object data, Color messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, object data, Color messageColor)
        {
            LogData(new LogTargetCollection(targets), category, data, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, object data, Color messageColor)
        {
            LogData(new LogTargetCollection(targets), category, data, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, object data, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Default, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, object data, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Default, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogDebug(IEnumerable<ILogTarget> targets, object data, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Debug, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogInfo(IEnumerable<ILogTarget> targets, object data, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Info, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogImportant(IEnumerable<ILogTarget> targets, object data, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Important, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogMessage(IEnumerable<ILogTarget> targets, object data, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Message, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogWarning(IEnumerable<ILogTarget> targets, object data, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Warning, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogError(IEnumerable<ILogTarget> targets, object data, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Error, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object, Color)"/>
        public void LogFatal(IEnumerable<ILogTarget> targets, object data, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Fatal, data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, object data, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, string category, object data, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), data, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, object data, ConsoleColor messageColor)
        {
            LogData(new LogTargetCollection(targets), category, data, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, object data, ConsoleColor messageColor)
        {
            LogData(new LogTargetCollection(targets), category, data, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #endregion
    }
}
