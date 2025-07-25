﻿using BepInEx.Logging;
using LogUtils.Helpers;
using LogUtils.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LogUtils.Enums
{
    /// <summary>
    /// A type of LogCategory featuring properties of enum bitflags
    /// </summary>
    /// <remarks>LogCategory instances can be combined using overloaded bitflag operators to create a composite instance</remarks>
    public sealed class CompositeLogCategory : LogCategory
    {
        private static CompositeLogCategory _empty;
        public static CompositeLogCategory Empty
        {
            get
            {
                if (_empty == null)
                {
                    if (UtilitySetup.CurrentStep < UtilitySetup.InitializationStep.INITIALIZE_ENUMS)
                    {
                        UtilityLogger.Log("Too early to create a log category");
                        return null;
                    }
                    _empty = new CompositeLogCategory();
                }
                return _empty;
            }
        }

        internal static readonly HashSet<LogCategory> EmptySet = new HashSet<LogCategory>();

        /// <summary>
        /// Contains the flags that represent the composite instance
        /// </summary>
        internal readonly HashSet<LogCategory> Set = EmptySet;

        public int FlagCount => Set.Count;

        public bool IsEmpty => FlagCount == 0;

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
                if (FlagCount == 1)
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
                if (FlagCount == 1)
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

                if (FlagCount == 1)
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

        public override Color ConsoleColor
        {
            get
            {
                //Composite represents an exact category in this case
                if (!isInitialized || FlagCount == 1)
                    return base.ConsoleColor;

                if (IsEmpty)
                    return None.ConsoleColor;

                LogCategory[] mostRelevantFlags = GetMostRelevantFlags();

                //Flags with a non-default color set have priority over flags that inherit the default color for their specific LogGroup
                LogCategory flagWithColorOverride = Array.Find(mostRelevantFlags, flag => flag.HasColorOverride);

                if (flagWithColorOverride != null)
                    return flagWithColorOverride.ConsoleColor;

                return mostRelevantFlags[0].ConsoleColor;
            }
            set
            {
                if (!isInitialized || FlagCount == 1)
                {
                    base.ConsoleColor = value;
                    return;
                }
                UtilityLogger.Log("Setting ConsoleColor for a composite category is unsupported");
            }
        }

        internal CompositeLogCategory() : base(None.ToString(), false)
        {
            isInitialized = true;
        }

        internal CompositeLogCategory(HashSet<LogCategory> elements) : base(ToStringInternal(elements.Normalize()), false)
        {
            //"All", and "None" are special flags. These flags may not be grouped with other flags. LogUtils enforces this by restricting constructor access
            //"None" flag is removed when elements are normalized. For all intents and purposes, the "None" flag is treated as equivalent to a composite
            //with no flags.
            Set = elements ?? EmptySet;
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
        /// Combines any number of enum values into a composite LogCategory
        /// </summary>
        internal static CompositeLogCategory FromFlags(params LogLevel[] flags)
        {
            if (flags.Length == 0)
                return Empty;

            if (flags.Length == 1)
            {
                return new CompositeLogCategory(new HashSet<LogCategory>()
                {
                    GetEquivalent(flags[0])
                });
            }

            //Create a composite LogCategory from the available enum flags
            CompositeLogCategory composite = Empty;
            for (int i = 1; i < flags.Length; i++)
            {
                if (composite == Empty)
                {
                    composite = GetEquivalent(flags[i - 1]) | GetEquivalent(flags[i]);
                    continue;
                }

                //Value at i - 1 will already be part of the composition
                composite |= GetEquivalent(flags[i]);
            }
            return composite;
        }

        /// <summary>
        /// Combines any number of enum values into a composite LogCategory
        /// </summary>
        internal static CompositeLogCategory FromFlags(params LogType[] flags)
        {
            if (flags.Length == 0)
                return Empty;

            if (flags.Length == 1)
            {
                return new CompositeLogCategory(new HashSet<LogCategory>()
                {
                    GetEquivalent(flags[0])
                });
            }

            //Create a composite LogCategory from the available enum flags
            CompositeLogCategory composite = Empty;
            for (int i = 1; i < flags.Length; i++)
            {
                if (composite == Empty)
                {
                    composite = GetEquivalent(flags[i - 1]) | GetEquivalent(flags[i]);
                    continue;
                }

                //Value at i - 1 will already be part of the composition
                composite |= GetEquivalent(flags[i]);
            }
            return composite;
        }

        #region Search methods

        /// <summary>
        /// Finds all flags contained within the most relevant LogGroup for the composite instance
        /// </summary>
        public LogCategory[] GetMostRelevantFlags()
        {
            if (IsEmpty)
                return Array.Empty<LogCategory>();

            if (FlagCount == 1)
                return [Set.First()];

            //The broadest LogGroup is considered the most relevant
            LogGroup mostRelevantGroup = (LogGroup)FlagUtils.GetHighestBit((int)Group);

            return Set.Where(flag => (flag.Group & mostRelevantGroup) != 0).ToArray();
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

            //It shouldn't matter which flag is present - "All" represents every flag, except "None", which cannot exist in this set
            if (Set.Contains(All))
                return true;

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

            //It shouldn't matter which flag is present - "All" represents every flag, except "None", which cannot exist in this set
            if (Set.Contains(All))
                return true;

            if (flags.HasFlag(All))
                return !IsEmpty;

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
            if (Set.Contains(flag) || (searchOptions == FlagSearchOptions.MatchAny && flag == All))
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
        #endregion
        #region Object inherited methods
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            //ExtEnum value field for composites is the same as the joined string of its elements, but without any elements for LogCategory.None,
            //this override is necessary to ensure equality checks are consistent
            if (isInitialized && IsEmpty)
                return None.GetHashCode();
            return base.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return isInitialized ? ToStringInternal(Set) : base.ToString();
        }

        internal static string ToStringInternal(HashSet<LogCategory> set)
        {
            if (set.Count == 0)
                return None.ToString();

            return string.Join(", ", set);
        }
        #endregion
    }
}
