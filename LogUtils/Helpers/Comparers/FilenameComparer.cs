using System;
using System.IO;

namespace LogUtils.Helpers.Comparers
{
    public class FilenameComparer : ComparerBase<string>
    {
        public FilenameComparer() : base()
        {
        }

        public FilenameComparer(StringComparison comparisonOption) : base(comparisonOption)
        {
        }

        public int Compare(string filename, string filenameOther, bool ignoreExtensions)
        {
            if (ignoreExtensions)
            {
                filename = FileUtils.RemoveExtension(filename);
                filenameOther = FileUtils.RemoveExtension(filename);
            }
            return Compare(filename, filenameOther);
        }

        public override int Compare(string filename, string filenameOther)
        {
            if (filename == null)
                return filenameOther != null ? int.MinValue : 0;

            if (filenameOther == null)
                return int.MaxValue;

            //The path is unimportant, this function is designed to evaluate the filename only
            filename = Path.GetFileName(filename);
            filenameOther = Path.GetFileName(filenameOther);

            return base.Compare(filename, filenameOther);
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

            return base.Equals(filename, filenameOther);
        }
    }
}
