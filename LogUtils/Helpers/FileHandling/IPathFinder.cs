namespace LogUtils.Helpers.FileHandling
{
    /// <summary>
    /// Type provided to a <see cref="PathResolver"/> that is responsible for matching to specific directory or file path
    /// </summary>
    public interface IPathFinder
    {
        /// <summary>
        /// Searches for a file, or directory path corresponding with a provided path, directory, or filename
        /// </summary>
        /// <param name="path">The path, directory, or filename to search for</param>
        /// <returns>A file, or directory path corresponding with a provided path, directory, or filename, or null if match criteria was not found</returns>
        public string FindMatch(string path);

        /// <summary>
        /// Searches for a file, or directory path corresponding with a provided path, directory, or filename, and returns the base root portion of the corresponding path
        /// </summary>
        /// <param name="path">The path, directory, or filename to search for</param>
        /// <returns>A directory path considered as the base root corresponding with a provided path, directory, or filename, or null if match criteria was not found</returns>
        public string FindRootMatch(string path);
    }
}
