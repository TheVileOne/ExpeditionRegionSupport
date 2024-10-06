using System.IO;

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

            path = PathWithoutFilename(path);

            short maxPathChecksAllowed = 3; //The maximum number of containing paths to check
            for (int i = 0; i < maxPathChecksAllowed; i++)
            {
                if (Directory.Exists(path))
                    return true;
                path = Path.GetDirectoryName(path);
            }
            return false;
        }

        /// <summary>
        /// Returns a path string without the filename (filename must have an extension)
        /// </summary>
        public static string PathWithoutFilename(string path)
        {
            return PathWithoutFilename(path, out _);
        }

        /// <summary>
        /// Returns a path string without the filename (filename must have an extension)
        /// </summary>
        public static string PathWithoutFilename(string path, out string filename)
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

            var pathComparer = EqualityComparer.PathComparer;
            string keyword = null;

            if (pathComparer.InternalEquals(path, Paths.StreamingAssetsPath))
                keyword = UtilityConsts.PathKeywords.STREAMING_ASSETS;
            else if (pathComparer.InternalEquals(path, Paths.GameRootPath))
                keyword = UtilityConsts.PathKeywords.ROOT;

            return keyword;
        }

        public static string GetPathFromKeyword(string pathString)
        {
            if (!IsPathKeyword(pathString))
                return PathWithoutFilename(pathString);

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
            return EqualityComparer.PathComparer.Equals(path1, path2);
        }
    }
}
