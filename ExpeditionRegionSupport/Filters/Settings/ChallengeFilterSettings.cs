using Expedition;
using ExpeditionRegionSupport.Filters.Utils;
using System.Collections.Generic;

namespace ExpeditionRegionSupport.Filters.Settings
{
    public static class ChallengeFilterSettings
    {
        public static Dictionary<string, List<ChallengeFilter>> Filters;

        /// <summary>
        /// The Expedition challenge that the filter is handling, or is about to handle
        /// </summary>
        public static Challenge FilterTarget;

        static ChallengeFilterSettings()
        {
            Filters = new Dictionary<string, List<ChallengeFilter>>();

            //Iterate through challenge types to populate challenge filters 
            foreach (string name in ExpeditionGame.challengeNames.Keys)
            {
                List<ChallengeFilter> filters = new List<ChallengeFilter>();

                ChallengeFilter f = processFilter(name);
                if (f != null)
                    filters.Add(f);

                Filters.Add(name, filters);
            }
        }

        private static ChallengeFilter processFilter(string name)
        {
            FilterOption filterType = FilterOption.VisitedRegionsOnly; //The only type managed by default

            ChallengeFilter filter;
            switch (name)
            {
                case ExpeditionConsts.ChallengeNames.ECHO:
                case ExpeditionConsts.ChallengeNames.PEARL_HOARD:
                    filter = new ChallengeFilter(filterType);
                    break;
                case ExpeditionConsts.ChallengeNames.PEARL_DELIVERY:
                    filter = new PearlDeliveryChallengeFilter(filterType);
                    break;
                case ExpeditionConsts.ChallengeNames.NEURON_DELIVERY:
                    filter = new NeuronDeliveryChallengeFilter(filterType);
                    break;
                default:
                    filter = null;
                    break;
            }
            return filter;
        }

        /// <summary>
        /// Retrieves filters that are associated with the FilterTarget
        /// </summary>
        public static List<ChallengeFilter> GetFilters()
        {
            if (FilterTarget == null) return new List<ChallengeFilter>();

            return Filters[FilterTarget.GetTypeName()];
        }

        public static bool CheckConditions()
        {
            //Get the filters that apply to the target
            List<ChallengeFilter> availableFilters = GetFilters();

            return availableFilters.TrueForAll(f => f.ConditionMet());
        }

        public static void ApplyFilter(List<string> allowedRegions)
        {
            //Get the filters that apply to the target
            List<ChallengeFilter> availableFilters = GetFilters();

            foreach (ChallengeFilter filter in availableFilters)
                filter.Apply(allowedRegions);
        }
    }
}
