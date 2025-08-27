using Expedition;
using ExpeditionRegionSupport.Regions;
using System.Collections.Generic;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public class ChallengeFilter : ListFilter<string>
    {
        /// <summary>
        /// Contains a list of regions that must contain any valid filter entry
        /// </summary>
        public override List<string> CompareValues => RegionUtils.GetAvailableRegions(ExpeditionData.slugcatPlayer);

        public ChallengeFilter() : base()
        {
        }
    }

    public class PearlDeliveryChallengeFilter : ChallengeFilter
    {
        private bool deliveryRegionFiltered;

        public PearlDeliveryChallengeFilter() : base()
        {
        }

        public override void Apply(List<string> allowedRegions)
        {
            deliveryRegionFiltered = false; //Ensure value is never stale

            if (!Enabled) return;

            string deliveryRegion = RegionUtils.GetPearlDeliveryRegion(Plugin.ActiveWorldState);

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
        public NeuronDeliveryChallengeFilter() : base()
        {
        }

        /// <summary>
        /// Check that necessary regions aren't filtered
        /// </summary>
        /// <returns>Filter state (true means not filtered)</returns>
        public override bool ConditionMet()
        {
            return !Enabled || Evaluate("SL", true) && Evaluate("SS", true);
        }
    }
}
