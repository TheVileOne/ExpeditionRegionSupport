using ExpeditionRegionSupport.Regions;
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

    public class PearlDeliveryChallengeFilter : ChallengeFilter
    {
        private bool deliveryRegionFiltered;

        public PearlDeliveryChallengeFilter(FilterOptions filterID) : base(filterID)
        {
        }

        public override void Apply(List<string> allowedRegions, Func<string, string> valueModifier = null)
        {
            deliveryRegionFiltered = false; //Ensure value is never stale

            if (!Enabled) return;

            string deliveryRegion = RegionUtils.GetPearlDeliveryRegion(Plugin.ActiveWorldState);

            //We cannot choose this challenge type if we haven't visited the delivery region yet
            if (!allowedRegions.Contains(deliveryRegion) || Evaluate(deliveryRegion, false))
            {
                deliveryRegionFiltered = true;
                allowedRegions.Clear();
                return;
            }

            base.Apply(allowedRegions, valueModifier);
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
        public NeuronDeliveryChallengeFilter(FilterOptions filterID) : base(filterID)
        {
        }

        /// <summary>
        /// Check that necessary regions aren't filtered
        /// </summary>
        /// <returns>Filter state (true means not filtered)</returns>
        public override bool ConditionMet()
        {
            //If player has not visited Shoreline, or Five Pebbles, this challenge type cannot be chosen
            return !Enabled || (Evaluate("SL", true) && Evaluate("SS", true));
        }
    }
}
