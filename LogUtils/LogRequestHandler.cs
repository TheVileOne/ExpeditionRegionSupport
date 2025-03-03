using LogUtils.Enums;
using LogUtils.Helpers.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;

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

        private readonly WeakReferenceCollection<Logger> availableLoggers = new WeakReferenceCollection<Logger>();

        /// <summary>
        /// A list of loggers available to handle remote log requests
        /// </summary>
        public IEnumerable<Logger> AvailableLoggers => availableLoggers;

        public GameLogger GameLogger = new GameLogger();

        /// <summary>
        /// Contains LogRequest objects that are submitted and waiting to be handled
        /// </summary>
        public LinkedLogRequestCollection UnhandledRequests;

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
            protected internal set
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
            UnhandledRequests = new LinkedLogRequestCollection(20);
        }

        public ILinkedListEnumerable<LogRequest> GetRequests(LogID logFile)
        {
            return UnhandledRequests.Where(req => req.Data.ID.Equals(logFile, doPathCheck: true));
        }

        /// <summary>
        /// Submit a request - Treated as an active pending log request unless the request itself did not qualify for submission. A request must meet the following conditions: 
        /// <br>I. No rejection reasons were found during initial processing of the request</br>
        /// <br>II. Under the situation that there is a reason to reject, that reason is not severe enough to prevent future attempts to process the request</br>
        /// <br>Submitted request may be retrieved through CurrentRequest under the above conditions, or from the return value</br>
        /// </summary>
        /// <param name="request">The request to be processed</param>
        /// <param name="handleSubmission">Whether a log attempt should be made on the request</param>
        /// <returns>This method returns the same request given to it under any condition. The return value is more reliable than checking CurrentRequest, which may be null</returns>
        public LogRequest Submit(LogRequest request, bool handleSubmission)
        {
            lock (RequestProcessLock)
            {
                if (request.Submitted)
                {
                    UtilityLogger.LogWarning("Submitted request has already been submitted at least once");
                    return request;
                }

                request.OnSubmit();

                LogID logFile = request.Data.ID;

                //Waiting requests must be handled before the submitted request
                ProcessRequests(logFile);

                //There are requests that could not be handled before the pending request - we cannot handle the pending request right away
                if (request.WaitingOnOtherRequests)
                {
                    request.Reject(RejectionReason.WaitingOnOtherRequests);
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
            ProcessRequests(logger);
        }

        /// <summary>
        /// Unregisters a logger for remote logging
        /// </summary>
        public void Unregister(Logger logger)
        {
            availableLoggers.Remove(logger);
        }

        #region Find methods
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

        private ILoggerBase findCompatibleLoggerBase(LogID logFile, RequestType requestType)
        {
            if (logFile.IsGameControlled)
                return GameLogger;

            findCompatibleLoggers(logFile, out Logger localLogger, out Logger remoteLogger);
            return requestType == RequestType.Local ? localLogger : remoteLogger;
        }
        #endregion

        protected bool PrepareRequest(LogRequest request, long processTimestamp = -1)
        {
            if (request == null)
            {
                UtilityLogger.LogWarning("Processed a null log request... aborting operation");
                return false;
            }

            //Before a request can be handled properly, we need to treat it as if it is an unprocessed request
            request.ResetStatus();

            //The HandleRecord needs to conditionally be reset here for WaitingOnOtherRequests to produce an accurate result
            if (processTimestamp < 0 || request.Data.Properties.HandleRecord.LastUpdated < processTimestamp)
                request.Data.Properties.HandleRecord.Reset();

            if (request.WaitingOnOtherRequests)
            {
                request.Reject(RejectionReason.WaitingOnOtherRequests);
                return false;
            }
            return true;
        }

        protected bool PrepareRequestNoReset(LogRequest request)
        {
            if (request == null)
            {
                UtilityLogger.LogWarning("Processed a null log request... aborting operation");
                return false;
            }

            //Before a request can be handled properly, we need to treat it as if it is an unprocessed request
            request.ResetStatus();

            if (request.WaitingOnOtherRequests)
            {
                request.Reject(RejectionReason.WaitingOnOtherRequests);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to handle all unhandled log requests belonging to a single LogID in the order they were submitted
        /// </summary>
        public void ProcessRequests(LogID logFile)
        {
            lock (RequestProcessLock)
            {
                //Ensure that we do not handle a stale record
                logFile.Properties.HandleRecord.Reset();

                ILinkedListEnumerable<LogRequest> requests = GetRequests(logFile);
                ILoggerBase selectedLogger = null;

                //Evaluate all requests waiting to be handled for this log file
                foreach (LogRequest request in requests)
                {
                    bool shouldHandle = PrepareRequestNoReset(request);

                    if (!shouldHandle) continue;

                    if (selectedLogger == null || !selectedLogger.CanHandle(request))
                    {
                        //Select a logger to handle the request
                        selectedLogger = findCompatibleLoggerBase(logFile, request.Type);
                    }

                    HandleRequest(request, selectedLogger);
                }

                DiscardHandledRequests(requests);
            }
        }

        public void ProcessRequests(Logger logger)
        {
            lock (RequestProcessLock)
            {
                foreach (LogID logFile in logger.GetTargetsForHandler())
                {
                    //Ensure that we do not handle a stale record
                    logFile.Properties.HandleRecord.Reset();

                    ILinkedListEnumerable<LogRequest> requests = GetRequests(logFile);

                    LogID loggerID = null;
                    foreach (LogRequest request in requests)
                    {
                        PrepareRequestNoReset(request);
                        logger.HandleRequest(request, ref loggerID);
                    }

                    DiscardHandledRequests(requests);
                }
            }
        }

        /// <summary>
        /// Attempts to handle unhandled requests 
        /// </summary>
        public void ProcessRequests()
        {
            LogID requestID = null,
                  lastRequestID;
            ILoggerBase selectedLogger = null;
            bool verifyRemoteAccess = false;

            int requestsProcessed = 0;
            long processStartTime = Stopwatch.GetTimestamp();

            lock (RequestProcessLock)
            {
                var requests = UnhandledRequests.GetRequestsSorted();

                if (requests.Length == 0) return;

                UtilityLogger.DebugLog($"Processing {requests.Length} request" + (requests.Length > 1 ? "s" : ""));

                foreach (LogRequest request in requests)
                {
                    bool shouldHandle = PrepareRequest(request, processStartTime);

                    if (!shouldHandle)
                    {
                        UtilityLogger.DebugLog("Request skipped");
                        continue;
                    }

                    if (request.IsCompleteOrInvalid)
                    {
                        //Probably an indication some mischief has happened
                        UtilityLogger.DebugLog("Request skipped - Already complete or rejected");
                        continue;
                    }

                    lastRequestID = requestID;
                    requestID = request.Data.ID;

                    requestsProcessed++;
                    UtilityLogger.DebugLog($"Request # [{requestsProcessed}] {request}");

                    CurrentRequest = request;

                    //Find a logger that can be used for this LogID
                    if (requestID.IsGameControlled)
                        selectedLogger = GameLogger;
                    else
                    {
                        if (selectedLogger == null || requestID != lastRequestID || (verifyRemoteAccess && request.Type == RequestType.Remote && !((Logger)selectedLogger).AllowRemoteLogging))
                        {
                            verifyRemoteAccess = request.Type == RequestType.Local; //Make the system aware that the logger expects local requests
                            selectedLogger = findCompatibleLogger(requestID, request.Type, doPathCheck: false);
                        }

                        if (selectedLogger != null)
                        {
                            //Try to handle the log request, and recheck the status
                            RejectionReason result = selectedLogger.HandleRequest(request, skipAccessValidation: true);

                            if (request.IsCompleteOrInvalid) continue;

                            if (result == RejectionReason.PathMismatch)
                            {
                                //Attempt to find a logger that accepts the target LogID with this exact path
                                selectedLogger = findCompatibleLogger(requestID, request.Type, doPathCheck: true);
                            }
                        }
                    }

                    HandleRequest(request, selectedLogger);
                }

                DiscardHandledRequests();
            }
        }

        internal void ProcessRequest(LogRequest request)
        {
            bool shouldHandle = PrepareRequest(request);

            if (!shouldHandle) return;

            LogID logFile = request.Data.ID;

            //Beyond this point, we can assume that there are no preexisting unhandled requests for this log file
            ILoggerBase selectedLogger = !logFile.IsGameControlled
                ? findCompatibleLogger(logFile, request.Type, doPathCheck: true) : GameLogger;

            HandleRequest(request, selectedLogger);
            RequestMayBeCompleteOrInvalid(request);
        }

        internal void HandleRequest(LogRequest request, ILoggerBase logger)
        {
            if (logger == null)
            {
                request.Reject(RejectionReason.LogUnavailable);
                return;
            }

            logger.HandleRequest(request, skipAccessValidation: true);
        }

        public void RejectRequests(LogID logFile, RejectionReason reason)
        {
            RejectRequests(GetRequests(logFile), reason);
        }

        public void RejectRequests(IEnumerable<LogRequest> requests, RejectionReason reason)
        {
            UtilityLogger.Log(LogCategory.Debug, "Rejecting requests in bulk for reason: " + reason);

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
    }
}
