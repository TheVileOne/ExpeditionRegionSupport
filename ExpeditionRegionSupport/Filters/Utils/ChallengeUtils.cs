using Expedition;
using MoreSlugcats;
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

        public static List<Challenge> GetCustomRegionChallenges()
        {
            return ChallengeOrganizer.availableChallengeTypes.FindAll(c => c is IRegionChallenge);
        }

        public static List<string> GetApplicableEchoRegions(SlugcatStats.Name slugcat)
        {
            List<string> applicableRegions = new List<string>(ExtEnum<GhostWorldPresence.GhostID>.values.entries);

            applicableRegions.Remove("NoGhost"); //Not a region

            if (ModManager.MSC)
            {
                applicableRegions.Remove("MS"); //This echo cannot be chosen for echo challenges

                //Remove echoes that only apply to Saint
                if (slugcat != MoreSlugcatsEnums.SlugcatStatsName.Saint)
                    applicableRegions.Remove("SL");
            }
            return applicableRegions;
        }
    }
}
