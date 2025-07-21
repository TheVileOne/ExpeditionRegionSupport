using LogUtils.Helpers;
using System;

namespace LogUtils.Enums
{
    public static class LogCategoryFilter
    {
        /*
         * ByCategory
         * Supports matching against specific LogCategory entries, and can function as a whitelist, or blacklist
         * This is the best option when you want to limit to a single LogCategory entry
         * ByGroup
         * Supports matching against, and up to a specific LogGroup value, the means to which logging categories are organized based on their importance
         * or "severity" of the message. For instance, setting the filter to LogGroup.Warning will filter LogGroup.Info messages. but not LogGroup.Error
         * messages. 
         * Supports matching against a specific LogGroup which contains all LogCategory instances belonging to that group.
         */
        public class ByCategory : IFilter<LogCategory>
        {
            public CompositeLogCategory Flags;

            private readonly bool allowEntryOnMatch;

            public ByCategory(LogCategory category, bool useAsWhitelist)
            {
                Flags |= category;
                allowEntryOnMatch = useAsWhitelist;
            }

            /// <inheritdoc/>
            public bool IsAllowed(LogCategory entry)
            {
                return allowEntryOnMatch == Flags.HasFlag(entry, CompositeLogCategory.FlagSearchOptions.MatchAny);
            }

            /// <inheritdoc/>
            public int CompareTo(LogCategory entry)
            {
                return IsAllowed(entry) ? 1 : 0;
            }
        }

        public class ByGroup : IFilter<LogCategory>
        {
            public LogGroup AllowedGroups;

            public ByGroup(LogGroup allowedGroups)
            {
                if (FlagUtils.HasMultipleFlags((int)allowedGroups))
                    UtilityLogger.Log("Chosen filter implementation does not support flagged options");

                //This logic ensures that AllowedGroups contains every flag less than it
                LogGroup filterMask = ~allowedGroups;
                AllowedGroups = allowedGroups ^ filterMask;
            }

            /// <inheritdoc/>
            public bool IsAllowed(LogCategory entry)
            {
                //Check that AllowedGroups contains a flag also contained in the entry group
                return (AllowedGroups & entry.Group) != 0;
            }

            /// <inheritdoc/>
            public int CompareTo(LogCategory entry)
            {
                return (int)(AllowedGroups & entry.Group);
            }
        }

        public class BySpecificGroup : IFilter<LogCategory>
        {
            public LogGroup AllowedGroups;

            public BySpecificGroup(LogGroup allowedGroups)
            {
                AllowedGroups = allowedGroups;
            }

            /// <inheritdoc/>
            public bool IsAllowed(LogCategory entry)
            {
                //Check that AllowedGroups contains a flag also contained in the entry group
                return (AllowedGroups & entry.Group) != 0;
            }

            /// <inheritdoc/>
            public int CompareTo(LogCategory entry)
            {
                return (int)(AllowedGroups & entry.Group);
            }
        }
    }

    public interface IFilter<T> : IComparable<T>
    {
        bool IsAllowed(T entry);
    }
}
