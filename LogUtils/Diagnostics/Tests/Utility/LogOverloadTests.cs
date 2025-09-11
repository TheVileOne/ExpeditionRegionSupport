using LogUtils.Enums;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LogUtils.Diagnostics.Tests.Utility
{
    /// <summary>
    /// These tests check that the Logger API translate user input into the correct logging overload
    /// </summary>
    internal sealed class LogOverloadTests : TestCase, ITestable
    {
        internal const string TEST_NAME = "Test - Logging Overloads";

        private List<Hook> testHooks = new List<Hook>();

        private MethodInfo methodCalled;

        public LogOverloadTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            ApplyHooks();
            TestSingleArgument();
            RemoveHooks();
        }

        internal void TestSingleArgument()
        {
            string argument = "test";

            Logger logger = new DiscreteLogger(false);

            logger.Log(argument);
            AssertResultAndClear([typeof(string)]);

            logger.Log((object)argument);
            AssertResultAndClear([typeof(object)]);

            logger.Log($"{argument}");
            AssertResultAndClear([typeof(FormattableString)]);

            logger.Log(LogCategory.Default);
            AssertResultAndClear([typeof(object)]);

            logger.Log(LogID.NotUsed);
            AssertResultAndClear([typeof(object)]);

            logger.Log(Color.red);
            AssertResultAndClear([typeof(object)]);

            logger.Log(ConsoleColor.Red);
            AssertResultAndClear([typeof(object)]);
        }

        internal void AssertResultAndClear(params object[] expectedParams)
        {
            if (methodCalled == null) //Fail and return instead of throwing a null reference
            {
                AssertThat(methodCalled).IsNotNull(); //This should not happen, and can be removed when tests are complete
                return;
            }

            var parameters = methodCalled.GetParameters();

            AssertThat(parameters.Length).IsEqualTo(expectedParams.Length); //Parameters must be exactly equal

            UtilityLogger.Log("First arg type: " + parameters[0].ParameterType);
            UtilityLogger.Log("First expected type: " + expectedParams[0]);

            int checkCount = Math.Min(parameters.Length, expectedParams.Length);
            for (int i = 0; i < checkCount; i++)
                AssertThat(parameters[i].ParameterType).IsEqualTo(expectedParams[i].GetType()); //Each type must match in the exact same order

            //Reset state for the next test
            methodCalled = null;
        }

        internal void ApplyHooks()
        {
            Type loggerType = typeof(Logger);

            var methods = loggerType.GetMethods().Where(method => method.Name.StartsWith("Log"));

            foreach (MethodInfo method in methods)
            {
                var parameters = method.GetParameters();

                Type firstParam = parameters[0].ParameterType;

                if (firstParam == typeof(string))
                {
                    if (tryCreateHook<string>(method, parameters, out Hook hook))
                        testHooks.Add(hook);
                }
                else if (firstParam == typeof(object))
                {
                    if (tryCreateHook<string>(method, parameters, out Hook hook))
                        testHooks.Add(hook);
                }
                else if (firstParam == typeof(LogCategory))
                {
                    if (tryCreateHook<LogCategory>(method, parameters, out Hook hook))
                        testHooks.Add(hook);
                }
                else if (firstParam == typeof(FormattableString))
                {
                    if (tryCreateHook<FormattableString>(method, parameters, out Hook hook))
                        testHooks.Add(hook);
                }
            }

            foreach (Hook h in testHooks)
                h.Apply();
        }

        internal void RemoveHooks()
        {
            testHooks.ForEach(hook => hook.Free());
            testHooks.Clear();
        }

        private bool tryCreateHook<TFirst>(MethodInfo method, ParameterInfo[] parameters, out Hook hook)
        {
            hook = null;
            switch (parameters.Length)
            {
                case 1:
                    hook = createHookWithOneArgument<TFirst>(method);
                    break;
                case 2:
                    Type secondParam = parameters[1].ParameterType;

                    if (secondParam == typeof(string))
                    {
                        hook = createHookWithTwoArguments<TFirst, string>(method);
                    }
                    else if (secondParam == typeof(object))
                    {
                        hook = createHookWithTwoArguments<TFirst, object>(method);
                    }
                    else if (secondParam == typeof(FormattableString))
                    {
                        hook = createHookWithTwoArguments<TFirst, FormattableString>(method);
                    }
                    else if (secondParam == typeof(object[]))
                    {
                        hook = createHookWithTwoArguments<TFirst, object[]>(method);
                    }
                    else if (secondParam == typeof(Color))
                    {
                        hook = createHookWithTwoArguments<TFirst, Color>(method);
                    }
                    break;
            }
            return hook != null;
        }

        private Hook createHookWithOneArgument<TArg>(MethodInfo method)
        {
            return new Hook(method, (Action<Logger, TArg> orig, Logger self, TArg arg) =>
            {
                if (methodCalled == null)
                    methodCalled = method;
                orig(self, arg);
            });
        }

        private Hook createHookWithTwoArguments<TArg1, TArg2>(MethodInfo method)
        {
            return new Hook(method, (Action<Logger, TArg1, TArg2> orig, Logger self, TArg1 arg1, TArg2 arg2) =>
            {
                if (methodCalled == null)
                    methodCalled = method;
                orig(self, arg1, arg2);
            });
        }

        private Hook createHookWithThreeArguments<TArg1, TArg2, TArg3>(MethodInfo method)
        {
            return new Hook(method, (Action<Logger, TArg1, TArg2, TArg3> orig, Logger self, TArg1 arg1, TArg2 arg2, TArg3 arg3) =>
            {
                if (methodCalled == null)
                    methodCalled = method;
                orig(self, arg1, arg2, arg3);
            });
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
        }
    }
}
