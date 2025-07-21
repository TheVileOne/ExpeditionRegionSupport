using ExpeditionRegionSupport.Filters.Settings;
using ExpeditionRegionSupport.Regions;
using System.Collections.Generic;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public class ChallengeFilter : ListFilter<string>
    {
        public FilterOption FilterID;

        public override bool Enabled => ChallengeFilterSettings.CurrentFilter == FilterID;

        public ChallengeFilter(FilterOption filterID) : base(PrepareFilter(filterID))
        {
            FilterID = filterID;
        }

        protected static List<string> PrepareFilter(FilterOption filterID)
        {
            if (filterID == FilterOption.VisitedRegionsOnly)
                return RegionUtils.RegionsVisitedCache.Regions;

            return new List<string>();
        }
    }

    public class PearlDeliveryChallengeFilter : ChallengeFilter
    {
        private bool deliveryRegionFiltered;

        public PearlDeliveryChallengeFilter(FilterOption filterID) : base(filterID)
        {
        }

        public override void Apply(List<string> allowedRegions)
        {
            deliveryRegionFiltered = false; //Ensure value is never stale

            if (!Enabled) return;

            string deliveryRegion = RegionUtils.GetPearlDeliveryRegion(Plugin.ActiveWorldState);

            //We cannot choose this challenge type if we haven't visited the delivery region yet
            if (!allowedRegions.Contains(deliveryRegion) || !Evaluate(deliveryRegion, true))
            {
                deliveryRegionFiltered = true;
                allowedRegions.Clear();
                return;
            }

            base.Apply(allowedRegions);
        }

        /// <summary>
        /// Checks Enabled state, and if delivery region is 
        /// </summary>
        /// <returns>Filter state (true means not filtered)</returns>
        public override bool ConditionMet()
        {
            return !deliveryRegionFiltered;
        }
    }

    public class NeuronDeliveryChallengeFilter : ChallengeFilter
    {
        public NeuronDeliveryChallengeFilter(FilterOption filterID) : base(filterID)
        {
        }

        /// <summary>
        /// Check that necessary regions aren't filtered
        /// </summary>
        /// <returns>Filter state (true means not filtered)</returns>
        public override bool ConditionMet()
        {
            //If player has not visited Shoreline, or Five Pebbles, this challenge type cannot be chosen
            return !Enabled || Evaluate("SL", true) && Evaluate("SS", true);
        }
    }
}
