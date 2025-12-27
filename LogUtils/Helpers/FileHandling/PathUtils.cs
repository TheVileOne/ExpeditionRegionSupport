using LogUtils.Helpers.Comparers;
using System;
using System.IO;
using System.Text;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils.Helpers.FileHandling
{
    public static class PathUtils
    {
        /// <summary>
        /// The length of a Windows path volume
        /// </summary>
        public const int PATH_VOLUME_LENGTH = 3;

        /// <summary>
        /// Checks that a directory is contained within a path string
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <param name="dirName">The directory name to search for</param>
        public static bool ContainsDirectory(string path, string dirName)
        {
            if (IsEmpty(path)) return false;

            string[] pathDirs = SplitPath(PathWithoutFilename(path));

            return Array.Exists(pathDirs, dir => DirectoryUtils.IsDirectoryName(dir, dirName));
        }

        /// <summary>
        /// Checks that a directory is contained within a path string
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <param name="dirName">The directory name to search for</param>
        /// <param name="dirLevelsToCheck">The number of directory separators to check starting from the right</param>
        public static bool ContainsDirectory(string path, string dirName, int dirLevelsToCheck)
        {
            if (IsEmpty(path)) return false;

            path = PathWithoutFilename(path);

            bool dirFound = false;
            while (dirLevelsToCheck > 0)
            {
                if (IsEmpty(path))
                {
                    dirLevelsToCheck = 0;
                    break;
                }

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
                }
            }
            return dirFound;
        }

        /// <summary>
        /// Checks the second path is contained by the first path
        /// </summary>
        /// <example>
        /// ContainsOtherPath("path/FolderA/FolderB", "path/FolderA") returns true, because the second path is a substring to the first path.
        /// </example>
        public static bool ContainsOtherPath(string path, string pathOther)
        {
            path = ResolvePath(path);
            pathOther = ResolvePath(pathOther);

            return path.StartsWith(pathOther, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Finds a path string that two provided paths have in common. When a partial path is provided, the path will be resolved to a fully qualified path targeting
        /// either Rain World, StreamingAssets, or BepInEx directory paths, defaulting to StreamingAssets when no match is found. 
        /// </summary>
        public static string FindCommonRoot(string path, string pathOther)
        {
            //Find the best fit
            path = ResolvePath(path);
            pathOther = ResolvePath(pathOther);

            PathInfo info = new PathInfo(path);
            PathInfo infoOther = new PathInfo(pathOther);
            return FindCommonRoot(info, infoOther);
        }

        /// <summary>
        /// Finds a path string that two provided paths have in common.
        /// </summary>
        public static string FindCommonRoot(PathInfo info, PathInfo infoOther)
        {
            //Root should never be empty here
            string pathRoot = info.GetRoot();
            string pathRootOther = infoOther.GetRoot();

            if (!pathRoot.Equals(pathRootOther, StringComparison.OrdinalIgnoreCase)) //Path root doesn't match
                return string.Empty;

            var dirEnumerator = info.GetDirectoryEnumerator();
            var dirEnumeratorOther = infoOther.GetDirectoryEnumerator();

            StringBuilder pathBuilder = new StringBuilder();

            pathBuilder.Append(pathRoot);

            //Find a string of directories that are shared between the two paths here 
            while (dirEnumerator.MoveNext() && dirEnumeratorOther.MoveNext() && DirectoryUtils.IsDirectoryName(dirEnumerator.Current, dirEnumeratorOther.Current))
            {
                pathBuilder.Append(dirEnumerator.Current)
                           .Append(Path.DirectorySeparatorChar);
            }
            return pathBuilder.ToString().TrimEnd(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Trims the common difference between the first path and the second path from the first path. If the common difference will consume the path, an empty string
        /// with be returned.
        /// </summary>
        /// <param name="path">First path to evaluate</param>
        /// <param name="pathOther">Second path to evaluate</param>
        public static string TrimCommonRoot(string path, string pathOther)
        {
            //Find the best fit
            path = ResolvePath(path);
            pathOther = ResolvePath(pathOther);

            PathInfo info = new PathInfo(path);
            PathInfo infoOther = new PathInfo(pathOther);
            string commonRoot = FindCommonRoot(info, infoOther);

            return path.Substring(Math.Min(commonRoot.Length + 1, path.Length));
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

            path = Normalize(path);
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
        /// Takes a current path, and transfers a subpath to a new containing path
        /// </summary>
        /// <param name="basePath">A path that contains <paramref name="subPath"/></param>
        /// <param name="newBasePath">A new path that should contain <paramref name="subPath"/></param>
        /// <param name="subPath">A path that is part of <paramref name="basePath"/></param>
        /// <returns>A path that is the combination of <paramref name="subPath"/>, and <paramref name="newBasePath"/></returns>
        public static string Rebase(string subPath, string basePath, string newBasePath)
        {
            subPath = TrimCommonRoot(subPath, basePath); //Remove current path from subpath
            return Path.Combine(newBasePath, subPath);                 //Combine it with new path
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
