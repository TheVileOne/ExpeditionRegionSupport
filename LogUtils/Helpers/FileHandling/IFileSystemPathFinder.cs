//using System.Collections.Generic;

namespace LogUtils.Helpers.FileHandling
{
    public interface IFileSystemPathFinder
    {
        /// <summary>
        /// Searches for a file, or directory path corresponding with a provided path, directory, or filename
        /// </summary>
        /// <param name="path">The path, directory, or filename to search for</param>
        /// <returns>A file, or directory path corresponding with a provided path, directory, or filename, or null if match criteria was not found</returns>
        string FindMatch(string path);

        /// <summary>
        /// Enumerates the search result of file, or directory paths corresponding with a provided path, directory, or filename
        /// </summary>
        /// <param name="path">The path, directory, or filename to search for</param>
        /// <returns>Any file, or directory paths corresponding with a provided path, directory, or filename</returns>
        //IEnumerable<string> EnumerateMatches(string path);
    }
}
