using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters.Settings
{
    public static class RegionFilterSettings
    {
        public static readonly FilterToggle AllowVanillaRegions;
        public static readonly FilterToggle AllowMoreSlugcatsRegions;
        public static readonly FilterToggle AllowCustomRegions;
        public static readonly FilterToggle VisitedRegionsOnly;
        public static readonly FilterToggle ShelterSpawnsOnly;
        public static readonly FilterToggle DetectShelterSpawns;

        /// <summary>
        /// The setting toggles that are managed by ExpeditionSettingDialog
        /// </summary>
        public static List<FilterToggle> Settings = new List<FilterToggle>();

        /// <summary>
        /// The setting toggles that have been changed since the last time ExpeditionSettingDialog was opened 
        /// </summary>
        public static List<SimpleToggle> ChangedSettings = new List<SimpleToggle>();

        static RegionFilterSettings()
        {
            SimpleToggle.OnCreate += onToggleCreated;

            AllowVanillaRegions = new FilterToggle(FilterOption.NoVanilla, true);
            AllowMoreSlugcatsRegions = new FilterToggle(FilterOption.NoMSC, true);
            AllowCustomRegions = new FilterToggle(FilterOption.NoCustom, true);
            VisitedRegionsOnly = new FilterToggle(FilterOption.VisitedRegionsOnly, false);
            ShelterSpawnsOnly = new FilterToggle(FilterOption.SheltersOnly, false);
            DetectShelterSpawns = new FilterToggle(FilterOption.AllShelters, false);

            SimpleToggle.OnCreate -= onToggleCreated;

            static void onToggleCreated(SimpleToggle toggle)
            {
                toggle.ValueChanged += onValueChanged;
                Settings.Add((FilterToggle)toggle);
            }

            static void onValueChanged(SimpleToggle toggle)
            {
                //We want the toggle to removed, or added every time the value changes
                if (!ChangedSettings.Remove(toggle))
                    ChangedSettings.Add(toggle);
            }
        }

        /// <summary>
        /// Searches through all filter option settings and returns the filter options that are enabled
        /// </summary>
        public static List<FilterOption> GetActiveFilters()
        {
            return Settings.FindAll(s => s.IsChanged).Select(s => s.OptionID).ToList();
        }

        public static void RestoreToDefaults()
        {
            Settings.ForEach(s => s.RestoreDefault());
        }
    }

    public class FilterToggle : SimpleToggle
    {
        public FilterOption OptionID;

        public FilterToggle(FilterOption optionID, bool defaultValue) : base(defaultValue)
        {
            OptionID = optionID;
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
        AllShelters,
        SheltersOnly,
        VisitedRegionsOnly,
        NoVanilla,
        NoMSC,
        NoCustom
    }
}
