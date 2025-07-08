using BepInEx.Logging;
using LogUtils.Console;
using LogUtils.Helpers.Extensions;
using System;
using System.Linq;
using UnityEngine;

namespace LogUtils.Enums
{
    public partial class LogCategory : SharedExtEnum<LogCategory>
    {
        /// <summary>
        /// The default conversion type for LogLevel enum
        /// </summary>
        public const LogLevel LOG_LEVEL_DEFAULT = LogLevel.Info;

        /// <summary>
        /// The default conversion type for LogType enum
        /// </summary>
        public const LogType LOG_TYPE_DEFAULT = LogType.Log;

#pragma warning disable IDE0055 //Fix formatting
        private LogLevel  _bepInExConversion = LOG_LEVEL_DEFAULT;
        private LogType   _unityConversion = LOG_TYPE_DEFAULT;
        private LogGroup  _defaultGroup = LogGroupMap.DefaultGroup;
        private Color     _defaultConsoleColor = ConsoleColorMap.DefaultColor;
        private LogGroup? _userDefinedGroup;
        private LogGroup? _userDefinedColorGroup;
        private Color?    _userDefinedConsoleColor;
#pragma warning restore IDE0055 //Fix formatting

        private bool conversionFieldsNeedUpdating;
        private bool defaultsNeedUpdating;

        /// <summary>
        /// The category value translated to the category enum used for BepInEx logging
        /// </summary>
        public virtual LogLevel BepInExCategory
        {
            get
            {
                if (!ReferenceEquals(ManagedReference, this))
                    return ManagedReference.BepInExCategory;
                return _bepInExConversion;
            }
        }

        /// <summary>
        /// The category value translated to the category enum used for Unity logging
        /// </summary>
        public virtual LogType UnityCategory
        {
            get
            {
                if (!ReferenceEquals(ManagedReference, this))
                    return ManagedReference.UnityCategory;
                return _unityConversion;
            }
        }

        /// <summary>
        /// The bitflag translation representing this LogCategory
        /// </summary>
        public virtual int FlagValue => indexToConversionValue();

        /// <summary>
        /// The overall logging group defined for this category - used for tasks such as filtering by one or more categories
        /// </summary>
        public virtual LogGroup Group
        {
            get
            {
                if (!ReferenceEquals(ManagedReference, this))
                    return ManagedReference.Group;

                return _userDefinedGroup.HasValue ? _userDefinedGroup.Value : _defaultGroup;
            }
            set
            {
                if (!ReferenceEquals(ManagedReference, this))
                    ManagedReference.Group = value;
                _userDefinedGroup = value;
            }
        }

        /// <summary>
        /// The color that will be used in the console, or other write implementation that supports text coloration  
        /// </summary>
        public virtual Color ConsoleColor
        {
            get
            {
                if (!ReferenceEquals(ManagedReference, this))
                    return ManagedReference.ConsoleColor;

                return _userDefinedConsoleColor.HasValue ? _userDefinedConsoleColor.Value : _defaultConsoleColor;
            }
            set
            {
                if (!ReferenceEquals(ManagedReference, this))
                    ManagedReference.ConsoleColor = value;
                _userDefinedConsoleColor = value;
            }
        }

        internal bool HasColorOverride => ConsoleColor != _defaultConsoleColor;

        public static LogCategory[] RegisteredEntries => values.entries.Select(entry => new LogCategory(entry)).ToArray();

        public static ICombiner<LogCategory, CompositeLogCategory> Combiner = new LogCategoryCombiner();

        public static IFilter<LogCategory> GlobalFilter;

        /// <summary>
        /// Constructs a registered LogCategory instance
        /// </summary>
        /// <param name="value">The ExtEnum value associated with this instance</param>
        /// <param name="bepInExEquivalent">
        /// The enum value to be used when this category is used by the BepInEx logger
        /// Set to null assigns a custom LogLevel, otherwise will take the value of the LogLevel provided
        /// </param>
        /// <param name="unityEquivalent">
        /// The enum value to be used when this category is used by the Unity logger
        /// Set to null assigns a custom LogType, otherwise will take the value of the LogType provided
        /// </param>
        public LogCategory(string value, LogLevel? bepInExEquivalent, LogType? unityEquivalent) : base(value, true)
        {
            if (!ReferenceEquals(ManagedReference, this))
            {
                //When the managed reference is registered at the same time as this instance, propagate constructor parameters to the managed reference
                if (ManagedReference.conversionFieldsNeedUpdating)
                {
                    int customValue = indexToConversionValue();
                    ManagedReference.updateConversionFields(bepInExEquivalent ?? (LogLevel)customValue, unityEquivalent ?? (LogType)customValue);
                    ManagedReference.updateDefaults();
                }

                //Make sure that backing fields always have the same values as the managed reference
                updateConversionFields(ManagedReference._bepInExConversion, ManagedReference._unityConversion);
            }
            else
            {
                int customValue = indexToConversionValue();
                updateConversionFields(bepInExEquivalent ?? (LogLevel)customValue, unityEquivalent ?? (LogType)customValue);
            }
            updateDefaults();
        }

        /// <summary>
        /// Constructs a LogCategory instance
        /// </summary>
        /// <param name="value">The ExtEnum value associated with this instance</param>
        /// <param name="register">Whether or not this instance should be registered as a unique ExtEnum entry</param>
        public LogCategory(string value, bool register = false) : base(value, register)
        {
            //We can only give custom conversions to registered instances because these instances have a valid index assigned
            if (Registered)
            {
                if (!ReferenceEquals(ManagedReference, this))
                {
                    if (ManagedReference.conversionFieldsNeedUpdating)
                    {
                        int customValue = indexToConversionValue();
                        ManagedReference.updateConversionFields((LogLevel)customValue, (LogType)customValue);
                        ManagedReference.updateDefaults();
                    }

                    //Make sure that backing fields always have the same values as the managed reference
                    updateConversionFields(ManagedReference._bepInExConversion, ManagedReference._unityConversion);
                }
                else
                {
                    int customValue = indexToConversionValue();
                    updateConversionFields((LogLevel)customValue, (LogType)customValue);
                }
                updateDefaults();
            }
        }

