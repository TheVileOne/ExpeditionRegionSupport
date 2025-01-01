using LogUtils.Enums;
using System;
using UnityEngine.Assertions;

namespace LogUtils.Diagnostics
{
    public class AssertHandler : IConditionHandler, ICloneable
    {
        public static readonly AssertHandler DefaultHandler = new AssertHandler(new Logger(LogID.Unity));

        public AssertBehavior Behavior = AssertBehavior.LogOnFail;

        public Logger Logger;

        public string FailResponse = UtilityConsts.AssertResponse.FAIL;
        public string PassResponse = UtilityConsts.AssertResponse.PASS;

        public AssertHandler(Logger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Process a condition result
        /// </summary>
        /// <param name="result">The result to evaluate</param>
        /// <exception cref="AssertionException">Throws when AssertBehavior.Throw is set, and assert fails</exception>
        public virtual void Handle(in Condition.Result result)
        {
            if (Behavior == AssertBehavior.DoNothing) return;

            //Set flags that will determine if we will log, throw an exception, or both
            bool shouldLog = false,
                 shouldThrow = false;
            string responseString = null;
            if (result.Passed)
            {
                if ((Behavior & AssertBehavior.LogOnPass) != 0)
                {
                    shouldLog = true;
                    responseString = PassResponse;
                }
                shouldThrow = false;
            }
            else
            {
                if ((Behavior & AssertBehavior.LogOnFail) != 0)
                {
                    shouldLog = true;
                    responseString = FailResponse;
                }
                shouldThrow = (Behavior & AssertBehavior.Throw) != 0;
            }

            //Check the behavior flags, and apply the appropriate behaviors
            if (shouldLog)
            {
                if (string.IsNullOrEmpty(responseString))
                    responseString = result.ToString();
                else
                    responseString += ": " + result.ToString();
                Logger.Log(LogCategory.Assert, responseString);
            }

            if (shouldThrow)
                throw new AssertionException("Assertion failed", result.ToString());
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

    [Flags]
    public enum AssertBehavior
    {
        DoNothing = 0,
        LogOnFail = 1,
        LogOnPass = 2,
        Throw = 4,
        LogAndThrow = LogOnFail & Throw,
    }
}
