using LogUtils.Diagnostics.Tests;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
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

        public MessageFormatter Formatter = new MessageFormatter();
        public Logger Logger;

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

            bool shouldLogOnPass = (Behavior & AssertBehavior.LogOnPass) != 0,
                 shouldLogOnFail = (Behavior & AssertBehavior.LogOnFail) != 0;

            if (result.PassedWithExpectations())
            {
                shouldLog = result.Passed ? shouldLogOnPass : shouldLogOnFail;
            }
            else
            {
                //Unexpected results, and failed results are handled similarly
                shouldLog = shouldLogOnFail;
                shouldThrow = (Behavior & AssertBehavior.Throw) != 0;
            }

            //Check the behavior flags, and apply the appropriate behaviors
            if (shouldLog)
            {
                string responseHeader = result.Passed ? Formatter.PassResponse : Formatter.FailResponse;
                string responseString = Formatter.Format(result, responseHeader);
                Logger.Log(LogCategory.Assert, responseString);
            }

            if (shouldThrow)
            {
                string responseString = Formatter.Format(result);
                throw new AssertionException(UtilityConsts.AssertResponse.FAIL, responseString);
            }
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

        public class MessageFormatter
        {
            public string FailResponse = UtilityConsts.AssertResponse.FAIL;
            public string PassResponse = UtilityConsts.AssertResponse.PASS;

            public string Format(in Condition.Result result, string messageHeader = null)
            {
                string messageBase = result.ToString();

                if (messageHeader != null)
                {
                    //We don't want separator formatting when there isn't a message to show
                    if (string.IsNullOrEmpty(messageBase))
                        messageBase = messageHeader;
                    else
                        messageBase = messageHeader + ": " + messageBase;
                }

                //Find content we need to append to the end of the message
                result.CompileMessageTags();

                HashSet<string> messageTags = result.Message.Tags;

                if (messageTags.Count > 0)
                    messageBase += $" ({string.Join(", ", messageTags)})";
                return messageBase;
            }
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
