using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters.Settings
{
    public static class RegionFilterSettings
    {
        public static readonly SimpleToggle RememberSettings;

        public static readonly FilterToggle AllowVanillaRegions;
        public static readonly FilterToggle AllowDownpourRegions;
        public static readonly FilterToggle AllowCustomRegions;
        public static readonly FilterToggle VisitedRegionsOnly;
        public static readonly FilterToggle StoryAndOptionalRegionsOnly;
        public static readonly FilterToggle DetectCustomShelters;

        /// <summary>
        /// The setting toggles that are managed by ExpeditionSettingDialog
        /// </summary>
        public static List<FilterToggle> Settings = new List<FilterToggle>();

        /// <summary>
        /// The setting toggles that have been changed since the last time ExpeditionSettingDialog was opened 
        /// </summary>
        public static List<FilterToggle> ChangedSettings = new List<FilterToggle>();

        static RegionFilterSettings()
        {
            SimpleToggle.OnCreate += onToggleCreated;

            RememberSettings = new SimpleToggle(false);

            AllowVanillaRegions = new FilterToggle(FilterOption.NoVanilla, true, false);
            AllowDownpourRegions = new FilterToggle(FilterOption.NoDownpour, true, false);
            AllowCustomRegions = new FilterToggle(FilterOption.NoCustom, true, false);
            VisitedRegionsOnly = new FilterToggle(FilterOption.VisitedRegionsOnly, false, true);
            StoryAndOptionalRegionsOnly = new FilterToggle(FilterOption.StoryAndOptionalRegionsOnly, false, true);
            DetectCustomShelters = new FilterToggle(FilterOption.InheritCustomShelters, false, true);

            SimpleToggle.OnCreate -= onToggleCreated;

            static void onToggleCreated(SimpleToggle toggle)
            {
                FilterToggle filterSetting = toggle as FilterToggle;

                if (filterSetting != null)
                {
                    toggle.ValueChanged += onSettingChanged;
                    Settings.Add(filterSetting);
                }
            }

            static void onSettingChanged(SimpleToggle toggle)
            {
                FilterToggle filterSetting = (FilterToggle)toggle;

                //We want the toggle to be removed, or added every time the value changes
                if (!ChangedSettings.Remove(filterSetting))
                    ChangedSettings.Add(filterSetting);
            }
        }

        public static bool IsFilterActive(FilterOption filterOption)
        {
            return Settings.Find(f => f.OptionID == filterOption).Enabled;
        }

        /// <summary>
        /// Searches through all filter option settings and returns the filter options that are enabled
        /// </summary>
        public static List<FilterOption> GetActiveFilters()
        {
            return Settings.FindAll(s => s.Enabled).Select(s => s.OptionID).ToList();
        }

        public static void RestoreToDefaults()
        {
            Settings.ForEach(s => s.RestoreDefault());
        }
    }

    public class FilterToggle : SimpleToggle
    {
        public FilterOption OptionID;

        /// <summary>
        /// The state that determines whether a filter is enabled
        /// </summary>
        public readonly bool FilterEnableState;

        public bool Enabled => Value == FilterEnableState;

        public FilterToggle(FilterOption optionID, bool defaultValue, bool enableState) : base(defaultValue)
        {
            OptionID = optionID;
            FilterEnableState = enableState;
        }
    }

    public class SimpleToggle
    {
        /// <summary>
        /// An event that applies each time the SimpleToggle constructor is invoked
        /// </summary>
        public static Action<SimpleToggle> OnCreate;

        /// <summary>
        /// Should events such as ValueChanged invoke, or be ignored
        /// </summary>
        public bool SuppressEvents;

        public Action<SimpleToggle> ValueChanged;

        public bool DefaultValue;

        private bool _value;
        public bool Value
        {
            get => _value;

            set
            {
                if (_value != value)
                {
                    _value = value;

                    if (!SuppressEvents)
                        ValueChanged?.Invoke(this);
                }
            }
        }

        public bool IsChanged => Value != DefaultValue;

        public SimpleToggle(bool defaultValue)
        {
            SuppressEvents = true; //For the first time this value is set don't handle events
            Value = DefaultValue = defaultValue;

            OnCreate?.Invoke(this);
            SuppressEvents = false;
        }

        public void RestoreDefault()
        {
            Value = DefaultValue;
        }
    }

    public enum FilterOption
    {
        None,
        InheritCustomShelters,
        VisitedRegionsOnly,
        StoryAndOptionalRegionsOnly,
        NoVanilla,
        NoDownpour,
        NoCustom
    }
}
