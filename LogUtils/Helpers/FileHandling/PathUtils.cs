using LogUtils.Helpers.Comparers;
using System;
using System.IO;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils.Helpers.FileHandling
{
    public static class PathUtils
    {
        /// <summary>
        /// The length of a Windows path volume
        /// </summary>
        public const int PATH_VOLUME_LENGTH = 3;

        /// <inheritdoc cref="DirectoryUtils.ContainsDirectory(string, string)"/>
        public static bool ContainsDirectory(string path, string dirName) => DirectoryUtils.ContainsDirectory(path, dirName);

        /// <inheritdoc cref="DirectoryUtils.ContainsDirectory(string, string, int)"/>
        public static bool ContainsDirectory(string path, string dirName, int dirLevelsToCheck) => DirectoryUtils.ContainsDirectory(path, dirName, dirLevelsToCheck);

        /// <summary>
        /// Checks the second path is contained with the first path
        /// </summary>
        public static bool ContainsOtherPath(string path, string pathOther)
        {
            path = Path.GetFullPath(path);
            pathOther = Path.GetFullPath(pathOther);

            return path.StartsWith(pathOther, StringComparison.InvariantCultureIgnoreCase);
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
            return FindCommonRootNoChecks(path, pathOther);
        }

        internal static string FindCommonRootNoChecks(string path, string pathOther)
        {
            int charsMatched, charsMatchedThisDir = 0;
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

        public static string GetRandomFilename(string fileExt)
        {
            string filename = Path.GetRandomFileName();
            return Path.ChangeExtension(filename, fileExt);
        }

        /// <summary>
        /// Converts a partial, or non-partial path into a fully qualified absolute path
        /// </summary>
        /// <remarks>Supports files and directories</remarks>
        public static string ResolvePath(string path)
        {
            if (IsEmpty(path))
                return RainWorldPath.StreamingAssetsPath;

            if (tryExpandPath(ref path))
                return path;

            string result = RainWorldDirectory.Locate(path);

            if (result != null)
                return result;
            return Path.Combine(RainWorldPath.StreamingAssetsPath, path); //Unrecognized partial paths default to StreamingAssets
        }

        private static bool tryExpandPath(ref string path)
        {
            bool isExpanded = false;

            //Expand if path is a relative path, or lacks drive information
            if (path[0] == '.' || path[0] == Path.DirectorySeparatorChar || path[0] == Path.AltDirectorySeparatorChar)
            {
                path = Path.GetFullPath(path);
                isExpanded = true;
            }

            isExpanded |= Path.IsPathRooted(path);
            return isExpanded;
        }

        /// <summary>
        /// Gets the length of any relative, or root path information at the start of the path string
        /// </summary>
        public static int GetPrefixLength(string path)
        {
            if (IsEmpty(path))
                return path == null ? 0 : path.Length;

            if (IsAbsolute(path))
                return PATH_VOLUME_LENGTH;

            int matchCount = 0;
            while (matchCount < path.Length)
            {
                char c = path[matchCount];
                if (c == '.' || c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
                {
                    matchCount++;
                    continue;
                }
                break;
            }
            return matchCount;
        }

        /// <summary>
        /// Replace all directory separator characters with the default platform-specific directory separator character 
        /// </summary>
        public static string Normalize(string path)
        {
            if (IsEmpty(path))
                return path;

            //TODO: Decide if this should trim trailing separator characters
            //Path.GetFullPath will replace Path.DirectorySeparatorChar '//' for us, but not Path.Combine
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Removes any trailing directory separator characters
        /// </summary>
        public static string Trim(string path)
        {
            return path?.TrimEnd(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
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
            if (Path.HasExtension(path)) //TODO: This needs to use the FileExtension helper class to avoid it detecting folder paths with periods
            {
                filename = Path.GetFileName(path);
                return Path.GetDirectoryName(path); //Gets the path of the containing directory of a file/directory path
            }
            return path;
        }

        /// <summary>
        /// Prepends a single directory separator character to a given path string
        /// </summary>
        /// <remarks>This method will remove any existing separator characters at the start of the path</remarks>
        public static string PrependWithSeparator(string path)
        {
            //Remove any existing separators, so that we don't have more than one
            return Path.DirectorySeparatorChar + path.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <summary>
        /// Separates a path into its directory and/or file components
        /// </summary>
        public static string[] SplitPath(string path)
        {
            if (IsEmpty(path))
                return Array.Empty<string>();

            char[] separators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

            int prefixLength = GetPrefixLength(path);

            if (prefixLength > 0)
                path = path.Substring(prefixLength);

            return path.TrimEnd(separators) //Trimming avoids empty data making it into the results
                       .Split(separators);
        }

        /// <summary>
        /// Check for a path root, and strip it from the path
        /// </summary>
        public static string Unroot(string path)
        {
            if (Path.IsPathRooted(path))
            {
                if (path.Length >= PATH_VOLUME_LENGTH && path[1] == Path.VolumeSeparatorChar) //Path begins with a drive map
                    return path.Substring(PATH_VOLUME_LENGTH);

                return path.Substring(1);
            }
            return path;
        }

        /// <summary>
        /// Checks that the path given contains drive letter information
        /// </summary>
        public static bool IsAbsolute(string path)
        {
            return Path.IsPathRooted(path) && path[0] != Path.DirectorySeparatorChar && path[0] != Path.AltDirectorySeparatorChar;
        }

        public static bool IsEmpty(string path)
        {
            return string.IsNullOrWhiteSpace(path);
        }

        /// <summary>
        /// Checks that the path string contains both directory and file information
        /// </summary>
        public static bool IsFilePath(string path)
        {
            path = PathWithoutFilename(path, out string filename);

            return !string.IsNullOrEmpty(path) &&
                   !string.IsNullOrEmpty(filename);
        }

        public static bool IsPathKeyword(string pathString)
        {
            if (IsEmpty(pathString) || Path.IsPathRooted(pathString)) return false;

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

            if (pathComparer.InternalEquals(path, RainWorldPath.StreamingAssetsPath))
                keyword = UtilityConsts.PathKeywords.STREAMING_ASSETS;
            else if (pathComparer.InternalEquals(path, RainWorldPath.RootPath))
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
                    pathString = RainWorldPath.RootPath;
                    break;
                case UtilityConsts.PathKeywords.STREAMING_ASSETS:
                    pathString = RainWorldPath.StreamingAssetsPath;
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
