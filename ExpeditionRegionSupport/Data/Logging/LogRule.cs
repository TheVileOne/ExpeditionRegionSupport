using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class LogRule
    {
        /// <summary>
        /// The default priority for LogRule instances. Rules are applied in order of priority from lowest to highest
        /// </summary>
        public virtual float ApplyPriority => 0.7f;

        public virtual string ApplyRule(string message)
        {
            return message;
        }
    }

    public class ShowCategoryRule : LogRule
    {
        /// <summary>
        /// Categories should apply after almost all non-custom message modifications
        /// </summary>
        public override float ApplyPriority => 0.9995f;
    }

    public class ShowLineCountRule : LogRule
    {
        /// <summary>
        /// Line count should apply after all non-custom message modifications
        /// </summary>
        public override float ApplyPriority => 1.0f;
    }
}
