using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expedition;
using ExpeditionRegionSupport.Filters.Utils;
using ExpeditionRegionSupport.Logging.Utils;
using ExpeditionRegionSupport.Regions;

namespace ExpeditionRegionSupport.Filters
{
    public static class ChallengeAssignment
    {
        public static List<ChallengeRequestInfo> Requests = new List<ChallengeRequestInfo>();

        public static int RequestsProcessed => Requests.Count;

        /// <summary>
        /// A challenge request before it is processed, and validated
        /// </summary>
        private static ChallengeRequestInfo requestInProgress;

        public static ChallengeRequestInfo CurrentRequest
        {
            get
            {
                if (requestInProgress != null)
                    return requestInProgress;

                return RequestsProcessed > 0 ? Requests[RequestsProcessed - 1] : null;
            }
        }

        public static FilterApplicator<Challenge> ChallengeRemover;

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
        /// The amount of process attempts to handle before logging process amount
        /// </summary>
        public const int REPORT_THRESHOLD = 30;

        /// <summary>
        /// The process of choosing, or assigning an Expedition challenge has started, or is in progress
        /// </summary>
        public static bool AssignmentInProgress
        {
            get => ChallengesRequested > 0;
            set
            {
                if (value)
                {
                    bool firstProcess;

                    //Handles late stage assignment
                    if (AssignmentInProgress != value)
                    {
                        OnProcessStart(1); //We can only assume one challenge is requested
                        firstProcess = true;
                    }
                    else
                    {
                        firstProcess = AssignmentStage == ProcessStage.Assignment;
                    }

                    //Any code that needs to run at the start of the challenge request process goes here
                    if (firstProcess)
                        requestInProgress = new ChallengeRequestInfo();
                }
                else if (LateStageProcessing)
                {
                    //Process must be handled by the same method that triggered it
                    bool canFinishAtThisStage = AssignmentStage == AssignmentStageLate || AssignmentStage == ProcessStage.PostProcessing;

                    if (AssignmentInProgress != value && canFinishAtThisStage)
                    {
                        OnRequestHandled();
                        OnProcessFinish();
                    }
                }
                else if (AssignmentStage != ProcessStage.Creation) //Prevent request from being handled twice
                {
                    //This could be part of a batch. Only handle the current request
                    OnRequestHandled();
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

        public static bool AllowLateStageProcessing = true;

        public static Action HandleOnProcessComplete;

        /// <summary>
        /// Invoked before a challenge assignment batch is handled
        /// Mods should invoke this before AssignChallenge is called, especially when assigning multiple challenges as a batch
        /// </summary>
        public static void OnProcessStart(int requestAmount)
        {
            if (AssignmentInProgress)
                throw new InvalidOperationException("Process must be ended before another one can begin.");

            //Handle situations where OnProcessStart has not been called before AssignChallenge, or RandomChallenge is called
            AssignmentStageLate = AssignmentStage;

            LogUtils.LogBoth("Challenge Assignment IN PROGRESS");
            LogUtils.LogBoth($"{requestAmount} challenges requested");

            if (LateStageProcessing)
            {
                LogUtils.LogBoth("Processing late");

                if (!AllowLateStageProcessing)
                    throw new InvalidOperationException("Late stage processing has been disabled");
            }

            //Enable caching and create the primary region filter
            RegionUtils.CacheAvailableRegions = true;
            RegionUtils.AssignFilter(ExpeditionData.slugcatPlayer);

            ChallengesRequested = requestAmount;

            //Apply any filters that should apply to all challenge assignments
            foreach (Challenge challenge in ChallengeOrganizer.availableChallengeTypes)
            {
                if (challenge is ItemHoardChallenge || challenge is AchievementChallenge) continue; //This challenge cannot be determined based on slugcat alone

                if (!challenge.ValidForThisSlugcat(ExpeditionData.slugcatPlayer))
                    ChallengeRemover.ItemsToRemove.Add(challenge);
            }

            ChallengeRemover.Apply();
        }

        /// <summary>
        /// Invoked after a challenge assignment batch is handled
        /// Mods should invoke this after challenge assignment processing is finished
        /// </summary>
        public static void OnProcessFinish()
        {
            HandleOnProcessComplete?.Invoke();

            //Set back to default values
            AssignmentStage = AssignmentStageLate = ProcessStage.None;

            LogUtils.LogBoth(createAssignmentReport());

            RegionUtils.CacheAvailableRegions = false;
            RegionUtils.ClearFilters();

            ChallengeRemover.Restore();
            ChallengesRequested = 0;
            Requests.Clear();
        }

        /// <summary>
        /// Invoked before AssignChallenge is invoked
        /// </summary>
        public static void OnAssignStart()
        {
            AssignmentStage = ProcessStage.Assignment;
            AssignmentInProgress = true;
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
        /// Processing a challenge that was generated and accepted by ChallengeOrganizer
        /// </summary>
        /// <param name="challenge">The challenge accepted</param>
        public static void OnChallengeAccepted(Challenge challenge)
        {
            if (challenge == null)
                throw new ArgumentNullException(nameof(challenge), "Challenge handler received null parameter");

            string challengeName = challenge.GetTypeName();
            Challenge challengeType = ChallengeUtils.GetChallengeType(challenge);

            switch (challengeName)
            {
                //These challenge types do not have any challenge specific filtering by default
                case ExpeditionConsts.ChallengeNames.NEURON_DELIVERY:
                case ExpeditionConsts.ChallengeNames.GLOBAL_SCORE:
                case ExpeditionConsts.ChallengeNames.CYCLE_SCORE:
                case ExpeditionConsts.ChallengeNames.PEARL_HOARD:
                case ExpeditionConsts.ChallengeNames.PIN:
                    ChallengeRemover.ItemsToRemove.Add(challengeType);
                    break;
                case ExpeditionConsts.ChallengeNames.ECHO:
                    List<string> availableRegions = RegionUtils.CurrentFilter.ApplyTemp(ExpeditionData.challengeList,
                        challenge => challenge is EchoChallenge,
                        challenge =>
                        {
                            EchoChallenge echoChallenge = (EchoChallenge)challenge;
                            return echoChallenge.ghost.value;
                        });

                    if (availableRegions.Count == 0)
                        ChallengeRemover.ItemsToRemove.Add(challengeType);
                    break;
            }
            CurrentRequest.Challenge = challenge;
        }

        /// <summary>
        /// Processing a challenge that was generated and rejected by ChallengeOrganizer
        /// </summary>
        /// <param name="challenge">The challenge rejected</param>
        /// <param name="failCode">The code indicating the reason for rejection</param>
        public static void OnChallengeRejected(Challenge challenge, int failCode)
        {
            if (challenge == null)
                throw new ArgumentNullException(nameof(challenge), "Challenge handler received null parameter");

            FailCode code = (FailCode)failCode;

            //Plugin.Logger.LogDebug(code);

            switch (code)
            {
                case FailCode.NotValidForSlugcat:
                    if (!(challenge is ItemHoardChallenge || challenge is AchievementChallenge))
                        ChallengeRemover.ItemsToRemove.Add(ChallengeUtils.GetChallengeType(challenge));
                    break;
                case FailCode.InvalidDuplication:
                    {
                        //These challenge types do not have any challenge specific filtering by default
                        if (challenge is NeuronDeliveryChallenge
                         || challenge is GlobalScoreChallenge
                         || challenge is CycleScoreChallenge
                         || challenge is PearlHoardChallenge
                         || challenge is PinChallenge)
                            ChallengeRemover.ItemsToRemove.Add(ChallengeUtils.GetChallengeType(challenge));
                    }
                    break;
                case FailCode.InvalidHidden:
                    break;
            }

            CurrentRequest.FailedAttempts++;
        }

        internal static void OnRequestHandled()
        {
            if (requestInProgress == null) return;

            if (Requests.Count == ChallengesRequested)
                throw new InvalidOperationException("Too many requests handled");

            Requests.Add(requestInProgress);
            requestInProgress = null;
        }

        /// <summary>
        /// Creates a formatted string containing information about processed requests
        /// </summary>
        private static string createAssignmentReport()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("~_ ASSIGNMENT REPORT _~");
            sb.AppendLine();
            sb.AppendLine("REQUESTS PROCESSED " + RequestsProcessed);

            if (ChallengesRequested != RequestsProcessed)
                sb.AppendLine("REQUESTS ABORTED " + (ChallengesRequested - RequestsProcessed));

            StringBuilder sbChallenges = new StringBuilder();

            int totalAttempts = 0;
            int totalFailedAttempts = 0;
            foreach (ChallengeRequestInfo request in Requests)
            {
                totalFailedAttempts += request.FailedAttempts;
                totalAttempts += request.TotalAttempts;

                sbChallenges.AppendLine(request.ToString());
            }

            sb.AppendLine("FAILED ATTEMPTS " + totalFailedAttempts);

            if (totalFailedAttempts >= REPORT_THRESHOLD)
                sb.AppendLine("Excessive amount of challenge attempts handled");

            sb.AppendLine("CHALLENGES");

            //Include individual challenge information
            sb.AppendLine(sbChallenges.ToString().TrimEnd(Environment.NewLine.ToCharArray()));

            return sb.ToString();
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
