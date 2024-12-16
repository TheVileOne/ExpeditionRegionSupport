using System.Reflection;

namespace LogUtils.Diagnostics
{
    public interface IConditionHandler
    {
        /// <summary>
        /// Apply post-processing logic to the condition results
        /// </summary>
        public void Handle(ConditionResults results);
    }
}
