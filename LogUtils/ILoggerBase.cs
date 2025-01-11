namespace LogUtils
{
    public interface ILoggerBase
    {
        /// <summary>
        /// Can this logger instance accept, and process a specific LogRequest instance
        /// </summary>
        public bool CanHandle(LogRequest request, bool doPathCheck = false);

        /// <summary>
        /// Accepts and processes a LogRequest instance, and returns the reason for rejecting the request, or returns None if it wasn't rejected
        /// </summary>
        public RejectionReason HandleRequest(LogRequest request, bool skipAccessValidation = false);
    }
}
