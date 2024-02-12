using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Settings
{
    public static class ExpeditionSettings
    {
        public static RegionFilterSettings Filters = new RegionFilterSettings();
        public static SimpleToggle DetectShelterSpawns = new SimpleToggle(false); //Adds every shelter as a spawnable location for custom (and vanilla) regions

        public static void RestoreToDefaults()
        {
            Filters.RestoreToDefaults();
            DetectShelterSpawns.RestoreDefault();
        }
    }

    public class RegionFilterSettings
    {
        public SimpleToggle AllowVanillaRegions;
        public SimpleToggle AllowMoreSlugcatsRegions;
        public SimpleToggle AllowCustomRegions;
        public SimpleToggle VisitedRegionsOnly; //Restricts spawns to just visited regions for that save slot
        public SimpleToggle ShelterSpawnsOnly; //Restricts spawns to only shelters

        public RegionFilterSettings()
        {
            AllowVanillaRegions = new SimpleToggle(true);
            AllowMoreSlugcatsRegions = new SimpleToggle(true);
            AllowCustomRegions = new SimpleToggle(true);
            VisitedRegionsOnly = new SimpleToggle(false);
            ShelterSpawnsOnly = new SimpleToggle(false);
        }

        public void RestoreToDefaults()
        {
            restoreToDefaults(AllowVanillaRegions, AllowMoreSlugcatsRegions, AllowCustomRegions, VisitedRegionsOnly, ShelterSpawnsOnly);
        }

        private void restoreToDefaults(params SimpleToggle[] settings)
        {
            foreach (SimpleToggle setting in settings)
                setting.RestoreDefault();
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
