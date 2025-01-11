using LogUtils.Enums;
using LogUtils.Events;
using System.Threading;

namespace LogUtils
{
    /// <summary>
    /// A class for storing log details until a logger is available to process the request
    /// </summary>
    public class LogRequest
    {
        public const byte NO_RETRY_MAXIMUM = 5;

        private int managedThreadID = -1;

        public event LogRequestEventHandler StatusChanged;
        public LogMessageEventArgs Data;

        /// <summary>
        /// The logger instance that has taken responsibility for handling the write process for this request
        /// </summary>
        public Logger Host;

        /// <summary>
        /// Request has been handled, and no more attempts to process the request should be made
        /// </summary>
        public bool IsCompleteOrInvalid => Status == RequestStatus.Complete || !CanRetryRequest();

        public RequestStatus Status { get; private set; }

        /// <summary>
        /// Whether this request has once been submitted through the log request system
        /// </summary>
        public bool Submitted;

        public bool ThreadCanWrite => Status == RequestStatus.WritePending && Thread.CurrentThread.ManagedThreadId == managedThreadID; 

        public readonly RequestType Type;

        public RejectionReason UnhandledReason { get; private set; }

        public static string StringFormat = "[Log Request][{0}] {1}";

        public LogRequest(RequestType type, LogMessageEventArgs data)
        {
            Data = data;
            Status = RequestStatus.Pending;
            Type = type;
        }

        public bool CanRetryRequest()
        {
            byte reasonValue = (byte)UnhandledReason;

            return reasonValue > NO_RETRY_MAXIMUM || reasonValue == 0;
        }

        public void ResetStatus()
        {
            Status = RequestStatus.Pending;
            UnhandledReason = RejectionReason.None;
        }

        public void Complete()
        {
            if (Status == RequestStatus.Complete) return;

            Status = RequestStatus.Complete;
            managedThreadID = -1;

            if (Data.ShouldFilter)
                LogFilter.AddFilterEntry(Data.ID, new FilteredStringEntry(Data.Message, Data.FilterDuration));

            StatusChanged?.Invoke(this);
        }

        public void Reject(RejectionReason reason)
        {
            try
            {
                if (Status == RequestStatus.Complete)
                {
                    UtilityLogger.LogWarning("Completed requests cannot be rejected");
                    return;
                }

                Status = RequestStatus.Rejected;
            }
            finally
            {
                managedThreadID = -1;
            }

            UtilityLogger.Log("Log request was rejected REASON: " + reason);

            if (reason != RejectionReason.None
             && reason != RejectionReason.ExceptionAlreadyReported
             && reason != RejectionReason.FilterMatch) //Temporary conditions should not be recorded
                Data.Properties.HandleRecord.Reason = reason;

            if (UnhandledReason != reason)
            {
                //A hacky attempt to make it possible to notify of path mismatches without overwriting an already existing reason 
                if (reason != RejectionReason.PathMismatch || UnhandledReason == RejectionReason.None)
                    UnhandledReason = reason;

                StatusChanged?.Invoke(this);
            }
        }

        public void WriteInProcess()
        {
            if (Status != RequestStatus.Pending) return;

            if (!Submitted)
            {
                UtilityCore.RequestHandler.Submit(this, false);

                if (Status == RequestStatus.Rejected) return;
            }

            Status = RequestStatus.WritePending;
            Interlocked.CompareExchange(ref managedThreadID, Thread.CurrentThread.ManagedThreadId, -1);
        }

        public override string ToString()
        {
            return string.Format(StringFormat, Data.ID, Data.Message);
        }

        public delegate void LogRequestEventHandler(LogRequest request);
    }

    public enum RequestStatus
    {
        Pending,
        WritePending,
        Rejected,
        Complete
    }

    public enum RequestType : byte
    {
        Local,
        Remote,
        Game
    }

    public enum RejectionReason : byte
    {
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
        /// No logger is available that accepts the LogID, or the logger accepts the LogID, but enforces a build period on the log file that is not yet satisfied
        /// </summary>
        LogUnavailable = 8,
        /// <summary>
        /// Attempt to log to a ShowLogs aware log before ShowLogs is initialized
        /// </summary>
        ShowLogsNotInitialized = 9
    }
}
