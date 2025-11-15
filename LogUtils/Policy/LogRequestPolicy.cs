using BepInEx.Configuration;
using LogUtils.Requests;
using static LogUtils.UtilityConsts;

namespace LogUtils.Policy
{
    public static class LogRequestPolicy
    {
        /// <summary>
        /// A flag that affects whether a detected <see cref="RejectionReason"/> is logged to file when they occur
        /// </summary>
        public static bool ShowRejectionReasons
        {
            get => Config.ShowRejectionReasons.Value;
            set => Config.ShowRejectionReasons.SetValueSilently(value);
        }

        internal static void InitializeEntries()
        {
            Config.InitializeEntries();
        }

#pragma warning disable CS1591 //Missing XML comment for publicly visible type or member
        public static class Config
        {
            public static CachedConfigEntry<bool> ShowRejectionReasons;

            internal static void InitializeEntries()
            {
                BindEntries();
                AssignEvents();
            }

            internal static void BindEntries()
            {
                UtilityConfig config = UtilityCore.Config;

                ShowRejectionReasons = config.Bind(
                    new ConfigDefinition(ConfigCategory.LogRequests, PolicyNames.LogRequests.ShowRejectionReasons), defaultValue: false,
                    new ConfigDescription("Log the specific reason a logged message could not be handled."));
            }

            internal static void AssignEvents()
            {
                //No events are necessary
            }
        }
#pragma warning restore CS1591 //Missing XML comment for publicly visible type or member
    }
}
