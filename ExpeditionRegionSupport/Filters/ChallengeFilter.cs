using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters
{
    public class ChallengeFilter : ListFilter<string>
    {
        public FilterOptions FilterID;

        public override bool Enabled => ChallengeFilterSettings.CurrentFilter == FilterID;

        public ChallengeFilter(FilterOptions filterID) : base(PrepareFilter(filterID))
        {
        }

        protected static List<string> PrepareFilter(FilterOptions filterID)
        {
            if (filterID == FilterOptions.VisitedRegions)
                return Plugin.RegionsVisited;

            return new List<string>();
        }
    }

    public class NeuronDeliveryChallengeFilter : ChallengeFilter
    {
        public NeuronDeliveryChallengeFilter(FilterOptions filterID) : base(filterID)
        {
        }

        /// <summary>
        /// Check that delivery regions aren't filtered
        /// </summary>
        /// <returns>Filter state (true means not filtered)</returns>
        public override bool ConditionMet()
        {
            return !Enabled || (Evaluate("SL", true) && Evaluate("SS", true)); //Both regions must be visited to get this challenge
        }
    }
}
