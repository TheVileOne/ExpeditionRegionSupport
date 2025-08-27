using Expedition;
using ExpeditionRegionSupport.Filters.Settings;
using ExpeditionRegionSupport.Regions;
using System.Collections.Generic;
using System.Linq;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public static class RegionFilter
    {
        public static StringFilter CurrentFilter;

        /// <summary>
        /// Applies filter logic to available regions
        /// </summary>
        /// <returns>An array of region codes that passed all applicable region conditions</returns>
        public static List<string> Apply()
        {
            return CurrentFilter.Apply();
        }

        public static void UpdateFilter()
        {
            CurrentFilter = new StringFilter(SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer));
            CurrentFilter.Criteria += defaultFilter;

            static IEnumerable<string> defaultFilter(IEnumerable<string> regions)
            {
                //Default filter should always run first. This probably wont interfere with any custom filtering
                if (RegionFilterSettings.VisitedRegionsOnly.Value)
                    regions = RegionUtils.GetVisitedRegions(ExpeditionData.slugcatPlayer);

                if (!RegionFilterSettings.AllowVanillaRegions.Value)
                    regions = regions.Where(r => !RegionUtils.IsVanillaRegion(r));

                if (!RegionFilterSettings.AllowDownpourRegions.Value)
                    regions = regions.Where(r => !RegionUtils.IsDownpourRegion(r));

                if (!RegionFilterSettings.AllowCustomRegions.Value)
                    regions = regions.Where(r => !RegionUtils.IsCustomRegion(r));
                return regions;
            }
        }
    }
}
