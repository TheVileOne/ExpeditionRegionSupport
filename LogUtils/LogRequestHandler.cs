﻿using System;
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

            ProcessRequests(logFile);

            //Ensures consistent handling of the request
            request.ResetStatus();

            if (logFile.Properties.HandleRecord.Rejected)
            {
                request.Reject(logFile.Properties.HandleRecord.Reason);

                if (request.CanRetryRequest()) //As long as the rejection is recoverable, treat the request as pending
                    PendingRequest = request;
                else
                    CurrentRequest = request; //Otherwise store it in CurrentRequest to avoid potential null references

                return request;
            }

            PendingRequest = request;

            if (handleSubmission)
                ProcessRequest(request);
            return request;
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

            if (request.Data.Properties.HandleRecord.Rejected)
            {
                TryResolveRecord(logFile);

                if (request.Data.Properties.HandleRecord.Rejected)
                    RejectRequests(logFile, request.Data.Properties.HandleRecord.Reason);
                else
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

            if (request.Status == RequestStatus.Complete || !request.CanRetryRequest())
                UnhandledRequests.Remove(request);
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

                if (requestCanBeHandled = target.Status == RequestStatus.Pending || (target.Status == RequestStatus.Rejected && target.CanRetryRequest()))
                {
                    //Find a logger that can be used for this LogID
                    if (selectedLogger == null || targetID != lastTargetID)
                        selectedLogger = findCompatibleLogger(targetID, doPathCheck: false);

                    if (selectedLogger != null)
                    {
                        //Try to handle the log request, and recheck the status
                        RejectionReason result = selectedLogger.HandleRequest(target, true);

                        if (requestCanBeHandled = target.Status == RequestStatus.Pending || (target.Status == RequestStatus.Rejected && target.CanRetryRequest()))
                        {
                            if (result == RejectionReason.PathMismatch)
                            {
                                //Attempt to find a logger that accepts the target LogID with this exact path
                                selectedLogger = findCompatibleLogger(targetID, doPathCheck: true);

                                if (selectedLogger != null)
                                {
                                    selectedLogger.HandleRequest(target, true);
                                    requestCanBeHandled = target.Status == RequestStatus.Complete || !target.CanRetryRequest();
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
        /// Remove requests that have been handled since being processed
        /// </summary>
        internal void DiscardHandledRequests(IEnumerable<LogRequest> requests)
        {
            //Check the status of all processed requests to remove the handled ones
            foreach (LogRequest request in requests.Where(r => r.Status == RequestStatus.Complete || !r.CanRetryRequest()))
                UnhandledRequests.Remove(request);
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
