using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters
{
    public class ChallengeFilter : ListFilter<string>
    {
        public ChallengeFilterOptions FilterID;

        public override bool Enabled => ChallengeFilterSettings.CurrentFilter == FilterID;

        public ChallengeFilter(ChallengeFilterOptions filterID) : base(PrepareFilter(filterID))
        {
        }

        protected static List<string> PrepareFilter(ChallengeFilterOptions filterID)
        {
            if (filterID == ChallengeFilterOptions.VisitedRegions)
                return Plugin.RegionsVisited;

            return new List<string>();
        }
    }
}
