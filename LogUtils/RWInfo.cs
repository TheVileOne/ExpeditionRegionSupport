﻿using LogUtils.Enums;
using System.Collections.Generic;

namespace LogUtils
{
    /// <summary>
    /// A static class for storing Rain World associated data
    /// </summary>
    public static class RWInfo
    {
        /// <summary>
        /// A period during which it is safe to evaluate RainWorld.ShowLogs because it is guaranteed to be initialized at this time
        /// </summary>
        public const SetupPeriod SHOW_LOGS_ACTIVE_PERIOD = SetupPeriod.PreMods;

        /// <summary>
        /// The period during which it becomes too late to initiate the startup routine (such as for replacing log files)
        /// </summary>
        public const SetupPeriod STARTUP_CUTOFF_PERIOD = SetupPeriod.ModsInit;

        /// <summary>
        /// Dictionary of last reported errors logged to a specific log file
        /// </summary>
        public static Dictionary<LogID, ExceptionInfo> LastReportedException = new Dictionary<LogID, ExceptionInfo>();

        static RWInfo()
        {
            //Initialize the utility when this class is accessed
            if (!UtilityCore.IsInitialized)
                UtilityCore.Initialize();
        }

        //private static SharedField<SetupPeriod> _latestSetupPeriodReached = UtilityCore.DataHandler.GetField<SetupPeriod>(nameof(LatestSetupPeriodReached));

        /// <summary>
        /// The latest point in the initialization process that Rain World has reached since the application began
        /// </summary>
        public static SetupPeriod LatestSetupPeriodReached = SetupPeriod.Pregame;
        /*{
            get => _latestSetupPeriodReached.Value;
            set => _latestSetupPeriodReached.Value = value;
        }*/

        public static void ReportException(LogID logID, ExceptionInfo exceptionInfo)
        {
            LastReportedException[logID] = exceptionInfo;
        }

        public static bool CheckExceptionMatch(LogID logID, ExceptionInfo exceptionInfo)
        {
            if (LastReportedException.TryGetValue(logID, out ExceptionInfo lastReported))
                return lastReported.Equals(exceptionInfo);
            return false;
        }
    }

    public enum SetupPeriod
    {
        Pregame,
        RWAwake,
        PreMods,
        ModsInit,
        PostMods
    }
}
