using System.IO;
using System.Linq;

namespace LogUtils
{
    public static class FileUtils
    {
        public static string[] SupportedExtensions = { ".log", ".txt" };

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
