﻿using System;
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
        public const byte NO_RETRY_MAXIMUM = 3;

        public LogEvents.LogMessageEventArgs Data;

        public RequestStatus Status { get; private set; }

        public RejectionReason UnhandledReason { get; private set; }

        public LogRequest(LogEvents.LogMessageEventArgs data)
        {
            Data = data;
            Status = RequestStatus.Pending;
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
        }

        public void Reject(RejectionReason reason)
        {
            if (Status == RequestStatus.Complete)
            {
                UtilityCore.BaseLogger.LogInfo("Completed requests cannot be rejected");
                return;
            }

            if (UnhandledReason == reason) return;

            Status = RequestStatus.Rejected;

            //A hacky attempt to make it possible to notify of path mismatches without overwriting an already existing reason 
            if (reason != RejectionReason.PathMismatch || UnhandledReason == RejectionReason.None)
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

    public enum RejectionReason : byte
    {
        None = 0,
        AccessDenied = 1, //LogID is private
        LogDisabled = 2, //LogID is not enabled, Logger is not accepting logs, or LogID is ShowLogs aware and ShowLogs is false
        FailedToWrite = 3, //Attempt to log failed due to an error
        PathMismatch = 4, //The path information for the LogID accepted by the logger does not match the path information of the LogID in the request
        LogUnavailable = 5, //No logger is available that accepts the LogID, or the logger accepts the LogID, but enforces a build period on the log file that is not yet satisfied
        PregameUnityRequest = 6, //Requested action to the Unity logger before the game is initialized
        ShowLogsNotInitialized = 7 //Requested action to a ShowLogs aware log before ShowLogs is initialized 
    }
}
