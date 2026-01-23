using System;
using System.IO;
using System.Linq;
using BepInExPath = LogUtils.Helpers.Paths.BepInEx;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils.Helpers.FileHandling
{
    /// <summary>
    /// A class for searching for a best fit path match against one or multiple directories
    /// </summary>
    public class RainWorldPathFinder : IPathFinder
    {
        /// <inheritdoc/>
        public string FindMatch(string path)
        {
            PathInfo info = new PathInfo(path);

            if (!info.HasPath)
            {
                if (info.HasFilename)
                    return matchFilenameNoPath(info.Target.Name);

                if (info.HasDirectory)
                    return matchDirectoryNoPath(info.Target.Name);
            }
            else if (info.IsFilePath)
            {
                return matchFilePath(info);
            }
            else if (info.IsDirectoryPath)
            {
                return matchDirectoryPath(info);
            }
            //This path is either invalid, or cannot be located within Rain World directory
            return null;
        }

        /// <inheritdoc/>
        public string FindRootMatch(string path)
        {
            path = FindMatch(path);

            if (path == null)
                return null;

            //Order of these checks is important - do not change
            if (path.StartsWith(RainWorldPath.StreamingAssetsPath)) //Case does not matter
                return RainWorldPath.StreamingAssetsPath;

            if (path.StartsWith(BepInExPath.RootPath))
                return BepInExPath.RootPath;

            //By process of elimination - it must be this path
            return RainWorldPath.RootPath;
        }

        private string matchFilenameNoPath(string filename)
        {
            //Custom root
            if (tryMatch(RainWorldPath.StreamingAssetsPath, filename, out string result))
                return result;
            //Root
            if (tryMatch(RainWorldPath.RootPath, filename, out result))
                return result;
            //BepInEx
            if (tryMatch(BepInExPath.RootPath, filename, out result))
                return result;
            return null;

            static bool tryMatch(string searchPath, string filename, out string result)
            {
                result = null;
                searchPath = Path.Combine(searchPath, filename);
                if (File.Exists(searchPath))
                {
                    result = searchPath;
                    return true;
                }
                return false;
            }
        }

        private string matchDirectoryNoPath(string directory)
        {
            //Custom root
            if (tryMatch(RainWorldPath.StreamingAssetsPath, directory, out string result))
                return result;
            //Root
            if (tryMatch(RainWorldPath.RootPath, directory, out result))
                return result;
            //BepInEx
            if (tryMatch(BepInExPath.RootPath, directory, out result))
                return result;
            return null;

            static bool tryMatch(string searchPath, string directory, out string result)
            {
                result = null;
                if (searchPath.EndsWith(directory, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = searchPath;
                    return true;
                }

                searchPath = Path.Combine(searchPath, directory);
                if (Directory.Exists(searchPath))
                {
                    result = searchPath;
                    return true;
                }
                return false;
            }
        }

        private string matchFilePath(PathInfo info)
        {
            string dirPath = matchDirectoryPath(info);

            if (dirPath != null)
                return Path.Combine(dirPath, info.Target.Name); //Filename needs to be added to the path result
            return null;
        }

        private string matchDirectoryPath(PathInfo info)
        {
            //Custom root
            if (tryMatch(RainWorldPath.StreamingAssetsPath, info, out string result))
                return result;
            //Root
            if (tryMatch(RainWorldPath.RootPath, info, out result))
                return result;
            //BepInEx
            if (tryMatch(BepInExPath.RootPath, info, out result))
                return result;
            return null;

            static bool tryMatch(string searchPath, PathInfo info, out string result)
            {
                result = null;

                string searchDir = Path.GetFileName(searchPath);
                UtilityLogger.DebugLog($"Searching for {searchDir} in {info.TargetPath}");

                int dirIndex = DirectoryUtils.GetLocationInPath(info, searchDir);

                if (dirIndex != -1)
                {
                    string subPath = info.TargetPath.Substring(dirIndex + searchDir.Length + 1); //Add one to account for the separator

                    UtilityLogger.DebugLog($"Combining {searchDir} path with {subPath}");
                    result = Path.Combine(searchPath, subPath);
                    return true;
                }

                //The search path is not part of the path string. Only partial paths can match at this point.
                if (Path.IsPathRooted(info.TargetPath))
                    return false;

                //Get the first directory that is part of the target path
                string firstDir = info.GetDirectories().First();
                string[] matches = Directory.GetDirectories(searchPath, firstDir);

                if (matches.Length > 0)
                {
                    UtilityLogger.DebugLog($"Combining {searchPath} path with {info.TargetPath}");
                    result = Path.Combine(searchPath, info.TargetPath);
                    return true;
                }

                //The partial path doesn't exist in this directory - no match was found
                return false;
            }
        }
    }
}
