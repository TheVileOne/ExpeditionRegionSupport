using System;

namespace LogUtils.Requests
{
    public abstract class RequestHandlerModule
    {
        protected LogRequest Request;

        protected bool RequiresAccessValidation;

        /// <summary>
        /// Whether more than one request is being processed as a batch
        /// </summary>
        protected bool IsBatching;

        protected bool ShouldIgnore;

        public event Func<LogRequest, bool> PrepareRequest;

        public void Handle(LogRequest request, bool skipAccessValidation = false)
        {
            Request = request;
            RequiresAccessValidation = !skipAccessValidation;

            Start();
            if (!ShouldIgnore)
                HandleRequest();
            Complete();
        }

        public void Handle(LogRequest[] requests, bool skipAccessVerification = false)
        {
            IsBatching = true;
            foreach (LogRequest request in requests)
            {
                Handle(request, skipAccessVerification);
            }
            IsBatching = false;
        }

        protected void Start()
        {
            if (Request.Submitted)
                UtilityCore.RequestHandler.CurrentRequest = Request;
            Request.ResetStatus(); //Ensure that processing request is handled in a consistent way

            ShouldIgnore = false;

            if (PrepareRequest != null)
                ShouldIgnore = !PrepareRequest(Request);
        }

        protected void Complete()
        {
            Request.Processed();
            UtilityCore.RequestHandler.RequestMayBeCompleteOrInvalid(Request);
            Request = null;
        }

        protected abstract void HandleRequest();
    }
}
