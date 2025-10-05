using System.IO;

namespace LogUtils.Helpers.FileHandling
{
    /// <summary>
    /// A helper class for manipulating file extension information
    /// </summary>
    public static class FileExtension
    {
        /// <summary>
        /// Case insensitive file extensions that are supports by LogUtils for use in log filenames, or log backup filenames
        /// </summary>
        public static string[] SupportedExtensions = { FileExt.LOG, FileExt.TEXT, FileExt.TEMP };

        /// <summary>
        /// The current standard applied to long file extension handling
        /// </summary>
        public static readonly LongExtensionSupport LongExtensionSupport = LongExtensionSupport.Ignore;

        /// <summary>
        /// Returns true if string contains a file extension listed as a supported extension for the utility
        /// </summary>
        public static bool IsSupported(string filename)
        {
            return FileExtensionInfo.FromFilename(filename).IsSupported;
        }

        /// <summary>
        /// Case insensitive comparison of the file extensions of two filenames
        /// </summary>
        public static bool Match(string filename, string filenameOther)
        {
            FileExtensionInfo extInfo = FileExtensionInfo.FromFilename(filename),
                              extInfoOther = FileExtensionInfo.FromFilename(filenameOther);
            return extInfo.Equals(extInfoOther);
        }

        /// <inheritdoc cref="Remove(string, out string)"/>
        public static string Remove(string filename)
        {
            return Remove(filename, out _);
        }

        /// <summary>
        /// Removes the file extension from the provided filename
        /// </summary>
        /// <param name="filename">The provided filename</param>
        /// <param name="fileExt">File extension information (including the period), stores an empty string if filename is null, or does not contain a file extension</param>
        /// <returns>The filename (or path) without the file extension</returns>
        /// <remarks>Filenames with long extensions may be unaffected by this operation</remarks>
        public static string Remove(string filename, out string fileExt)
        {
            if (filename == null)
            {
                fileExt = string.Empty;
                return null;
            }

            string result;
            switch (LongExtensionSupport)
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

        /// <summary>
        /// Replace the file extension of the provided filename with a provided file extension
        /// </summary>
        /// <param name="target">The provided filename</param>
        /// <param name="fileExt">The file extension to use</param>
        /// <returns>The filename (or path) with the provided file extension</returns>
        /// <remarks>Filenames with long extensions may be unaffected by this operation</remarks>
        public static string Replace(string target, string fileExt)
        {
            string result;
            switch (LongExtensionSupport)
            {
                case LongExtensionSupport.SupportedOnly:
                    result = LongFileExtensionUtils.ReplaceSupportedOnly(target, fileExt);
                    break;
                case LongExtensionSupport.Ignore:
                    result = LongFileExtensionUtils.ReplaceIgnore(target, fileExt);
                    break;
                default:
                case LongExtensionSupport.Full:
                    //result = LongFileExtensionUtils.ReplaceNoRestrictions(target, fileExt);
                    result = Path.ChangeExtension(target, fileExt);
                    break;
            }
            return result;
        }

        /// <summary>
        /// Takes the file extension from one filename and applies it to another filename
        /// </summary>
        /// <param name="transferFrom">The file extension source</param>
        /// <param name="transferTo">The file extension replace target</param>
        /// <returns>The filename (or path) with the applied file extension</returns>
        /// <remarks>Filenames with long extensions may be unaffected by this operation</remarks>
        public static string Transfer(string transferFrom, string transferTo)
        {
            return Replace(transferTo, Path.GetExtension(transferFrom));
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
