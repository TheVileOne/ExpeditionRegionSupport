using LogUtils.Enums;

namespace LogUtils.Policy
{
    public static class DebugPolicy
    {
        private static bool _showDebugLog = true;
        private static bool _showActivityLog = true;

        /// <summary>
        /// Enables, or disables LogUtils development build
        /// </summary>
        public static bool DebugMode
        {
            get => UtilityCore.Build == UtilitySetup.Build.DEVELOPMENT;
            set
            {
                UtilityCore.Build = value ? UtilitySetup.Build.DEVELOPMENT : UtilitySetup.Build.RELEASE;
                UpdateAllowConditions();
            }
        }

        /// <summary>
        /// Enables, or disables the LogUtils debug log file
        /// </summary>
        public static bool ShowDebugLog
        {
            get => DebugMode && _showDebugLog;
            set
            {
                _showDebugLog = value;
                UpdateAllowConditions();
            }
        }

        /// <summary>
        /// Enables, or disables the LogUtils activity log file
        /// </summary>
        public static bool ShowActivityLog
        {
            get => !UtilityLogger.PerformanceMode && DebugMode && _showActivityLog;
            set
            {
                _showActivityLog = value;
                UpdateAllowConditions();
            }
        }

        /// <summary>
        /// Affects whether test cases apply, or LogUtils based assert statements have an effect
        /// </summary>
        public static bool AssertsEnabled = true;

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
    }
}
