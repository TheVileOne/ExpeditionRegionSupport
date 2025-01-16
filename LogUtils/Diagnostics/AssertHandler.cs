using LogUtils.Diagnostics.Tests;
using LogUtils.Enums;
using System;
using UnityEngine.Assertions;

namespace LogUtils.Diagnostics
{
    public class AssertHandler : IConditionHandler, ICloneable
    {
        public static IConditionHandler CurrentTemplate => TestSuite.ActiveSuite?.Handler ?? DefaultHandler;

        public static readonly AssertHandler DefaultHandler = new AssertHandler(new Logger(LogID.Unity));

        private AssertBehavior _behavior = AssertBehavior.LogOnFail;
        public AssertBehavior Behavior
        {
            get => IsEnabled ? _behavior : AssertBehavior.DoNothing;
            set => _behavior = value;
        }

        public Logger Logger;

        public string FailResponse = UtilityConsts.AssertResponse.FAIL;
        public string PassResponse = UtilityConsts.AssertResponse.PASS;

        public virtual bool IsEnabled => Debug.AssertsEnabled;

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

                PostProcessResponseString(ref responseString);
                Logger.Log(LogCategory.Assert, responseString);
            }

            if (shouldThrow)
                throw new AssertionException(UtilityConsts.AssertResponse.FAIL, result.ToString());
        }

        /// <summary>
        /// Allows changes to the response string prior to logging the response
        /// </summary>
        protected virtual void PostProcessResponseString(ref string response)
        {
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

        public static AssertHandler GetTemplateWithBehavior(AssertBehavior behavior, IConditionHandler preferredHandler = null)
        {
            //In order to apply the AssertBehavior, we must be using an instance type that can handle it
            AssertHandler handler = getCompatibleTemplate(preferredHandler);

            bool isNewBehavior = behavior != handler.Behavior;

            //On new behaviors, we must clone the existing handler to ensure that this behavior only applies to a single assert chain
            if (isNewBehavior)
                handler = handler.Clone(behavior);
            return handler;
        }

        private static AssertHandler getCompatibleTemplate(IConditionHandler handler)
        {
            AssertHandler template = handler as AssertHandler;

            if (template == null)
                template = CurrentTemplate as AssertHandler;

            return template ?? DefaultHandler;
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
