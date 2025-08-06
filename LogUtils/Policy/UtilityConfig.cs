using BepInEx;
using BepInEx.Configuration;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using ConfigCategory = LogUtils.UtilityConsts.ConfigCategory;
using PolicyNames = LogUtils.UtilityConsts.PolicyNames;

namespace LogUtils.Policy
{
    /// <summary>
    /// A container for utility settings, and user preferences
    /// </summary>
    public sealed class UtilityConfig : ConfigFile
    {
        /// <summary>
        /// Path to the LogUtils core config file
        /// </summary>
        public static readonly string CONFIG_PATH = Path.Combine(Paths.ConfigPath, "LogUtils.cfg");

        internal Dictionary<ConfigDefinition, string> OrphanedEntries;

        internal List<ConfigEntryBase> NewEntries = new List<ConfigEntryBase>();

        private UtilityConfig() : base(CONFIG_PATH, true)
        {
            SaveOnConfigSet = false; //Saving on set causes too many issues

            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            Type baseType = GetType().BaseType;
            OrphanedEntries = (Dictionary<ConfigDefinition, string>)baseType.GetProperty(nameof(OrphanedEntries), flags).GetValue(this);

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

        public new ConfigEntry<T> Bind<T>(ConfigDefinition definition, T defaultValue, ConfigDescription description = null)
        {
            bool hasDefinition = OrphanedEntries.ContainsKey(definition);

            ConfigEntry<T> entry = base.Bind(definition, defaultValue, description);

            if (!hasDefinition)
                NewEntries.Add(entry);
            return entry;
        }

        /// <summary>
        /// Resolves entry data differences between cached config entries and the config file
        /// </summary>
        public void SyncData()
        {
            //TODO: Need a process for handling marked entries for saving on game close
            if (NewEntries.Count == 0) return;

            if (UtilityCore.IsControllingAssembly)
                TrySave();

            NewEntries.Clear(); //Entries will be saved from a different process
        }

        /// <summary>
        /// Process safe method of saving entry values to the config file
        /// </summary>
        public bool TrySave()
        {
            if (TryInvoke(Save))
            {
                UtilityLogger.Log("Config data saved");
                return true;
            }
            UtilityLogger.LogWarning("Unable to save config");
            return false;
        }

        /// <summary>
        /// Process safe method of reading entry values from the config file
        /// </summary>
        public bool TryReload()
        {
            if (TryInvoke(Reload))
            {
                UtilityLogger.Log("Config data read from file");
                return true;
            }
            UtilityLogger.LogWarning("Unable to read config");
            return false;
        }

        internal static bool TryInvoke(Action action)
        {
            /*
             * Read/Write operation may fail due to differing FileShare permissions when this file is accessed from multiple processes.
             * Give a few attempts to retry to improve the chance that save operation will be successful
             */
            int retryCount = 3;
            do
            {
                try
                {
                    action.Invoke();
                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(25);
                    retryCount--;
                }
            }
            while (retryCount > 0);
            return false;
        }
    }
}
