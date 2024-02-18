﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expedition;

namespace ExpeditionRegionSupport.Filters
{
    public static partial class ChallengeFilterSettings
    {
        public static ChallengeFilterOptions CurrentFilter;

        /// <summary>
        /// The Expedition challenge that the filter is handling, or is about to handle
        /// </summary>
        public static Challenge FilterTarget;

        public static bool HasFilter => CurrentFilter != ChallengeFilterOptions.None;

        /// <summary>
        /// A flag that indicates that not all assignment requests could be processed successfully
        /// </summary>
        public static bool FailedToAssign;

        private static void applyEchoChallengeFilter(List<string> allowedRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == ChallengeFilterOptions.VisitedRegions)
                allowedRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r));
        }

        private static void applyPearlDeliveryChallengeFilter(List<string> allowedRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == ChallengeFilterOptions.VisitedRegions)
                allowedRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r));
        }

        private static void applyPearlHoardChallengeFilter(List<string> allowedRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == ChallengeFilterOptions.VisitedRegions)
                allowedRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r));
        }

        private static void applyVistaChallengeFilter(List<string> allowedRegions)
        {
            if (!HasFilter) return;

            if (CurrentFilter == ChallengeFilterOptions.VisitedRegions)
                allowedRegions.RemoveAll(r => !Plugin.RegionsVisited.Contains(r.Split('_')[0]));
        }

        /// <summary>
        /// Handle when a challenge was unable to be selected
        /// </summary>
        private static void onGenerationFailed(List<Challenge> availableChallenges)
        {
            Plugin.Logger.LogInfo($"Challenge type {FilterTarget.ChallengeName()} could not be selected. Generating another");
            availableChallenges.Remove(FilterTarget);
        }
    }

    public enum ChallengeFilterOptions
    {
        None,
        VisitedRegions
    }
}
