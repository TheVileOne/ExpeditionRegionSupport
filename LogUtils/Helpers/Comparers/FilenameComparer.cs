using LogUtils.Helpers.FileHandling;
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

        /// <inheritdoc cref="ComparerBase{T}.Compare(T, T)"/>
        public int Compare(string filename, string filenameOther, bool ignoreExtensions)
        {
            if (ignoreExtensions)
            {
                filename = FileExtension.Remove(filename);
                filenameOther = FileExtension.Remove(filenameOther);
            }
            return Compare(filename, filenameOther);
        }

        /// <inheritdoc/>
        public override int Compare(string filename, string filenameOther)
        {
            if (filename == null || filenameOther == null)
            {
                if (filename == filenameOther)
                    return 0;

                return filename == null ? int.MinValue : int.MaxValue;
            }

            //The path is unimportant, this function is designed to evaluate the filename only
            filename = Path.GetFileName(filename);
            filenameOther = Path.GetFileName(filenameOther);

            return base.Compare(filename, filenameOther);
        }

        /// <inheritdoc cref="ComparerBase{T}.Equals(T, T)"/>
        public bool Equals(string filename, string filenameOther, bool ignoreExtensions)
        {
            if (ignoreExtensions)
            {
                filename = FileExtension.Remove(filename);
                filenameOther = FileExtension.Remove(filenameOther);
            }
            return Equals(filename, filenameOther);
        }

        /// <inheritdoc/>
        public override bool Equals(string filename, string filenameOther)
        {
            if (filename == null || filenameOther == null)
                return filename == filenameOther;

            //The path is unimportant, this function is designed to evaluate the filename only
            filename = Path.GetFileName(filename);
            filenameOther = Path.GetFileName(filenameOther);

            return base.Equals(filename, filenameOther);
        }
    }
}
