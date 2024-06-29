using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data.Logging
{
    /// <summary>
    /// A LogRule that stores its apply logic inside of a delegate
    /// </summary>
    public class DelegatedLogRule : LogRule
    {
        public ApplyDelegate RuleAction;

        /// <summary>
        /// Create a DelegatedLogRule instance
        /// </summary>
        /// <param name="name">The name associated with the LogRule. (Make it unique)</param>
        /// <param name="enabled">Whether the rule is applied</param>
        public DelegatedLogRule(string name, bool enabled) : base(name, enabled)
        {
        }

        /// <summary>
        /// Create a DelegatedLogRule instance
        /// </summary>
        /// <param name="name">The name associated with the LogRule. (Make it unique)</param>
        /// <param name="action">The delegate to assign as the rule logic</param>
        /// <param name="enabled">Whether the rule is applied</param>
        public DelegatedLogRule(string name, ApplyDelegate action, bool enabled) : base(name, enabled)
        {
            RuleAction = action;
        }

        protected override string ApplyRule(string message)
        {
            return RuleAction.Invoke(message);
        }
    }
}
