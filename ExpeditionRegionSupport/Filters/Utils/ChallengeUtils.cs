using Expedition;
using System.Collections.Generic;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public static class ChallengeUtils
    {
        public static List<Challenge> GetChallenges(SlugcatStats.Name challengeOwner)
        {
            if (!ExpeditionData.allChallengeLists.ContainsKey(challengeOwner))
                ExpeditionData.allChallengeLists[challengeOwner] = new List<Challenge>();

            return ExpeditionData.allChallengeLists[challengeOwner];
        }

        /// <summary>
        /// Gets the challenge reference used by ChallengeOrganizer to manage challenge assignment
        /// </summary>
        public static Challenge GetChallengeType(Challenge challenge)
        {
            return ChallengeOrganizer.availableChallengeTypes.FindType(challenge);
        }

        public static string GetTypeName(this Challenge self)
        {
            return self.GetType().Name;
        }
    }
}
