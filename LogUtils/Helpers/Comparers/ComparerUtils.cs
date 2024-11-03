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
}
