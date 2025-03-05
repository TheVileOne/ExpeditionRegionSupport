using LogUtils.CompatibilityServices;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace LogUtils.Diagnostics
{
    public class AssertHandler : IConditionHandler, ICloneable
    {
        private static AssertHandler _default = new AssertHandler(new UnityLogger()); 
        private static AssertHandler _customDefault;

        public static AssertHandler Default
        {
            get
            {
                //Custom handlers always get priority
                if (_customDefault != null)
                    return _customDefault;

                if (UtilitySetup.CurrentStep < UtilitySetup.InitializationStep.INITIALIZE_ENUMS)
                {
                    if (_default.Logger != UtilityLogger.Logger)
                    {
                        //Handle asserts using the UtilityLogger instance during this very early initialization period
                        UtilityLogger.Log("Assert system accessed before LogIDs have initialized");
                        UtilityLogger.Log("Deploying fallback logger");
                        _default.Logger = UtilityLogger.Logger;
                    }
                }
                else if (_default.Logger == UtilityLogger.Logger && (UnityLogger.ReceiveUnityLogEvents || (LogID.Unity != null && LogID.Unity.Properties.CanBeAccessed)))
                {
                    UtilityLogger.Log("Fallback logger no longer necessary");
                    _default.Logger = new UnityLogger();
                }
                return _default;
            }
            set => _customDefault = value;
        }

        private AssertBehavior _behavior = AssertBehavior.LogOnFail;
        public AssertBehavior Behavior
        {
            get => IsEnabled ? _behavior : AssertBehavior.DoNothing;
            set => _behavior = value;
        }

        public MessageFormatter Formatter = new MessageFormatter();
        public ILogger Logger;

        public virtual bool IsEnabled => Debug.AssertsEnabled;

        public AssertHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Process a condition result
        /// </summary>
        /// <param name="result">The result to evaluate</param>
        /// <exception cref="AssertionException">Throws when AssertBehavior.Throw is set, and assert fails</exception>
        public virtual void Handle(Condition.Result result)
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
            AssertHandler handler = GetCompatibleTemplate(preferredHandler);

            bool isNewBehavior = behavior != handler.Behavior;

            //On new behaviors, we must clone the existing handler to ensure that this behavior only applies to a single assert chain
            if (isNewBehavior)
                handler = handler.Clone(behavior);
            return handler;
        }

        internal static AssertHandler GetCompatibleTemplate(IConditionHandler handler)
        {
            AssertHandler template = handler as AssertHandler;

            if (template == null)
                template = Default;

            return template;
        }

        public class MessageFormatter
        {
            public string FailResponse = UtilityConsts.AssertResponse.FAIL;
            public string PassResponse = UtilityConsts.AssertResponse.PASS;

            public string Format(Condition.Result result, string messageHeader = null)
            {
                string messageBase = result.ToString();

                if (messageHeader != null)
                {
                    //We don't want separator formatting when there isn't a message to show
                    if (string.IsNullOrEmpty(messageBase))
                        messageBase = messageHeader;
                    else
                        messageBase = $"{messageHeader}: {messageBase}";
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
        LogAndThrow = LogOnFail | Throw
    }
}
