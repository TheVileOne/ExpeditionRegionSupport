using System;
using System.IO;

namespace LogUtils.Helpers
{
    public static class EqualityComparer
    {
        public static readonly FilenameEqualityComparer FilenameComparer = new FilenameEqualityComparer();
        public static readonly PathEqualityComparer PathComparer = new PathEqualityComparer();
        public static readonly StringComparer StringComparerIgnoreCase = StringComparer.InvariantCultureIgnoreCase;

        internal static int GetHashCode(object obj)
        {
            //I don't know how to hashcode - I guess this will work
            return obj?.GetHashCode() ?? 0;
        }

        public static StringComparer GetComparer(StringComparison compareOption)
        {
            switch (compareOption)
            {
                case StringComparison.CurrentCulture:
                    return StringComparer.CurrentCulture;
                case StringComparison.CurrentCultureIgnoreCase:
                    return StringComparer.CurrentCultureIgnoreCase;
                case StringComparison.InvariantCulture:
                    return StringComparer.InvariantCulture;
                case StringComparison.InvariantCultureIgnoreCase:
                    return StringComparer.InvariantCultureIgnoreCase;
                case StringComparison.Ordinal:
                    return StringComparer.Ordinal;
                case StringComparison.OrdinalIgnoreCase:
                    return StringComparer.OrdinalIgnoreCase;
            }
            throw new ArgumentException("Invalid comparison option");
        }
    }

    public class FilenameEqualityComparer : StringComparer
    {
        public override int Compare(string filename, string filenameOther)
        {
            return string.Compare(filename, filenameOther, true);
        }

        public bool Equals(string filename, string filenameOther, bool ignoreExtensions)
        {
            if (ignoreExtensions)
            {
                filename = FileUtils.RemoveExtension(filename);
                filenameOther = FileUtils.RemoveExtension(filename);
            }
            return Equals(filename, filenameOther);
        }

        public override bool Equals(string filename, string filenameOther)
        {
            if (filename == null)
                return filenameOther == null;

            if (filenameOther == null)
                return false;

            //The path is unimportant, this function is designed to evaluate the filename only
            filename = Path.GetFileName(filename);
            filenameOther = Path.GetFileName(filenameOther);

            return EqualityComparer.StringComparerIgnoreCase.Equals(filename, filenameOther);
        }

        public override int GetHashCode(string obj)
        {
            return EqualityComparer.GetHashCode(obj);
        }
    }

    public class PathEqualityComparer : StringComparer
    {
        public override int Compare(string path, string pathOther)
        {
            return string.Compare(path, pathOther, true);
        }

        public override bool Equals(string path, string pathOther)
        {
            //Make sure we are comparing path data, not keywords
            path = PathUtils.GetPathFromKeyword(path);
            pathOther = PathUtils.GetPathFromKeyword(path);

            return InternalEquals(path, pathOther);
        }

        public override int GetHashCode(string obj)
        {
            return EqualityComparer.GetHashCode(obj);
        }

        /// <summary>
        /// Assumes path info is being compared, not keywords
        /// </summary>
        internal bool InternalEquals(string path, string pathOther)
        {
            if (path == null)
                return pathOther == null;

            if (pathOther == null)
                return false;

            path = Path.GetFullPath(path).TrimEnd('\\');
            pathOther = Path.GetFullPath(pathOther).TrimEnd('\\');

            return EqualityComparer.StringComparerIgnoreCase.Equals(path, pathOther);
        }
    }
}
