using BepInEx.Configuration;
using System;

namespace LogUtils.Compatibility.BepInEx
{
    /// <summary>
    /// Config values inherited from BepInEx
    /// </summary>
    /// <remarks>Changing these settings may not have an effect on LogUtils functionality</remarks>
    public static class Config
    {
        /// <summary>
        /// Contains the BepInEx Core config instance
        /// </summary>
        public static ConfigFile Base = ConfigFile.CoreConfig;

        /// <summary>
        /// Config setting affecting whether the a console is displayed when BepInEx loads
        /// </summary>
        public static ConfigEntry<bool> AllowConsole;

        /// <summary>
        /// Config setting affecting whether any logs make it to file, error logging excluded
        /// </summary>
        /// <remarks>Needs implementation</remarks>
        public static ConfigEntry<bool> AllowLoggingToFile;

        /// <summary>
        /// Config setting enabling, or disabling Unity logging
        /// </summary>
        /// <remarks>LogUtils provides this functionality through the properties file. Config option is unnecessary</remarks>
        public static ConfigEntry<bool> AllowUnityLogging;

        /// <summary>
        /// Config setting affecting whether the BepInEx log file gets replaced when BepInEx loads
        /// </summary>
        /// <remarks>LogUtils doesn't support this behavior yet</remarks>
        public static ConfigEntry<bool> AppendLogEntries;

        /// <summary>
        /// Legacy config setting affecting the filtering of logged messages based on their LogLevel
        /// </summary>
        /// <remarks>LogUtils provides filter options through other means. This setting has no effect on logging anymore</remarks>
        //public static ConfigEntry<LogLevel> GlobalFilter;

        /// <summary>
        /// Config setting affecting whether Unity logs also appear in the BepInEx output log file
        /// </summary>
        /// <remarks>LogUtils doesn't support this yet</remarks>
        public static ConfigEntry<bool> SendUnityLogsToOutputLog;

        /// <summary>
        /// Config setting affecting whether Unity logs appear in the BepInEx console
        /// </summary>
        //public static ConfigEntry<bool> SendUnityLogsToConsole;

        internal static void InitializeEntries()
        {
            AllowConsole = Bind(new ConfigDefinition("Logging.Console", "Enabled"), false);
            AllowLoggingToFile = Bind(new ConfigDefinition("Logging.Disk", "Enabled"), true);
            AllowUnityLogging = Bind(new ConfigDefinition("Logging", "UnityLogListening"), true);
            AppendLogEntries = Bind(new ConfigDefinition("Logging.Disk", "AppendLog"), true);
            SendUnityLogsToOutputLog = Bind(new ConfigDefinition("Logging.Disk", "WriteUnityLog"), false);
        }

        internal static ConfigEntry<T> Bind<T>(ConfigDefinition key, T defaultValue)
        {
            if (Base.TryGetEntry(key, out ConfigEntry<T> entry))
                return entry;

            UtilityLogger.LogWarning($"Unable to process config entry [{key}]");
            return Base.Bind(key, defaultValue); //Ensure that config bindings are never null
        }

        static Config()
        {
            UtilityCore.EnsureInitializedState();
            try
            {
                InitializeEntries();
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("BepInEx config could not be accessed", ex);
            }
        }
    }
}
