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
        /// Returns the char position of the first directory name matching a provided directory string 
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <param name="dirName">The directory name to search for</param>
        /// <returns>Returns a char position, or -1 if a match is not found</returns>
        /// <remarks>The returned index will be representative of the position of the first character in the directory string (separator characters are ignored)</remarks>
        public static int GetLocationInPath(string path, string dirName)
        {
            if (PathUtils.IsEmpty(path)) return -1;

            string[] pathDirs = PathUtils.SplitPath(PathUtils.PathWithoutFilename(path));

            int charCount = PathUtils.GetPrefixLength(path);
            int pathIndex;
            for (pathIndex = 0; pathIndex < pathDirs.Length; pathIndex++)
            {
                if (pathIndex > 0)
                    charCount++; //Account for the separator

                if (IsDirectoryName(pathDirs[pathIndex], dirName)) //Break if we found a match
                    break;

                charCount += pathDirs[pathIndex].Length;
            }

            if (pathIndex == pathDirs.Length) //Directory is not included in the path string
                return -1;
            return charCount;
        }

        /// <summary>
        /// Returns the char position of the first directory name matching a provided directory string 
        /// </summary>
        /// <param name="info">The <see cref="PathInfo"/> instance to check</param>
        /// <param name="dirName">The directory name to search for</param>
        /// <returns>Returns a char position, or -1 if a match is not found</returns>
        /// <remarks>The returned index will be representative of the position of the first character in the directory string (separator characters are ignored)</remarks>
        public static int GetLocationInPath(PathInfo info, string dirName)
        {
            if (!info.HasPath || info.Target.Type == PathType.Empty || info.Target.Type == PathType.Root)
                return -1;

            int dirIndex = info.GetPrefixLength();
            using (var dirEnumerator = info.GetDirectoryEnumerator())
            {
                //Enumerate until we find a match
                int dirCount = 0;
                while (dirEnumerator.MoveNext() && !IsDirectoryName(dirEnumerator.Current, dirName))
                {
                    dirCount++;
                    dirIndex += dirEnumerator.Current.Length + 1; //Add one to account for the separator
                }

                if (dirCount > 0) //We will overcount by one inside the while loop
                    dirIndex--;
            }

            if (dirIndex == info.TargetPath.Length) //Was there a match?
                return -1;
            return dirIndex;
        }

        public static bool IsSafeToMove(string folderPath)
        {
            return RainWorldDirectory.GetDirectoryCategory(folderPath) == PathCategory.ModSourced;
        }

        /// <summary>
        /// Determines if the given path has an existing parent directory
        /// </summary>
        public static bool ParentExists(string path)
        {
            if (PathUtils.IsEmpty(path) || path.Length <= PathUtils.PATH_VOLUME_LENGTH) return false;

            return Directory.Exists(Path.GetDirectoryName(path));
        }

        /// <inheritdoc cref="DeletePermanently(string, DirectoryDeletionScope, string)"/>
        public static bool DeletePermanently(string path, bool deleteOnlyIfEmpty = true)
        {
            return DeleteInternal(path, deleteOnlyIfEmpty ? DirectoryDeletionScope.OnlyIfEmpty : DirectoryDeletionScope.AllFilesAndFolders);
        }

        /// <inheritdoc cref="DeletePermanently(string, DirectoryDeletionScope, string)"/>
        public static bool DeletePermanently(string path, DirectoryDeletionScope deleteOption)
        {
            return DeleteInternal(path, deleteOption);
        }

        /// <summary>
        /// Permanently deletes a directory at the specified path
        /// </summary>
        /// <param name="path">Location of the directory</param>
        /// <param name="deleteOption">Condition that must be met for deletion operation to complete successfully</param>
        /// <param name="customErrorMsg">Message that will be logged in the event of an exception</param>
        /// <returns>true, if the operation was successful, false if an exception was throw, or condition for deletion was not met</returns>
        public static bool DeletePermanently(string path, DirectoryDeletionScope deleteOption, string customErrorMsg = null)
        {
            return DeleteInternal(path, deleteOption, customErrorMsg);
        }

        internal static bool DeleteInternal(string path, DirectoryDeletionScope deleteOption = DirectoryDeletionScope.OnlyIfEmpty, string customErrorMsg = null)
        {
            try
            {
                if (!Directory.Exists(path)) //Return value is consistent with how files are handled
                    return true;

                bool canDelete = deleteOption == DirectoryDeletionScope.AllFilesAndFolders || !Directory.EnumerateFiles(path).Any();

                if (canDelete)
                    Directory.Delete(path, true);
                return canDelete; //Consider not meeting deletion conditions as a failed operation
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(customErrorMsg ?? "Unable to delete directory", ex);
                return false;
            }
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

    /// <summary>
    /// Specifies a condition that must be met for delete operation to complete
    /// </summary>
    public enum DirectoryDeletionScope
    {
        /// <summary>
        /// Directory is only touched if there are no files, or other directories inside
        /// </summary>
        OnlyIfEmpty = 0,
        /// <summary>
        /// Directory, and all of its contents will be deleted
        /// </summary>
        AllFilesAndFolders = 1,
    }
}
