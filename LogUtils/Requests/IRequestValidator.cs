namespace LogUtils.Requests
{
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
