using LogUtils.Helpers.Comparers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils.Helpers.FileHandling
{
    public static class DirectoryUtils
    {
        /// <summary>
        /// Checks that a directory is contained within a path string
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <param name="dirName">The directory name to search for</param>
        public static bool ContainsDirectory(string path, string dirName)
        {
            if (PathUtils.IsEmpty(path)) return false;

            string[] pathDirs = PathUtils.Separate(PathUtils.PathWithoutFilename(path));

            return Array.Exists(pathDirs, dir => IsDirectoryName(dir, dirName));
        }

        /// <summary>
        /// Checks that a directory is contained within a path string
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <param name="dirName">The directory name to search for</param>
        /// <param name="dirLevelsToCheck">The number of directory separators to check starting from the right</param>
        public static bool ContainsDirectory(string path, string dirName, int dirLevelsToCheck)
        {
            if (PathUtils.IsEmpty(path)) return false;

            path = PathUtils.PathWithoutFilename(path);

            bool dirFound = false;
            while (dirLevelsToCheck > 0)
            {
                if (PathUtils.IsEmpty(path))
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
        /// Compares the equality of two directory names
        /// </summary>
        /// <param name="dirName">The first directory name to check</param>
        /// <param name="dirNameOther">The second directory name to check</param>
        /// <param name="trimLeadingSeparators">Removes leading separator characters before matching</param>
        /// <returns>Returns whether both directory names have an equivalent value (case insensitive)</returns>
        public static bool IsDirectoryName(string dirName, string dirNameOther, bool trimLeadingSeparators = false)
        {
            if (dirName == null || dirNameOther == null)
                return dirName == dirNameOther;

            char[] separatorChars = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];

            //Trim out characters that can interfere with matching
            if (trimLeadingSeparators)
            {
                dirName = dirName.TrimStart(separatorChars);
                dirNameOther = dirNameOther.TrimStart(separatorChars);
            }
            dirName = dirName.TrimEnd(separatorChars);
            dirNameOther = dirNameOther.TrimEnd(separatorChars);
            return ComparerUtils.StringComparerIgnoreCase.Equals(dirName, dirNameOther);
        }

        /// <summary>
        /// Determines if the given path has an existing parent directory
        /// </summary>
        public static bool ParentExists(string path)
        {
            if (PathUtils.IsEmpty(path) || path.Length <= 3) return false;

            return Directory.Exists(Path.GetDirectoryName(path));
        }

        public static void SafeDelete(string path, bool deleteOnlyIfEmpty, string customErrorMsg = null)
        {
            try
            {
                if (Directory.Exists(path) && (!deleteOnlyIfEmpty || !Directory.EnumerateFiles(path).Any()))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(customErrorMsg ?? "Unable to delete directory", ex);
            }
        }

        public static void SafeDelete(string path, string customErrorMsg = null)
        {
            SafeDelete(path, false, customErrorMsg);
        }

        public static int DirectoryFileCount(string path)
        {
            return Directory.Exists(path) ? Directory.GetFiles(path).Length : 0;
        }

        public static void Copy(string sourceDir, string destinationDir, bool recursive)
        {
            List<string> failedToCopy = new List<string>();

            // Get information about the source directory
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            //Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            //Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            //Create the destination directory
            Directory.CreateDirectory(destinationDir);

            //Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                try
                {
                    string targetFilePath = Path.Combine(destinationDir, file.Name);
                    file.CopyTo(targetFilePath);
                }
                catch
                {
                    failedToCopy.Add(file.Name);
                }
            }

            //If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    Copy(subDir.FullName, newDestinationDir, true);
                }
            }

            UtilityLogger.Log(failedToCopy + " failed to copy");

            foreach (string file in failedToCopy)
                UtilityLogger.Log(file);
        }
    }
}
