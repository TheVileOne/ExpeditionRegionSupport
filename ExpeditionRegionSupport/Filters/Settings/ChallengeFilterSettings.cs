using Expedition;
using ExpeditionRegionSupport.Filters.Utils;
using System.Collections.Generic;

namespace ExpeditionRegionSupport.Filters.Settings
{
    public static partial class ChallengeFilterSettings
    {
        public static Dictionary<string, List<ChallengeFilter>> Filters;

        public static FilterOption CurrentFilter;

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

            ChallengeFilter filter = name switch
            {
                ExpeditionConsts.ChallengeNames.ECHO or ExpeditionConsts.ChallengeNames.PEARL_HOARD => new ChallengeFilter(filterType),
                ExpeditionConsts.ChallengeNames.PEARL_DELIVERY => new PearlDeliveryChallengeFilter(filterType),
                ExpeditionConsts.ChallengeNames.NEURON_DELIVERY => new NeuronDeliveryChallengeFilter(filterType),
                _ => null,
            };
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
