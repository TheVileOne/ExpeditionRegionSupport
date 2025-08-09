using BepInEx.Configuration;
using LogUtils.Enums;
using static LogUtils.UtilityConsts;

namespace LogUtils.Policy
{
    public static class DebugPolicy
    {
        /// <summary>
        /// Enables, or disables LogUtils development build
        /// </summary>
        public static bool DebugMode
        {
            get => UtilityCore.Build == UtilitySetup.Build.DEVELOPMENT;
            set
            {
                UtilityCore.Build = value ? UtilitySetup.Build.DEVELOPMENT : UtilitySetup.Build.RELEASE;

                Config.DebugMode.SetValueSilently(value);
                UpdateAllowConditions();
            }
        }

        /// <summary>
        /// Enables, or disables the LogUtils debug log file
        /// </summary>
        public static bool ShowDebugLog
        {
            get => DebugMode && Config.ShowDebugLog.Value;
            set
            {
                Config.ShowDebugLog.SetValueSilently(value);
                UpdateAllowConditions();
            }
        }

        /// <summary>
        /// Enables, or disables the LogUtils activity log file
        /// </summary>
        public static bool ShowActivityLog
        {
            get => !UtilityLogger.PerformanceMode && DebugMode && Config.ShowActivityLog.Value;
            set
            {
                Config.ShowActivityLog.SetValueSilently(value);
                UpdateAllowConditions();
            }
        }

        /// <summary>
        /// Affects whether test cases apply, or LogUtils based assert statements have an effect
        /// </summary>
        public static bool AssertsEnabled
        {
            get => Config.AssertsEnabled.Value;
            set => Config.AssertsEnabled.SetValueSilently(value);
        }

        /// <summary>
        /// Activate, or deactivate development build specific log files based on several criteria
        /// </summary>
        internal static void UpdateAllowConditions()
        {
            UtilityLogger.DebugLogger.AllowLogging = ShowDebugLog;

            if (UtilitySetup.CurrentStep < UtilitySetup.InitializationStep.INITIALIZE_ENUMS)
                return;

            LogID.FileActivity.Properties.AllowLogging = ShowActivityLog;
        }

        internal static void InitializeEntries()
        {
            Config.InitializeEntries();
        }

#pragma warning disable CS1591 //Missing XML comment for publicly visible type or member
        public static class Config
        {
            public static CachedConfigEntry<bool> AssertsEnabled;
            public static CachedConfigEntry<bool> DebugMode;
            public static CachedConfigEntry<bool> ShowDebugLog;
            public static CachedConfigEntry<bool> ShowActivityLog;

            internal static void InitializeEntries()
            {
                BindEntries();
                AssignEvents();

                DebugPolicy.DebugMode = DebugMode.Value;
                DebugPolicy.ShowDebugLog = ShowDebugLog.Value;
                DebugPolicy.ShowActivityLog = ShowActivityLog.Value;
            }

            internal static void BindEntries()
            {
                UtilityConfig config = UtilityCore.Config;

                DebugMode = config.Bind(
                    new ConfigDefinition(ConfigCategory.Debug, PolicyNames.Debug.Mode), defaultValue: false,
                    new ConfigDescription("Enables development build."));
                ShowDebugLog = config.Bind(
                    new ConfigDefinition(ConfigCategory.Debug, PolicyNames.Debug.ShowDebugLog), defaultValue: false,
                    new ConfigDescription("Activates LogUtils debugging log file. (This file shows additional log information often too sensitive to be handled through a typical log file)."));
                ShowActivityLog = config.Bind(
                    new ConfigDefinition(ConfigCategory.Debug, PolicyNames.Debug.ShowActivityLog), defaultValue: false,
                    new ConfigDescription("Activates LogUtils logging activity log file. (This file shows a record of log file operations)."));
                AssertsEnabled = config.Bind(
                    new ConfigDefinition(ConfigCategory.Asserts, PolicyNames.Testing.AssertsEnabled), defaultValue: true,
                    new ConfigDescription("Affects whether test cases apply, or LogUtils based assert statements have an effect."));
            }

            internal static void AssignEvents()
            {
                DebugMode.ValueChanged += (entry, oldValue) => DebugPolicy.DebugMode = entry.Value;
                ShowDebugLog.ValueChanged += (entry, oldValue) => DebugPolicy.ShowDebugLog = entry.Value;
                ShowActivityLog.ValueChanged += (entry, oldValue) => DebugPolicy.ShowActivityLog = entry.Value;
            }
        }
#pragma warning restore CS1591 //Missing XML comment for publicly visible type or member
    }
}
