using LogUtils.Enums;
using LogUtils.Helpers.Extensions;

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

            if (targetID.Properties.ShowLogsAware && !RainWorld.ShowLogs)
            {
                if (RWInfo.LatestSetupPeriodReached < RWInfo.SHOW_LOGS_ACTIVE_PERIOD)
                    return RejectionReason.ShowLogsNotInitialized;
                return RejectionReason.LogDisabled;
            }

            if (request.Type == RequestType.Remote && (targetID.Access == LogAccess.Private || !Handler.AllowRemoteLogging))
                return RejectionReason.AccessDenied;

            return RejectionReason.None;
        }
    }

    public interface IRequestValidator
    {
        /// <summary>
        /// Evaluates a LogRequest object
        /// </summary>
        /// <param name="request">The request to evaluate</param>
        /// <returns>The processed handle state based on logger specific validation rules</returns>
        RejectionReason GetResult(LogRequest request);
    }
}
