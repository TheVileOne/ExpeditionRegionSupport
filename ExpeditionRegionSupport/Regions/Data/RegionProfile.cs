using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions.Data
{
    public readonly struct RegionProfile
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

        /// <summary>
        /// This region should not be assigned any base equivalent regions even if it serves as a substitute for another region  
        /// </summary>
        public readonly bool IsPermanentBaseRegion;

        public bool IsDefault => Equals(default(RegionProfile));

        /// <summary>
        /// Stores the RegionProfile requesting to be made an equivalency to another RegionProfile
        /// </summary>
        public readonly StrongBox<RegionProfile> PendingEquivalency;

        public RegionProfile(string regionCode)
        {
            RegionCode = regionCode;
            EquivalentBaseRegions = new List<RegionProfile>();
            EquivalentRegions = new Dictionary<SlugcatStats.Name, RegionProfile>();
            PendingEquivalency = new StrongBox<RegionProfile>();

            IsPermanentBaseRegion = RegionUtils.IsVanillaRegion(RegionCode)
                                 || RegionCode == "OE" || RegionCode == "VS" || RegionCode == "HR" || RegionCode == "MS" || RegionCode == "LC";
        }

        /// <summary>
        /// Establish an equivalent relationship with another RegionProfile
        /// </summary>
        /// <param name="slugcat">The slugcat conditions for loading a specific equivalent region</param>
        /// <param name="region">The equivalent region that will be loaded based on a specific slugcat</param>
        public void RegisterEquivalency(SlugcatStats.Name slugcat, RegionProfile region)
        {
            if (region.Equals(this) || region.IsDefault || EquivalentBaseRegions.Contains(region)) return;

            PendingEquivalency.Value = region;

            //Check that this region already has an equivalent region assigned to this slugcat
            if (EquivalentRegions.TryGetValue(slugcat, out RegionProfile existingProfile)
             || EquivalentRegions.TryGetValue(SlugcatUtils.AnySlugcat, out existingProfile))
            {
                //Don't process if region has already been assigned for this slugcat
                if (existingProfile.Equals(region)) return;

                if (!HasIllegalRelationships(region, slugcat))
                {
                    Plugin.Logger.LogInfo("Changing equivalent region targetting " + RegionCode);
                    EquivalentRegions[slugcat] = region;

                    if (!region.IsPermanentBaseRegion) //Prevent certain regions from having base equivalencies
                        region.EquivalentBaseRegions.Add(this);
                }
            }

            if (slugcat != SlugcatUtils.AnySlugcat)
            {
                EquivalentRegions.Remove(SlugcatUtils.AnySlugcat); //Region either applies to anyone, or only certain slugcats
                region.EquivalentBaseRegions.Add(this);
            }
            else if (!EquivalentRegions.Any())
                EquivalentRegions[slugcat] = region;
        }

        /// <summary>
        /// Check for situations that would make an equivalency relationship with this region redundant, or incompatible 
        /// </summary>
        public bool HasIllegalRelationships(RegionProfile region, SlugcatStats.Name slugcat)
        {
            //This is stored in the registering RegionProfile (the callback region), and is required to ensure complicated, but possible loop relationships are detected
            if (!region.PendingEquivalency.Value.IsDefault && (slugcat == SlugcatUtils.AnySlugcat || !EquivalentRegions.ContainsKey(slugcat)))
                region = region.PendingEquivalency.Value;

            //The AnySlugcat specification requires all slugcat relationships to be checked for illegal relationships
            if (slugcat == SlugcatUtils.AnySlugcat)
            {
                //Checks the equivalent region data of the region we want to process to see if it can reach the callback region, which would form an illegal loop 
                RegionProfile callbackRegion = this;
                return region.EquivalentRegions.Exists(equivalencyEntry => callbackRegion.HasIllegalRelationships(equivalencyEntry.Value, equivalencyEntry.Key));
            }

            RegionProfile compareRegion = region;

            bool continueLoop = true;
            bool hasIllegalRelationships = false;
            do
            {
                RegionProfile regionCheck = region.GetRegionCandidate(slugcat);

                if (regionCheck.IsDefault) //The slugcat is not associated with any additional equivalent regions
                {
                    continueLoop = false;
                }
                else if (regionCheck.Equals(this) || regionCheck.Equals(compareRegion)) //Illegal loop detected
                {
                    hasIllegalRelationships = true;
                    continueLoop = false;
                }
                else
                {
                    region = regionCheck; //Set as next region to evaluate
                }
            }
            while (continueLoop);

            return hasIllegalRelationships;
        }

        public RegionProfile GetSlugcatEquivalentRegion(SlugcatStats.Name slugcat)
        {
            RegionProfile baseEquivalentRegion = GetEquivalentBaseRegion(slugcat); //Region candidacy checking should start at a base equivalent region
            return baseEquivalentRegion.GetRegionCandidateRecursive(slugcat, new HashSet<string>() { RegionCode });
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

        internal RegionProfile GetRegionCandidate(SlugcatStats.Name slugcat)
        {
            if (slugcat == SlugcatUtils.AnySlugcat)
            {
                Plugin.Logger.LogInfo("Returning a region candidate for any slugcat. Is this intentional?");
                return EquivalentRegions.FirstOrDefault().Value;
            }

            //Check that there is an equivalent region specific to this slugcat
            EquivalentRegions.TryGetValue(slugcat, out RegionProfile equivalentRegion);

            //If there are no slugcat specific equivalent regions, check for any unspecific equivalent regions
            if (equivalentRegion.IsDefault)
                EquivalentRegions.TryGetValue(SlugcatUtils.AnySlugcat, out equivalentRegion);

            return equivalentRegion;
        }

        internal RegionProfile GetRegionCandidateRecursive(SlugcatStats.Name slugcat, HashSet<string> checkedRegions)
        {
            RegionProfile equivalentRegion = GetRegionCandidate(slugcat);

            if (!equivalentRegion.IsDefault)
            {
                //We found a valid equivalent region, but we're not finished. Check if that equivalent region has equivalent regions
                if (!checkedRegions.Contains(equivalentRegion.RegionCode))
                {
                    checkedRegions.Add(equivalentRegion.RegionCode);
                    equivalentRegion = equivalentRegion.GetRegionCandidateRecursive(slugcat, checkedRegions);
                }
                return equivalentRegion;
            }
            return this; //Return this when this is the most valid equivalent region
        }
    }
}
