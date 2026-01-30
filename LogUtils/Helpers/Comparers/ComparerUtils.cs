using System;
using System.Collections.Generic;

namespace LogUtils.Helpers.Comparers
{
    public static class ComparerUtils
    {
        /// <summary>
        /// Default implementation for comparing filenames
        /// </summary>
        public static readonly FilenameComparer FilenameComparer = new FilenameComparer(StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Default implementation for comparing file/folder paths
        /// </summary>
        public static readonly PathComparer PathComparer = new PathComparer(StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Default invariant case string comparer used by LogUtils
        /// </summary>
        public static readonly StringComparer StringComparerIgnoreCase = StringComparer.InvariantCultureIgnoreCase;

        public static StringComparer GetComparer(StringComparison compareOption)
        {
            #pragma warning disable IDE0055 //Fix formatting
            return compareOption switch
            {
                StringComparison.CurrentCulture             => StringComparer.CurrentCulture,
                StringComparison.CurrentCultureIgnoreCase   => StringComparer.CurrentCultureIgnoreCase,
                StringComparison.InvariantCulture           => StringComparer.InvariantCulture,
                StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
                StringComparison.Ordinal                    => StringComparer.Ordinal,
                StringComparison.OrdinalIgnoreCase          => StringComparer.OrdinalIgnoreCase,
                _ => throw new ArgumentException("Invalid comparison option"),
            };
            #pragma warning restore IDE0055 //Fix formatting
        }

        public static int GetNullSafeHashCode<T>(T value, IEqualityComparer<T> comparer)
        {
            if (value == null)
                return 0;

            if (comparer == null)
                return value.GetHashCode();

            return comparer.GetHashCode(value);
        }
    }
}
