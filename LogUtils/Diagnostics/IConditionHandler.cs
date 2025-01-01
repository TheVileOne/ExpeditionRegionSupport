namespace LogUtils.Diagnostics
{
    public interface IConditionHandler
    {
        /// <summary>
        /// Apply post-processing logic to a condition result
        /// </summary>
        public void Handle(in Condition.Result result);
    }
}
