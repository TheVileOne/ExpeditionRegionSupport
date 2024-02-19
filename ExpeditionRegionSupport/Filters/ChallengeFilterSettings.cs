using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expedition;

namespace ExpeditionRegionSupport.Filters
{
    public static partial class ChallengeFilterSettings
    {
        public static Dictionary<string, List<ChallengeFilter>> Filters;

        public static FilterOptions CurrentFilter;

        /// <summary>
        /// The Expedition challenge that the filter is handling, or is about to handle
        /// </summary>
        public static Challenge FilterTarget;

        public static bool HasFilter => CurrentFilter != FilterOptions.None;

        /// <summary>
        /// A flag that indicates that not all assignment requests could be processed successfully
        /// </summary>
        public static bool FailedToAssign;

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
            FilterOptions filterType = FilterOptions.VisitedRegions; //The only type managed by default

            switch (name)
            {
                case CHALLENGE_NAME_ECHO:
                case CHALLENGE_NAME_PEARL_HOARD:
                    return new ChallengeFilter(filterType);
                case CHALLENGE_NAME_VISTA:
                    return new ChallengeFilter(filterType)
                    {
                        ValueModifier = (v) => v.Split('_')[0] //This challenge stores room codes, which need underscore parsing
                    };
                case CHALLENGE_NAME_PEARL_DELIVERY:
                    return new PearlDeliveryChallengeFilter(filterType);
                case CHALLENGE_NAME_NEURON_DELIVERY:
                    return new NeuronDeliveryChallengeFilter(filterType);
            }
            return null;
        }

        /// <summary>
        /// Retrieves filters that are associated with the FilterTarget
        /// </summary>
        public static List<ChallengeFilter> GetFilters()
        {
            if (FilterTarget == null) return new List<ChallengeFilter>();

            return Filters[FilterTarget.GetType().Name];
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

        /*
        private static void applyEchoChallengeFilter(List<string> allowedRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == FilterOptions.VisitedRegions)
                allowedRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r));
        }

        private static void applyPearlDeliveryChallengeFilter(List<string> allowedRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == FilterOptions.VisitedRegions)
                allowedRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r));
        }

        private static void applyPearlHoardChallengeFilter(List<string> allowedRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == FilterOptions.VisitedRegions)
                allowedRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r));
        }

        private static void applyVistaChallengeFilter(List<string> allowedRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == FilterOptions.VisitedRegions)
                allowedRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r.Split('_')[0]));
        }
        */

        /// <summary>
        /// Handle when a challenge was unable to be selected
        /// </summary>
        private static void onGenerationFailed(List<Challenge> availableChallenges)
        {
            Plugin.Logger.LogInfo($"Challenge type {FilterTarget.ChallengeName()} could not be selected. Generating another");
            availableChallenges.Remove(FilterTarget);
        }

        #region consts
        public const string CHALLENGE_NAME_ACHIEVEMENT = "AchievementChallenge";
        public const string CHALLENGE_NAME_CYCLE_SCORE = "CycleScoreChallenge";
        public const string CHALLENGE_NAME_ECHO = "EchoChallenge";
        public const string CHALLENGE_NAME_GLOBAL_SCORE = "GlobalScoreChallenge";
        public const string CHALLENGE_NAME_HUNT = "HuntChallenge";
        public const string CHALLENGE_NAME_ITEM_HOARD = "ItemHoardChallenge";
        public const string CHALLENGE_NAME_NEURON_DELIVERY = "NeuronDeliveryChallenge";
        public const string CHALLENGE_NAME_PEARL_DELIVERY = "PearlDeliveryChallenge";
        public const string CHALLENGE_NAME_PEARL_HOARD = "PearlHoardChallenge";
        public const string CHALLENGE_NAME_PIN = "PinChallenge";
        public const string CHALLENGE_NAME_VISTA = "VistaChallenge";
        #endregion
    }

    public enum FilterOptions
    {
        None,
        VisitedRegions
    }
}
