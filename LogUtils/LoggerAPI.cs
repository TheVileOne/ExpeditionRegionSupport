using LogUtils.Console;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        public void LogOnce(string category, object messageObj)
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
        public void Log(string category, object messageObj, Color messageColor)
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
        public void LogOnce(string category, object messageObj, Color messageColor)
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
        public void Log(string category, object messageObj, ConsoleColor messageColor)
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
        public void LogOnce(string category, object messageObj, ConsoleColor messageColor)
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
        public void Log(ILogTarget target, string category, object messageObj)
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
        public void Log(ILogTarget target, string category, object messageObj, Color messageColor)
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
        public void Log(ILogTarget target, string category, object messageObj, ConsoleColor messageColor)
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

        #region Log Overloads (FormattableString)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(FormattableString messageObj)
        {
            Log(LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(FormattableString messageObj)
        {
            LogOnce(LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogDebug(FormattableString messageObj)
        {
            Log(LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogInfo(FormattableString messageObj)
        {
            Log(LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogImportant(FormattableString messageObj)
        {
            Log(LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogMessage(FormattableString messageObj)
        {
            Log(LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogWarning(FormattableString messageObj)
        {
            Log(LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogError(FormattableString messageObj)
        {
            Log(LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogFatal(FormattableString messageObj)
        {
            Log(LogCategory.Fatal, messageObj);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(FormattableString messageObj, Color messageColor)
        {
            Log(LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(FormattableString messageObj, Color messageColor)
        {
            LogOnce(LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogDebug(FormattableString messageObj, Color messageColor)
        {
            Log(LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogInfo(FormattableString messageObj, Color messageColor)
        {
            Log(LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogImportant(FormattableString messageObj, Color messageColor)
        {
            Log(LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogMessage(FormattableString messageObj, Color messageColor)
        {
            Log(LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogWarning(FormattableString messageObj, Color messageColor)
        {
            Log(LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogError(FormattableString messageObj, Color messageColor)
        {
            Log(LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogFatal(FormattableString messageObj, Color messageColor)
        {
            Log(LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(FormattableString messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogDebug(FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Debug, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogInfo(FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Info, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogImportant(FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Important, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogMessage(FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Message, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogWarning(FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Warning, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogError(FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Error, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogFatal(FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.Fatal, messageObj, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogType category, FormattableString messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogLevel category, FormattableString messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(string category, FormattableString messageObj)
        {
            Log(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogCategory category, FormattableString messageObj)
        {
            LogBase(category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(LogType category, FormattableString messageObj)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(LogLevel category, FormattableString messageObj)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(string category, FormattableString messageObj)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(LogCategory category, FormattableString messageObj)
        {
            LogBase(category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogType category, FormattableString messageObj, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogLevel category, FormattableString messageObj, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(string category, FormattableString messageObj, Color messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogCategory category, FormattableString messageObj, Color messageColor)
        {
            LogBase(category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(LogType category, FormattableString messageObj, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(LogLevel category, FormattableString messageObj, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(string category, FormattableString messageObj, Color messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(LogCategory category, FormattableString messageObj, Color messageColor)
        {
            LogBase(category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogType category, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogLevel category, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(string category, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(LogCategory category, FormattableString messageObj, ConsoleColor messageColor)
        {
            LogBase(category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(LogType category, FormattableString messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(LogLevel category, FormattableString messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(string category, FormattableString messageObj, ConsoleColor messageColor)
        {
            LogOnce(LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(LogCategory category, FormattableString messageObj, ConsoleColor messageColor)
        {
            LogBase(category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #region Rain World Overloads
        #region BepInEx

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogBepEx(FormattableString messageObj)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogBepEx(LogLevel category, FormattableString messageObj)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogBepEx(LogCategory category, FormattableString messageObj)
        {
            LogBase(LogID.BepInEx, category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogBepEx(FormattableString messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogBepEx(LogLevel category, FormattableString messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogBepEx(LogCategory category, FormattableString messageObj, Color messageColor)
        {
            LogBase(LogID.BepInEx, category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogBepEx(FormattableString messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.Default, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogBepEx(LogLevel category, FormattableString messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, LogCategory.ToCategory(category), messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogBepEx(LogLevel, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogBepEx(LogCategory category, FormattableString messageObj, ConsoleColor messageColor)
        {
            LogBase(LogID.BepInEx, category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #region Unity

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogUnity(FormattableString messageObj)
        {
            LogBase(LogID.Unity, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogUnity(LogType category, FormattableString messageObj)
        {
            LogBase(LogCategory.GetUnityLogID(category), LogCategory.ToCategory(category), messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogUnity(LogType, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogUnity(LogCategory category, FormattableString messageObj)
        {
            LogBase(LogCategory.GetUnityLogID(category.UnityCategory), category, messageObj, false);
        }
        #endregion
        #region Expedition

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogExp(FormattableString messageObj)
        {
            LogBase(LogID.Expedition, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogExp(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogExp(LogCategory category, FormattableString messageObj)
        {
            LogBase(LogID.Expedition, category, messageObj, false);
        }
        #endregion
        #region JollyCoop

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogJolly(FormattableString messageObj)
        {
            LogBase(LogID.JollyCoop, LogCategory.Default, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Game.LogJolly(LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogJolly(LogCategory category, FormattableString messageObj)
        {
            LogBase(LogID.JollyCoop, category, messageObj, false);
        }
        #endregion
        #endregion
        #endregion
        #region  Log Overloads (ILogTarget, FormattableString)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, FormattableString messageObj)
        {
            Log(target, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(ILogTarget target, FormattableString messageObj)
        {
            Log(target, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogDebug(ILogTarget target, FormattableString messageObj)
        {
            Log(target, LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogInfo(ILogTarget target, FormattableString messageObj)
        {
            Log(target, LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogImportant(ILogTarget target, FormattableString messageObj)
        {
            Log(target, LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogMessage(ILogTarget target, FormattableString messageObj)
        {
            Log(target, LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogWarning(ILogTarget target, FormattableString messageObj)
        {
            Log(target, LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogError(ILogTarget target, FormattableString messageObj)
        {
            Log(target, LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogFatal(ILogTarget target, FormattableString messageObj)
        {
            Log(target, LogCategory.Fatal, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, LogLevel category, FormattableString messageObj)
        {
            Log(target, LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, string category, FormattableString messageObj)
        {
            Log(target, LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, LogCategory category, FormattableString messageObj)
        {
            LogUnresolvedTarget(target, category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(ILogTarget target, LogCategory category, FormattableString messageObj)
        {
            LogUnresolvedTarget(target, category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, FormattableString messageObj, Color messageColor)
        {
            Log(target, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(ILogTarget target, FormattableString messageObj, Color messageColor)
        {
            Log(target, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogDebug(ILogTarget target, FormattableString messageObj, Color messageColor)
        {
            Log(target, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogInfo(ILogTarget target, FormattableString messageObj, Color messageColor)
        {
            Log(target, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogImportant(ILogTarget target, FormattableString messageObj, Color messageColor)
        {
            Log(target, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogMessage(ILogTarget target, FormattableString messageObj, Color messageColor)
        {
            Log(target, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogWarning(ILogTarget target, FormattableString messageObj, Color messageColor)
        {
            Log(target, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogError(ILogTarget target, FormattableString messageObj, Color messageColor)
        {
            Log(target, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogFatal(ILogTarget target, FormattableString messageObj, Color messageColor)
        {
            Log(target, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, LogLevel category, FormattableString messageObj, Color messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, string category, FormattableString messageObj, Color messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, LogCategory category, FormattableString messageObj, Color messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(ILogTarget target, LogCategory category, FormattableString messageObj, Color messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(ILogTarget target, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Default, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogDebug(ILogTarget target, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Debug, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogInfo(ILogTarget target, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Info, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogImportant(ILogTarget target, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Important, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogMessage(ILogTarget target, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Message, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogWarning(ILogTarget target, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Warning, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogError(ILogTarget target, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Error, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(ILogTarget, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogFatal(ILogTarget target, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.Fatal, messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, LogLevel category, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, string category, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(target, LogCategory.ToCategory(category), messageObj, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(ILogTarget target, LogCategory category, FormattableString messageObj, ConsoleColor messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(ILogTarget, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(ILogTarget target, LogCategory category, FormattableString messageObj, ConsoleColor messageColor)
        {
            LogUnresolvedTarget(target, category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
        #endregion
        #region  Log Overloads (IEnumerable<ILogTarget>, FormattableString)

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, FormattableString messageObj)
        {
            Log(targets, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(IEnumerable<ILogTarget> targets, FormattableString messageObj)
        {
            Log(targets, LogCategory.Default, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogDebug(IEnumerable<ILogTarget> targets, FormattableString messageObj)
        {
            Log(targets, LogCategory.Debug, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogInfo(IEnumerable<ILogTarget> targets, FormattableString messageObj)
        {
            Log(targets, LogCategory.Info, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogImportant(IEnumerable<ILogTarget> targets, FormattableString messageObj)
        {
            Log(targets, LogCategory.Important, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogMessage(IEnumerable<ILogTarget> targets, FormattableString messageObj)
        {
            Log(targets, LogCategory.Message, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogWarning(IEnumerable<ILogTarget> targets, FormattableString messageObj)
        {
            Log(targets, LogCategory.Warning, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogError(IEnumerable<ILogTarget> targets, FormattableString messageObj)
        {
            Log(targets, LogCategory.Error, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogFatal(IEnumerable<ILogTarget> targets, FormattableString messageObj)
        {
            Log(targets, LogCategory.Fatal, messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, FormattableString messageObj)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, string category, FormattableString messageObj)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, FormattableString messageObj)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, FormattableString messageObj)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true);
        }
        #region Color Overloads

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, FormattableString messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(IEnumerable<ILogTarget> targets, FormattableString messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogDebug(IEnumerable<ILogTarget> targets, FormattableString messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogInfo(IEnumerable<ILogTarget> targets, FormattableString messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogImportant(IEnumerable<ILogTarget> targets, FormattableString messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogMessage(IEnumerable<ILogTarget> targets, FormattableString messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogWarning(IEnumerable<ILogTarget> targets, FormattableString messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogError(IEnumerable<ILogTarget> targets, FormattableString messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogFatal(IEnumerable<ILogTarget> targets, FormattableString messageObj, Color messageColor)
        {
            Log(targets, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, FormattableString messageObj, Color messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, string category, FormattableString messageObj, Color messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, FormattableString messageObj, Color messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, FormattableString messageObj, Color messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(IEnumerable<ILogTarget> targets, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Default, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogDebug(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogDebug(IEnumerable<ILogTarget> targets, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Debug, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogInfo(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogInfo(IEnumerable<ILogTarget> targets, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Info, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogImportant(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogImportant(IEnumerable<ILogTarget> targets, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Important, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogMessage(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogMessage(IEnumerable<ILogTarget> targets, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Message, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogWarning(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogWarning(IEnumerable<ILogTarget> targets, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Warning, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogError(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogError(IEnumerable<ILogTarget> targets, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Error, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogFatal(IEnumerable{ILogTarget}, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogFatal(IEnumerable<ILogTarget> targets, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.Fatal, messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, LogLevel category, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, string category, FormattableString messageObj, ConsoleColor messageColor)
        {
            Log(targets, LogCategory.ToCategory(category), messageObj, messageColor);
        }

        /// <inheritdoc cref="LoggerDocs.Standard.Log(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Log(IEnumerable<ILogTarget> targets, LogCategory category, FormattableString messageObj, ConsoleColor messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, false, ConsoleColorMap.GetColor(messageColor));
        }

        /// <inheritdoc cref="LoggerDocs.Standard.LogOnce(IEnumerable{ILogTarget}, LogCategory, object, Color)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void LogOnce(IEnumerable<ILogTarget> targets, LogCategory category, FormattableString messageObj, ConsoleColor messageColor)
        {
            LogBase(new LogTargetCollection(targets), category, messageObj, true, ConsoleColorMap.GetColor(messageColor));
        }
        #endregion
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
        public void Log(string category, string format, params object[] formatArgs)
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
        public void LogOnce(string category, string format, params object[] formatArgs)
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
        public void Log(ILogTarget target, string category, string format, params object[] formatArgs)
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
        public void Log(IEnumerable<ILogTarget> targets, string category, string format, params object[] formatArgs)
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
    }
}
