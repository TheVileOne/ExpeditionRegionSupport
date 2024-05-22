﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions
{
    public class RegionsCache : SimpleListCache<string>
    {
        /// <summary>
        /// The current/last active slugcat to be associated with cached region data
        /// </summary>
        public SlugcatStats.Name LastAccessed;
        public string RegionCode;

        /// <summary>
        /// Returns the base list cache renamed as Regions
        /// </summary>
        public List<string> Regions
        {
            get => Items;
            set => Items = value;
        }

        public RegionsCache()
        {
        }

        public RegionsCache(string regionCode, IEnumerable<string> regions)
        {
            RegionCode = regionCode;
            Store(regions);
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
