namespace LogUtils.Diagnostics
{
    public interface IConditionHandler
    {
        /// <summary>
        /// Determines whether result should be handled or ignored
        /// </summary>
        public bool IsEnabled { get; }

        /// <summary>
        /// Apply post-processing logic to a condition result
        /// </summary>
        public void Handle(in Condition.Result result);
    }
}
