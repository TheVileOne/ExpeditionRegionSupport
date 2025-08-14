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

        /// <summary>
        /// The current standard applied to long file extension handling
        /// </summary>
        private static readonly LongExtensionSupport longExtensionSupport = LongExtensionSupport.Ignore; 

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

        public static string RemoveExtension(string filename)
        {
            return RemoveExtension(filename, out _);
        }

        public static string RemoveExtension(string filename, out string fileExt)
        {
            if (filename == null)
            {
                fileExt = string.Empty;
                return null;
            }

            string result;
            switch (longExtensionSupport)
            {
                case LongExtensionSupport.SupportedOnly:
                    result = LongFileExtensionUtils.RemoveSupportedOnly(filename, out fileExt);
                    break;
                case LongExtensionSupport.Ignore:
                    result = LongFileExtensionUtils.RemoveIgnore(filename, out fileExt);
                    break;
                default:
                case LongExtensionSupport.Full:
                    //result = LongFileExtensionUtils.RemoveNoRestrictions(filename, out fileExt);
                    int extIndex = filename.LastIndexOf('.');

                    if (extIndex >= 0)
                    {
                        fileExt = filename.Substring(extIndex);
                        result = filename.Substring(0, extIndex);
                    }
                    else
                    {
                        fileExt = string.Empty;
                        result = filename;
                    }
                    break;
            }
            return result;
        }

        public static string TransferExtension(string transferFrom, string transferTo)
        {
            string result;
            switch (longExtensionSupport)
            {
                case LongExtensionSupport.SupportedOnly:
                    result = LongFileExtensionUtils.TransferSupportedOnly(transferFrom, transferTo);
                    break;
                case LongExtensionSupport.Ignore:
                    result = LongFileExtensionUtils.TransferIgnore(transferFrom, transferTo);
                    break;
                case LongExtensionSupport.Full:
                    //result = LongFileExtensionUtils.TransferNoRestrictions(transferFrom, transferTo);
                    result = Path.ChangeExtension(transferTo, Path.GetExtension(transferFrom));
                    break;
                default:
                    result = transferTo;
                    break;
            }
            return result;
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

            bool useExtension;
            switch (longExtensionSupport)
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

    internal static class LongFileExtensionUtils
    {
        internal static string RemoveIgnore(string target, out string fileExt)
        {
            FileExtensionInfo extInfo = new FileExtensionInfo(target);

            bool extensionCanBeRemoved = !extInfo.IsEmpty && !extInfo.IsLong;

            if (!extensionCanBeRemoved)
            {
                //Since we cannot extract file extension info, we will default to an empty string
                fileExt = string.Empty;
                return target;
            }

            //Assign the correct file extension, and return the target substring without the extension
            fileExt = extInfo.Extension;
            target = target.TrimEnd(); //Account for trailing whitespace

            int newTargetLength = target.Length - fileExt.Length;
            return target.Substring(0, newTargetLength);
        }

        internal static string RemoveSupportedOnly(string target, out string fileExt)
        {
            FileExtensionInfo extInfo = new FileExtensionInfo(target);

            bool extensionCanBeRemoved = !extInfo.IsEmpty && (!extInfo.IsLong || extInfo.IsSupported);

            if (!extensionCanBeRemoved)
            {
                //Since we cannot extract file extension info, we will default to an empty string
                fileExt = string.Empty;
                return target;
            }

            //Assign the correct file extension, and return the target substring without the extension
            fileExt = extInfo.Extension;
            target = target.TrimEnd(); //Account for trailing whitespace

            int newTargetLength = target.Length - fileExt.Length;
            return target.Substring(0, newTargetLength);
        }

        internal static string TransferIgnore(string transferFrom, string transferTo)
        {
            FileExtensionInfo extensionFrom = new FileExtensionInfo(transferFrom),
                              extensionTo = new FileExtensionInfo(transferTo);

            bool extensionCanBeProvided = !extensionFrom.IsLong;
            bool extensionCanBeReplaced = !extensionTo.IsLong;

            if (extensionCanBeProvided)
            {
                if (extensionCanBeReplaced)
                    return Path.ChangeExtension(transferTo, extensionFrom.Extension);
                return transferTo + extensionFrom.Extension;
            }
            return transferTo;
        }

        internal static string TransferSupportedOnly(string transferFrom, string transferTo)
        {
            FileExtensionInfo extensionFrom = new FileExtensionInfo(transferFrom),
                              extensionTo = new FileExtensionInfo(transferTo);
            /*
             * A file extension must satisfy one of these conditions to be provided, or replaced
             * I.  The file extension is not a long file extension
             * II. The file extension is a supported extension (i.e. LogUtils recognizes and supports the extension)
             */
            bool extensionCanBeProvided = !extensionFrom.IsLong || extensionFrom.IsSupported;
            bool extensionCanBeReplaced = !extensionTo.IsLong || extensionTo.IsSupported;

            if (extensionCanBeProvided)
            {
                if (extensionCanBeReplaced)
                    return Path.ChangeExtension(transferTo, extensionFrom.Extension);
                return transferTo + extensionFrom.Extension;
            }
            return transferTo;
        }
    }
}
