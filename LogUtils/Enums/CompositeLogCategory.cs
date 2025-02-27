using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LogUtils.Enums
{
    public sealed class CompositeLogCategory : LogCategory
    {
        internal readonly HashSet<LogCategory> Set;

        public bool IsEmpty => Set.Count == 0;

        private readonly bool isInitialized;

        public override LogLevel BepInExCategory
        {
            get
            {
                if (!isInitialized)
                    return base.BepInExCategory;

                if (IsEmpty)
                    return LogLevel.None;

                //When there is only one value, favor the unconverted value
                if (Set.Count == 1)
                {
                    LogCategory firstEntry = Set.First();
                    return firstEntry.BepInExCategory;
                }

                //LogLevel natively supports bitflags unlike LogType. For this reason we can freely assign the equivalent category directly 
                LogLevel flags = LogLevel.None;
                foreach (LogCategory category in Set)
                {
                    LogLevel flag = category.BepInExCategory;
                    if (!category.Registered && flag == LOG_LEVEL_DEFAULT)
                    {
                        //Default conversions for unregistered categories should be ignored - it will be confusing otherwise
                        UtilityLogger.Log("Unregistered categories do not support this operation");
                        continue;
                    }
                    flags |= flag;
                }

                //Note: The None flag cannot coexist with other flags by normal means.
                //This value indicates that no entries could contribute a valid flag value.
                //In this situation, we should still return a log compatible value, and the default works for this.
                if (flags == LogLevel.None)
                    return LOG_LEVEL_DEFAULT;

                return flags;
            }
        }

        public override LogType UnityCategory
        {
            get
            {
                if (!isInitialized)
                    return base.UnityCategory;

                if (IsEmpty)
                    return None.UnityCategory;

                //When there is only one value, favor the unconverted value
                if (Set.Count == 1)
                {
                    LogCategory firstEntry = Set.First();
                    return firstEntry.UnityCategory;
                }

                //When there is more than one value, each individual flag amounts to the value of the composite category
                int flagSum = 0;
                foreach (LogCategory category in Set)
                {
                    if (!category.Registered)
                    {
                        //Default conversions for unregistered categories should be ignored - it will be confusing otherwise
                        UtilityLogger.Log("Unregistered categories do not support this operation");
                        continue;
                    }
                    flagSum += category.FlagValue;
                }

                //Similar check as with LogLevel, but with a composite LogType all values must be conversion values, and that means they cannot sum to zero
                if (flagSum == 0)
                    return LOG_TYPE_DEFAULT;

                return (LogType)flagSum;
            }
        }

        /// <summary>
        /// The combined bitflag translation of all contained flags within this instance
        /// </summary>
        public override int FlagValue
        {
            get
            {
                if (!isInitialized)
                    return base.FlagValue;

                if (IsEmpty)
                    return None.FlagValue;

                if (Set.Count == 1)
                {
                    LogCategory firstEntry = Set.First();
                    return firstEntry.FlagValue;
                }

                int flagSum = 0;
                foreach (LogCategory category in Set)
                {
                    if (!category.Registered)
                    {
                        //Default conversions for unregistered categories should be ignored - the flag value will be negative
                        UtilityLogger.Log("Unregistered categories do not support this operation");
                        continue;
                    }
                    flagSum += category.FlagValue;
                }
                return flagSum;
            }
        }

        public override LogGroup Group
        {
            get
            {
                if (!isInitialized)
                    return base.Group;

                //TODO: Make sure that it isn't possible to desync this output since this code doesn't reference ManagedReference
                LogGroup flags = LogGroup.None;
                foreach (LogCategory category in Set)
                {
                    flags |= category.Group;
                }
                return flags;
            }
        }

        internal CompositeLogCategory(HashSet<LogCategory> elements) : base(ToStringInternal(elements), false)
        {
            Set = elements ?? new HashSet<LogCategory>();
            isInitialized = true;
        }

        public override void Register()
        {
            if (isInitialized)
            {
                UtilityLogger.LogWarning("Registration of composite ExtEnum entries is not supported");
                return;
            }
            base.Register(); //This is a special case - the composite represents a single value entry
        }

        /// <summary>
        /// Breaks the composite ExtEnum back into its component elements
        /// </summary>
        public LogCategory[] Deconstruct()
        {
            LogCategory[] elements = new LogCategory[Set.Count];
            Set.CopyTo(elements);
            return elements;
        }

        /// <summary>
        /// Checks whether this instance contains the specified flag element
        /// </summary>
        /// <param name="flag">The element to look for</param>
        /// <returns>true, if the flag element is contained by this instance; otherwise, false</returns>
        public bool Contains(LogCategory flag)
        {
            //This is intentionally not using HasFlag implementation - does not involve any edge checks
            return Set.Contains(flag);
        }

        /// <summary>
        /// Checks whether this instance contains all elements specified within the composite flag
        /// </summary>
        /// <param name="flags">Contains a set of flags to look for</param>
        /// <returns>true, if all flag elements are contained by this instance; otherwise, false</returns>
        public bool Contains(CompositeLogCategory flags)
        {
            return HasAll(flags);
        }

        /// <summary>
        /// Checks whether this instance contains every element of another compatible composite instance
        /// </summary>
        /// <param name="flags">Contains a set of flags to look for</param>
        /// <returns>true, if all flag elements are contained by this instance; otherwise, false</returns>
        public bool HasAll(CompositeLogCategory flags)
        {
            if (flags == null || flags.IsEmpty) return false;

            //TODO: The code should be hardcoded to not allow the None flag to be stored inside a composite - verify that this is true
            //"All" flag represents all flags except "None", but cannot be compared like other flags
            if (Set.Contains(All))
                return !flags.HasFlag(None);

            return Set.IsSupersetOf(flags.Set);
        }

        /// <summary>
        /// Checks whether this instance contains any element of another compatible composite instance
        /// </summary>
        /// <param name="flags">Contains a set of flags to look for</param>
        /// <returns>true, if at least one flag element is contained by this instance; otherwise, false</returns>
        public bool HasAny(CompositeLogCategory flags)
        {
            if (flags == null || flags.IsEmpty) return false;

            //TODO: The code should be hardcoded to not allow the None flag to be stored inside a composite - verify that this is true
            //"All" flag represents all flags except "None", but cannot be compared like other flags
            if (Set.Contains(All))
                return !flags.HasFlag(None);

            if (flags.HasFlag(All))
                return !IsEmpty && !Set.Contains(None);

            return Set.Overlaps(flags.Set);
        }

        /// <summary>
        /// Checks whether this instance contains a flag as a whole value, or elements of the flag based on a specified search behavior 
        /// </summary>
        /// <param name="flag">Contains a flag, or set of flags to look for</param>
        /// <param name="searchOptions">Specifies the search behavior when the flag represents multiple elements, has no effect when there is only one element</param>
        /// <returns>true, when an element has been matched based on the provided search criteria</returns>
        public bool HasFlag(LogCategory flag, FlagSearchOptions searchOptions = FlagSearchOptions.MatchAll)
        {
            if (IsEmpty) return false;

            //It is highly unlikely we are comparing against another composite
            if (Set.Contains(flag) || (searchOptions == FlagSearchOptions.MatchAny && flag == All && !Set.Contains(None)))
                return true;

            CompositeLogCategory flags = flag as CompositeLogCategory;

            if (flags == null)
                return false;

            //Check whether we want to match a single flag, or every flag
            return searchOptions switch
            {
                FlagSearchOptions.MatchAll => HasAll(flags),
                FlagSearchOptions.MatchAny => HasAny(flags),
                _ => false
            };
        }

        public enum FlagSearchOptions
        {
            MatchAll,
            MatchAny
        }

        #region Object inherited methods
        public override int GetHashCode()
        {
            //ExtEnum value field for composites is the same as the joined string of its elements, but without any elements for LogCategory.None,
            //this override is necessary to ensure equality checks are consistent
            if (isInitialized && IsEmpty)
                return None.GetHashCode();
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return isInitialized ? ToStringInternal(Set) : base.ToString();
        }

        internal static string ToStringInternal(HashSet<LogCategory> set)
        {
            if (set.Count == 0)
                return None.ToString();

            return string.Join(" | ", set);
        }
        #endregion
    }
}
