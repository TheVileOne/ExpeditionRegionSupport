using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions.Restrictions
{
    public class RestrictionCheck
    {
        public delegate bool ApplyDelegate();

        public ApplyDelegate Handler;

        public string RegionCode;
        public List<SlugcatStats.Name> Slugcats = new List<SlugcatStats.Name>();

        public RestrictionCheck(string regionCode, ApplyDelegate applyHandler)
        {
            RegionCode = regionCode;
            Handler = applyHandler;
        }

        public RestrictionCheck(string regionCode, RestrictCondition condition)
        {
            RegionCode = regionCode;

            switch (condition)
            {
                case RestrictCondition.SlugcatUnlocked:
                    Handler += () =>
                    {
                        return RegionSelector.Instance.UnlockedSlugcats.Exists(Slugcats.Contains);
                    };
                    break;
                case RestrictCondition.UseSpecificSlugcats:
                    Handler += () =>
                    {
                        return Slugcats.Contains(RegionSelector.Instance.ActiveSlugcat);
                    };
                    break;
            }
        }

        public bool CheckRegion()
        {
            return Handler.Invoke();
        }
    }

    public enum RestrictCondition
    {
        None,
        SlugcatUnlocked,
        UseSpecificSlugcats
    }
}
