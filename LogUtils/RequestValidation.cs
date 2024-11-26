using LogUtils.Enums;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    public static class RequestValidation
    {
        public static RejectionReason CheckAccessPeriod(LogID logFile)
        {
            //Check this first - it is useful to reject as ShowLogsNotInitialized before rejecting as LogUnavailable
            if (logFile.Properties.ShowLogsAware && !RainWorld.ShowLogs)
            {
                if (RWInfo.LatestSetupPeriodReached < RWInfo.SHOW_LOGS_ACTIVE_PERIOD)
                    return RejectionReason.ShowLogsNotInitialized;
                else
                    return RejectionReason.LogDisabled;
            }

            if (!logFile.Properties.CanBeAccessed)
                return RejectionReason.LogUnavailable;

            return RejectionReason.None;
        }

        public static void ValidateAccessPeriod(LogRequest request)
        {
            LogID logFile = request.Data.ID;

            //Check this first - it is useful to reject as ShowLogsNotInitialized before rejecting as LogUnavailable
            if (logFile.Properties.ShowLogsAware && !RainWorld.ShowLogs)
            {
                if (RWInfo.LatestSetupPeriodReached < RWInfo.SHOW_LOGS_ACTIVE_PERIOD)
                    request.Reject(RejectionReason.ShowLogsNotInitialized);
                else
                    request.Reject(RejectionReason.LogDisabled);
                return;
            }

            if (!logFile.Properties.CanBeAccessed)
                request.Reject(RejectionReason.LogUnavailable);
        }
    }
}
