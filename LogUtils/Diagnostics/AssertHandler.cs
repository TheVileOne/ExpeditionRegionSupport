using LogUtils.Enums;
using LogUtils.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LogUtils.Diagnostics
{
    public class AssertHandler : IConditionHandler
    {
        /// <summary>
        /// The default log file used to log assert results when a mod plugin doesn't specify a preference
        /// </summary>
        public static LogID DefaultLogID = LogID.Unity;

        public Assembly Caller { get; set; }

        public bool AcceptsCallerOnCondition(bool conditionPassed)
        {
            return conditionPassed == false && ModData.HasAssertTargets; //No targets, no need to fetch the calling assembly
        }

        public void Handle(ConditionResults condition)
        {
            if (!condition.HasPassed)
            {
                List<LogID> logTargets = getLogTargets();

                foreach (LogID logTarget in logTargets)
                {
                    //TODO: Log
                }
            }
        }

        private List<LogID> getLogTargets()
        {
            bool hasData = ModData.TryGet(AssemblyUtils.GetPlugin(Caller), out ModData data);

            List<LogID> logTargets = new List<LogID>();
            if (!hasData || data.AssertTargets.Count == 0)
            {
                logTargets.Add(DefaultLogID);
                return logTargets;
            }

            logTargets.AddRange(data.AssertTargets);
            return logTargets;
        }
    }

    public record struct ConditionResults(ConditionID ID, bool HasPassed);
}
