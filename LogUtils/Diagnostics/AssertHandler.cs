using LogUtils.Enums;

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

        public AssertHandler(Logger logger)
        {
            AssertLogger = logger;
        }

        public virtual void Handle(ConditionResults condition)
        {
            if (condition.Passed) return;

            string response = (UtilityConsts.AssertResponse.FAIL + ": " + condition.ToString()).TrimEnd();
            AssertLogger.Log(LogCategory.Assert, response);
        }
    }
}
