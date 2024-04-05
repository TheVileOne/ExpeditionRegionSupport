using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters.Settings
{
    public static class RegionFilterSettings
    {
        public static FilterToggle AllowVanillaRegions = new FilterToggle(FilterOption.NoVanilla, true);
        public static FilterToggle AllowMoreSlugcatsRegions = new FilterToggle(FilterOption.NoMSC, true);
        public static FilterToggle AllowCustomRegions = new FilterToggle(FilterOption.NoCustom, true);
        public static FilterToggle VisitedRegionsOnly = new FilterToggle(FilterOption.VisitedRegionsOnly, false);
        public static FilterToggle ShelterSpawnsOnly = new FilterToggle(FilterOption.SheltersOnly, false);
        public static FilterToggle DetectShelterSpawns = new FilterToggle(FilterOption.AllShelters, false);

        public static List<FilterToggle> Settings = new List<FilterToggle>()
        {
            AllowVanillaRegions, AllowMoreSlugcatsRegions, AllowCustomRegions,
            VisitedRegionsOnly,
            ShelterSpawnsOnly, //Currently unused
            DetectShelterSpawns //Adds every shelter as a spawnable location for custom (and vanilla) regions
        };

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
