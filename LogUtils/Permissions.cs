using LogUtils.Enums.FileSystem;
using LogUtils.Helpers.FileHandling;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LogUtils
{
    internal static class Permissions
    {
        private static Dictionary<string, FolderPermissions> folderPermissions = new Dictionary<string, FolderPermissions>();

        public static FolderPermissions GetFolderPermissions(string folderPath)
        {
            folderPath = PathUtils.ResolvePath(folderPath);
            if (folderPermissions.TryGetValue(folderPath, out FolderPermissions permissions))
                return permissions;
            return FolderPermissions.None; //The default permissions value
        }

        public static void SetFolderPermissions(string folderPath, FolderPermissions permissions)
        {
            folderPath = PathUtils.ResolvePath(folderPath);
            folderPermissions[folderPath] = permissions;
        }
    }

    /// <summary>
    /// An exception type used when insufficient permission conditions are present
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public sealed class PermissionDeniedException([Optional] string message) : Exception(message ?? "Permission to access has been denied.");
}
