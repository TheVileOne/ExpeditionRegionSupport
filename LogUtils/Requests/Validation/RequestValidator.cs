using LogUtils.Enums;

namespace LogUtils.Requests.Validation
{
    public class RequestValidator : IRequestValidator
    {
        public ILogHandler Handler;

        public RequestValidator(ILogHandler handler)
        {
            Handler = handler;
        }

        public RejectionReason GetResult(LogRequest request)
        {
            LogID targetID = Handler.FindEquivalentTarget(request.Data.ID);

            //There are no suitable targets
            if (targetID == null)
            {
                if (Handler.AvailableTargets.NearestEquivalent(request.Data.ID) != null)
                    return RejectionReason.PathMismatch;

                return RejectionReason.NotAllowedToHandle;
            }

            //There are other handler specific reasons to not handle this request
            if (!Handler.CanHandle(request))
                return RejectionReason.NotAllowedToHandle;

            if (!Handler.AllowLogging || !targetID.IsEnabled)
                return RejectionReason.LogDisabled;

            RejectionReason showLogsViolation = ShowLogsValidation(targetID);

            if (showLogsViolation != RejectionReason.None)
                return showLogsViolation;

            if (request.Type == RequestType.Remote && (targetID.Access == LogAccess.Private || !Handler.AllowRemoteLogging))
                return RejectionReason.AccessDenied;

            return RejectionReason.None;
        }

        /// <summary>
        /// Checks that the <see cref="RainWorld.ShowLogs"/> property is applicable to the provided <see cref="LogID"/> instance, and returns the applicable <see cref="RejectionReason"/> when it does
        /// </summary>
        public static RejectionReason ShowLogsValidation(LogID logID)
        {
            if (logID.Properties.ShowLogsAware && !RainWorld.ShowLogs)
                return ShowLogsViolation();
            return RejectionReason.None;
        }

        /// <summary>
        /// Gets the applicable <see cref="RejectionReason"/> for a <see cref="RainWorld.ShowLogs"/> aware log request
        /// </summary>
        public static RejectionReason ShowLogsViolation()
        {
            if (RainWorldInfo.LatestSetupPeriodReached < RainWorldInfo.SHOW_LOGS_ACTIVE_PERIOD)
                return RejectionReason.ShowLogsNotInitialized;
            return RejectionReason.LogDisabled;
        }
    }

    public interface IRequestValidator
    {
        /// <summary>
        /// Evaluates a <see cref="LogRequest"/> object
        /// </summary>
        /// <param name="request">The request to evaluate</param>
        /// <returns>The processed handle state based on logger specific validation rules</returns>
        RejectionReason GetResult(LogRequest request);
    }
}
