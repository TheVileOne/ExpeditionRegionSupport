using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogUtils
{
    public class LogRequestHandler : UtilityComponent
    {
        public override string Tag => UtilityConsts.ComponentTags.REQUEST_DATA;

        private List<BetaLogger> availableLoggers = new List<BetaLogger>();

        /// <summary>
        /// A list of loggers available to handle remote log requests
        /// </summary>
        public IEnumerable<BetaLogger> AvailableLoggers => availableLoggers;

        public GameLogger GameLogger = new GameLogger();

        public BufferedLinkedList<LogRequest> UnhandledRequests;
        private BufferedLinkedList<LogRequest>.Enumerator requestEnumerator;

        private LogRequest _currentRequest;

        /// <summary>
        /// The request currently being handled. The property is cleared when request has been properly handled, or the request has been swapped out to another request
        /// </summary>
        public LogRequest CurrentRequest
        {
            get => _currentRequest ?? PendingRequest;
            set => _currentRequest = value;
        }

        /// <summary>
        /// The latest request that has yet to be handled
        /// </summary>
        public LogRequest PendingRequest
        {
            get
            {
                if (UnhandledRequests.Count == 0) //No requests to handle
                    return null;

                LogRequest request = UnhandledRequests.Last.Value;

                if (request.Status == RequestStatus.Pending)
                    return request;

                return null;
            }

            private set
            {
                if (value == null)
                {
                    UnhandledRequests.RemoveLast();
                    return;
                }

                LogRequest lastUnhandledRequest = PendingRequest;

                //Ensure that only one pending request is handled by design. This shouldn't be the case normally, and handling it this way will consume the unhandled request
                if (lastUnhandledRequest != null && (lastUnhandledRequest.Status == RequestStatus.Complete || lastUnhandledRequest.Status == RequestStatus.Pending))
                    UnhandledRequests.RemoveLast();

                UnhandledRequests.AddLast(value);
            }
        }

        public LogRequestHandler()
        {
            enabled = true;
            UnhandledRequests = new BufferedLinkedList<LogRequest>(20);

            requestEnumerator = (BufferedLinkedList<LogRequest>.Enumerator)UnhandledRequests.GetEnumerator();
        }

        public IEnumerable<LogRequest> GetRequests(LogID logFile)
        {
            return UnhandledRequests.Where(req => req.Data.Properties.IDMatch(logFile));
        }

        /// <summary>
        /// Submit a request - will be treated as an active pending log request
        /// </summary>
        public LogRequest Submit(LogRequest request, bool handleSubmission = true)
        {
            LogID logFile = request.Data.ID;

            //Waiting requests must be handled before the submitted request
            ProcessRequests(logFile);

            //Ensures consistent handling of the request
            request.ResetStatus();

            if (logFile.Properties.HandleRecord.Rejected)
            {
                request.Reject(logFile.Properties.HandleRecord.Reason);

                handleRejection(request);
                return request;
            }

            //Check RainWorld.ShowLogs for logs that are restricted by it
            if (logFile.Properties.ShowLogsAware && !RainWorld.ShowLogs)
            {
                if (RWInfo.LatestSetupPeriodReached < RWInfo.SHOW_LOGS_ACTIVE_PERIOD)
                    request.Reject(RejectionReason.ShowLogsNotInitialized);
                else
                    request.Reject(RejectionReason.LogDisabled);
            }

            //Check one last time for rejected status
            if (request.Status == RequestStatus.Rejected)
            {
                handleRejection(request);
                return request;
            }

            //The pending request has not been rejected, and is available to be processed 
            PendingRequest = request;

            //Submission are either going to be handled through the request system, or the submission serves as a notification that the request is being handled at the source
            if (handleSubmission)
                ProcessRequest(request); //Processing will clean up for us when there is a rejection

            return request;

            void handleRejection(LogRequest request)
            {
                //A request shall be treated as pending when the rejection reason permits future process attempts
                if (request.CanRetryRequest())
                    PendingRequest = request;
                else if (!handleSubmission)
                    CurrentRequest = request; //Request cannot be handled anymore, but it should be stored until it is permitted to be handled
            }
        }

        /// <summary>
        /// Registers a logger for remote logging
        /// </summary>
        public void Register(BetaLogger logger)
        {
            UtilityCore.BaseLogger.LogInfo("Registering logger");
            UtilityCore.BaseLogger.LogInfo("Log targets: " + string.Join(" ,", logger.LogTargets));

            availableLoggers.Add(logger);

            foreach (LogID logFile in logger.LogTargets.Where(log => !log.IsGameControlled && log.Access != LogAccess.RemoteAccessOnly)) //Game controlled logids cannot be handled here
            {
                IEnumerable<LogRequest> requests = GetRequests(logFile);

                logger.HandleRequests(requests, true);
                DiscardHandledRequests(requests);
            }
        }

        /// <summary>
        /// Unregisters a logger for remote logging
        /// </summary>
        public void Unregister(BetaLogger logger)
        {
            UtilityCore.BaseLogger.LogInfo("Unregistering logger");
            UtilityCore.BaseLogger.LogInfo("Log targets: " + string.Join(" ,", logger.LogTargets));
            availableLoggers.Remove(logger);
        }

        /// <summary>
        /// Find a logger instance that accepts log requests for a specified LogID
        /// </summary>
        /// <param name="logFile">LogID to check</param>
        /// <param name="doPathCheck">Should the log file's containing folder bear significance when finding a logger match</param>
        private BetaLogger findCompatibleLogger(LogID logFile, bool doPathCheck)
        {
            BetaLogger bestLoggerMatch = null;
            foreach (var logger in availableLoggers.FindAll(logger => logger.CanAccess(logFile, doPathCheck)))
            {
                if (checkForBestMatch(logger))
                    bestLoggerMatch = logger;
            }

            return bestLoggerMatch;

            bool checkForBestMatch(BetaLogger matchCandidate)
            {
                return
                    bestLoggerMatch == null
                || (matchCandidate.AllowLogging && matchCandidate.AllowRemoteLogging) //The best possible case
                || (matchCandidate.AllowRemoteLogging && !bestLoggerMatch.AllowRemoteLogging)
                || (matchCandidate.AllowLogging && !bestLoggerMatch.AllowLogging);
            }
        }

        public void TryResolveRecord(LogID logFile)
        {
            //TODO: Check the handle record, and attempt to resolve the last known rejection reason

        /// <summary>
        /// Attempts to handle all unhandled log requests belonging to a single LogID in the order they were submitted
        /// </summary>
        public void ProcessRequests(LogID logFile)
        {
            if (logFile.Properties.HandleRecord.Rejected)
            {
                TryResolveRecord(logFile);

                if (logFile.Properties.HandleRecord.Rejected)
                {
                    RejectRequests(logFile, logFile.Properties.HandleRecord.Reason);
                    return;
                }
            }

            var requests = GetRequests(logFile);

            if (!logFile.IsGameControlled)
            {
                BetaLogger selectedLogger = findCompatibleLogger(logFile, doPathCheck: true);

                if (selectedLogger == null)
                {
                    RejectRequests(requests, RejectionReason.LogUnavailable);
                    return;
                }

                selectedLogger.HandleRequests(requests);
            }
            else
            {
                GameLogger.HandleRequests(requests);
            }

            DiscardHandledRequests(requests);
        }

        internal void ProcessRequest(LogRequest request)
        {
            LogID logFile = request.Data.ID;

            //This shouldn't be necessary, as rejections should be resolved before ProcessRequest is resolved
            if (logFile.Properties.HandleRecord.Rejected)
            {
                TryResolveRecord(logFile);

                if (logFile.Properties.HandleRecord.Rejected)
                {
                    RejectRequests(logFile, logFile.Properties.HandleRecord.Reason);
                    return;
                }

                //Waiting requests need to be checked and request will be included in the process operation 
                ProcessRequests(logFile); 
                return;
            }

            //Beyond this point, we can assume that there are no preexisting unhandled requests for this log file
            if (!logFile.IsGameControlled)
            {
                BetaLogger selectedLogger = findCompatibleLogger(logFile, doPathCheck: true);

                if (selectedLogger != null)
                    selectedLogger.HandleRequest(request, true);
                else
                    request.Reject(RejectionReason.LogUnavailable);
            }
            else
            {
                GameLogger.HandleRequest(request);
            }

            RequestMayBeCompleteOrInvalid(request);
        }

        /// <summary>
        /// Attempts to handle unhandled requests 
        /// </summary>
        public void ProcessRequests()
        {
            LogID targetID, lastTargetID;

            targetID = lastTargetID = null;
            BetaLogger selectedLogger = null;

            //Check every unhandled request, removing recently handled or invalid entries, and handling entries that are capable of being handled
            foreach (LogRequest target in requestEnumerator.EnumerateAll())
            {
                lastTargetID = targetID;
                targetID = target.Data.ID;

                bool requestCanBeHandled = true;

                if (requestCanBeHandled = !target.IsCompleteOrInvalid)
                {
                    //Find a logger that can be used for this LogID
                    if (selectedLogger == null || targetID != lastTargetID)
                        selectedLogger = findCompatibleLogger(targetID, doPathCheck: false);

                    if (selectedLogger != null)
                    {
                        //Try to handle the log request, and recheck the status
                        RejectionReason result = selectedLogger.HandleRequest(target, true);

                        if (requestCanBeHandled = !target.IsCompleteOrInvalid)
                        {
                            if (result == RejectionReason.PathMismatch)
                            {
                                //Attempt to find a logger that accepts the target LogID with this exact path
                                selectedLogger = findCompatibleLogger(targetID, doPathCheck: true);

                                if (selectedLogger != null)
                                {
                                    selectedLogger.HandleRequest(target, true);
                                    requestCanBeHandled = !target.IsCompleteOrInvalid;
                                }
                            }
                        }

                        if (selectedLogger == null)
                            target.Reject(RejectionReason.LogUnavailable);
                    }
                }

                if (!requestCanBeHandled)
                    UnhandledRequests.Remove(requestEnumerator.CurrentNode);
            }
        }

        public void RejectRequests(LogID logFile, RejectionReason reason)
        {
            //TODO: This doesn't account for requests targeting different log paths
            RejectRequests(GetRequests(logFile), reason);
        }

        public void RejectRequests(IEnumerable<LogRequest> requests, RejectionReason reason)
        {
            foreach (LogRequest request in requests)
                request.Reject(reason);

            DiscardHandledRequests(requests);
        }

        /// <summary>
        /// Clean up process for requests that need to be removed from the request handling system
        /// </summary>
        public void RequestMayBeCompleteOrInvalid(LogRequest request)
        {
            if (request.IsCompleteOrInvalid)
            {
                UnhandledRequests.Remove(request);

                if (CurrentRequest == request) //Removing the request may not clear this field
                    CurrentRequest = null;
            }
        }

        /// <summary>
        /// Remove requests that have been handled since being processed
        /// </summary>
        internal void DiscardHandledRequests(IEnumerable<LogRequest> requests)
        {
            //Check the status of all processed requests to remove the handled ones

            foreach (LogRequest request in requests)
                RequestMayBeCompleteOrInvalid(request);
        }

        public void Update()
        {
            //Requests should not be regularly checked until after the game, mods, and associated logger instances have been given time to initialize
            //There is a separate process for handling log requests earlier in the setup process
            if (RWInfo.LatestSetupPeriodReached >= SetupPeriod.PostMods)
                ProcessRequests();
        }

        /// <summary>
        /// Dumps all unhandled log requests to a special dump file
        /// </summary>
        public void DumpRequestsToFile()
        {
            LogID logDump = new LogID("LogDump"); //TODO: Timestamp, allow new log file to be created at anytime
            BetaLogger logger = new BetaLogger(logDump);

            string writePath = logDump.Properties.CurrentFilePath;

            //Delete existing log file before write
            if (File.Exists(writePath))
                File.Delete(writePath);

            logger.Log(UnhandledRequests);
        }
    }
}
