﻿using System;
using System.IO;
using System.Linq;

namespace LogUtils.Helpers
{
    public static class FileUtils
    {
        public static string[] SupportedExtensions = { FileExt.LOG, FileExt.TEXT, FileExt.TEMP };

        public static void CreateTextFile(string filepath)
        {
            var stream = File.CreateText(filepath);

            stream.Close();
            stream = null;
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

        public static void WriteLine(string path, string message)
        {
            File.AppendAllText(path, message + Environment.NewLine);
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
