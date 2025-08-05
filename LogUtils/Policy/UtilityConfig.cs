using BepInEx;
using BepInEx.Configuration;
using LogUtils.Enums;
using System.IO;
using ConfigCategory = LogUtils.UtilityConsts.ConfigCategory;
using PolicyNames = LogUtils.UtilityConsts.PolicyNames;

namespace LogUtils.Policy
{
    /// <summary>
    /// A container for utility settings, and user preferences
    /// </summary>
    public class UtilityConfig : ConfigFile
    {
        /// <summary>
        /// Path to the LogUtils core config file
        /// </summary>
        public static readonly string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "LogUtils.cfg");

        private UtilityConfig() : base(CONFIG_PATH, true)
        {
            BindEntries();
            ReloadValues();
        }

        internal static void Initialize()
        {
            UtilityCore.Config = new UtilityConfig();
        }

        public ConfigEntry<T> GetEntry<T>(string section, string key)
        {
            return (ConfigEntry<T>)this[section, key];
        }

        /// <summary>
        /// Assigns values stored in the config to their associated policy
        /// </summary>
        public void ReloadValues()
        {
            DebugPolicy.DebugMode = GetEntry<bool>(ConfigCategory.Debug, PolicyNames.Debug.Mode).Value;
            DebugPolicy.ShowDebugLog = GetEntry<bool>(ConfigCategory.Debug, PolicyNames.Debug.ShowDebugLog).Value;
            DebugPolicy.ShowActivityLog = GetEntry<bool>(ConfigCategory.Debug, PolicyNames.Debug.ShowActivityLog).Value;

            PatcherPolicy.HasAskedForPermission = GetEntry<bool>(ConfigCategory.Patcher, PolicyNames.Patcher.HasAskedForPermission).Value;
            PatcherPolicy.ShouldDeploy = GetEntry<bool>(ConfigCategory.Patcher, PolicyNames.Patcher.ShouldDeploy).Value;
            PatcherPolicy.ShowPatcherLog = GetEntry<bool>(ConfigCategory.Patcher, PolicyNames.Patcher.ShowPatcherLog).Value;

            TestCasePolicy.PreferExpectationsAsFailures = GetEntry<bool>(ConfigCategory.Testing, PolicyNames.Testing.PreferExpectationsAsFailures).Value;
            TestCasePolicy.FailuresAreAlwaysReported = GetEntry<bool>(ConfigCategory.Testing, PolicyNames.Testing.FailuresAreAlwaysReported).Value;
            TestCasePolicy.ReportVerbosity = GetEntry<FormatEnums.FormatVerbosity>(ConfigCategory.Testing, PolicyNames.Testing.ReportVerbosity).Value;

            DebugPolicy.AssertsEnabled = GetEntry<bool>(ConfigCategory.Asserts, PolicyNames.Testing.AssertsEnabled).Value;
            LogRequestPolicy.ShowRejectionReasons = GetEntry<bool>(ConfigCategory.LogRequests, PolicyNames.LogRequests.ShowRejectionReasons).Value;
        }

        internal void BindEntries()
        {
            //Debug
            Bind(new ConfigDefinition(ConfigCategory.Debug, PolicyNames.Debug.Mode), defaultValue: false,
                     new ConfigDescription("Enables development build."));
            Bind(new ConfigDefinition(ConfigCategory.Debug, PolicyNames.Debug.ShowDebugLog), defaultValue: false,
                     new ConfigDescription("Activates LogUtils debugging log file. (This file shows additional log information often too sensitive to be handled through a typical log file)."));
            Bind(new ConfigDefinition(ConfigCategory.Debug, PolicyNames.Debug.ShowActivityLog), defaultValue: false,
                     new ConfigDescription("Activates LogUtils logging activity log file. (This file shows a record of log file operations)."));

            //Patcher
            Bind(new ConfigDefinition(ConfigCategory.Patcher, PolicyNames.Patcher.HasAskedForPermission), defaultValue: false,
                     new ConfigDescription("Indicates whether user was notified about deploying the VersionLoader."));
            Bind(new ConfigDefinition(ConfigCategory.Patcher, PolicyNames.Patcher.ShouldDeploy), defaultValue: false,
                     new ConfigDescription("Indicates whether user has given permission to deploy the VersionLoader. The VersionLoader cannot be used if this is not true."));
            Bind(new ConfigDefinition(ConfigCategory.Patcher, PolicyNames.Patcher.ShowPatcherLog), defaultValue: true,
                     new ConfigDescription("Indicates whether VersionLoader should maintain a separate dedicated status log file"));

            //Testing
            Bind(new ConfigDefinition(ConfigCategory.Testing, PolicyNames.Testing.PreferExpectationsAsFailures), defaultValue: true,
                     new ConfigDescription("When a test case has explicitly given expectation conditions, this affects whether expectation conditions can apply as a failed test condition."));
            Bind(new ConfigDefinition(ConfigCategory.Testing, PolicyNames.Testing.FailuresAreAlwaysReported), defaultValue: false,
                     new ConfigDescription("Affects whether all failed results are reported, or only the unexpected ones."));
            Bind(new ConfigDefinition(ConfigCategory.Testing, PolicyNames.Testing.ReportVerbosity), defaultValue: FormatEnums.FormatVerbosity.Standard,
                     new ConfigDescription("Affects the level of detail revealed in the test case report."));

            //Testing.Asserts
            Bind(new ConfigDefinition(ConfigCategory.Asserts, PolicyNames.Testing.AssertsEnabled), defaultValue: true,
                     new ConfigDescription("Affects whether test cases apply, or LogUtils based assert statements have an effect."));

            //Logging.Requests
            Bind(new ConfigDefinition(ConfigCategory.LogRequests, PolicyNames.LogRequests.ShowRejectionReasons), defaultValue: false,
                     new ConfigDescription("Log the specific reason a logged message could not be handled."));
        }
    }
}
