using Expedition;
using MoreSlugcats;
using System.Collections.Generic;
using System.Linq;
using Watcher;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public static class ChallengeUtils
    {
        public static HashSet<GhostWorldPresence.GhostID> EchoBlacklist = new HashSet<GhostWorldPresence.GhostID>();

        public static void GenerateEchoBlacklist(SlugcatStats.Name slugcat)
        {
            EchoBlacklist.Clear();

            if (ModManager.Watcher)
                EchoBlacklist.Add(WatcherEnums.GhostID.SpinningTop);

            EchoBlacklist.Add(GhostWorldPresence.GhostID.NoGhost); //Not a region

            if (ModManager.MSC)
            {
                EchoBlacklist.Add(MoreSlugcatsEnums.GhostID.MS); //This echo cannot be chosen for echo challenges

                //Remove echoes that only apply to Saint
                if (slugcat != MoreSlugcatsEnums.SlugcatStatsName.Saint)
                    EchoBlacklist.Add(MoreSlugcatsEnums.GhostID.SL);
            }

            for (int i = 0; i < ChallengeTools.echoScores.Length; i++)
            {
                if (ChallengeTools.echoScores[i] <= 0)
                    EchoBlacklist.Add(new GhostWorldPresence.GhostID(GhostWorldPresence.GhostID.values.entries[i]));
            }

            if (ChallengeTools.echoScores.Length < GhostWorldPresence.GhostID.values.entries.Count)
            {
                Plugin.Logger.LogWarning("Echo scores are nopt updated");
                foreach (string value in GhostWorldPresence.GhostID.values.entries.Skip(ChallengeTools.echoScores.Length))
                {
                    EchoBlacklist.Add(new GhostWorldPresence.GhostID(value));
                }
            }
        }

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
    }
}
