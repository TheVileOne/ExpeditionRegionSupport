using LogUtils.Helpers;
using System;

namespace LogUtils.Enums
{
    /// <summary>
    /// <see cref="ByCategory"/><br/>
    /// Supports matching against specific <see cref="LogCategory"/> entries, and can function as a whitelist, or blacklist.<br/>
    /// This is the best option when you want to limit to a single <see cref="LogCategory"/> entry.<br/>
    /// <see cref="ByLevel"/><br/>
    /// Supports matching against, and up to a specific <see cref="LogCategoryLevels"/> value, the means to which logging categories are organized based on their importance<br/>
    /// or "severity" of the message. For instance, setting the filter to <see cref="LogCategoryLevels.Warning"/> will filter <see cref="LogCategoryLevels.Info"/> messages,
    /// but not <see cref="LogCategoryLevels.Error"/> messages.<br/>
    /// Supports matching against a specific <see cref="LogCategoryLevels"/> which contains all <see cref="LogCategory"/> instances belonging to that level.
    /// </summary>
    public static class LogCategoryFilter
    {

        /// <summary>
        /// See <see cref="LogCategoryFilter"/> for class description
        /// </summary>
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
                return allowEntryOnMatch == Flags.HasFlag(entry, FlagSearchOption.MatchAny);
            }

            /// <inheritdoc/>
            public int CompareTo(LogCategory entry)
            {
                return IsAllowed(entry) ? 1 : 0;
            }
        }

        /// <summary>
        /// See <see cref="LogCategoryFilter"/> for class description
        /// </summary>
        public class ByLevel : IFilter<LogCategory>
        {
            public LogCategoryLevels AllowedLevels;

            public ByLevel(LogCategoryLevels allowedLevels)
            {
                if (FlagUtils.HasMultipleFlags((int)allowedLevels))
                    UtilityLogger.Log("Chosen filter implementation does not support flagged options");

                //This logic ensures that AllowedLevels contains every flag less than it
                LogCategoryLevels filterMask = ~allowedLevels;
                AllowedLevels = allowedLevels ^ filterMask;
            }

            /// <inheritdoc/>
            public bool IsAllowed(LogCategory entry)
            {
                //Check that AllowedLevels contains a flag also contained in the entry group
                return (AllowedLevels & entry.Level) != 0;
            }

            /// <inheritdoc/>
            public int CompareTo(LogCategory entry)
            {
                return (int)(AllowedLevels & entry.Level);
            }
        }

        /// <summary>
        /// Allow filtering by one, or more specific category levels. Unlock <see cref="ByLevel"/> filtering, this filter does not operate on a range of values
        /// </summary>
        public class BySpecificLevel : IFilter<LogCategory>
        {
            public LogCategoryLevels AllowedLevels;

            public BySpecificLevel(LogCategoryLevels allowedLevels)
            {
                AllowedLevels = allowedLevels;
            }

            /// <inheritdoc/>
            public bool IsAllowed(LogCategory entry)
            {
                //Check that AllowedLevels contains a flag also contained in the entry group
                return (AllowedLevels & entry.Level) != 0;
            }

            /// <inheritdoc/>
            public int CompareTo(LogCategory entry)
            {
                return (int)(AllowedLevels & entry.Level);
            }
        }
    }

    public interface IFilter<T> : IComparable<T>
    {
        /// <summary>
        /// Check that an entry is allowed according to defined filter criteria
        /// </summary>
        bool IsAllowed(T entry);
    }
}
