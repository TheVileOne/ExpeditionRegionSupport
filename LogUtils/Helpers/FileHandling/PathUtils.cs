using LogUtils.Helpers.Comparers;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        internal readonly static char[] PATH_SEPARATORS = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

        /// <summary>
        /// Checks that a directory is contained within a path string
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <param name="dirName">The directory name to search for</param>
        /// <exception cref="ArgumentException">Path contains illegal characters</exception>
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
        /// <exception cref="ArgumentException">Path contains illegal characters</exception>
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

                string pathTarget = Path.GetFileName(path);
                if (DirectoryUtils.IsDirectoryName(dirName, pathTarget))
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

            bool containsExactPath = path.StartsWith(pathOther, StringComparison.InvariantCultureIgnoreCase);

            if (containsExactPath)
            {
                if (path.Length == pathOther.Length) //Exact match
                    return true;

                //Find the character after the matched portion of the path string
                char checkChar = path.Length > pathOther.Length
                               ? path[pathOther.Length]
                               : pathOther[path.Length];

                //Path separator marks the end of the directory. This confirms there is a match of the complete directory name
                bool hasTrailingSeparator = PATH_SEPARATORS.Contains(checkChar);
                return hasTrailingSeparator;
            }
            return false;
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
        /// Accepts a path, and checks if there is an existing subdirectory after a given length of the path string
        /// </summary>
        /// <param name="checkPath">Path to evaluate</param>
        /// <param name="startAfter">Checks for an existing directory path after a specified character position in the <paramref name="checkPath"/> string</param>
        /// <returns><see langword="true"/>, when an subpath exists, <see langword="false"/> otherwise.
        /// Also returns <see langword="false"/> when <paramref name="startAfter"/> is equal to the length of the checked path.</returns>
        /// <exception cref="ArgumentNullException">A path given was null or empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">The index was negative, or greater than the path length</exception>
        public static bool SubPathExists(string checkPath, int startAfter)
        {
            if (IsEmpty(checkPath))
                throw new ArgumentNullException(nameof(checkPath));

            if (startAfter < 0 || startAfter > checkPath.Length)
                throw new ArgumentOutOfRangeException(nameof(startAfter));

            if (startAfter == checkPath.Length) //A subdirectory cannot exists
                return false;

            bool pathExists = false;
            PathInfo pathInfo = new PathInfo(checkPath);
            foreach (string path in pathInfo.GetFullDirectoryNames())
            {
                if (path.Length <= startAfter) //We don't care about the path until we reach the desired path length
                    continue;
                pathExists = Directory.Exists(path);
                break;
            }
            return pathExists;
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
            return RainWorldDirectory.PathResolver.Resolve(path);
        }

        /// <summary>
        /// A slightly quicker version of path resolution that doesn't handle paths that could be reliably resolved through a GetFullPath call
        /// </summary>
        internal static string QuickResolve(string path)
        {
            if (!IsResolutionCandidate(path)) //Avoids path normalization, and expansion of fully qualified paths. Relative paths will return as relative paths.
                return path;
            return RainWorldDirectory.PathResolver.Resolve(path);
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

            //Path.GetFullPath will replace Path.DirectorySeparatorChar '//' for us, but not Path.Combine
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }


        /// <summary>
        /// Replace all directory separator characters with the default platform-specific directory separator character 
        /// </summary>
        /// <returns>Path string with normalized separator characters with no trailing separator characters (except those that define the path root)</returns>
        public static string NormalizeAndTrim(string path)
        {
            if (IsEmpty(path))
                return path;

            string result = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            if (result.Length <= PATH_VOLUME_LENGTH)
                return result;

            //Path.GetFullPath will replace Path.DirectorySeparatorChar '//' for us, but not Path.Combine
            return result.TrimEnd(PATH_SEPARATORS);
        }

        /// <summary>
        /// Returns a path string without the filename (filename must have an extension)
        /// </summary>
        /// <exception cref="ArgumentException">Path contains illegal characters</exception>
        public static string PathWithoutFilename(string path)
        {
            return PathWithoutFilename(path, out _);
        }

        /// <summary>
        /// Returns a path string without the filename (filename must have an extension)
        /// </summary>
        /// <exception cref="ArgumentException">Path contains illegal characters</exception>
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
            return Path.DirectorySeparatorChar + path.TrimStart(PATH_SEPARATORS);
        }

        /// <summary>
        /// Separates a path into its directory and/or file components
        /// </summary>
        public static string[] SplitPath(string path)
        {
            if (IsEmpty(path))
                return Array.Empty<string>();

            return SplitPathInternal(path);
        }

        /// <summary>
        /// Separates a path into its directory and/or file components
        /// </summary>
        /// <param name="path">Path to target</param>
        /// <param name="startAt">The character position in the string to start beging the split</param>
        /// <exception cref="ArgumentOutOfRangeException">Index position given was negative or greater than the length of the path string</exception>
        public static string[] SplitPath(string path, int startAt = 0)
        {
            if (startAt < 0 || (startAt > 0 && (path == null || startAt > path.Length)))
                throw new ArgumentOutOfRangeException(nameof(startAt));

            if (IsEmpty(path) || startAt == path.Length)
                return Array.Empty<string>();

            return SplitPathInternal(path.Substring(startAt));
        }

        internal static string[] SplitPathInternal(string path)
        {
            int prefixLength = GetPrefixLength(path);

            if (prefixLength > 0)
                path = path.Substring(prefixLength);

            return path.Trim(PATH_SEPARATORS) //Trimming avoids empty data making it into the results
                       .Split(PATH_SEPARATORS);
        }

        /// <summary>
        /// Removes path root information from a path
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>A <see langword="string"/> not containing path root information</returns>
        /// <exception cref="ArgumentException">Path contains illegal characters</exception>
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
        /// Checks that <paramref name="path"/> contains drive letter information
        /// </summary>
        public static bool IsAbsolute(string path)
        {
            //TODO: This is an inaccurate definition of an absolute path
            return Path.IsPathRooted(path) && path[0] != Path.DirectorySeparatorChar && path[0] != Path.AltDirectorySeparatorChar;
        }

        /// <summary>
        /// Checks that <paramref name="path"/> contains useable path information
        /// </summary>
        public static bool IsEmpty(string path)
        {
            return string.IsNullOrWhiteSpace(path);
        }

        /// <summary>
        /// Checks that the path string contains both directory and file information
        /// </summary>
        public static bool IsFilePath(string path)
        {
            if (IsEmpty(path)) return false;

            char lastChar = path[path.Length - 1];

            if (lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar)
                return false;

            try
            {
                path = PathWithoutFilename(path, out string filename);

                return !string.IsNullOrEmpty(path) &&
                       !string.IsNullOrEmpty(filename);
            }
            catch (ArgumentException)
            {
                return false;
            }
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

        /// <summary>
        /// Is the path string going to benefit from going through the path resolution process
        /// </summary>
        public static bool IsResolutionCandidate(string pathString)
        {
            return IsEmpty(pathString) || (!Path.IsPathRooted(pathString) && pathString[0] != '.');
        }

        /// <summary>
        /// Combines path without any trailing separators
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CombineAndTrim(string path1, string path2)
        {
            string result = Path.Combine(path1, path2);

            if (result.Length <= PATH_VOLUME_LENGTH)
                return result;

            return result.TrimEnd(PATH_SEPARATORS);
        }

        /// <summary>
        /// Converts to full path without trailing path separators
        /// </summary>
        /// <param name="path">Path to convert to a fully qualified path</param>
        /// <returns>A fully qualified path, or the path itself when given an empty path string</returns>
        public static string GetFullPathAndTrim(string path)
        {
            if (IsEmpty(path)) return path;

            string result = Path.GetFullPath(path);

            if (result.Length <= PATH_VOLUME_LENGTH)
                return result;

            return result.TrimEnd(Path.DirectorySeparatorChar);
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
            return Path.Combine(newBasePath, subPath);   //Combine it with new path
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
