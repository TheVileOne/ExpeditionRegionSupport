using System;
using System.IO;
using System.Linq;

namespace LogUtils.Helpers
{
    public static class FileUtils
    {
        public static string[] SupportedExtensions = { ".log", ".txt", ".tmp" };

        public static bool CompareFilenames(string filename, string filename2, bool ignoreExtensions = true)
        {
            if (filename == null)
                return filename2 == null;

            if (filename2 == null)
                return false;

            if (ignoreExtensions)
            {
                filename = RemoveExtension(filename);
                filename2 = RemoveExtension(filename);
            }

            //Strip any path info that may be present
            filename = Path.GetFileName(filename);
            filename2 = Path.GetFileName(filename2);

            return string.Equals(filename, filename2, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string RemoveExtension(string filename)
        {
            return Path.ChangeExtension(filename, string.Empty);
        }

        public static string GetExtension(string filename, bool normalize = true)
        {
            if (normalize)
                return Path.GetExtension(filename).ToLower();
            return Path.GetExtension(filename);
        }

        /// <summary>
        /// Returns true if string contains a file extension listed as a supported extension for the utility
        /// </summary>
        public static bool IsSupportedExtension(string filename)
        {
            return SupportedExtensions.Contains(GetExtension(filename));
        }

        public static bool ExtensionsMatch(string filename, string filename2)
        {
            string fileExt = GetExtension(filename);
            string fileExt2 = GetExtension(filename2);

            return fileExt == fileExt2;
        }
    }
}
