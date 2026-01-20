using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogUtils.Helpers.FileHandling
{
    public sealed record class PathInfo
    {
        /// <summary>
        /// Contains information about the last segment in the path string (typically refers to a filename, or directory)
        /// </summary>
        public readonly PathTarget Target;

        /// <summary>
        /// Contains path information extracted from the path string
        /// </summary>
        /// <value>Field will be empty when no path information was provided, or the path consisted of a single filename or directory</value>
        public readonly string TargetPath;

        /// <summary>
        /// Checks that path string contains a filename target
        /// </summary>
        public bool HasFilename => Target.Type == PathType.Filename;

        /// <summary>
        /// Checks that path string contains a directory target
        /// </summary>
        public bool HasDirectory => Target.Type == PathType.Directory;

        /// <summary>
        /// Checks that path string contained a partial, or full path
        /// </summary>
        public bool HasPath => TargetPath != string.Empty;

        /// <summary>
        /// Checks that there is path information pertaining to a filename
        /// </summary>
        public bool IsFilePath => HasPath && HasFilename;

        /// <summary>
        /// Checks that there is path information pertaining to a directory
        /// </summary>
        public bool IsDirectoryPath => HasPath && HasDirectory;

        public PathInfo(string path, bool includeFilenameInPath = false)
        {
            if (PathUtils.IsEmpty(path))
            {
                Target = new PathTarget();
                TargetPath = string.Empty;
                return;
            }

            //TODO: Handle relative paths properly
            //if (path[0] == '.')
            //{
            //    UtilityLogger.Log("Resolving short path");
            //    path = Path.GetFullPath(path);
            //}

            string pathTarget;
            if (includeFilenameInPath)
            {
                //TODO: This doesn't support periods in filename/directory
                pathTarget = Path.GetFileName(path); //This can be a filename or directory

                if (Path.HasExtension(pathTarget)) //The target is probably a filename
                {
                    Target = new PathTarget(PathType.Filename, pathTarget);

                    if (path.StartsWith(pathTarget)) //Check that there is path info associated with the target
                    {
                        TargetPath = string.Empty;
                        return;
                    }
                }
                else
                {
                    if (pathTarget == string.Empty) //Most likely means path ends in a directory separator
                    {
                        path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        pathTarget = Path.GetFileName(path); //This is either going to be a directory, or empty if there is only root information
                    }

                    bool hasDirectoryInfo = pathTarget != string.Empty;
                    if (hasDirectoryInfo)
                    {
                        Target = new PathTarget(PathType.Directory, pathTarget);

                        if (path.StartsWith(pathTarget)) //Check that there is path info associated with the target
                        {
                            TargetPath = string.Empty;
                            return;
                        }
                    }
                    else
                    {
                        Target = new PathTarget(PathType.Root, pathTarget);
                    }
                }
                TargetPath = path;
                return;
            }

            path = PathUtils.PathWithoutFilename(path, out string filename);

            if (filename != null)
            {
                Target = new PathTarget(PathType.Filename, filename);

                if (PathUtils.IsEmpty(path)) //Check that there is path info associated with the target
                {
                    TargetPath = string.Empty;
                    return;
                }
            }
            else //TargetPath should not be null, or empty here
            {
                pathTarget = Path.GetFileName(path);

                if (pathTarget == string.Empty) //Most likely means path ends in a directory separator
                {
                    path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    pathTarget = Path.GetFileName(path); //This is either going to be a directory, or empty if there is only root information
                }

                bool hasDirectoryInfo = pathTarget != string.Empty;
                if (hasDirectoryInfo)
                {
                    Target = new PathTarget(PathType.Directory, pathTarget);

                    if (path.StartsWith(pathTarget)) //Check that there is path info associated with the target
                    {
                        TargetPath = string.Empty;
                        return;
                    }
                }
                else
                {
                    Target = new PathTarget(PathType.Root, pathTarget);
                }
            }
            TargetPath = path;
        }

        /// <summary>
        /// Creates an object for the streamlined building of subpath strings
        /// </summary>
        public IPathBuilder BuildPath()
        {
            return BuildPath(false);
        }

        /// <summary>
        /// Creates an object for the streamlined building of subpath strings
        /// </summary>
        public IPathBuilder BuildPath(bool includeRoot, bool includeFilenameInResult = true)
        {
            return new PathBuilder(this)
            {
                IncludeRoot = includeRoot,
                IncludeFilenameInResult = includeFilenameInResult,
            };
        }

        /// <summary>
        /// Incrementally adds directory names from the target path until the entire target path is returned 
        /// </summary>
        public IEnumerable<string> GetFullDirectoryNames()
        {
            if (!HasPath)
            {
                if (HasDirectory)
                    yield return Target.Name;
                yield break;
            }

            using (var builder = BuildPath(includeRoot: true, includeFilenameInResult: false))
            {
                while (builder.MoveNext())
                {
                    yield return builder.GetResult();
                }
            }
            yield break;
        }

        /// <summary>
        /// Enumerates directory names parsed from the path string
        /// </summary>
        public IEnumerable<string> GetDirectories()
        {
            if (!HasPath)
            {
                if (HasDirectory)
                    yield return Target.Name;
                yield break;
            }

            int dirIndex = GetPrefixLength(),
                lastDirIndex = dirIndex;

            while (dirIndex < TargetPath.Length)
            {
                char c = TargetPath[dirIndex];

                if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
                {
                    yield return TargetPath.Substring(lastDirIndex, dirIndex - lastDirIndex);
                    lastDirIndex = dirIndex + 1; //Add one to account for separator
                }
                dirIndex++;
            }

            if (lastDirIndex != dirIndex)
                yield return TargetPath.Substring(lastDirIndex, dirIndex - lastDirIndex);
            yield break;
        }

        /// <summary>
        /// Begins a new directory enumeration returning the enumerator
        /// </summary>
        public IEnumerator<string> GetDirectoryEnumerator()
        {
            return GetDirectories().GetEnumerator();
        }

        /// <summary>
        /// Extracts the path root from the path string
        /// </summary>
        /// <remarks>Returns an empty string if no root information is present</remarks>
        public string GetRoot()
        {
            if (TargetPath == string.Empty)
                return string.Empty;

            if (TargetPath[0] == '.' || TargetPath[0] == Path.DirectorySeparatorChar || TargetPath[0] == Path.AltDirectorySeparatorChar)
                return Directory.GetDirectoryRoot(TargetPath); //Let .NET API resolve the relative path for us

            //For the general case, we resolve the root manually - .NET helper will do a full path check if we use it
            return TargetPath.Substring(0, GetRootLength())
                             .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Gets the length of the path root
        /// </summary>
        public int GetRootLength()
        {
            return TargetPath.Length - PathUtils.Unroot(TargetPath).Length;
        }

        /// <summary>
        /// Gets the length of any relative, or root path information at the start of the path string
        /// </summary>
        public int GetPrefixLength()
        {
            if (PathUtils.IsAbsolute(TargetPath))
                return GetRootLength();

            int matchCount = 0;
            while (matchCount < TargetPath.Length)
            {
                char c = TargetPath[matchCount];
                if (c == '.' || c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
                {
                    matchCount++;
                    continue;
                }
                break;
            }
            return matchCount;
        }
    }

    public readonly struct PathTarget
    {
        /// <summary>
        /// Describes the nature of the path information
        /// </summary>
        public readonly PathType Type;

        /// <summary>
        /// Identifies a component of a path
        /// </summary>
        public readonly string Name;

        public PathTarget()
        {
            Type = PathType.Empty;
            Name = string.Empty;
        }

        public PathTarget(PathType type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    public enum PathType
    {
        Empty,
        Root,
        Filename,
        Directory,
    }
}
