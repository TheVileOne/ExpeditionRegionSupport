using LogUtils.Helpers.FileHandling;
using System;
using System.Linq;

namespace LogUtils
{
    public readonly partial struct FileExtensionInfo : IEquatable<FileExtensionInfo>
    {
        /// <summary>
        /// The minimum amount of characters (including the period) to satisfy the long file extension property
        /// </summary>
        public const short MIN_LONG_EXTENSION_LENGTH = 6;

        /// <summary>
        /// Is the file extension null, or empty
        /// </summary>
        public readonly bool IsEmpty;

        /// <summary>
        /// Is the file extension in a comparison neutral format (i.e. all lowercase)
        /// </summary>
        public readonly bool IsNormalized;

        /// <summary>
        /// Is the file extension supported by LogUtils
        /// </summary>
        public bool IsSupported => FileExtension.SupportedExtensions.Contains(Normalize());

        /// <summary>
        /// Does the file extension exceed a set amount of characters determined by the utility
        /// </summary>
        public readonly bool IsLong => Extension.Length >= MIN_LONG_EXTENSION_LENGTH;

        /// <summary>
        /// The value of the file extension
        /// </summary>
        public readonly string Extension;

        /// <summary>
        /// Creates a new <see cref="FileExtensionInfo"/> object with no extension information
        /// </summary>
        public FileExtensionInfo()
        {
            IsEmpty = true;
            IsNormalized = true;
            Extension = string.Empty;
        }

        private FileExtensionInfo(string extension)
        {
            string fileExt = extension?.TrimEnd();

            IsEmpty = string.IsNullOrEmpty(fileExt);

            if (IsEmpty)
            {
                IsNormalized = true;
                Extension = string.Empty;
                return;
            }
            Extension = fileExt;
            IsNormalized = string.Equals(Extension, Extension.ToLower());
        }

        /// <summary>
        /// Converts the file extension into a comparison neutral format (i.e. all lowercase)
        /// </summary>
        public string Normalize() => IsNormalized ? Extension : Extension.ToLower();

        /// <inheritdoc/>
        public bool Equals(FileExtensionInfo other) => string.Equals(Normalize(), other.Normalize());

        /// <inheritdoc/>
        public override string ToString() => Extension;
    }

    public enum LongExtensionSupport
    {
        /// <summary>
        /// Long file extensions of any form should be ignored
        /// </summary>
        Ignore,
        /// <summary>
        /// Long file extensions that are not listed as a supported file extension are ignored
        /// </summary>
        SupportedOnly,
        /// <summary>
        /// Long file extensions of any form are allowed
        /// </summary>
        Full
    }
}
