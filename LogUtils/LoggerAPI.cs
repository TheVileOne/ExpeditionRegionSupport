using LogUtils.Console;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using LoggerDocs = LogUtils.Documentation.LoggerDocumentation;
using LogLevel = BepInEx.Logging.LogLevel;

namespace LogUtils
{
    public partial class Logger
    {
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
        public void Log(string category, object messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        public void Log(LogCategory category, object messageObj)
        {
            LogBase(Targets, category, messageObj, false);
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
        public void LogOnce(string category, object messageObj)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        public void LogOnce(LogCategory category, object messageObj)
        {
            LogBase(Targets, category, messageObj, true);
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
        public void Log(string category, object messageObj, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogCategory category, object messageObj, Color messageColor)
        {
            LogBase(Targets, category, messageObj, false, messageColor);
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
        public void LogOnce(string category, object messageObj, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogCategory category, object messageObj, Color messageColor)
        {
            LogBase(Targets, category, messageObj, true, messageColor);
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
        public void Log(string category, object messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        public void Log(LogCategory category, object messageObj, ConsoleColor messageColor)
        {
            LogBase(Targets, category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
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
        public void LogOnce(string category, object messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        public void LogOnce(LogCategory category, object messageObj, ConsoleColor messageColor)
        {
            LogBase(Targets, category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
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
        public void Log(ILogTarget target, string category, object messageObj)
        {
            Log(target, LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        public void Log(ILogTarget target, LogCategory category, object messageObj)
        {
            LogInternal(target, category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object)"/>
        public void LogOnce(ILogTarget target, LogCategory category, object messageObj)
        {
            LogInternal(target, category, messageObj, true);
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
        public void Log(ILogTarget target, string category, object messageObj, Color messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, LogCategory category, object messageObj, Color messageColor)
        {
            LogInternal(target, category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        public void LogOnce(ILogTarget target, LogCategory category, object messageObj, Color messageColor)
        {
            LogInternal(target, category, messageObj, true, messageColor);
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
        public void Log(ILogTarget target, string category, object messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        public void Log(ILogTarget target, LogCategory category, object messageObj, ConsoleColor messageColor)
        {
            LogInternal(target, category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        public void LogOnce(ILogTarget target, LogCategory category, object messageObj, ConsoleColor messageColor)
        {
            LogInternal(target, category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
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
        public void Log(IEnumerable<ILogTarget> targets, string category, object messageObj)
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
        public void Log(IEnumerable<ILogTarget> targets, string category, object messageObj, Color messageColor)
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
        public void Log(IEnumerable<ILogTarget> targets, string category, object messageObj, ConsoleColor messageColor)
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
    }
}
