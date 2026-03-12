namespace LogUtils
{
    public static class TempFolder
    {
        private static IAccessToken accessToken => UtilityCore.TempFolder;

        /// <summary>
        /// Signal access for the LogUtils temporary folder
        /// </summary>
        /// <returns>A token for revoking access</returns>
        public static IAccessToken Access()
        {
            return accessToken.Access();
        }

        /// <summary>
        /// Signal that access should be revoked for the LogUtils temporary folder
        /// </summary>
        public static void RevokeAccess()
        {
            accessToken.RevokeAccess();
        }
    }
}
