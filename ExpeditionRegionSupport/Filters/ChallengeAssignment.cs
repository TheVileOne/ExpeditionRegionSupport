using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expedition;
using ExpeditionRegionSupport.Logging.Utils;

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
        public static int ChallengesRequested { get; private set; }

        /// <summary>
        /// The active amount of valid challenges processed
        /// </summary>
        public static int ChallengesProcessed { get; private set; }

        /// <summary>
        /// The amount of attempts processed between process start and finish
        /// </summary>
        public static int TotalAttemptsProcessed = 0;

        /// <summary>
        /// The amount of process attempts to handle before logging process amount
        /// </summary>
        public const int REPORT_THRESHOLD = 30;

        /// <summary>
        /// The process of choosing, or assigning an Expedition challenge has started, or is in progress
        /// </summary>
        public static bool AssignmentInProgress
        {
            get => ChallengesRequested > 0;
            set //Setter is only used for a specific use case in the code
            {
                if (AssignmentInProgress != value)
                {
                    if (value)
                    {
                        OnProcessStart(1); //We can only assume one challenge is requested
                    }
                    else if (LateStageProcessing)
                    {
                        //Process must be handled by the same method that triggered it.
                        //Assign stage process is handled in assign handlers. Creation stage process is handled in creation stage process.
                        if (AssignmentStage == AssignmentStageLate || AssignmentStage == ProcessStage.PostProcessing)
                            OnProcessFinish();
                    }
                }
            }
        }

        /// <summary>
        /// Indicates that AssignChallenge, or RandomChallenge has been called before OnProcessStart
        /// </summary>
        public static bool LateStageProcessing => AssignmentStageLate != ProcessStage.None;

        /// <summary>
        /// The current method handled during assignment
        /// </summary>
        public static ProcessStage AssignmentStage { get; private set; }

        /// <summary>
        /// The method handled during a late assignment
        /// </summary>
        public static ProcessStage AssignmentStageLate { get; private set; }

        /// <summary>
        /// Invoked before a challenge assignment batch is handled
        /// Mods should invoke this before AssignChallenge is called, especially when assigning multiple challenges as a batch
        /// </summary>
        public static void OnProcessStart(int requestAmount)
        {
            //Handle situations where OnProcessStart has not been called before AssignChallenge, or RandomChallenge is called
            AssignmentStageLate = AssignmentStage;

            LogUtils.LogBoth("Challenge Assignment IN PROGRESS");
            LogUtils.LogBoth($"{requestAmount} challenges requested");

            ChallengesRequested = requestAmount;
            ValidRegions = SlugcatStats.getSlugcatStoryRegions(ExpeditionData.slugcatPlayer).ToList();
        }

        /// <summary>
        /// Invoked after a challenge assignment batch is handled
        /// Mods should invoke this after challenge assignment processing is finished
        /// </summary>
        public static void OnProcessFinish()
        {
            //Set back to default values
            AssignmentStage = AssignmentStageLate = ProcessStage.None;

            int failedToProcessAmount = ChallengesRequested - ChallengesProcessed;

            if (dataToReport())
            {
                LogUtils.LogBoth("~_ ASSIGNMENT REPORT _~");

                if (failedToProcessAmount > 0)
                    LogUtils.LogBoth($"Unable to process {failedToProcessAmount} challenge" + (failedToProcessAmount > 1 ? "s" : string.Empty));

                if (TotalAttemptsProcessed >= REPORT_THRESHOLD)
                    LogUtils.LogBoth("Excessive amount of challenge attempts handled");
            }
            else
            {
                LogUtils.LogBoth("Challenge Assignment COMPLETE");
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
            AssignmentStage = ProcessStage.Assignment;
            AssignmentInProgress = true;

            if (Plugin.byUpdate)
                LogUtils.LogBoth("Source Update");
            else if (Plugin.bySignal)
                LogUtils.LogBoth("Source Signal");
            else if (Plugin.byConstructor)
                LogUtils.LogBoth("Source Constructor");
        }

        /// <summary>
        /// Invoked after AssignChallenge is invoked
        /// </summary>
        public static void OnAssignFinish()
        {
            AssignmentStage = ProcessStage.PostProcessing;
            AssignmentInProgress = false;
        }

        /// <summary>
        /// Invoked before RandomChallenge is invoked
        /// </summary>
        public static void OnChallengeSelect()
        {
            AssignmentStage = ProcessStage.Creation;
            AssignmentInProgress = true;

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

            AssignmentInProgress = false;
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

        public enum ProcessStage
        {
            None,
            Assignment, //When AssignChallenge is called
            Creation, //When RandomChallenge is called
            PostProcessing //After challenge has been generated and returned back to AssignChallenge
        }
    }
}
