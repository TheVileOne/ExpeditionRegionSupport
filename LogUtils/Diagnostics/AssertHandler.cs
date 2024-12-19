using LogUtils.Enums;
using System;

namespace LogUtils.Diagnostics
{
    public class AssertHandler : IConditionHandler
    {
        public static readonly AssertHandler DefaultHandler = new AssertHandler(new Logger(LogID.Unity));

        static AssertHandler()
        {
            Condition.AssertHandlers.Add(DefaultHandler);
        }

        public Logger Logger;

        public AssertHandler(Logger logger)
        {
            Logger = logger;
        }

        public virtual void Handle(AssertArgs assertSettings, ConditionResults condition)
        {
            if (condition.Passed || assertSettings.Behavior == AssertBehavior.DoNothing) return;

            bool shouldLog = assertSettings.Behavior == AssertBehavior.Log || assertSettings.Behavior == AssertBehavior.LogAndThrow;
            bool shouldThrow = assertSettings.Behavior == AssertBehavior.ThrowOnly || assertSettings.Behavior == AssertBehavior.LogAndThrow;

            if (shouldLog)
            {
                string response = UtilityConsts.AssertResponse.FAIL + ": " + condition.ToString();
                Logger.Log(LogCategory.Assert, response);
            }

            if (shouldThrow)
                throw new AssertFailedException("Assert triggered");
        }
    }

    public class AssertFailedException : Exception
    {
        public AssertFailedException(string message) : base(message) { }
    }
}
