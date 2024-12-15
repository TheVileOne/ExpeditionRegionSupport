using System.Reflection;

namespace LogUtils.Diagnostics
{
    public interface IConditionHandler
    {
        /// <summary>
        /// Should the handler be supplied with the calling assembly on a specified condition
        /// </summary>
        public bool AcceptsCallerOnCondition(bool condition);

        /// <summary>
        /// The calling assembly associated with the condition results
        /// </summary>
        public Assembly Caller { get; set; }

        /// <summary>
        /// Apply post-processing logic to the condition results
        /// </summary>
        public void Handle(ConditionResults results);
    }
}
