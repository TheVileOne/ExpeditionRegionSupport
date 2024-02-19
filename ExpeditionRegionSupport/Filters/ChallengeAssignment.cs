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
        /// Keep track of a separate list of types for restoring entries to the list in an exact order
        /// </summary>
        public static List<Challenge> ChallengeTypesBackup;

        /// <summary>
        /// A list of challenges that were determined to be unselectable due to selected filter options.
        /// </summary>
        public static List<Challenge> RemovedChallengeTypes = new List<Challenge>();

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

        /// <summary>
        /// Invoked before a challenge assignment batch is handled
        /// </summary>
        public static void OnProcessStart(int requestAmount)
        {
            string msg = "Challenge Assignment IN PROGRESS";
            ExpLog.Log(msg);
            Plugin.Logger.LogInfo(msg);

            ChallengesRequested = requestAmount;
            ValidRegions = SlugcatStats.getSlugcatStoryRegions(ExpeditionData.slugcatPlayer).ToList();
        }

        /// <summary>
        /// Invoked after a challenge assignment batch is handled
        /// </summary>
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

        /// <summary>
        /// Invoked before AssignChallenge is invoked
        /// </summary>
        public static void OnAssignStart()
        {
            if (Plugin.byUpdate)
                ExpLog.Log("Source Update");
            else if (Plugin.bySignal)
                ExpLog.Log("Source Signal");
            else if (Plugin.byConstructor)
                ExpLog.Log("Source Constructor");
        }

        /// <summary>
        /// Invoked after AssignChallenge is invoked
        /// </summary>
        public static void OnAssignFinish()
        {
        }

        /// <summary>
        /// Invoked before RandomChallenge is invoked
        /// </summary>
        public static void OnChallengeSelect()
        {
            //Remove all challenges that we do not want the game to handle
            ChallengeOrganizer.availableChallengeTypes.RemoveAll(RemovedChallengeTypes);
        }

        /// <summary>
        /// Invoked after RandomChallenge is invoked
        /// </summary>
        public static void OnChallengeSelectFinish()
        {
            //Add them back here
            if (RemovedChallengeTypes.Count > 0)
            {
                ChallengeOrganizer.availableChallengeTypes.Clear();
                ChallengeOrganizer.availableChallengeTypes.AddRange(ChallengeTypesBackup);
            }
        }

        /// <summary>
        /// Handle when a challenge was unable to be selected
        /// </summary>
        public static void OnGenerationFailed(List<Challenge> availableChallenges)
        {
            Challenge target = ChallengeFilterSettings.FilterTarget;

            Plugin.Logger.LogInfo($"Challenge type { target.ChallengeName()} could not be selected. Generating another");

            RemovedChallengeTypes.Add(target);
            availableChallenges.Remove(target);

            ChallengeFilterSettings.FilterTarget = null;
        }

        /// <summary>
        /// Processing a challenge that was generated and rejected by ChallengeOrganizer
        /// </summary>
        /// <param name="challenge">The Challenge rejected</param>
        /// <param name="failCode">The code indicating the reason for rejection</param>
        public static void OnChallengeRejected(Challenge challenge, int failCode)
        {
            FailCode code = (FailCode)failCode;

            //Plugin.Logger.LogDebug(code);

            switch (code)
            {
                case FailCode.NotValidForSlugcat:
                    break;
                case FailCode.InvalidDuplication:
                    break;
                case FailCode.InvalidHidden:
                    break;
            }
        }

        private enum FailCode
        {
            NotValidForSlugcat = 0,
            InvalidDuplication = 1,
            InvalidHidden = 2,
        };
    }
}
