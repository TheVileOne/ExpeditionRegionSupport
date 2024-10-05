using System;
using System.Collections.Generic;
using System.IO;

namespace LogUtils.Helpers
{
    public static class EqualityComparer
    {
        public static FilenameEqualityComparer FilenameComparer = new FilenameEqualityComparer();
        public static PathEqualityComparer PathComparer = new PathEqualityComparer();
    }

    public class StringComparer : IEqualityComparer<string>
    {
        public virtual bool Equals(string str, string strOther)
        {
            return string.Equals(str, strOther, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            //I don't know how to hashcode - I guess this will work
            return obj?.GetHashCode() ?? 0;
        }
    }

    public class FilenameEqualityComparer : StringComparer
    {
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

            return base.Equals(filename, filenameOther);
        }
    }

    public class PathEqualityComparer : StringComparer
    {
        public override bool Equals(string path, string pathOther)
        {
            //Make sure we are comparing path data, not keywords
            path = PathUtils.GetPathFromKeyword(path);
            pathOther = PathUtils.GetPathFromKeyword(path);

            return InternalEquals(path, pathOther);
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

            return base.Equals(path, pathOther);
        }
    }
}
