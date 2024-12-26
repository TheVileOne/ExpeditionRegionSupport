using LogUtils.Enums;
using System;

namespace LogUtils.Diagnostics
{
    public class AssertHandler : IConditionHandler, ICloneable
    {
        public static readonly AssertHandler DefaultHandler = new AssertHandler(new Logger(LogID.Unity));

        static AssertHandler()
        {
            Condition.AssertHandlers.Add(DefaultHandler);
        }

        public AssertBehavior Behavior = AssertBehavior.Log;

        public Logger Logger;

        public AssertHandler(Logger logger)
        {
            Logger = logger;
        }

        public virtual void Handle<T>(Condition<T> condition)
        {
            if (condition.Passed || Behavior == AssertBehavior.DoNothing) return;

            bool shouldLog = Behavior == AssertBehavior.Log || Behavior == AssertBehavior.LogAndThrow;
            bool shouldThrow = Behavior == AssertBehavior.Throw || Behavior == AssertBehavior.LogAndThrow;

            if (shouldLog)
            {
                string response = UtilityConsts.AssertResponse.FAIL + ": " + condition.ToString();
                Logger.Log(LogCategory.Assert, response);
            }

            if (shouldThrow)
                throw new AssertFailedException("Assert triggered");
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public AssertHandler Clone(AssertBehavior behavior)
        {
            var clone = (AssertHandler)Clone();

            clone.Behavior = behavior;
            return clone;
        }
    }

    public enum AssertBehavior
    {
        Log,
        LogAndThrow,
        Throw,
        DoNothing //Disable
    }

    public class AssertFailedException : Exception
    {
        public AssertFailedException(string message) : base(message) { }
    }
}
