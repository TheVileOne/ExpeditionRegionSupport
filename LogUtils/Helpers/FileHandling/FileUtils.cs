using LogUtils.Helpers.Comparers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils.Helpers.FileHandling
{
    /// <summary>
    /// Helper class for interacting with the file system that is safe and supported by LogUtils
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Used to attach information to a filename
        /// </summary>
        public const string BRACKET_FORMAT = "{0}[{1}]{2}";

        public static string ApplyBracketInfo(string filename, string info)
        {
            filename = FileExtension.Remove(filename, out string fileExt);
            return string.Format(BRACKET_FORMAT, filename, info, fileExt);
        }

        public static string GetBracketInfo(string filename)
        {
            int bracketIndexLeft = filename.LastIndexOf('['),
                bracketIndexRight = filename.LastIndexOf(']');

            if (bracketIndexLeft == -1 || bracketIndexRight == -1)
                return null;

            return filename.Substring(bracketIndexLeft + 1, bracketIndexRight - (bracketIndexLeft + 1));
        }

        public static string RemoveBracketInfo(string filename)
        {
            int bracketIndex = filename.LastIndexOf('[');

            if (bracketIndex == -1)
                return filename;

            FileExtensionInfo extInfo = FileExtensionInfo.FromFilename(filename);

            bool useExtension;
            switch (FileExtension.LongExtensionSupport)
            {
                case LongExtensionSupport.SupportedOnly:
                    useExtension = !extInfo.IsLong || extInfo.IsSupported;
                    break;
                case LongExtensionSupport.Ignore:
                    useExtension = !extInfo.IsLong;
                    break;
                default:
                case LongExtensionSupport.Full:
                    useExtension = true;
                    break;
            }

            //Strips the bracket info at the end, while retaining the file extension
            return filename.Substring(0, bracketIndex) + (useExtension ? extInfo.Extension : string.Empty);
        }

        public static string GetCollisionFriendlyName(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            int pathHash = filePath.GetHashCode();

            filePath = FileExtension.Remove(filePath, out string fileExt) + '_' + pathHash + fileExt;
            return filePath;
        }

        /// <summary>
        /// Attempts to delete a file at the specified path
        /// </summary>
        public static bool TryDelete(string path, string customErrorMsg = null)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError(customErrorMsg ?? "Unable to delete file", ex);
            }
            return false;
        }

        /// <summary>
        /// Attempts to copy a file to a specified path 
        /// </summary>
        /// <remarks>Any file at the destination path will be overwritten</remarks>
        public static bool TryCopy(string sourcePath, string destPath, int attemptsAllowed = 1)
        {
            string sourceFilename = Path.GetFileName(sourcePath);
            string destFilename = Path.GetFileName(destPath);

            UtilityLogger.Log($"Copying {sourceFilename} to {destFilename}");

            if (ComparerUtils.PathComparer.CompareFilenameAndPath(sourcePath, destPath, true) == 0)
            {
                UtilityLogger.LogError($"Copy target file {sourceFilename} cannot be copied to its source path");
                return false;
            }

            bool exceptionLogged = false;
            while (attemptsAllowed > 0)
            {
                try
                {
                    File.Copy(sourcePath, destPath, true);
                    return true;
                }
                catch (FileNotFoundException)
                {
                    UtilityLogger.LogError($"Copy target file {sourceFilename} could not be found");
                    return false;
                }
                catch (IOException ioex)
                {
                    if (ioex.Message.StartsWith("Sharing violation"))
                        UtilityLogger.LogError($"Copy target file {sourceFilename} is currently in use");
                    handleException(ioex);
                }
                catch (Exception ex)
                {
                    handleException(ex);
                }
            }

            void handleException(Exception ex)
            {
                attemptsAllowed--;
                if (!exceptionLogged)
                {
                    UtilityLogger.LogError(ex);
                    exceptionLogged = true;
                }
            }
            return false;
        }

        /// <summary>
        /// Attempts to move a file to a specified path
        /// </summary>
        /// <remarks>Any file at the destination path will be overwritten</remarks>
        public static bool TryMove(string sourcePath, string destPath, int attemptsAllowed = 1)
        {
            string sourceFilename = Path.GetFileName(sourcePath);
            string destFilename = Path.GetFileName(destPath);

            UtilityLogger.Log($"Moving {sourceFilename} to {destFilename}");

            if (ComparerUtils.PathComparer.CompareFilenameAndPath(sourcePath, destPath, true) == 0)
            {
                UtilityLogger.Log($"Same filepath for {sourceFilename}");
                return true;
            }

            bool destEmpty = !File.Exists(destPath);
            bool exceptionLogged = false;
            while (attemptsAllowed > 0)
            {
                try
                {
                    //Make sure destination is clear
                    if (!destEmpty)
                    {
                        if (!TryDelete(destPath))
                        {
                            attemptsAllowed--;
                            continue;
                        }
                        destEmpty = true;
                    }

                    File.Move(sourcePath, destPath);
                    return true;
                }
                catch (FileNotFoundException)
                {
                    UtilityLogger.LogError($"Move target file {sourceFilename} could not be found");
                    return false;
                }
                catch (IOException ioex)
                {
                    if (ioex.Message.StartsWith("Sharing violation"))
                        UtilityLogger.LogError($"Move target file {sourceFilename} is currently in use");
                    handleException(ioex);
                }
                catch (Exception ex)
                {
                    handleException(ex);
                }
            }

            void handleException(Exception ex)
            {
                attemptsAllowed--;
                if (!exceptionLogged)
                {
                    UtilityLogger.LogError(ex);
                    exceptionLogged = true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="attemptsAllowed"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static bool TryMove(string sourcePath, string destPath, int attemptsAllowed, FileMoveOption option)
        {
        }

        public static bool TryReplace(string sourcePath, string destPath)
        {
            UtilityLogger.Log("Replacing file");

            if (PathUtils.IsEmpty(sourcePath) || PathUtils.IsEmpty(destPath))
            {
                UtilityLogger.LogError("Source or destination path is null");
                return false;
            }

            using (IAccessToken accessToken = TempFolder.Access())
            {
                if (!TempFolder.TryCreate())
                {
                    UtilityLogger.LogWarning("Unable to replace file. Temp directory could not be created.");
                    return false;
                }

                bool destEmpty = !File.Exists(destPath);
                if (destEmpty)
                {
                    UtilityLogger.Log("No file located at destination path");

                    //Attempt to move source file to destination
                    if (!TryMove(sourcePath, destPath))
                    {
                        UtilityLogger.LogWarning("Unable to move source file");
                        return false;
                    }
                }
                else
                {
                    string tempPath = prepareToMoveFile();

                    string prepareToMoveFile()
                    {
                        string destFilename = Path.GetFileName(destPath);
                        //Create the folder that will contain this file, before returning the full filepath
                        return Path.Combine(TempFolder.CreateDirectoryFor(destPath), destFilename);

                    }

                    //Check orphan status in case we overwrite one
                    bool isOrphanedFile = TempFolder.OrphanedFiles.Contains(tempPath);

                    //Attempt to move file to the temp folder
                    if (!TryMove(destPath, tempPath))
                        return false;

                    //File was overwritten - it is no longer considered orphaned
                    if (isOrphanedFile)
                        TempFolder.OrphanedFiles.Remove(tempPath);

                    //Attempt to move source file to destination
                    if (!TryMove(sourcePath, destPath))
                    {
                        UtilityLogger.LogWarning("Unable to move source file");

                        //If it fails, we move file at destination back
                        if (!TryMove(tempPath, destPath))
                        {
                            UtilityLogger.LogWarning("Unable to restore destination file");
                            TempFolder.OrphanedFiles.Add(tempPath);
                        }
                        return false;
                    }
                }
                UtilityLogger.Log("Move successful");
                return true;
            }
        }

        /// <summary>
        /// Attempts to write one or more strings to file
        /// </summary>
        /// <remarks>File is created, its contents are overwritten if it already exists</remarks>
        public static bool TryWrite(string filePath, params string[] values)
        {
            try
            {
                File.WriteAllLines(filePath, values);
                return true;
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Unable to write to file " + filePath, ex);
            }
            return false;
        }

        /// <inheritdoc cref="TryWrite(string, string[])"/>
        public static bool TryWrite(string filePath, IEnumerable<string> values)
        {
            return TryWrite(filePath, values.ToArray());
        }

        private static readonly object writeLock = new object();

        /// <inheritdoc cref="File.AppendAllText(string, string)"/>
        /// <remarks>
        /// - Appends a new line after specified string.<br/>
        /// - Write lock is applied internally for thread safety, but is not safe to run from multiple processes.
        /// </remarks>
        public static void AppendLine(string path, string contents)
        {
            AppendText(path, contents + Environment.NewLine);
        }

        /// <inheritdoc cref="File.AppendAllText(string, string)"/>
        /// <remarks>
        /// - Write lock is applied internally for thread safety, but is not safe to run from multiple processes.
        /// </remarks>
        public static void AppendText(string path, string contents)
        {
            /*
            using (FileStream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                stream.Seek(0, SeekOrigin.End);
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(message);
                }
            }
            */
            lock (writeLock)
            {
                File.AppendAllText(path, contents);
            }
        }
    }

    public enum FileMoveOption
    {
        None,
        OverwriteDestination,
        RenameSourceIfNecessary
    }
}
