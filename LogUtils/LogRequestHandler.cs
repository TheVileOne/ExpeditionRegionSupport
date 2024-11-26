using LogUtils.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    public class LogRequestHandler : UtilityComponent
    {
        /// <summary>
        /// This lock object marshals control over submission of LogRequests, and processing requests stored in UnhandledRequests. When there is a need to
        /// process LogRequests directly from UnhandledRequests, it is recommended to use this lock object to achieve thread safety
        /// </summary>
        public object RequestProcessLock = new object();

        public override string Tag => UtilityConsts.ComponentTags.REQUEST_DATA;

        private List<Logger> availableLoggers = new List<Logger>();

        /// <summary>
        /// A list of loggers available to handle remote log requests
        /// </summary>
        public IEnumerable<Logger> AvailableLoggers => availableLoggers;

        public GameLogger GameLogger = new GameLogger();

        /// <summary>
        /// Contains LogRequest objects that are submitted and waiting to be handled
        /// </summary>
        public BufferedLinkedList<LogRequest> UnhandledRequests;

        /// <summary>
        /// Contains LogRequest objects that need to be submitted on the next available frame
        /// </summary>
        public Queue<LogRequest> HandleOnNextAvailableFrame = new Queue<LogRequest>();

        private LogRequest _currentRequest;

        /// <summary>
        /// The request currently being handled. The property is cleared when request has been properly handled, or the request has been swapped out to another request
        /// </summary>
        public LogRequest CurrentRequest
        {
            get => _currentRequest ?? PendingRequest;
            set
            {
                if (value != null && !value.Submitted)
                {
                    Submit(value, false); //CurrentRequest will be set on submission
                    return;
                }

                lock (RequestProcessLock)
                {
                    if (CurrentRequest != value)
                        _currentRequest = value;
                }
            }
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
                lock (RequestProcessLock)
                {
                    LogRequest lastUnhandledRequest = PendingRequest;

                    if (lastUnhandledRequest == value)
                        return;

                    if (lastUnhandledRequest != null)
                    {
                        //Enforce only one pending request by design by consuming the last pending request if it exists
                        lastUnhandledRequest.Submitted = false;
                        UnhandledRequests.RemoveLast();
                    }

                    if (value != null)
                        UnhandledRequests.AddLast(value);
                }
            }
        }

        /// <summary>
        /// A flag that can be used to do a full check to discard handled, or no longer valid requests
        /// </summary>
        public bool CheckForHandledRequests;

        public LogRequestHandler()
        {
            enabled = true;
            UnhandledRequests = new BufferedLinkedList<LogRequest>(20);
        }

        public ILinkedListEnumerable<LogRequest> GetRequests(LogID logFile)
        {
            return UnhandledRequests.Where(req => req.Data.Properties.HasID(logFile));
        }

        /// <summary>
        /// Submit a request - Treated as an active pending log request unless the request itself did not qualify for submission. A request must meet the following conditions: 
        /// I. No rejection reasons were found during initial processing of the request
        /// II. Under the situation that there is a reason to reject, that reason is not severe enough to prevent future attempts to process the request
        /// Submitted request may be retrieved through CurrentRequest under the above conditions, or from the return value
        /// </summary>
        /// <param name="request">The request to be processed</param>
        /// <param name="handleSubmission">Whether a log attempt should be made on the request</param>
        /// <returns>This method returns the same request given to it under any condition. The return value is more reliable than checking CurrentRequest, which may be null</returns>
        public LogRequest Submit(LogRequest request, bool handleSubmission)
        {
            lock (RequestProcessLock)
            {
                if (request.Submitted)
                    UtilityLogger.LogWarning("Submitted request has already been submitted at least once");

                //Ensures consistent handling of the request
                request.ResetStatus();
                request.Submitted = true;

                LogRequest lastPendingRequest = null;
                LogID logFile = request.Data.ID;

                //Waiting requests must be handled before the submitted request
                ProcessRequests(logFile);

                if (logFile.Properties.HandleRecord.Rejected)
                {
                    UtilityLogger.DebugLog("Aborting early due to rejection");
                    request.Reject(logFile.Properties.HandleRecord.Reason);

                    handleRejection(request);
                    return request;
                }

                //These checks will be handled by the respective loggers, it is handled here to avoid a logger check, and to ensure that
                //these checks are applied without a log attempt
                if (!handleSubmission || !logFile.IsGameControlled)
                {
                    //Check RainWorld.ShowLogs for logs that are restricted by it
                    if (logFile.Properties.ShowLogsAware && !RainWorld.ShowLogs)
                    {
                        if (RWInfo.LatestSetupPeriodReached < RWInfo.SHOW_LOGS_ACTIVE_PERIOD)
                            request.Reject(RejectionReason.ShowLogsNotInitialized);
                        else
                            request.Reject(RejectionReason.LogDisabled);
                    }

                    if (!logFile.Properties.CanBeAccessed)
                        request.Reject(RejectionReason.LogUnavailable);

                    //Check one last time for rejected status
                    if (request.Status == RequestStatus.Rejected)
                    {
                        handleRejection(request);
                        return request;
                    }
                }

                lastPendingRequest = PendingRequest;

                //The pending request has not been rejected, and is available to be processed 
                PendingRequest = request;

                //Submission are either going to be handled through the request system, or the submission serves as a notification that the request is being handled at the source
                if (handleSubmission)
                    ProcessRequest(request); //Processing will clean up for us when there is a rejection
                return request;
            }

            void handleRejection(LogRequest request)
            {
                //A request shall be treated as pending when the rejection reason permits future process attempts
                if (request.CanRetryRequest())
                    PendingRequest = request;
            }
        }

        public LogRequest TrySubmit(LogRequest request, bool handleSubmission)
        {
            try
            {
                return Submit(request, handleSubmission);
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Unable to submit request", ex);

                //Assign request using a safer method with less rigorous validation checks - wont fail
                if (request.CanRetryRequest())
                    PendingRequest = request;
                return request;
            }
        }

        /// <summary>
        /// Registers a logger for remote logging
        /// </summary>
        public void Register(Logger logger)
        {
            UtilityLogger.Log("Registering logger");
            UtilityLogger.Log("Log targets: " + string.Join(" ,", logger.LogTargets));

            availableLoggers.Add(logger);

            foreach (LogID logFile in logger.LogTargets.Where(log => !log.IsGameControlled && log.Access != LogAccess.RemoteAccessOnly)) //Game controlled logids cannot be handled here
            {
                ILinkedListEnumerable<LogRequest> requests = GetRequests(logFile);

                logger.HandleRequests(requests, true);
                DiscardHandledRequests(requests);
            }
        }

        /// <summary>
        /// Unregisters a logger for remote logging
        /// </summary>
        public void Unregister(Logger logger)
        {
            UtilityLogger.Log("Unregistering logger");
            UtilityLogger.Log("Log targets: " + string.Join(" ,", logger.LogTargets));
            availableLoggers.Remove(logger);
        }

        /// <summary>
        /// Finds a list of all logger instances that accepts log requests for a specified LogID
        /// </summary>
        /// <param name="logFile">LogID to check</param>
        /// <param name="requestType">The request type expected</param>
        /// <param name="doPathCheck">Should the log file's containing folder bear significance when finding a logger match</param>
        private List<Logger> findCompatibleLoggers(LogID logFile, RequestType requestType, bool doPathCheck)
        {
            return availableLoggers.FindAll(logger => logger.CanAccess(logFile, requestType, doPathCheck));
        }

        private void findCompatibleLoggers(LogID logFile, out Logger localLogger, out Logger remoteLogger)
        {
            localLogger = remoteLogger = null;

            foreach (Logger logger in findCompatibleLoggers(logFile, RequestType.Remote, doPathCheck: true))
            {
                //Most situations wont make it past the first assignment
                if (localLogger == null)
                {
                    localLogger = remoteLogger = logger;
                    continue;
                }

                //Choose the first logger match that allows logging
                if (!localLogger.AllowLogging)
                {
                    localLogger = logger;

                    //Align the remote logger reference with the local logger when remote logging is still unavailable
                    if (!remoteLogger.AllowRemoteLogging)
                        remoteLogger = localLogger;
                    continue;
                }

                //The local logger is the perfect match for the remote logger
                if (localLogger.AllowRemoteLogging)
                {
                    remoteLogger = localLogger;
                    break;
                }

                int results = RemoteLoggerComparer.DefaultComparer.Compare(remoteLogger, logger);

                if (results > 0)
                {
                    remoteLogger = logger;

                    if (results == RemoteLoggerComparer.MAX_SCORE)
                        break;
                }
            }

            //Check specifically for a logger instance that handles local requests in the unusual case that no logger instances can handle remote requests
            if (localLogger == null)
            {
                remoteLogger = null;

                foreach (Logger logger in findCompatibleLoggers(logFile, RequestType.Local, doPathCheck: true))
                {
                    if (localLogger == null || logger.AllowLogging)
                    {
                        localLogger = logger;

                        if (logger.AllowLogging)
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Find a logger instance that accepts log requests for a specified LogID
        /// </summary>
        /// <param name="logFile">LogID to check</param>
        /// <param name="requestType">The request type expected</param>
        /// <param name="doPathCheck">Should the log file's containing folder bear significance when finding a logger match</param>
        private Logger findCompatibleLogger(LogID logFile, RequestType requestType, bool doPathCheck)
        {
            UtilityLogger.DebugLog("Finding logger");

            if (requestType == RequestType.Game)
                return null;

            List<Logger> candidates = findCompatibleLoggers(logFile, requestType, doPathCheck);

            if (candidates.Count == 0)
                return null;

            if (candidates.Count == 1)
                return candidates[0];

            Logger bestCandidate;
            if (requestType == RequestType.Local)
            {
                bestCandidate = candidates.Find(logger => logger.AllowLogging) ?? candidates[0];
            }
            else
            {
                bestCandidate = candidates[0];

                if (bestCandidate.AllowLogging && bestCandidate.AllowRemoteLogging)
                    return bestCandidate;

                foreach (Logger logger in candidates)
                {
                    int results = RemoteLoggerComparer.DefaultComparer.Compare(bestCandidate, logger);

                    if (results > 0)
                    {
                        bestCandidate = logger;

                        if (results == RemoteLoggerComparer.MAX_SCORE)
                            break;
                    }
                }
            }
            return bestCandidate;
        }

        public void TryResolveRecord(LogID logFile)
        {
            //TODO: Check the handle record, and attempt to resolve the last known rejection reason
            logFile.Properties.HandleRecord.Reason = RejectionReason.None;
            return;

            LogRequestRecord handleRecord = logFile.Properties.HandleRecord;

            bool resolved = false;
            switch (handleRecord.Reason)
            {
                case RejectionReason.LogUnavailable:
                case RejectionReason.AccessDenied:
                    //BetaLogger selectedLogger = findCompatibleLogger(logFile, doPathCheck: true);

                    //resolved = selectedLogger != null && selectedLogger.AllowLogging && selectedLogger.LogTargets.Find(log => log == logFile).Access != LogAccess.Private;
                    break;

                case RejectionReason.ExceptionAlreadyReported:
                //FailedToWrite is not guaranteed to apply to every request with a specified LogID. It can be resolved immediately
                case RejectionReason.FailedToWrite:
                    resolved = true;
                    break;
            }

            if (resolved)
                logFile.Properties.HandleRecord.Reason = RejectionReason.None;
        }

        /// <summary>
        /// Attempts to handle all unhandled log requests belonging to a single LogID in the order they were submitted
        /// </summary>
        public void ProcessRequests(LogID logFile)
        {
            lock (RequestProcessLock)
            {
                if (logFile.Properties.HandleRecord.Rejected)
                {
                    UtilityLogger.DebugLog("Rejection record detected for this request");

                    TryResolveRecord(logFile);

                    if (logFile.Properties.HandleRecord.Rejected)
                    {
                        RejectRequests(logFile, logFile.Properties.HandleRecord.Reason);
                        return;
                    }
                }

                ILinkedListEnumerable<LogRequest> requests = GetRequests(logFile);

                if (!requests.Any()) return;

                if (!logFile.IsGameControlled)
                {
                    Logger selectedLogger = null;
                    Logger localLogger = null;
                    Logger remoteLogger = null;

                    bool shouldFetchLoggers = true;
                    LogRequest lastRequest = null;

                    foreach (LogRequest request in requests)
                    {
                        if (lastRequest != null)
                            shouldFetchLoggers = !lastRequest.Data.Properties.HasID(request.Data.ID); //TODO: Need to check for path here

                        if (shouldFetchLoggers)
                        {
                            findCompatibleLoggers(logFile, out localLogger, out remoteLogger);

                            selectedLogger = request.Type == RequestType.Remote ? remoteLogger : localLogger;
                            shouldFetchLoggers = false;
                        }

                        if (selectedLogger != null)
                            selectedLogger.HandleRequest(request);
                        else
                            request.Reject(RejectionReason.LogUnavailable);

                        lastRequest = request;
                    }
                }
                else
                {
                    GameLogger.HandleRequests(requests);
                }

                DiscardHandledRequests(requests);
            }
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
                Logger selectedLogger = findCompatibleLogger(logFile, request.Type, doPathCheck: true);

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
            LinkedListNode<LogRequest> targetNode = null;

            targetID = lastTargetID = null;
            Logger selectedLogger = null;
            bool verifyRemoteAccess = false;

            int requestNumber = 1;
            string targetString = string.Empty;

            lock (RequestProcessLock)
            {
                var requestEnumerator = UnhandledRequests.GetLinkedListEnumerator();

                //Check every unhandled request, removing recently handled or invalid entries, and handling entries that are capable of being handled
                while (requestEnumerator.MoveNext())
                {
                    LogRequest target = requestEnumerator.Current;

                    if (target == null)
                    {
                        UtilityLogger.LogWarning("Processed a null log request... aborting operation");
                        break;
                    }

                    UtilityLogger.DebugLog($"Request # [{requestNumber}] {target}");

                    requestNumber++;

                    lastTargetID = targetID;
                    targetID = target.Data.ID;
                    targetNode = requestEnumerator.CurrentNode;

                    bool requestCanBeHandled = true;

                    if (requestCanBeHandled = !target.IsCompleteOrInvalid)
                    {
                        if (!targetID.IsGameControlled)
                        {
                            //Find a logger that can be used for this LogID
                            if (selectedLogger == null || targetID != lastTargetID || (verifyRemoteAccess && target.Type == RequestType.Remote && !selectedLogger.AllowRemoteLogging))
                            {
                                verifyRemoteAccess = target.Type == RequestType.Local; //Make the system aware that the logger expects local requests
                                selectedLogger = findCompatibleLogger(targetID, target.Type, doPathCheck: false);
                            }

                            if (selectedLogger != null)
                            {
                                //Try to handle the log request, and recheck the status
                                RejectionReason result = selectedLogger.HandleRequest(target, true);

                                if (requestCanBeHandled = !target.IsCompleteOrInvalid)
                                {
                                    if (result == RejectionReason.PathMismatch)
                                    {
                                        //Attempt to find a logger that accepts the target LogID with this exact path
                                        selectedLogger = findCompatibleLogger(targetID, target.Type, doPathCheck: true);

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
                        else
                        {
                            UtilityLogger.DebugLog("Handling game request");
                            GameLogger.HandleRequest(target);
                            requestCanBeHandled = !target.IsCompleteOrInvalid;
                        }
                    }

                    if (target.Submitted && !requestCanBeHandled)
                    {
                        if (targetNode == null)
                        {
                            UtilityLogger.DebugLog("Status: " + target.Status);
                            UtilityLogger.DebugLog("Rejection Reason: " + target.UnhandledReason);
                            UtilityLogger.DebugLog("Attempted to remove a null node");
                            continue;
                        }

                        target.Submitted = false;
                        UnhandledRequests.Remove(targetNode);
                    }
                }
            }
        }

        public void RejectRequests(LogID logFile, RejectionReason reason)
        {
            //TODO: This doesn't account for requests targeting different log paths
            RejectRequests(GetRequests(logFile), reason);
        }

        public void RejectRequests(IEnumerable<LogRequest> requests, RejectionReason reason)
        {
            UtilityLogger.Log(LogCategory.Debug, "Rejection requests in bulk for reason: " + reason);

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
                if (UnhandledRequests.Remove(request))
                    request.Submitted = false;

                if (CurrentRequest == request) //Removing the request may not clear this field
                    CurrentRequest = null;
            }
        }

        /// <summary>
        /// Checks that requests in enumerable have been completed, or are no longer valid, removing any that have from UnhandledRequests 
        /// </summary>
        internal void DiscardHandledRequests(IEnumerable<LogRequest> requests)
        {
            //Check the status of all processed requests to remove the handled ones
            foreach (LogRequest request in requests)
                RequestMayBeCompleteOrInvalid(request);
        }

        /// <summary>
        /// Checks all requests stored in UnhandledRequests, removing any that have been completed, or are no longer valid
        /// </summary>
        public void DiscardHandledRequests()
        {
            DiscardHandledRequests(UnhandledRequests); //All requests are checked over
            CheckForHandledRequests = false;
        }

        public void Update()
        {
            if (HandleOnNextAvailableFrame.Any())
            {
                lock (RequestProcessLock)
                {
                    UtilityLogger.DebugLog("Handling scheduled requests");
                    while (HandleOnNextAvailableFrame.Any())
                    {
                        LogRequest request = HandleOnNextAvailableFrame.Dequeue();

                        try
                        {
                            request = Submit(request, true);
                        }
                        catch
                        {
                            //A request of this nature is important enough to bypass the request process by sending the request directly to the LogWriter 
                            if (request.Data.ID == LogID.Exception && request.CanRetryRequest())
                            {
                                UtilityLogger.LogWarning("Exception message forcefully logged to file");

                                request.ResetStatus();
                                LogWriter.Writer.WriteFrom(request);
                            }
                        }
                    }
                }

                //CheckForHandledRequests = true;
            }

            if (CheckForHandledRequests)
                DiscardHandledRequests();

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
            if (!UnhandledRequests.Any()) return;

            LogID logDump = LogID.CreateTemporaryID("LogDump", UtilityConsts.PathKeywords.ROOT); //TODO: Timestamp, allow new log file to be created at anytime

            logDump.Properties.ShowIntroTimestamp = true;

            Logger logger = new Logger(false, logDump);

            /*
            string writePath = logDump.Properties.CurrentFilePath;

            //Delete existing log file before write
            if (File.Exists(writePath))
                File.Delete(writePath);
            */

            logger.Log(UnhandledRequests);

            logDump.Properties.EndLogSession();
            UtilityCore.RequestHandler.Unregister(logger); //Logger is required to be registered for logging to work, it needs to be unregistered after use
        }
    }
}
