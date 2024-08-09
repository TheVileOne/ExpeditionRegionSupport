using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        static RWInfo()
        {
            //Initialize the utility when this class is accessed
            if (!UtilityCore.IsInitialized)
                UtilityCore.Initialize();
        }

        private static SharedField<SetupPeriod> _latestSetupPeriodReached = UtilityCore.DataHandler.GetField<SetupPeriod>(nameof(LatestSetupPeriodReached));

        /// <summary>
        /// The latest point in the initialization process that Rain World has reached since the application began
        /// </summary>
        public static SetupPeriod LatestSetupPeriodReached
        {
            get => _latestSetupPeriodReached.Value;
            set => _latestSetupPeriodReached.Value = value;
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
