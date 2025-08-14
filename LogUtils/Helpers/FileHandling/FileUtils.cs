using LogUtils.Helpers.Comparers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils.Helpers.FileHandling
{
    public static class FileUtils
    {
        /// <summary>
        /// Used to attach information to a filename
        /// </summary>
        public const string BRACKET_FORMAT = "{0}[{1}]{2}";

        /// <summary>
        /// Case insensitive file extensions that are supports by LogUtils for use in log filenames, or log backup filenames
        /// </summary>
        public static string[] SupportedExtensions = { FileExt.LOG, FileExt.TEXT, FileExt.TEMP };

        public static void CreateTextFile(string filepath)
        {
            var stream = File.CreateText(filepath);

            stream.Close();
            stream = null;
        }

        /// <summary>
        /// Extracts the file extension from the filename. If there is no extension, this method returns null
        /// </summary>
        public static FileExtensionInfo GetExtensionInfo(string filename)
        {
            return new FileExtensionInfo(filename);
        }

        public static string RemoveExtension(string filename, bool supportLongExtensions = false)
        {
            FileExtensionInfo extInfo = GetExtensionInfo(filename);

            if (!supportLongExtensions && extInfo.IsLong)
                return filename;

            return Path.ChangeExtension(filename, null);
        }

        public static string RemoveExtension(string filename, out string fileExt, bool supportLongExtensions = false)
        {
            FileExtensionInfo extInfo = new FileExtensionInfo(filename);

            fileExt = extInfo.Extension;

            if (!supportLongExtensions && extInfo.IsLong)
                fileExt = string.Empty;

            if (fileExt == string.Empty)
                return filename;

            filename = filename.TrimEnd(); //Account for trailing whitespace
            return filename.Substring(0, filename.Length - fileExt.Length);
        }

        public static string TransferExtension(string transferFrom, string transferTo, bool supportLongExtensions = false)
        {
            FileExtensionInfo extInfoFrom = new FileExtensionInfo(transferFrom),
                              extInfoTo = new FileExtensionInfo(transferTo);

            if (!supportLongExtensions && (extInfoFrom.IsLong || extInfoTo.IsLong))
            {
                if (!extInfoFrom.IsLong) //extInfoTo must be long
                    return transferTo + extInfoFrom.Extension;

                //In any other situation, we cannot support the transfer of a new extension
                return transferTo;
            }
            return Path.ChangeExtension(transferTo, extInfoFrom.Extension);
        }

        /// <summary>
        /// Returns true if string contains a file extension listed as a supported extension for the utility
        /// </summary>
        public static bool IsSupportedExtension(string filename)
        {
            return GetExtensionInfo(filename).IsSupported;
        }

        public static bool ExtensionsMatch(string filename, string filenameOther)
        {
            FileExtensionInfo extInfo = GetExtensionInfo(filename),
                              extInfoOther = GetExtensionInfo(filenameOther);
            return extInfo.Equals(extInfoOther);
        }

        public static string ApplyBracketInfo(string filename, string info)
        {
            filename = RemoveExtension(filename, out string fileExt);
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

            FileExtensionInfo extInfo = GetExtensionInfo(filename);

            //Strips the bracket info at the end, while retaining the file extension
            return filename.Substring(0, bracketIndex) + (!extInfo.IsLong ? extInfo.Extension : string.Empty);
        }

        public static bool SafeDelete(string path, string customErrorMsg = null)
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
                return false;
            }
        }

        public static bool SafeCopy(string sourcePath, string destPath, int attemptsAllowed = 1)
        {
            string sourceFilename = Path.GetFileName(sourcePath);
            string destFilename = Path.GetFileName(destPath);

            UtilityLogger.Log($"Copying {sourceFilename} to {destFilename}");

            if (ComparerUtils.PathComparer.CompareFilenameAndPath(sourcePath, destPath, true) == 0)
            {
                UtilityLogger.LogError($"Copy target file {sourceFilename} cannot be copied to its source path");
                return false;
            }

            bool destEmpty = !File.Exists(destPath);
            bool exceptionLogged = false;
            while (attemptsAllowed > 0)
            {
                try
                {
                    //Make sure destination is clear
                    /*if (!destEmpty)
                    {
                        SafeDeleteFile(destPath);

                        if (File.Exists(destPath)) //File removal failed
                        {
                            attemptsAllowed--;
                            continue;
                        }
                        destEmpty = true;
                    }*/

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

        public static bool SafeMove(string sourcePath, string destPath, int attemptsAllowed = 1)
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
                        SafeDelete(destPath);

                        if (File.Exists(destPath)) //File removal failed
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

        public static void SafeWriteToFile(string filePath, params string[] values)
        {
            bool fileWriteSuccess = false;
            Exception fileWriteError = null;

            try
            {
                using (TextWriter writer = File.CreateText(filePath))
                {
                    foreach (string entry in values)
                        writer.WriteLine(entry);
                    writer.Close();
                }

                fileWriteSuccess = File.Exists(filePath);
            }
            catch (Exception ex)
            {
                fileWriteError = ex;
            }

            if (!fileWriteSuccess)
            {
                UtilityLogger.LogError("Unable to write to file " + filePath);

                if (fileWriteError != null)
                    UtilityLogger.LogError(fileWriteError);
            }
        }

        public static void SafeWriteToFile(string filePath, IEnumerable<string> values)
        {
            SafeWriteToFile(filePath, values.ToArray());
        }

        private static readonly object writeLock = new object();

        public static void WriteLine(string path, string message)
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
                File.AppendAllText(path, message + Environment.NewLine);
            }
        }
    }

    public static class FileExt
    {
        public const string LOG = ".log";
        public const string TEXT = ".txt";
        public const string TEMP = ".tmp";

        public const string DEFAULT = LOG;
    }
}
