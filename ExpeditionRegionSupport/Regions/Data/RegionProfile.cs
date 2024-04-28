using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions.Data
{
    public struct RegionProfile
    {
        public readonly string RegionCode;

        /// <summary>
        /// Contains regions considered equivalent, but are replaced by this region under one of more conditions
        /// </summary>
        public readonly List<RegionProfile> EquivalentBaseRegions;

        /// <summary>
        /// Contains regions considered equivalent replacements for this region under one or more conditions.
        /// Conditions are organized by the slugcat name
        /// </summary>
        public readonly Dictionary<SlugcatStats.Name, RegionProfile> EquivalentRegions = new Dictionary<SlugcatStats.Name, RegionProfile>();

        /// <summary>
        /// This region does not replace any other equivalent regions
        /// </summary>
        public readonly bool IsBaseRegion => EquivalentBaseRegions.Count == 0;

        public bool IsDefault => Equals(default(RegionProfile));

        public RegionProfile(string regionCode)
        {
            RegionCode = regionCode;
            EquivalentBaseRegions = new List<RegionProfile>();
            EquivalentRegions = new Dictionary<SlugcatStats.Name, RegionProfile>();
        }

        public RegionProfile GetSlugcatEquivalentRegion(SlugcatStats.Name slugcat)
        {
            RegionProfile baseEquivalentRegion = GetEquivalentBaseRegion(slugcat); //Region candidacy checking should start at a base equivalent region
            return baseEquivalentRegion.GetRegionCandidate(slugcat, new HashSet<string>() { RegionCode });
        }

        /// <summary>
        /// Gets the base equivalent region that most closely associates with a vanilla/downpour region, and otherwise has no other equivalent regions
        /// </summary>
        public RegionProfile GetEquivalentBaseRegion()
        {
            if (IsBaseRegion)
                return this;

            if (EquivalentBaseRegions.Count > 1)
                Plugin.Logger.LogInfo($"Multiple base regions for {RegionCode} detected. Choosing one");

            return EquivalentBaseRegions[0].GetEquivalentBaseRegion(); //Default to the first registered base
        }

        /// <summary>
        /// Gets the base equivalent region that most closely associated with a vanilla/downpour region for a specified slugcat.
        /// This will probably return the same result in most cases. The main difference is that this method prioritizes base regions
        /// that target a specific slugcat.
        /// </summary>
        public RegionProfile GetEquivalentBaseRegion(SlugcatStats.Name slugcat)
        {
            if (IsBaseRegion)
                return this;

            if (EquivalentBaseRegions.Count == 1)
                return EquivalentBaseRegions[0].GetEquivalentBaseRegion(slugcat);

            Plugin.Logger.LogInfo($"Multiple base regions for {RegionCode} detected. Choosing one");

            RegionProfile mostRelevantBaseRegion = EquivalentBaseRegions.Find(r => r.EquivalentRegions.ContainsKey(slugcat));

            if (!mostRelevantBaseRegion.IsDefault)
                return mostRelevantBaseRegion;

            return EquivalentBaseRegions[0].GetEquivalentBaseRegion(slugcat); //Default to the first registered base
        }

        internal RegionProfile GetRegionCandidate(SlugcatStats.Name slugcat, HashSet<string> checkedRegions)
        {
            //Check that there is an equivalent region specific to this slugcat
            EquivalentRegions.TryGetValue(slugcat, out RegionProfile equivalentRegion);

            //If there are no slugcat specific equivalent regions, check for any unspecific equivalent regions
            if (equivalentRegion.IsDefault)
                EquivalentRegions.TryGetValue(new SlugcatStats.Name("ANY"), out equivalentRegion);

            if (!equivalentRegion.IsDefault)
            {
                //We found a valid equivalent region, but we're not finished. Check if that equivalent region has equivalent regions
                if (!checkedRegions.Contains(equivalentRegion.RegionCode))
                {
                    checkedRegions.Add(equivalentRegion.RegionCode);
                    equivalentRegion = equivalentRegion.GetRegionCandidate(slugcat, checkedRegions);
                }
                return equivalentRegion;
            }
            return this; //Return this when this is the most valid equivalent region
        }
    }
}
