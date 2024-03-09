using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
