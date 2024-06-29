using Expedition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExpeditionRegionSupport.Data.Logging
{
    public static class LogUtils
    {
        /// <summary>
        /// Logs to both mod-specific logger, and ExpLog
        /// </summary>
        /// <param name="entry"></param>
        public static void LogBoth(string entry)
        {
            ExpLog.Log(entry);
            Plugin.Logger.LogInfo(entry);
        }

        public static LogID GetLogID(string value)
        {
            LogID found = null;
            if (ExtEnumBase.TryParse(typeof(LogID), value, true, out ExtEnumBase extBase))
                found = (LogID)extBase;
            return found;
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
            return path;
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
            return path;
        }

        public static bool ComparePaths(string path1, string path2)
        {
            if (path1 == null)
                return path2 == null;

            if (path2 == null)
                return false;

            path1 = Path.GetFullPath(path1).TrimEnd('\\');
            path2 = Path.GetFullPath(path2).TrimEnd('\\');

            Debug.Log("Comparing paths");

            bool pathsAreEqual = string.Equals(path1, path2, StringComparison.InvariantCultureIgnoreCase);

            if (pathsAreEqual)
                Debug.Log("Paths are equal");
            else
            {
                Debug.Log("Comparing path: " + path1);
                Debug.Log("Comparing path: " + path2);

                Debug.Log("Paths are not equal");
            }

            return pathsAreEqual;
        }
    }
}
