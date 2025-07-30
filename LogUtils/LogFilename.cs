using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using System;

namespace LogUtils
{
    public class LogFilename : IEquatable<LogFilename>, IComparable<LogFilename>, IEquatable<string>, IComparable<string>
    {
        /// <summary>
        /// The file extension belonging to the filename
        /// </summary>
        public readonly string Extension;

        /// <summary>
        /// The value of the filename without extension
        /// </summary>
        internal readonly string Value;

        /// <summary>
        /// Does the value represent a valid filename
        /// </summary>
        public readonly bool IsValid;

        /// <summary>
        /// Constructs a LogFilename instance
        /// </summary>
        /// <param name="value">A filename string without path information</param>
        /// <exception cref="ArgumentNullException">Value provided is null</exception>
        public LogFilename(string value) : this(FileUtils.RemoveExtension(value, out string fileExt), fileExt)
        {
        }

        /// <summary>
        /// Constructs a LogFilename instance
        /// </summary>
        /// <param name="value">A filename string without path information, or file extension</param>
        /// <param name="fileExt">A supported file extension to be used along with the filename. May be null</param>
        /// <exception cref="ArgumentNullException">Value provided is null</exception>
        public LogFilename(string value, string fileExt)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Value = value.Trim();
            IsValid = Value != string.Empty;

            if (fileExt != null && !FileUtils.IsSupportedExtension(fileExt))
            {
                fileExt = null;
                UtilityLogger.LogWarning("File extension is unsupported");
            }

            Extension = fileExt ?? FileExt.DEFAULT; //Treat no extension as the default
        }

        internal bool Equals(LogFilename filenameOther, bool hasBracketInfo)
        {
            string value = Value,
                   valueOther = filenameOther;

            FilenameComparer comparer = ComparerUtils.FilenameComparer;

            if (!hasBracketInfo)
                return Equals(filenameOther);

            if (comparer.Equals(value, valueOther))
                return true;

            //When the common checks fail to find the match, check the remaining value combinations
            string valueWithoutInfo = FileUtils.RemoveBracketInfo(Value),
                   valueWithoutInfoOther = FileUtils.RemoveBracketInfo(filenameOther);

            if (comparer.Equals(valueWithoutInfo, valueOther))
                return true;

            if (comparer.Equals(valueWithoutInfo, valueWithoutInfoOther))
                return true;

            return comparer.Equals(value, valueWithoutInfoOther);
        }

        /// <inheritdoc/>
        public int CompareTo(string filenameOther)
        {
            return ComparerUtils.FilenameComparer.Compare(Value, filenameOther, ignoreExtensions: true);
        }

        /// <inheritdoc/>
        public bool Equals(string filenameOther)
        {
            return ComparerUtils.FilenameComparer.Equals(Value, filenameOther, ignoreExtensions: true);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            LogFilename filenameConversion = obj as LogFilename;

            if (filenameConversion != null)
                return Equals(filenameConversion);

            string stringConversion = obj as string;

            if (stringConversion != null)
                return Equals(stringConversion);

            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public bool Equals(LogFilename filenameOther)
        {
            return ReferenceEquals(this, filenameOther) || Equals(filenameOther.Value);
        }

        /// <inheritdoc/>
        public int CompareTo(LogFilename filenameOther)
        {
            if (filenameOther == null)
                return 1;

            return CompareTo(filenameOther.Value);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// The filename as a string (including file extension)
        /// </summary>
        public string WithExtension() => Value + Extension;

        /// <summary>
        /// The filename as a string (without file extension)
        /// </summary>
        public override string ToString() => Value;

        protected static bool IsNull(LogFilename filename) => Equals(filename, null);

        public static bool operator ==(LogFilename filename, string filenameOther)
        {
            if (IsNull(filename))
                return filenameOther == null;

            return filename.Equals(filenameOther);
        }

        public static bool operator !=(LogFilename filename, string filenameOther)
        {
            if (IsNull(filename))
                return filenameOther != null;

            return !filename.Equals(filenameOther);
        }

        public static implicit operator string(LogFilename filename) => filename?.Value;
        public static explicit operator LogFilename(string filename) => filename != null ? new LogFilename(filename) : null;
    }
}
