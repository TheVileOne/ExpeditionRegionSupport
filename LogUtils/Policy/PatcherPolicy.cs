using BepInEx.Configuration;
using static LogUtils.UtilityConsts;

namespace LogUtils.Policy
{
    /// <summary>
    /// Global LogUtils related setting flags pertaining to Patcher behavior
    /// </summary>
    public static class PatcherPolicy
    {
        /// <summary>
        /// Indicates whether the user should be prompted for permission to deploy the patcher
        /// </summary>
        public static bool HasAskedForPermission
        {
            get => Config.HasAskedForPermission.Value;
            set => Config.HasAskedForPermission.SetValueSilently(value);
        }

        /// <summary>
        /// Indicates that the patcher is able to be deployed
        /// </summary>
        public static bool ShouldDeploy
        {
            get => Config.ShouldDeploy.Value;
            set => Config.ShouldDeploy.SetValueSilently(value);
        }

        /// <summary>
        /// Indicates that extra information should be provided in a separate log file
        /// </summary>
        public static bool ShowPatcherLog
        {
            get => Config.ShowPatcherLog.Value;
            set => Config.ShowPatcherLog.SetValueSilently(value);
        }

        internal static void InitializeEntries()
        {
            Config.InitializeEntries();
        }

#pragma warning disable CS1591 //Missing XML comment for publicly visible type or member
        public static class Config
        {
            public static CachedConfigEntry<bool> HasAskedForPermission;
            public static CachedConfigEntry<bool> ShouldDeploy;
            public static CachedConfigEntry<bool> ShowPatcherLog;

            internal static void InitializeEntries()
            {
                BindEntries();
                AssignEvents();
            }

            internal static void BindEntries()
            {
                UtilityConfig config = UtilityCore.Config;

                HasAskedForPermission = config.Bind(
                    new ConfigDefinition(ConfigCategory.Patcher, PolicyNames.Patcher.HasAskedForPermission), defaultValue: false,
                    new ConfigDescription("Indicates whether user was notified about deploying the VersionLoader."));
                ShouldDeploy = config.Bind(
                    new ConfigDefinition(ConfigCategory.Patcher, PolicyNames.Patcher.ShouldDeploy), defaultValue: false,
                    new ConfigDescription("Indicates whether user has given permission to deploy the VersionLoader. The VersionLoader cannot be used if this is not true."));
                ShowPatcherLog = config.Bind(
                    new ConfigDefinition(ConfigCategory.Patcher, PolicyNames.Patcher.ShowPatcherLog), defaultValue: true,
                    new ConfigDescription("Indicates whether VersionLoader should maintain a separate dedicated status log file"));
            }

            internal static void AssignEvents()
            {
                //No events are necessary
            }
        }
#pragma warning restore CS1591 //Missing XML comment for publicly visible type or member
    }
}
