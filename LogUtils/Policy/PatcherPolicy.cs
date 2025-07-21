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
        public static bool HasAskedForPermission;

        /// <summary>
        /// Indicates that the patcher is able to be deployed
        /// </summary>
        public static bool ShouldDeploy;

        /// <summary>
        /// Indicates that extra information should be provided in a separate log file
        /// </summary>
        public static bool ShowPatcherLog;
    }
}