        /// <inheritdoc/>
        public override void Register()
        {
            //When a registration is applied, set a flag that allows conversion fields to be overwritten
            if (!ManagedReference.Registered || BaseIndex < 0) //Base index is checked here, because ManagedReference index would be used if we didn't
                conversionFieldsNeedUpdating = true;

            base.Register();
        }

        private int indexToConversionValue()
        {
            if (BaseIndex < 0) return -1; //Base index is checked here, because ManagedReference index would be used if we didn't

            //Map the registration index to an unmapped region of values designated for custom enum values
            //Assigned value will be a valid bit flag position
            return (int)Math.Min(CONVERSION_OFFSET * Math.Pow(BaseIndex, 2), int.MaxValue);
        }

        private void updateConversionFields(LogLevel logLevel, LogType logType)
        {
            defaultsNeedUpdating = _bepInExConversion != logLevel;

            _bepInExConversion = logLevel;
            _unityConversion = logType;

            conversionFieldsNeedUpdating = false;
        }

        private void updateDefaults()
        {
            if (defaultsNeedUpdating)
            {
                LogLevel category = BepInExCategory;
                _defaultGroup = LogGroupMap.GetEquivalent(category, true);
                _defaultConsoleColor = ConsoleColorMap.GetColor(_defaultGroup);
            }
            defaultsNeedUpdating = false;
        }

        /// <summary>
        /// Determines if the logging context is general purpose, or targets an exception
        /// </summary>
        /// <returns>The proper LogID for the provided logging context</returns>
        public static LogID GetUnityLogID(LogType logType)
        {
            return !IsErrorCategory(logType) ? LogID.Unity : LogID.Exception;
        }

        public static bool IsAllCategory(LogCategory category)
        {
            if (category == All)
                return true;

            var composite = category as CompositeLogCategory;

            return composite != null && composite.Contains(All);
        }

        public static bool IsAllCategory(LogType category)
        {
            return category.HasConvertedFlags() && (category & All.UnityCategory) != 0;
        }

        public static bool IsAllCategory(LogLevel category)
        {
            return category == LogLevel.All;
        }

        public static bool IsErrorCategory(LogCategory category)
        {
            var composite = category as CompositeLogCategory;

            if (composite != null)
            {
                //Exclude the All flag here - not relevant to error handling
                return !composite.Contains(All) && composite.HasAny(ErrorFlags);
            }
            return ErrorFlags.Contains(category);
        }

        public static bool IsErrorCategory(LogType category)
        {
            return category == LogType.Error || category == LogType.Exception || (category.HasConvertedFlags() && (category & ErrorFlags.UnityCategory) != 0);
        }

        public static bool IsErrorCategory(LogLevel category)
        {
            if (category == LogLevel.All) //This flag value will pass the error flag check
                return false;

            LogLevel errorFlags = category.HasConvertedFlags() ? ErrorFlags.BepInExCategory : (LogLevel.Error | LogLevel.Fatal);

            return (category & errorFlags) != 0;
        }

        public static CompositeLogCategory operator |(LogCategory a, LogCategory b)
        {
            return Combiner.Combine(a, b);
        }

        public static CompositeLogCategory operator &(LogCategory a, LogCategory b)
        {
            return Combiner.Intersect(a, b);
        }

        public static CompositeLogCategory operator ^(LogCategory a, LogCategory b)
        {
            return Combiner.Distinct(a, b);
        }

        public static LogCategory operator ~(LogCategory target)
        {
            return Combiner.GetComplement(target);
        }

        static LogCategory()
        {
            UtilityCore.EnsureInitializedState();
        }

#pragma warning disable IDE0055 //Fix formatting
        internal static void InitializeEnums()
        {
            //Registration order reflects the importance of the message category (errors being most important, and debug messages being least important)
            None      = new LogCategory("None",      LogLevel.None, null);
            Fatal     = new LogCategory("Fatal",     LogLevel.Fatal, LogType.Error);
            Error     = new LogCategory("Error",     LogLevel.Error, LogType.Error);
            Warning   = new LogCategory("Warning",   LogLevel.Warning, LogType.Warning);
            Assert    = new LogCategory("Assert",    null, LogType.Assert);
            Important = new LogCategory("Important", null, null);
            Message   = new LogCategory("Message",   LogLevel.Message, LogType.Log);
            Info      = new LogCategory("Info",      LogLevel.Info, LogType.Log);
            Debug     = new LogCategory("Debug",     LogLevel.Debug, null);
            All       = new LogCategory("All",       LogLevel.All, null);

            ErrorFlags = Error | Fatal; //TODO: Include Exception
            Default = Info;
        }
#pragma warning restore IDE0055 //Fix formatting

#pragma warning disable CS1591 //Missing XML comment for publicly visible type or member
        public static LogCategory All;
        public static LogCategory None;
        public static LogCategory Assert;
        public static LogCategory Debug;
        public static LogCategory Info;
        public static LogCategory Message;
        public static LogCategory Important;
        public static LogCategory Warning;
        public static LogCategory Error;
        public static LogCategory Fatal;

        public static CompositeLogCategory ErrorFlags;
        public static LogCategory Default;
#pragma warning restore CS1591 //Missing XML comment for publicly visible type or member
    }
}
