using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LogUtils
{
    public static class PathUtils
    {
        public static string RemoveFileFromPath(string path)
        {
            if (Path.HasExtension(path))
                return Path.GetDirectoryName(path); //Gets the path of the containing directory of a file/directory path
            return path;
        }

        /// <summary>
        /// Converts a placeholder path string into a useable path
        /// </summary>
        public static string ToPath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "customroot")
                return Application.streamingAssetsPath;

            if (path == "root")
                return Path.GetDirectoryName(Application.dataPath);
            return RemoveFileFromPath(path);
        }

        /// <summary>
        /// Converts a path string into a placeholder path string (if applicable)
        /// </summary>
        public static string ToPlaceholderPath(string path)
        {
            if (string.IsNullOrEmpty(path) || ComparePaths(path, Application.streamingAssetsPath))
                return "customroot";

            if (ComparePaths(path, Path.GetDirectoryName(Application.dataPath)))
                return "root";
            return RemoveFileFromPath(path);
        }

        public static bool ComparePaths(string path1, string path2)
        {
            if (path1 == null)
                return path2 == null;

            if (path2 == null)
                return false;

            path1 = Path.GetFullPath(path1).TrimEnd('\\');
            path2 = Path.GetFullPath(path2).TrimEnd('\\');

            var logger = UtilityCore.BaseLogger;

            logger.LogInfo("Comparing paths");

            bool pathsAreEqual = string.Equals(path1, path2, StringComparison.InvariantCultureIgnoreCase);

            if (pathsAreEqual)
                logger.LogInfo("Paths are equal");
            else
            {
                logger.LogInfo("Comparing path: " + path1);
                logger.LogInfo("Comparing path: " + path2);

                logger.LogInfo("Paths are not equal");
            }

            return pathsAreEqual;
        }
    }
}
