using LogUtils.Enums;

namespace LogUtils.Diagnostics
{
    public class AssertHandler : IConditionHandler
    {
        public const string DEFAULT_FAIL_MESSAGE = "Assertion failed";

        public static readonly AssertHandler DefaultHandler = new AssertHandler(new Logger(LogID.Unity));

        static AssertHandler()
        {
            Condition.AssertHandlers.Add(DefaultHandler);
        }

        public Logger AssertLogger;

        public string FailMessage = DEFAULT_FAIL_MESSAGE;

        public AssertHandler(Logger logger)
        {
            AssertLogger = logger;
        }

        public virtual void Handle(ConditionResults condition)
        {
            if (!condition.Passed)
                AssertLogger.Log(LogCategory.Assert, FailMessage);
        }
    }

    public record struct ConditionResults(ConditionID ID, bool Passed);
}
