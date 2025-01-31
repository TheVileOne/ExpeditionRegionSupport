using LogUtils.Enums;
using System;

namespace LogUtils
{
    /// <summary>
    /// Provides format options for handling internally supported LogRequest fields as arguments
    /// </summary>
    public class LogRequestStringFormatter
    {
        public virtual FormattableString GetFormat(string message)
        {
            return $"[Log Request] {message}";
        }

        public virtual FormattableString GetFormat(LogID requestID, string message)
        {
            return $"[Log Request][{requestID}] {message}";
        }

        public virtual FormattableString GetFormat(LogID requestID, RequestStatus status, string message)
        {
            return $"[Log Request][{requestID}][Status {status}] {message}";
        }

        public virtual FormattableString GetFormat(LogID requestID, RequestStatus status, RejectionReason reason, string message)
        {
            //This field is specific to the Rejected status and should be None for all other statuses
            if (reason == RejectionReason.None)
                return GetFormat(requestID, status, message);
            return $"[Log Request][{requestID}][Status {status} Reason {reason}] {message}";
        }
    }
}
