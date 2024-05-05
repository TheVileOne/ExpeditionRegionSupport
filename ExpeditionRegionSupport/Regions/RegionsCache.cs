using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions
{
    public class RegionsCache //Cannot be a struct due to challenge filters requiring an unchanging list to reference
    {
        public SlugcatStats.Name LastAccessed;
        public string RegionCode;

        private readonly List<string> _regions = new List<string>();

        public List<string> Regions
        {
            get => _regions;
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                //This reference should not be overwritten. Transfer values to existing reference instead.
                _regions.Clear();
                _regions.AddRange(value);
            }
        }

        public RegionsCache()
        {
        }

        public RegionsCache(string regionCode, IEnumerable<string> regions)
        {
            RegionCode = regionCode;
            Regions = regions.ToList();
        }

        public static RegionsCache GetOrCreate(RegionsCache regionCache, SlugcatStats.Name slugcat)
        {
            if (regionCache == null || regionCache.LastAccessed != slugcat)
            {
                regionCache = new RegionsCache();
                regionCache.LastAccessed = slugcat;
            }
            return regionCache;
        }
    }
}
