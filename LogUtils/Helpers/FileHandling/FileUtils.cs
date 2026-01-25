using LogUtils.Diagnostics;
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

        /// <summary>
        /// Attempts to delete a file at the specified path
        /// </summary>
        public static bool TryDelete(string path, string customErrorMsg = null)
        {
            try
            {
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

            while (attemptsAllowed > 0)
            {
                try
                {
                    File.Copy(sourcePath, destPath, true);
                    return true;
                }
                catch (Exception ex)
                {
                    FileSystemExceptionHandler handler = new FileSystemExceptionHandler(Path.GetFileName(sourcePath))
                    {
                        IsCopyContext = true,
                        Protocol = attemptsAllowed == 0 ? FailProtocol.LogAndIgnore : FailProtocol.FailSilently
                    };
                    handler.OnError(ex);

                    if (handler.CanContinue)
                        continue;
                    //Exception was of a form that is unlikely to complete on a reattempt
                    break;
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

            while (attemptsAllowed > 0)
            {
                try
                {
                    //Make sure destination is clear
                    if (!TryDelete(destPath))
                    {
                        attemptsAllowed--;
                        continue;
                    }

                    File.Move(sourcePath, destPath);
                    return true;
                }
                catch (Exception ex)
                {
                    attemptsAllowed--;
                    FileSystemExceptionHandler handler = new FileSystemExceptionHandler(Path.GetFileName(sourcePath))
                    {
                        IsCopyContext = false,
                        Protocol = attemptsAllowed == 0 ? FailProtocol.LogAndIgnore : FailProtocol.FailSilently
                    };
                    handler.OnError(ex);

                    if (handler.CanContinue)
                        continue;
                    //Exception was of a form that is unlikely to complete on a reattempt
                    break;
                }
            }
            return false;
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
}
