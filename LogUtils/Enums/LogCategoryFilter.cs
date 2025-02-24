using LogUtils.Helpers;
using System;

namespace LogUtils.Enums
{
    public static class LogCategoryFilter
    {
        public class ByCategoryAllowed : IFilter<LogCategory>
        {
            public CompositeLogCategory CategoryFlags;

            public ByCategoryAllowed(LogCategory category)
            {
                CategoryFlags |= category;
            }

            public bool IsAllowed(LogCategory entry)
            {
                return CategoryFlags.Contains(entry);
            }

            public int CompareTo(LogCategory entry)
            {
                return CategoryFlags.Contains(entry) ? 1 : 0;
            }
        }

        public class ByCategoryUnallowed : IFilter<LogCategory>
        {
            public CompositeLogCategory CategoryFlags;

            public ByCategoryUnallowed(LogCategory category)
            {
                CategoryFlags |= category;
            }

            public bool IsAllowed(LogCategory entry)
            {
                return !CategoryFlags.Contains(entry);
            }

            public int CompareTo(LogCategory entry)
            {
                return CategoryFlags.Contains(entry) ? 0 : 1;
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

            public bool IsAllowed(LogCategory entry)
            {
                //Check that AllowedGroups contains a flag also container in the entry group
                return (AllowedGroups & entry.Group) != 0;
            }

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

            public bool IsAllowed(LogCategory entry)
            {
                //Check that AllowedGroups contains a flag also container in the entry group
                return (AllowedGroups & entry.Group) != 0;
            }

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
