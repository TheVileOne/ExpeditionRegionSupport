using LogUtils.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils.Properties
{
    internal class BepInExHeaderRule : ShowCategoryRule
    {
        public BepInExHeaderRule(bool enabled) : base(enabled)
        {
        }

        protected override string ApplyRule(string message, LogMessageEventArgs logEventData)
        {
            return string.Format("[{0,-7}:{1,10}] {2}", logEventData.BepInExCategory, logEventData.LogSource?.SourceName ?? "Unknown", message);
        }
    }
}
