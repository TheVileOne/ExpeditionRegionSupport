using LogUtils.Console;
using LogUtils.Enums;
using LogUtils.Events;
using LogUtils.Formatting;
using LogUtils.Policy;
using LogUtils.Requests.Validation;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LogUtils.Requests
{
    /// <summary>
    /// A class for storing log details until a logger is available to process the request
    /// </summary>
    public class LogRequest : ICloneable
    {
        /// <summary>
        /// Rejection codes up to and including this value are not recoverable. A LogRequest that is rejected in this range will not be handled again
        /// </summary>
        public const byte UNABLE_TO_RETRY_RANGE = 5;

        private int managedThreadID = -1;
        private RequestState _state;

        public LogRequestEventArgs Data;

        /// <summary>
        /// The log handler that has taken responsibility for handling the write process for this request
        /// </summary>
        public ILogHandler Host;

        /// <summary>
        /// Request has been handled, and no more attempts to process the request should be made
        /// </summary>
        public bool IsCompleteOrInvalid => Status == RequestStatus.Complete || !CanRetryRequest();

        public bool IsCompleteOrRejected => Status == RequestStatus.Complete || Status == RequestStatus.Rejected;

        /// <summary>
        /// This field is primarily used by <see cref="LogRequestHandler"/> to communicate with unregistered loggers 
        /// </summary>
        public LogRequestEventHandler LogCallback;

        /// <summary>
        /// The log handler that was responsible for submitting the request
        /// </summary>
        public ILogger Sender;

        public RequestStatus Status => _state.Status;

        /// <summary>
        /// Whether this request has once been submitted through the log request system
        /// </summary>
        public bool Submitted;

        /// <summary>
        /// A flag indicating whether the request targets a log file
        /// </summary>
        public bool IsFileRequest => Type != RequestType.Invalid && Type != RequestType.Console && Type != RequestType.Batch;

        public bool ThreadCanWrite => Status == RequestStatus.WritePending && Thread.CurrentThread.ManagedThreadId == managedThreadID;

        public readonly RequestType Type;

        /// <summary>
        /// Indicates that a ConsoleID is the current target for this request (of which there may be multiple targets)
        /// </summary>
        public bool IsTargetingConsole => Data.IsTargetingConsole;

        public RejectionReason UnhandledReason => _state.UnhandledReason;

        public bool WaitingOnOtherRequests
        {
            get
            {
                /*
                 * This returns a state where the current request is pending, but the rejection record tells us there is a rejection
                 * presumably from an earlier request that was rejected with the possibility of retrying the request at a later time.
                 * This logic is not universally a guaranteed truth, but hopefully thread locking and careful management of internal
                 * request processing will ensure that we are never checking a stale HandleRecord
                 */
                return UnhandledReason == RejectionReason.WaitingOnOtherRequests
                    || (Status == RequestStatus.Pending
                    && Data.Properties.HandleRecord.Rejected && CanRetryRequest(Data.Properties.HandleRecord.Reason));
            }
        }

        public static LogRequestStringFormatter Formatter = new LogRequestStringFormatter();

        /// <summary>
        /// Constructs a new LogRequest instance
        /// </summary>
        /// <param name="type">The identifying request category (affects how request is handled)</param>
        /// <param name="data">Data used to construct a log message</param>
        public LogRequest(RequestType type, LogRequestEventArgs data)
        {
            Type = type;
            Data = data;

            if (Type == RequestType.Console)
            {
                Data.IsTargetingConsole = true;
                //Probably not needed
                //Data.ExtraArgs.Add(new ConsoleRequestEventArgs());
            }
        }

        public bool CanRetryRequest()
        {
            if (Type == RequestType.Console)
                return UnhandledReason == RejectionReason.None;

            return CanRetryRequest(UnhandledReason);
        }

        public static bool CanRetryRequest(RejectionReason reason)
        {
            return reason == RejectionReason.None || (byte)reason > UNABLE_TO_RETRY_RANGE;
        }

        internal void OnSubmit()
        {
            //Ensures consistent handling of the request
            ResetStatus();

            Submitted = true;
            LogRequestEvents.OnSubmit?.Invoke(this);
        }

        public void ResetStatus()
        {
            _state.Reset();
        }

        /// <summary>
        /// Reset the console target status back to its value set on construction
        /// </summary>
        public void ResetTarget()
        {
            Data.IsTargetingConsole = Type == RequestType.Console;
        }

        public ConsoleRequestEventArgs SetDataFromWriter(ConsoleLogWriter writer)
        {
            ConsoleRequestEventArgs consoleMessageData = Data.GetConsoleData();

            if (consoleMessageData == null)
                Data.ExtraArgs.Add(consoleMessageData = new ConsoleRequestEventArgs(writer.ID));

            consoleMessageData.TotalMessagesLogged = writer.TotalMessagesLogged;
            consoleMessageData.Writer = writer;
            return consoleMessageData;
        }

        public void TargetConsole()
        {
            Data.IsTargetingConsole = true;
        }

        internal void InheritHandledConsoleTargets(LogRequest targetProvider)
        {
            if (targetProvider == null) return;

            var consoleMessageData = targetProvider.Data.GetConsoleData();

            if (consoleMessageData != null)
                NotifyComplete(consoleMessageData.Handled);
        }

        public void Complete()
        {
            if (Status == RequestStatus.Complete) return;

            _state.Status = RequestStatus.Complete;
            managedThreadID = -1;

            if (Data.ShouldFilter)
                LogFilter.AddFilterEntry(Data.ID, new FilteredStringEntry(Data.Message, Data.FilterDuration));

            NotifyOnChange();
        }

        public void Reject(RejectionReason reason, object context = null)
        {
            if (Status == RequestStatus.Complete)
            {
                UtilityLogger.LogWarning("Completed requests cannot be rejected");
                return;
            }

            if (reason == RejectionReason.None)
            {
                UtilityLogger.LogWarning("Request cannot be rejected without specifying a reason");
                return;
            }

            ConsoleID consoleContext = context as ConsoleID;

            //The main difference between a console context and other types of requests is that a console request context can only be "completed",
            //the rejection reason should not be recorded when a context is provided
            if (consoleContext == null)
            {
                _state.Status = RequestStatus.Rejected;
                managedThreadID = -1;

                if (UnhandledReason != RejectionReason.None)
                    UtilityLogger.Logger.LogDebug("Unhandled reason already exists");

                if (Type != RequestType.Batch && //Batch requests do not have their own properties 
                    reason != RejectionReason.ExceptionAlreadyReported && reason != RejectionReason.FilterMatch) //Temporary conditions should not be recorded
                    Data.Properties.HandleRecord.SetReason(reason);

                if (UnhandledReason != reason)
                {
                    _state.UnhandledReason = reason;
                    NotifyOnChange();
                }
            }
            else
            {
                NotifyComplete(consoleContext);
            }

            bool showLogsActive = RainWorld.ShowLogs || RWInfo.LatestSetupPeriodReached < RWInfo.SHOW_LOGS_ACTIVE_PERIOD;

            if (LogRequestPolicy.ShowRejectionReasons && !UtilityLogger.PerformanceMode && showLogsActive && shouldBeReported(reason))
            {
                UtilityLogger.DebugLog("Log request was rejected REASON: " + reason);
                UtilityLogger.Log("Log request was rejected REASON: " + reason);

                if (context != null)
                    UtilityLogger.Log("CONTEXT: " + context);
            }

            bool shouldBeReported(RejectionReason reason)
            {
                //These conditions can get spammy
                return reason != RejectionReason.WaitingOnOtherRequests && (reason != RejectionReason.LogUnavailable || context is not ConsoleID);
            }
        }

        public void Validate(IRequestValidator validator)
        {
            RejectionReason reason = validator.GetResult(this);

            if (reason != RejectionReason.None)
                Reject(reason);
        }

        public void WriteInProcess()
        {
            if (Status != RequestStatus.Pending) return;

            if (!Submitted)
            {
                UtilityCore.RequestHandler.Submit(this, false);

                if (Status == RequestStatus.Rejected) return;
            }

            _state.Status = RequestStatus.WritePending;
            Interlocked.CompareExchange(ref managedThreadID, Thread.CurrentThread.ManagedThreadId, -1);

            NotifyOnChange();
        }

        /// <summary>
        /// Notify that the specified ConsoleID no longer needs to be processed
        /// </summary>
        public void NotifyComplete(ConsoleID consoleID)
        {
            var consoleRequestData = Data.GetConsoleData();

            //Console data may not exist if the ConsoleIDs are sourced from the LogID instead of a Logger
            if (consoleRequestData == null)
                Data.ExtraArgs.Add(consoleRequestData = new ConsoleRequestEventArgs());

            consoleRequestData.Pending.Remove(consoleID);
            consoleRequestData.Handled.Add(consoleID);
        }

        /// <summary>
        /// Notify that a collection of ConsoleIDs no longer needs to be processed
        /// </summary>
        public void NotifyComplete(IEnumerable<ConsoleID> consoleIDs)
        {
            var consoleRequestData = Data.GetConsoleData();

            //Console data may not exist if the ConsoleIDs are sourced from the LogID instead of a Logger
            if (consoleRequestData == null)
                Data.ExtraArgs.Add(consoleRequestData = new ConsoleRequestEventArgs());

            foreach (ConsoleID consoleID in consoleIDs)
            {
                consoleRequestData.Pending.Remove(consoleID);
                consoleRequestData.Handled.Add(consoleID);
            }
        }

        /// <summary>
        /// Raises an event when the LogRequest status, or the rejection reason changes. Currently does not raise when ResetStatus is invoked
        /// </summary>
        protected void NotifyOnChange()
        {
            LogRequestEvents.OnStatusChange?.Invoke(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(FormatEnums.FormatVerbosity.Standard);
        }

        public string ToString(FormatEnums.FormatVerbosity verbosity)
        {
            return ToString(Formatter, verbosity);
        }

        public string ToString(LogRequestStringFormatter formatter, FormatEnums.FormatVerbosity verbosity)
        {
            FormattableString stringFormatter;
            switch (verbosity)
            {
                case FormatEnums.FormatVerbosity.Compact:
                    stringFormatter = formatter.GetFormat(Data.Message);
                    break;
                case FormatEnums.FormatVerbosity.Standard:
                    stringFormatter = formatter.GetFormat(Data.ID, Data.Message);
                    break;
                case FormatEnums.FormatVerbosity.Verbose:
                    stringFormatter = formatter.GetFormat(Data.ID, Status, UnhandledReason, Data.Message);
                    break;
                default:
                    goto case FormatEnums.FormatVerbosity.Standard;
            }
            return stringFormatter.ToString();
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// A class for constructor helper methods, and method signatures for creating LogRequests
        /// </summary>
        public static class Factory
        {
            /// <summary>
            /// A delegate signature for creating a LogRequest instance
            /// </summary>
            /// <param name="requestType">The type of LogRequest to make</param>
            /// <param name="target">The log destination identifier</param>
            /// <param name="category">The logging context to use</param>
            /// <param name="messageObj">The object representation of the logged message</param>
            /// <param name="shouldFilter">Whether a filter should be applied when message is handled</param>
            public delegate LogRequest Callback(RequestType requestType, ILogTarget target, LogCategory category, object messageObj, bool shouldFilter);

            /// <summary>
            /// Constructs a new LogRequest instance
            /// </summary>
            /// <inheritdoc cref="Callback" section="param"/>
            public static LogRequest Create(RequestType requestType, ILogTarget target, LogCategory category, object messageObj, bool shouldFilter)
            {
                if (requestType == RequestType.Invalid)
                {
                    UtilityLogger.LogWarning("Log request could not be processed");
                    return null;
                }

                LogRequestEventArgs requestData;

                if (requestType == RequestType.Console)
                {
                    ConsoleID consoleTarget = (ConsoleID)target;
                    requestData = new LogRequestEventArgs(consoleTarget, messageObj, category);
                }
                else
                {
                    LogID fileTarget = (LogID)target;
                    requestData = new LogRequestEventArgs(fileTarget, messageObj, category);
                }

                if (shouldFilter)
                {
                    requestData.ShouldFilter = true;
                    requestData.FilterDuration = FilterDuration.OnClose;
                }
                return new LogRequest(requestType, requestData);
            }

            /// <summary>
            /// Creates a callback that will create a new LogRequest with the provided event data when invoked
            /// </summary>
            public static Callback CreateDataCallback(EventArgs extraData)
            {
                LogRequest addDataToRequest(RequestType type, ILogTarget target, LogCategory category, object messageObj, bool shouldFilter)
                {
                    LogRequest request = Factory.Create(type, target, category, messageObj, shouldFilter);

                    if (request != null)
                        request.Data.ExtraArgs.Add(extraData);
                    return request;
                }
                return addDataToRequest;
            }
        }
    }

    internal struct RequestState
    {
        public RequestStatus Status;
        public RejectionReason UnhandledReason;

        public void Reset()
        {
            Status = RequestStatus.Pending;
            UnhandledReason = RejectionReason.None;
        }
    }

    public enum RequestStatus
    {
        Pending,
        WritePending,
        Rejected,
        Complete
    }

    public enum RequestType
    {
        Invalid = -1,
        Local,
        Remote,
        Game,
        Console,
        Batch
    }

    /// <summary>
    /// Describes the reason why a LogRequest could not be handled
    /// </summary>
    public enum RejectionReason : byte
    {
        /// <summary>
        /// Default state
        /// </summary>
        None = 0,
        /// <summary>
        /// Logger available to handle the log request is private
        /// </summary>
        AccessDenied = 1,
        /// <summary>
        /// LogID is not enabled, Logger is not accepting logs, or LogID is ShowLogs aware and ShowLogs is false
        /// </summary>
        LogDisabled = 2,
        /// <summary>
        /// Attempt to log failed due to an error
        /// </summary>
        FailedToWrite = 3,
        /// <summary>
        /// Attempt to log the same Exception two, or more times to the same log file
        /// </summary>
        ExceptionAlreadyReported = 4,
        /// <summary>
        /// Attempt to log a string that is stored in FilteredStrings
        /// </summary>
        FilterMatch = 5,
        /// <summary>
        /// The path information for the LogID accepted by the logger does not match the path information of the LogID in the request
        /// </summary>
        PathMismatch = 6,
        /// <summary>
        /// A log request was sent to a logger that cannot handle the request
        /// </summary>
        NotAllowedToHandle = 7,
        /// <summary>
        /// Attempt to handle log request was prevented, because an earlier request could not be handled
        /// </summary>
        WaitingOnOtherRequests = 8,
        /// <summary>
        /// No logger is available that accepts the LogID, or the logger accepts the LogID, but enforces a build period on the log file that is not yet satisfied
        /// </summary>
        LogUnavailable = 9,
        /// <summary>
        /// Attempt to log to a ShowLogs aware log before ShowLogs is initialized
        /// </summary>
        ShowLogsNotInitialized = 10
    }
}
