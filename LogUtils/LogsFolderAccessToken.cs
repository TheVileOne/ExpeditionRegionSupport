using System;

namespace LogUtils
{
    public readonly struct LogsFolderAccessToken
    {
        public readonly FolderAccess Access;

        /// <summary>
        /// Contains folder paths considered as familiar to the access token provider (mod)
        /// </summary>
        public readonly string[] AllowedPaths;

        public LogsFolderAccessToken()
        {
            Access = FolderAccess.Unrestricted;
            AllowedPaths = Array.Empty<string>();
        }

        public LogsFolderAccessToken(FolderAccess access, params string[] folderPaths)
        {
            Access = access;
            AllowedPaths = folderPaths;

            if (Access != FolderAccess.Unrestricted && AllowedPaths.Length == 0)
                throw new ArgumentException("At least one folder path must be specified for the requested access setting");
        }
    }

    public enum FolderAccess
    {
        /// <summary>
        /// Folder path changes permitted for any path
        /// </summary>
        Unrestricted = 0,
        /// <summary>
        /// Folder path changes for familiar paths, base paths, but not foreign paths
        /// </summary>
        Normal = 1,
        /// <summary>
        /// Folder path changes for familiar paths only
        /// </summary>
        Strict = 2
    }

    public enum FolderRelationship
    {
        /// <summary>
        /// Represents a null, or invalid path
        /// </summary>
        None = 0,
        /// <summary>
        /// Represents a utility defined LogsFolder path
        /// </summary>
        Base,
        /// <summary>
        /// Represents a folder path recognized by an access token (overwrites Base relationship)
        /// </summary>
        Familiar,
        /// <summary>
        /// Represents a folder path that is unrecognized by an access token, and is not a utility defined LogsFolder path
        /// </summary>
        Foreign
    }
}
