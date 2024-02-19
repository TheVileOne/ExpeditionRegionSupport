using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expedition;

namespace ExpeditionRegionSupport.Filters
{
    public static class ChallengeAssignment
    {
        /// <summary>
        /// A cached list of regions used for challenge assignment  
        /// </summary>
        public static List<string> ValidRegions;

        /// <summary>
        /// The number of valid challenges expected to be assigned
        /// </summary>
        public static int ChallengesRequested;

        /// <summary>
        /// The active amount of valid challenges processed
        /// </summary>
        public static int ChallengesProcessed;

        /// <summary>
        /// The amount of attempts processed between process start and finish
        /// </summary>
        public static int TotalAttemptsProcessed = 0;

        /// <summary>
        /// The amount of process attempts to handle before logging process amount
        /// </summary>
        public const int REPORT_THRESHOLD = 30;

        /// <summary>
        /// Challenges are actively being assigned
        /// </summary>
        public static bool AssignmentsInProgress => ChallengesRequested > 0;

        public static void OnProcessStart(int requestAmount)
        {
            string msg = "Challenge Assignment IN PROGRESS";
            ExpLog.Log(msg);
            Plugin.Logger.LogInfo(msg);

            ChallengesRequested = requestAmount;
            ValidRegions = SlugcatStats.getSlugcatStoryRegions(ExpeditionData.slugcatPlayer).ToList();
        }

        public static void OnProcessFinish()
        {
            int failedToProcessAmount = ChallengesRequested - ChallengesProcessed;

            if (dataToReport())
            {
                string msg = "~_ ASSIGNMENT REPORT _~";
                ExpLog.Log(msg);
                Plugin.Logger.LogInfo(msg);

                if (failedToProcessAmount > 0)
                {
                    msg = $"Unable to process {failedToProcessAmount} challenge" + (failedToProcessAmount > 1 ? "s" : string.Empty);
                    ExpLog.Log(msg);
                    Plugin.Logger.LogInfo(msg);
                }

                if (TotalAttemptsProcessed >= REPORT_THRESHOLD)
                {
                    msg = "Excessive amount of challenge attempts handled";
                    ExpLog.Log(msg);
                    Plugin.Logger.LogInfo(msg);
                }
            }
            else
            {
                string msg = "Challenge Assignment COMPLETE";
                ExpLog.Log(msg);
                Plugin.Logger.LogInfo(msg);
            }

            ChallengesRequested = ChallengesProcessed = TotalAttemptsProcessed = 0;
            ValidRegions.Clear();

            bool dataToReport()
            {
                return failedToProcessAmount > 0 || TotalAttemptsProcessed >= REPORT_THRESHOLD;
            }
        }
    }
}
