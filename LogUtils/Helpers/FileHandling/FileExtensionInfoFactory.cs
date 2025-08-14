using System;
using System.IO;

namespace LogUtils
{
    public readonly partial struct FileExtensionInfo
    {
        /// <summary>
        /// Creates a new <see cref="FileExtensionInfo"/> object from a filename or path containing a filename 
        /// </summary>
        /// <exception cref="ArgumentException">The provided filename contains invalid path characters</exception>
        public static FileExtensionInfo FromFilename(string filename)
        {
            return new FileExtensionInfo(Path.GetExtension(filename));
        }

        /// <summary>
        /// Creates a new <see cref="FileExtensionInfo"/> object from a filename extension 
        /// </summary>
        public static FileExtensionInfo FromExtension(string extension)
        {
            return new FileExtensionInfo(extension);
        }
    }
}
