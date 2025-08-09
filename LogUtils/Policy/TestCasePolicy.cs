using BepInEx.Configuration;
using LogUtils.Enums;
using static LogUtils.UtilityConsts;
using ReportVerbosity = LogUtils.Enums.FormatEnums.FormatVerbosity;

namespace LogUtils.Policy
{
    public static class TestCasePolicy
    {
        /// <summary>
        /// A flag that affects whether failed expectations qualify as a failure result
        /// </summary>
        public static bool PreferExpectationsAsFailures
        {
            get => Config.PreferExpectationsAsFailures.Value;
            set => Config.PreferExpectationsAsFailures.SetValueSilently(value);
        }

        /// <summary>
        /// A flag that affects whether all failure results are reported, or only the unexpected ones
        /// </summary>
        public static bool FailuresAreAlwaysReported
        {
            get => Config.FailuresAreAlwaysReported.Value;
            set => Config.FailuresAreAlwaysReported.SetValueSilently(value);
        }

        /// <summary>
        /// This field affects the level of detail revealed in the test case report
        /// </summary>
        public static ReportVerbosity ReportVerbosity
        {
            get => Config.ReportVerbosity.Value;
            set => Config.ReportVerbosity.SetValueSilently(value);
        }

        internal static void InitializeEntries()
        {
            Config.InitializeEntries();
        }

#pragma warning disable CS1591 //Missing XML comment for publicly visible type or member
        public static class Config
        {
            public static CachedConfigEntry<bool> PreferExpectationsAsFailures;
            public static CachedConfigEntry<bool> FailuresAreAlwaysReported;
            public static CachedConfigEntry<ReportVerbosity> ReportVerbosity;

            internal static void InitializeEntries()
            {
                BindEntries();
                AssignEvents();
            }

            internal static void BindEntries()
            {
                UtilityConfig config = UtilityCore.Config;

                PreferExpectationsAsFailures = config.Bind(
                    new ConfigDefinition(ConfigCategory.Testing, PolicyNames.Testing.PreferExpectationsAsFailures), defaultValue: true,
                    new ConfigDescription("When a test case has explicitly given expectation conditions, this affects whether expectation conditions can apply as a failed test condition."));
                FailuresAreAlwaysReported = config.Bind(
                    new ConfigDefinition(ConfigCategory.Testing, PolicyNames.Testing.FailuresAreAlwaysReported), defaultValue: false,
                    new ConfigDescription("Affects whether all failed results are reported, or only the unexpected ones."));
                ReportVerbosity = config.Bind(
                    new ConfigDefinition(ConfigCategory.Testing, PolicyNames.Testing.ReportVerbosity), defaultValue: FormatEnums.FormatVerbosity.Standard,
                    new ConfigDescription("Affects the level of detail revealed in the test case report."));
            }

            internal static void AssignEvents()
            {
                //No events are necessary
            }
        }
#pragma warning restore CS1591 //Missing XML comment for publicly visible type or member
    }
}
