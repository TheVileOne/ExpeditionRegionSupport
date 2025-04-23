using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Helpers.Extensions;
using LogUtils.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace LogUtils.Requests
{
    public class LogRequestHandler : UtilityComponent
    {
        /// <summary>
        /// This lock object marshals control over submission of LogRequests, and processing requests stored in UnhandledRequests. When there is a need to
        /// process LogRequests directly from UnhandledRequests, it is recommended to use this lock object to achieve thread safety
        /// </summary>
        public Lock RequestProcessLock = new Lock();

        public override string Tag => UtilityConsts.ComponentTags.REQUEST_DATA;

        private readonly WeakReferenceCollection<ILogHandler> availableLoggers = new WeakReferenceCollection<ILogHandler>();

        /// <summary>
        /// A list of loggers available to handle local or remote log requests
        /// </summary>
        public IEnumerable<ILogHandler> AvailableLoggers => availableLoggers;

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

                using (BeginCriticalSection())
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
                using (BeginCriticalSection())
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

        /// <summary>
        /// Acquires the lock necessary for entering a critical state pertaining to LogRequest handling
        /// </summary>
        /// <returns>A disposable scope object purposed for leaving a critical state</returns>
        public Lock.Scope BeginCriticalSection()
        {
            return RequestProcessLock.Acquire();
        }

        /// <summary>
        /// Releases the lock used to enter a critical state
        /// </summary>
        public void EndCriticalSection()
        {
            RequestProcessLock.Release();
        }

        public LogRequest[] GetRequests(LogID logFile)
        {
            return UnhandledRequests.Where(req => req.Data.ID.Equals(logFile, doPathCheck: true)).ToArray();
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
            using (BeginCriticalSection())
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
        /// Registers a logger (required to use LogRequest system for local and remote LogRequests)
        /// </summary>
        public void Register(ILogHandler logger)
        {
            if (!logger.AllowRegistration)
                throw new InvalidOperationException("Log handler does not allow registration");

            UtilityEvents.OnRegistrationChanged?.Invoke(logger, new RegistrationChangedEventArgs(status: true));
            availableLoggers.Add(logger);
            ProcessRequests(logger);
        }

        /// <summary>
        /// Unregisters a logger
        /// </summary>
        public void Unregister(ILogHandler logger)
        {
            if (!logger.AllowRegistration)
                throw new InvalidOperationException("Log handler does not allow registration");

            UtilityEvents.OnRegistrationChanged?.Invoke(logger, new RegistrationChangedEventArgs(status: false));
            availableLoggers.Remove(logger);
        }

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
            using (BeginCriticalSection())
            {
                //Ensure that we do not handle a stale record
                logFile.Properties.HandleRecord.Reset();

                LogRequest[] requests = GetRequests(logFile);
                ILogHandler selectedLogger = null;

                //Evaluate all requests waiting to be handled for this log file
                foreach (LogRequest request in requests)
                {
                    bool shouldHandle = PrepareRequestNoReset(request);

                    if (!shouldHandle)
                    {
                        RequestMayBeCompleteOrInvalid(request);
                        continue;
                    }

                    if (selectedLogger == null || !selectedLogger.CanHandle(request))
                        selectLogger(logFile, request.Type);

                    HandleRequest(request, selectedLogger);
                }

                void selectLogger(LogID logFile, RequestType requestType)
                {
                    if (logFile.IsGameControlled)
                    {
                        selectedLogger = GameLogger;
                        return;
                    }

                    availableLoggers.FindCompatible(logFile, out ILogHandler localLogger, out ILogHandler remoteLogger);
                    selectedLogger = requestType == RequestType.Local ? localLogger : remoteLogger;
                }
            }
        }

        public void ProcessRequests(ILogHandler logger)
        {
            using (BeginCriticalSection())
            {
                foreach (LogID logFile in logger.GetAccessibleTargets())
                {
                    //Ensure that we do not handle a stale record
                    logFile.Properties.HandleRecord.Reset();

                    LogRequest[] requests = GetRequests(logFile);

                    foreach (LogRequest request in requests)
                    {
                        PrepareRequestNoReset(request);
                        HandleRequest(request, logger);
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to handle unhandled requests 
        /// </summary>
        public void ProcessRequests()
        {
            using (BeginCriticalSection())
            {
                int requestCount = UnhandledRequests.Count;

                if (requestCount > 0)
                {
                    UtilityLogger.DebugLog($"Processing {requestCount} request" + (requestCount > 1 ? "s" : ""));

                    foreach (var requestBatch in UnhandledRequests.GroupRequests())
                        processBatch(requestBatch);
                }
            }
        }

        private void processBatch(IGrouping<LogID, LogRequest> requests)
        {
            LogID requestID = requests.Key;
            MessageBuffer writeBuffer = requestID.Properties.WriteBuffer;

            writeBuffer.SetState(true, BufferContext.RequestConsolidation);

            //Hold onto a valid write handler - we will need one to schedule a flush of the message buffer after batch process is complete
            ILogWriter bufferWriter = null;
            LoggerSelection selectedLogger = default;
            int requestsProcessed = 0;
            long processStartTime = Stopwatch.GetTimestamp();

            try
            {
                foreach (LogRequest request in requests)
                {
                    bool shouldHandle = PrepareRequest(request, processStartTime);

                    if (!shouldHandle)
                    {
                        UtilityLogger.DebugLog("Request skipped");
                        RequestMayBeCompleteOrInvalid(request);
                        continue;
                    }

                    requestsProcessed++;
                    UtilityLogger.DebugLog($"Request # [{requestsProcessed}] {request}");

                    if (!selectedLogger.AppliesTo(request.Type))
                    {
                        //This will get overwritten during the selection process, but we still need it
                        RequestType lastAccessTarget = selectedLogger.AccessTarget;

                        selectLogger(request.Type);

                        //Assign writer for handling the message buffer
                        var selectedWriter = selectedLogger.GetWriter(requestID);

                        //Selection code presumes that all local, and remote log writers are qualified to handle batched messages and
                        //avoids replacing a compatible local/remote writer with a null entry
                        if (bufferWriter == null || selectedWriter != null || lastAccessTarget == RequestType.Game)
                            bufferWriter = selectedWriter;
                    }

                    HandleRequest(request, selectedLogger.Handler);
                }
            }
            finally
            {
                if (writeBuffer.SetState(false, BufferContext.RequestConsolidation))
                {
                    if (bufferWriter != null)
                        bufferWriter.WriteFromBuffer(requestID);
                    else
                        UtilityLogger.LogWarning("No writer was available to process the buffer");
                }
            }

            void selectLogger(RequestType requestType)
            {
                if (requestID.IsGameControlled)
                {
                    selectedLogger = new LoggerSelection(GameLogger, RequestType.Game);
                }
                else
                {
                    var logger = availableLoggers.FindCompatible(requestID, requestType, doPathCheck: false);
                    selectedLogger = new LoggerSelection(logger, requestType);
                }
            }
        }

        internal void ProcessRequest(LogRequest request)
        {
            bool shouldHandle = PrepareRequest(request);

            if (!shouldHandle) return;

            LogID logFile = request.Data.ID;

            //Beyond this point, we can assume that there are no preexisting unhandled requests for this log file
            ILogHandler selectedLogger = !logFile.IsGameControlled
                ? availableLoggers.FindCompatible(logFile, request.Type, doPathCheck: true) : GameLogger;

            HandleRequest(request, selectedLogger);
        }

        internal void HandleRequest(LogRequest request, ILogHandler logger)
        {
            CurrentRequest = request;

            try
            {
                if (logger == null)
                {
                    request.Reject(RejectionReason.LogUnavailable);
                    return;
                }

                logger.HandleRequest(request, skipAccessValidation: true);
            }
            finally
            {
                RequestMayBeCompleteOrInvalid(request);
            }
        }

        public void RejectRequests(LogID logFile, RejectionReason reason)
        {
            RejectRequests(GetRequests(logFile), reason);
        }

        public void RejectRequests(LogRequest[] requests, RejectionReason reason)
        {
            UtilityLogger.Log("Rejecting requests in bulk for reason: " + reason);

            foreach (LogRequest request in requests)
            {
                request.Reject(reason);
                RequestMayBeCompleteOrInvalid(request);
            }
        }

        /// <summary>
        /// Clean up process for requests that need to be removed from the request handling system
        /// </summary>
        public void RequestMayBeCompleteOrInvalid(LogRequest request)
        {
            if (request == null) return;

            DiscardStatus status = shouldDiscard();

            if (status != DiscardStatus.Keep)
            {
                if (status == DiscardStatus.Hard)
                {
                    if (UnhandledRequests.Remove(request))
                        request.Submitted = false;
                }

                if (CurrentRequest == request)
                    CurrentRequest = null;
            }

            DiscardStatus shouldDiscard()
            {
                if (request.IsCompleteOrInvalid)
                    return DiscardStatus.Hard;

                if (request.Status == RequestStatus.Rejected)
                    return DiscardStatus.Soft;
                return DiscardStatus.Keep;
            }
        }

        private enum DiscardStatus
        {
            /// <summary>
            /// Don't discard
            /// </summary>
            Keep,
            /// <summary>
            /// Remove from CurrentRequest
            /// </summary>
            Soft,
            /// <summary>
            /// Remove from UnhandledRequests and CurrentRequest
            /// </summary>
            Hard
        }

        /// <summary>
        /// Checks all requests stored in UnhandledRequests, removing any that have been completed, or are no longer valid
        /// </summary>
        public void DiscardHandledRequests()
        {
            //Check the status of all processed requests to remove the handled ones
            foreach (LogRequest request in UnhandledRequests)
                RequestMayBeCompleteOrInvalid(request);

            CheckForHandledRequests = false;
        }

        /// <summary>
        /// Ensures that CurrentRequest represents a pending unrejected request
        /// </summary>
        public void SanitizeCurrentRequest()
        {
            LogRequest requestBeforeProcessing = CurrentRequest;

            RequestMayBeCompleteOrInvalid(CurrentRequest);

            if (requestBeforeProcessing != CurrentRequest)
                UtilityLogger.DebugLog("Current request sanitized");
        }

        public void Update()
        {
            if (HandleOnNextAvailableFrame.Any())
            {
                using (BeginCriticalSection())
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

        private readonly struct LoggerSelection(ILogHandler handler, RequestType target) : ILogWriterProvider
        {
            public readonly ILogHandler Handler = handler;

            /// <summary>
            /// The access specification used to assign the handler
            /// </summary>
            public readonly RequestType AccessTarget = target;

            public bool AppliesTo(RequestType compareTarget)
            {
                //Handler must be defined, and have an AccessTarget consistent with the compare value
                return Handler != null && (AccessTarget == compareTarget || (AccessTarget == RequestType.Remote && compareTarget == RequestType.Local));
            }

            public ILogWriter GetWriter()
            {
                var provider = Handler as ILogWriterProvider;
                return provider?.GetWriter();
            }

            public ILogWriter GetWriter(LogID logFile)
            {
                var provider = Handler as ILogWriterProvider;
                return provider?.GetWriter(logFile);
            }
        }
    }
}
