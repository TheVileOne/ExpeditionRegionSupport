using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    /// <summary>
    /// A class for storing log details until a logger is available to process the request
    /// </summary>
    public class LogRequest
    {
        public LogEvents.LogMessageEventArgs Data;

        public RequestStatus Status { get; private set; }

        public RejectionReason UnhandledReason { get; private set; }

        public LogRequest(LogEvents.LogMessageEventArgs data)
        {
            Data = data;
            Status = RequestStatus.Pending;
        }

        public void Complete()
        {
            Status = RequestStatus.Complete;
        }

        public void Reject(RejectionReason reason)
        {
            Status = RequestStatus.Rejected;
            UnhandledReason = reason;
        }

        public override string ToString()
        {
            return string.Format("[Log Request][{0}] {1}", Data.ID, Data.Message);
        }
    }

    public enum RequestStatus
    {
        Pending,
        Rejected,
        Complete
    }

    public enum RequestProtocol
    {
        HandleWhenPossible,
        DiscardOnFail
    }

    public enum LogAvailability
    {
        Pregame,
        OnModsInIt
    }

    public enum RejectionReason
    {
        None = 0,
        AccessDenied = 1, //LogID is private
        LogDisabled = 2, //LogID is not enabled, Logger is not accepting logs, or LogID is ShowLogs aware and ShowLogs is false
        LogUnavailable = 3, //No logger is available that accepts the LogID, or the logger accepts the LogID, but enforces a build period on the log file that is not yet satisfied
        PregameUnityRequest = 4, //Requested action to the Unity logger before the game is initialized
        ShowLogsNotInitialized = 5 //Requested action to a ShowLogs aware log before ShowLogs is initialized 
    }
}
