using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters.Settings
{
    public static class RegionFilterSettings
    {
        public static SimpleToggle AllowVanillaRegions = new SimpleToggle(true);
        public static SimpleToggle AllowMoreSlugcatsRegions = new SimpleToggle(true);
        public static SimpleToggle AllowCustomRegions = new SimpleToggle(true);
        public static SimpleToggle VisitedRegionsOnly = new SimpleToggle(false);
        public static SimpleToggle ShelterSpawnsOnly = new SimpleToggle(false);
        public static SimpleToggle DetectShelterSpawns = new SimpleToggle(false);

        public static List<SimpleToggle> Settings = new List<SimpleToggle>()
        {
            AllowVanillaRegions, AllowMoreSlugcatsRegions, AllowCustomRegions,
            VisitedRegionsOnly,
            ShelterSpawnsOnly, //Currently unused
            DetectShelterSpawns //Adds every shelter as a spawnable location for custom (and vanilla) regions
        };

        public static void RestoreToDefaults()
        {
            Settings.ForEach(s => s.RestoreDefault());
        }
    }

    public class SimpleToggle
    {
        public bool DefaultValue;
        public bool Value;

        public bool IsChanged => Value != DefaultValue;

        public SimpleToggle(bool defaultValue)
        {
            Value = DefaultValue = defaultValue;
        }

        public void RestoreDefault()
        {
            Value = DefaultValue;
        }
    }
}
