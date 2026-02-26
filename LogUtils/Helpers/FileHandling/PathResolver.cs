using System.IO;

namespace LogUtils.Helpers.FileHandling
{
    public class PathResolver
    {
        /// <summary>
        /// A fully qualified path that tends to serve as a path to fallback on in necessary situations. Usage of this path depends on
        /// the <see cref="PathResolver"/> implementation.
        /// </summary>
        public string DefaultPath { get; }

        /// <summary>
        /// The path matching implementation used to determine the path resolution result
        /// </summary>
        public IPathFinder Finder { get; }

        public PathResolver(string defaultPath)
        {
            DefaultPath = defaultPath;
        }

        public PathResolver(string defaultPath, IPathFinder finder)
        {
            DefaultPath = defaultPath;
            Finder = finder;
        }

        /// <summary>
        /// Translates a provided path string into a new path string
        /// </summary>
        /// <param name="path">A fully qualified, relative, or partial path, or filename</param>
        public virtual string Resolve(string path)
        {
            if (PathUtils.IsEmpty(path))
                return DefaultPath;

            bool isFullPath = TryExpandPath(ref path);

            if (isFullPath)
                return path;

            if (Finder != null)
            {
                string result = Finder.FindMatch(path);

                if (result != null)
                    return result;
            }
            return Path.Combine(DefaultPath, path); //Path is unrecognized, assume it is part of default path 
        }

        /// <summary>
        /// Expands relative, or fully qualified paths into a normalized format
        /// </summary>
        /// <returns>A value indicating the fully qualified state of the path</returns>
        protected virtual bool TryExpandPath(ref string path)
        {
            //Expand if path is a relative path, or lacks drive information
            bool isExpanded;
            if (path[0] == '.' || path[0] == Path.DirectorySeparatorChar || path[0] == Path.AltDirectorySeparatorChar)
            {
                path = Path.GetFullPath(path);
                isExpanded = true;
            }
            else
            {
                path = PathUtils.Normalize(path);
                isExpanded = Path.IsPathRooted(path);
            }

            if (path.Length > PathUtils.PATH_VOLUME_LENGTH)
                path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return isExpanded;
        }
    }
}
