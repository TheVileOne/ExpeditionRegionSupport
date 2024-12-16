using LogUtils.Enums;
using LogUtils.Helpers;
using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public class AssertHandler : IConditionHandler
    {
        public static readonly AssertHandler DefaultHandler = new AssertHandler(new Logger(LogID.Unity));

        static AssertHandler()
        {
            Condition.AssertHandlers.Add(DefaultHandler);
        }

        public Logger AssertLogger; 

        public AssertHandler()
        {
        }

        public AssertHandler(Logger logger)
        {
            AssertLogger = logger;
        }

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
            //Attempt to get mod data based on the calling assembly
            bool hasData = ModData.TryGet(AssemblyUtils.GetPlugin(Caller), out ModData data);

            //Fallback to defaults when no valid targets are available
            if (!hasData || data.AssertTargets.Count == 0)
                data = ModData.Default;

            //Return targets as a new collection to avoid potential modification to the list
            return new List<LogID>(data.AssertTargets);
        }
    }

    public record struct ConditionResults(ConditionID ID, bool HasPassed);
}
