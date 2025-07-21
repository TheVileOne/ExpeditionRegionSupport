using System;

namespace LogUtils.Helpers.Comparers
{
    public static class ComparerUtils
    {
        public static readonly FilenameComparer FilenameComparer = new FilenameComparer(StringComparison.InvariantCultureIgnoreCase);
        public static readonly PathComparer PathComparer = new PathComparer(StringComparison.InvariantCultureIgnoreCase);
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
    }
}
