using LogUtils.Helpers.FileHandling;
using System.IO;
using System;

namespace LogUtils
{
    public static class TempFolder
    {
        /// <summary>
        /// Signal access for the LogUtils temporary folder
        /// </summary>
        /// <returns>A token for revoking access</returns>
        public static IAccessToken Access()
        {
            IAccessToken token = UtilityCore.TempFolder;
            return token.Access();
        }

        /// <summary>
        /// Signal that access should be revoked for the LogUtils temporary folder
        /// </summary>
        public static void RevokeAccess()
        {
            IAccessToken token = UtilityCore.TempFolder;
            token.RevokeAccess();
        }


        /// <summary>
        /// Creates the directory structure for a given file, or directory path
        /// </summary>
        /// <param name="tempFolder"></param>
        /// <param name="path">A file, or directory path</param>
        /// <returns>The created directory path, or null if path could not be created</returns>
        public static string CreateDirectoryFor(this TempFolderInfo tempFolder, string path)
        {
            string targetPath = getTargetPath(path);
            try
            {
                Directory.CreateDirectory(targetPath);
                return targetPath;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Unable to create directory", ex);
                return null;
            }

            string getTargetPath(string input)
            {
                string targetPath = tempFolder.MapPathToFolder(input); //Does not require path separator trimming

                bool isTargetingTempFolder = PathUtils.PathsAreEqual(targetPath, tempFolder.FullPath);

                if (isTargetingTempFolder)
                    return tempFolder.FullPath;

                //Targets the parent directory of the filename, or directory path provided
                return Path.GetDirectoryName(targetPath);
            }
        }

        /// <summary>
        /// Maps a path, filename, or directory to a location within the Temp folder, and returns the resulting path string
        /// </summary>
        /// <param name="tempFolder"></param>
        /// <param name="path">A path, filename, or directory name to locate</param>
        /// <returns>A fully qualified path inside the Temp folder</returns>
        /// <remarks>No attempt is made to ensure path exists within the Temp folder</remarks>
        public static string MapPathToFolder(this TempFolderInfo tempFolder, string path)
        {
            return tempFolder.Resolver.Resolve(path);
        }
    }
}
