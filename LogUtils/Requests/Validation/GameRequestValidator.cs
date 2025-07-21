using LogUtils.Enums;
using LogUtils.Helpers.Extensions;

namespace LogUtils.Requests.Validation
{
    /// <summary>
    /// Used to validate game-specific log requests
    /// </summary>
    public class GameRequestValidator : IRequestValidator
    {
        public ILogHandler Handler;

        public GameRequestValidator(ILogHandler handler)
        {
            Handler = handler;
        }

        public RejectionReason GetResult(LogRequest request)
        {
            if (!Handler.CanHandle(request))
                return RejectionReason.NotAllowedToHandle;

            LogID logFile = request.Data.ID;

            if (!logFile.IsEnabled)
                return RejectionReason.LogDisabled;

            //Check RainWorld.ShowLogs for logs that are restricted by it
            RejectionReason showLogsViolation = RequestValidator.ShowLogsValidation(logFile);

            if (showLogsViolation != RejectionReason.None)
                return showLogsViolation;

            if (!logFile.Properties.CanBeAccessed)
                return RejectionReason.LogUnavailable;

            return RejectionReason.None;
        }
    }
}
