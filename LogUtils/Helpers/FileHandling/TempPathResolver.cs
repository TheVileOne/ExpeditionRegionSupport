using System;
using System.IO;

namespace LogUtils.Helpers.FileHandling
{
    public class TempPathResolver : PathResolver
    {
        /// <summary>
        /// Used in situations in which use of the entire path is undesired. This value sets a limit to how many directory segments to use in the resolved path. 
        /// </summary>
        public int MaxFoldersToSelect { get; set; }

        public TempPathResolver(string defaultPath) : base(defaultPath, new TempPathFinder(defaultPath))
        {
            MaxFoldersToSelect = 3;
        }

        /// <inheritdoc/>
        /// <remarks>The output string will not contain a trailing separator character</remarks>
        public override string Resolve(string path)
        {
            string tempPath = DefaultPath;

            //The behavior of this process is to combine a provided path to a specific place inside a temporary folder path
            PathInfo info = new PathInfo(path);

            if (!info.HasPath || info.Target.Type == PathType.Root)
            {
                if (info.HasFilename || info.HasDirectory) //Path string is a filename or directory name stub
                {
                    string targetName = info.Target.Name;
                    return PathUtils.CombineWithoutTrailingSeparators(tempPath, targetName);
                }

                //Path is either empty, or does not contain any folder, or filename information
                UtilityLogger.LogWarning("Not enough path information available to resolve");
                return tempPath;
            }

            string targetPath = info.TargetPath;
            bool isFullPath = TryExpandPath(ref targetPath);

            if (isFullPath)
            {
                //A full path will fall into one of three buckets:
                //I.   Path belongs to the temporary folder.
                //II.  Path belongs to Rain World folder (current working directory).
                //III. Path belongs to neither, and we do not recognize the path.

                //Find out which bucket we fall into. Null output indicates an unrecognized path.
                string pathRoot = Finder.FindRootMatch(targetPath);

                if (pathRoot != null) //Root is compared, because we need the root length
                {
                    if (PathUtils.PathsAreEqual(pathRoot, Environment.CurrentDirectory))
                        pathRoot = Path.GetDirectoryName(pathRoot); //Get the path preceding the root path, but only for the root directory

                    targetPath = targetPath.Substring(pathRoot.Length + 1); //Plus one to account for directory separator

                    if (info.HasFilename)
                    {
                        string targetName = info.Target.Name;
                        return Path.Combine(tempPath, targetPath, targetName);
                    }
                    return PathUtils.CombineWithoutTrailingSeparators(tempPath, targetPath);
                }

                //Maintains consistent handling between directory, and file paths
                int maxFoldersToSelect = MaxFoldersToSelect;
                if (info.HasDirectory)
                    maxFoldersToSelect++; //Trailing directory is excluded from selection maximum

                //Handle the unrecognized case
                //Include only part of the whole path string for the output path. Filename is included in the output.
                targetPath = info.BuildPath()
                                 .TakeLast(maxFoldersToSelect)
                                 .GetResult();
                return PathUtils.CombineWithoutTrailingSeparators(tempPath, targetPath);
            }

            //The path that is handled here cannot be a full path, and cannot be a stub either.
            //Expected format: "path/folder/filename.txt" (filename optional)
            if (info.HasFilename)
            {
                string targetName = info.Target.Name;
                return Path.Combine(tempPath, targetPath, targetName);
            }
            return PathUtils.CombineWithoutTrailingSeparators(tempPath, targetPath);
        }
    }

    internal class TempPathFinder : IPathFinder
    {
        /// <summary>
        /// The fully qualified path that represents the location of a temporary folder
        /// </summary>
        internal string TargetPath;

        public TempPathFinder(string targetPath) : base()
        {
            TargetPath = targetPath;
        }

        public string FindMatch(string path)
        {
            int targetIndex = getTargetIndex(path);

            if (targetIndex != -1)
                return path;
            return null;
        }

        public string FindRootMatch(string path)
        {
            int targetIndex = getTargetIndex(path);

            if (targetIndex != -1)
            {
                string[] targets = GetTargets();
                return targets[targetIndex];
            }
            return null;
        }

        internal string[] GetTargets()
        {
            return new string[]
            {
                TargetPath,
                Environment.CurrentDirectory,
            };
        }

        private int getTargetIndex(string path)
        {
            string[] targets = GetTargets();

            int targetIndex;
            for (targetIndex = 0; targetIndex < targets.Length; targetIndex++)
            {
                if (PathUtils.ContainsOtherPath(path, targets[targetIndex]))
                    break;
            }

            if (targetIndex == targets.Length) //Return a negative value to make it clear we didn't find a match
                return -1;
            return targetIndex;
        }
    }
}
