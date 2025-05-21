using LogUtils.Helpers.Comparers;
using System;
using System.IO;

namespace LogUtils.Helpers.FileHandling
{
    public static class PathUtils
    {
        /// <summary>
        /// Checks that a directory is contained within a path string
        /// </summary>
        /// <param name="dirName">The directory name to check</param>
        /// <param name="dirLevelsToCheck">The number of directory separators to check starting from the right</param>
        public static bool ContainsDirectory(string path, string dirName, int dirLevelsToCheck)
        {
            if (path == null) return false;

            path = PathWithoutFilename(path);

            bool dirFound = false;
            while (dirLevelsToCheck > 0)
            {
                if (path.EndsWith(dirName, StringComparison.InvariantCultureIgnoreCase))
                {
                    dirFound = true;
                    dirLevelsToCheck = 0;
                }
                else
                {
                    //Keep stripping away directories, 
                    path = Path.GetDirectoryName(path);
                    dirLevelsToCheck--;

                    if (path == string.Empty)
                        dirLevelsToCheck = 0;
                }
            }
            return dirFound;
        }

        /// <summary>
        /// Finds a path string that two provided paths have in common
        /// </summary>
        public static string FindCommonRoot(string path, string pathOther)
        {
            if (!Path.IsPathRooted(path)) //Check that we are dealing with a full path
                path = Path.GetFullPath(path);

            if (!Path.IsPathRooted(pathOther))
                pathOther = Path.GetFullPath(pathOther);

            int charsMatched = 0;
            int charsMatchedThisDir = 0;
            for (charsMatched = 0; charsMatched < Math.Min(path.Length, pathOther.Length); charsMatched++)
            {
                char pathChar = path[charsMatched];
                char pathCharOther = pathOther[charsMatched];

                //Ensure that directory separators can be compared
                if (pathChar == Path.AltDirectorySeparatorChar)
                    pathChar = Path.DirectorySeparatorChar;

                if (pathCharOther == Path.AltDirectorySeparatorChar)
                    pathCharOther = Path.DirectorySeparatorChar;

                if (pathChar == pathCharOther)
                {
                    //Register that another char has been matched for the current directory, or set back to zero for the next directory
                    charsMatchedThisDir = pathChar == Path.DirectorySeparatorChar ? 0 : charsMatchedThisDir + 1;
                }
                else
                {
                    //Chars matched should only include full directory matches - exclude partial matches
                    if (pathChar == Path.DirectorySeparatorChar || pathCharOther == Path.DirectorySeparatorChar)
                        charsMatched -= charsMatchedThisDir;
                    break;
                }
            }

            if (charsMatched <= path.Length)
                return path.Substring(0, charsMatched);
            return pathOther.Substring(0, charsMatched);
        }

        /// <summary>
        /// Replace all directory separator characters with the default platform-specific directory separator character 
        /// </summary>
        public static string Normalize(string path)
        {
            //Path.GetFullPath will replace Path.DirectorySeparatorChar '//' for us, but not Path.Combine
            return path?.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

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
        /// Separates a path into its directory and/or file components
        /// </summary>
        public static string[] Separate(string path)
        {
            if (path == null)
                return Array.Empty<string>();

            //Leading, and trailing separator characters will create misleading results, trim them out
            path = Normalize(path).Trim(Path.DirectorySeparatorChar);

            return path.Split(Path.DirectorySeparatorChar);
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

        public static bool IsEmpty(string path)
        {
            return string.IsNullOrWhiteSpace(path);
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

            var pathComparer = ComparerUtils.PathComparer;
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
        public static bool PathsAreEqual(string path, string pathOther)
        {
            return ComparerUtils.PathComparer.Equals(path, pathOther);
        }
    }
}
