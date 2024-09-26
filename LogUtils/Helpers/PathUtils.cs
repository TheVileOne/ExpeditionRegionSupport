using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogUtils.Helpers
{
    public static class PathUtils
    {
        /// <summary>
        /// Checks that some portion of a path exists
        /// </summary>
        public static bool PathRootExists(string path)
        {
            if (path == null) return false;

            path = RemoveFileFromPath(path);

            short maxPathChecksAllowed = 3; //The maximum number of containing paths to check
            for (int i = 0; i < maxPathChecksAllowed; i++)
            {
                if (Directory.Exists(path))
                    return true;
                path = Path.GetDirectoryName(path);
            }
            return false;
        }

        public static string RemoveFileFromPath(string path)
        {
            return RemoveFileFromPath(path, out _);
        }

        public static string RemoveFileFromPath(string path, out string filename)
        {
            filename = null;
            if (Path.HasExtension(path))
            {
                filename = Path.GetFileName(path);
                return Path.GetDirectoryName(path); //Gets the path of the containing directory of a file/directory path
            }
            return path;
        }

        /// <summary>
        /// Check for a path root, and strip it from the path
        /// </summary>
        public static string Unroot(string path)
        {
            if (Path.IsPathRooted(path))
            {
                if (path.Length > 2 && path[1] == Path.VolumeSeparatorChar) //Path begins with a drive map
                    return path.Substring(2);

                return path.Substring(1);
            }
            return path;
        }

        public static bool IsPathKeyword(string pathString)
        {
            if (pathString == null) return false;

            pathString = pathString.ToLower();

            switch (pathString)
            {
                case UtilityConsts.PathKeywords.ROOT:
                case UtilityConsts.PathKeywords.STREAMING_ASSETS:
                    return true;
            }
            return false;
        }

        public static string GetPathKeyword(string path)
        {
            if (IsPathKeyword(path))
                return path.ToLower();

            string keyword = null;

            if (InternalPathsAreEqual(path, Paths.StreamingAssetsPath))
                keyword = UtilityConsts.PathKeywords.STREAMING_ASSETS;
            else if (InternalPathsAreEqual(path, Paths.GameRootPath))
                keyword = UtilityConsts.PathKeywords.ROOT;

            return keyword;
        }

        public static string GetPathFromKeyword(string pathString)
        {
            if (!IsPathKeyword(pathString))
                return RemoveFileFromPath(pathString);

            pathString = pathString.ToLower();

            switch (pathString)
            {
                case UtilityConsts.PathKeywords.ROOT:
                    pathString = Paths.GameRootPath;
                    break;
                case UtilityConsts.PathKeywords.STREAMING_ASSETS:
                    pathString = Paths.StreamingAssetsPath;
                    break;
            }
            return pathString;
        }

        /// <summary>
        /// Evaluates whether paths are logically equivalent
        /// </summary>
        public static bool PathsAreEqual(string path1, string path2)
        {
            //Make sure we are comparing path data, not keywords
            path1 = GetPathFromKeyword(path1);
            path2 = GetPathFromKeyword(path2);

            return InternalPathsAreEqual(path1, path2);
        }

        internal static bool InternalPathsAreEqual(string path1, string path2)
        {
            FileUtils.WriteLine("test.txt", "Checking path equality " + path1 + " " + path2);

            if (path1 == null)
                return path2 == null;

            if (path2 == null)
                return false;

            path1 = Path.GetFullPath(path1).TrimEnd('\\');
            path2 = Path.GetFullPath(path2).TrimEnd('\\');

            return string.Equals(path1, path2, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
