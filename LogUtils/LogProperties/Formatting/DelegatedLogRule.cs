using LogUtils.Events;
using LogUtils.Formatting;

namespace LogUtils.Properties.Formatting
{
    /// <summary>
    /// A LogRule that stores its apply logic inside of a delegate
    /// </summary>
    public class DelegatedLogRule : LogRule
    {
        /// <summary>
        /// Invoked when rule is applied
        /// </summary>
        protected ApplyDelegate Callback;

        /// <summary>
        /// Create a DelegatedLogRule instance
        /// </summary>
        /// <param name="name">The name associated with the LogRule. (Make it unique)</param>
        /// <param name="applyCallback">The delegate to assign as the rule logic</param>
        /// <param name="enabled">Whether the rule is applied</param>
        public DelegatedLogRule(string name, ApplyDelegate applyCallback, bool enabled) : base(name, enabled)
        {
            Callback = applyCallback;
        }

        /// <inheritdoc/>
        protected override string ApplyRule(LogMessageFormatter formatter, string message, LogRequestEventArgs logEventData)
        {
            return Callback.Invoke(formatter, message, logEventData);
        }
    }
}
