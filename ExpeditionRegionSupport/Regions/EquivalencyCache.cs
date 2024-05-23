using ExpeditionRegionSupport.Regions.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions
{
    public class EquivalencyCache : SimpleListCache<RegionProfile>
    {
        /// <summary>
        /// A history of cache ids that have registered since the mod was active
        /// </summary>
        protected static List<int> RegisteredIDs = new List<int>();

        /// <summary>
        /// Returns the base list cache renamed as Regions
        /// </summary>
        public List<RegionProfile> Regions => Items;

        public RegionProfile Store(string regionCode, bool skipChecks = true)
        {
            RegionProfile regionProfile = default;

            if (!skipChecks)
                regionProfile = FindProfile(regionCode); //Check if profile already exists

            if (regionProfile.IsDefault)
                regionProfile = Store(new RegionProfile(regionCode)); //Only store one profile per region
            return regionProfile;
        }

        public override RegionProfile Store(RegionProfile regionProfile)
        {
            if (!regionProfile.IsDefault) return regionProfile;

            return base.Store(regionProfile);
        }

        public RegionProfile FindProfile(string regionCode)
        {
            return Regions.Find(r => r.RegionCode == regionCode);
        }

        /// <summary>
        /// Gets the proper region equivalent of the region code for a particular slugcat
        /// </summary>
        /// <param name="regionBaseEquivalent">The slugcat independent region code equivalent</param>
        public string GetSlugcatEquivalentRegion(string regionCode, SlugcatStats.Name slugcat, out string regionBaseEquivalent)
        {
            RegionProfile regionProfile = FindProfile(regionCode);

            if (!regionProfile.IsDefault)
            {
                RegionProfile baseProfile = regionProfile.GetEquivalentBaseRegion(slugcat);

                regionBaseEquivalent = baseProfile.RegionCode;
                return baseProfile.GetSlugcatEquivalentRegion(slugcat).RegionCode;
            }
            regionBaseEquivalent = regionCode;
            return regionCode;
        }

        /// <summary>
        /// Gets the proper region equivalent of the region code for a particular slugcat
        /// </summary>
        public string GetSlugcatEquivalentRegion(string regionCode, SlugcatStats.Name slugcat)
        {
            RegionProfile regionProfile = FindProfile(regionCode);

            if (!regionProfile.IsDefault)
                return regionProfile.GetSlugcatEquivalentRegion(slugcat).RegionCode;

            return regionCode;
        }

        public IEnumerable<string> GetAllEquivalentRegions(string regionCode, bool includeSelf)
        {
            RegionProfile regionProfile = FindProfile(regionCode);

            if (regionProfile.IsDefault)
                return includeSelf ? new string[] { regionCode } : new string[] { };
            return regionProfile.ListEquivalences(includeSelf).Select(rp => rp.RegionCode);
        }

        protected override void AssignUniqueID()
        {
            int attemptsAllowed = 3;
            while (CacheID == -1 && attemptsAllowed > 0)
            {
                int assignedID = UnityEngine.Random.Range(1, 1000); //Assign an ID based on a random value

                if (!RegisteredIDs.Contains(assignedID))
                {
                    CacheID = assignedID;
                    RegisteredIDs.Add(assignedID); //Ensure no other EquivalencyCache object will have the same identifier
                }
                attemptsAllowed--;
            }

            //When all else fails, this is guaranteed to find a unique value for the cache identifier
            if (CacheID == -1)
            {
                CacheID = RegisteredIDs.Find(i => !RegisteredIDs.Contains(i + 1)) + 1;
                RegisteredIDs.Add(CacheID);
            }
        }

        public void LogEquivalencyRelationships()
        {
            //Only handle regions that are at the top of their equivalency chain - May not be failproof in all cases
            foreach (RegionProfile region in Regions.Where(r => !r.EquivalentRegions.Any()))
                region.LogEquivalences(true);
        }

        public void LogEquivalencyRelationships(string regionCode)
        {
            Plugin.Logger.LogInfo("Equivalency relations for region " + regionCode);
            Plugin.Logger.LogInfo(GetAllEquivalentRegions(regionCode, false).FormatToString(','));
        }
    }
}
