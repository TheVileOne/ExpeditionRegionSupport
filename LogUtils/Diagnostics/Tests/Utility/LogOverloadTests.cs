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
    internal sealed class LogOverloadTests : TestCaseGroup, ITestable
    {
        internal const string TEST_NAME = "Test - Logging Overloads";

        private List<Hook> testHooks = new List<Hook>();

        private MethodInfo methodCalled;

        private TestCase activeCase;

        public LogOverloadTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            ApplyHooks();
            TestSingleArgument();
            TestTwoArguments();
            RemoveHooks();
        }

        internal void TestSingleArgument()
        {
            Condition.Result.ResetCount();

            activeCase = new TestCase(this, "Test - Single Argument");
            Logger logger = new DiscreteLogger(false);

            //String
            logger.Log(Arguments.String);
            AssertResultAndClear(Types.String);

            //Object
            logger.Log(Arguments.Object);
            AssertResultAndClear(Types.Object);

            //Interpolated string
            logger.Log($"{Arguments.Object}");
            AssertResultAndClear(Types.InterpolatedStringHandler);

            //Null
            logger.Log(null);
            AssertResultAndClear(Types.String);

            //LogCategory
            logger.Log(Arguments.LogCategory);
            AssertResultAndClear(Types.Object);

            //LogID
            logger.Log(Arguments.LogID);
            AssertResultAndClear(Types.Object);

            //Color
            logger.Log(Arguments.Color);
            AssertResultAndClear(Types.Object);

            //ConsoleColor
            logger.Log(Arguments.ConsoleColor);
            AssertResultAndClear(Types.Object);

            logger.Dispose();
        }

        internal void TestTwoArguments()
        {
            Condition.Result.ResetCount();
            TestCaseGroup testGroup = new TestCaseGroup(this, "Test - Two Arguments");

            activeCase = testGroup;
            Logger logger = new DiscreteLogger(false);

            #region String tests

            testGroup.Add(activeCase = new TestCase("Test: String arguments"));

            //String, String
            logger.Log(Arguments.String, Arguments.String);
            AssertResultAndClear(Types.String, Types.String);

            //String, Object
            logger.Log(Arguments.String, Arguments.Object);
            AssertResultAndClear(Types.String, Types.Object);

            //String, Interpolated string
            logger.Log(Arguments.String, $"{Arguments.Object}");
            AssertResultAndClear(Types.String, Types.InterpolatedStringHandler);

            //String, Null (ambiguous)
            //logger.Log(Arguments.String, null);

            //String, LogCategory
            logger.Log(Arguments.String, Arguments.LogCategory);
            AssertResultAndClear(Types.String, Types.Object);

            //String, LogID
            logger.Log(Arguments.String, Arguments.LogID);
            AssertResultAndClear(Types.String, Types.Object);

            //String, Color
            logger.Log(Arguments.String, Arguments.Color);
            AssertResultAndClear(Types.String, Types.Color); //Color overload takes priority over object overload

            //String, ConsoleColor
            logger.Log(Arguments.String, Arguments.ConsoleColor);
            AssertResultAndClear(Types.String, Types.ConsoleColor); //Color overload takes priority over object overload
            #endregion
            #region Interpolated String tests

            testGroup.Add(activeCase = new TestCase("Test: Interpolated string arguments"));

            //Interpolated string, String
            logger.Log($"{Arguments.Object}", Arguments.String);
            AssertResultAndClear(Types.String, Types.String);

            //Interpolated string, Object
            logger.Log($"{Arguments.Object}", Arguments.Object);
            AssertResultAndClear(Types.String, Types.Object);

            //Interpolated string, Interpolated string
            logger.Log($"{Arguments.Object}", $"{Arguments.Object}");
            AssertResultAndClear(Types.String, Types.InterpolatedStringHandler);

            //Interpolated string, Null (ambiguous)
            //logger.Log($"{Arguments.Object}", null);

            //Interpolated string, LogCategory
            logger.Log($"{Arguments.Object}", Arguments.LogCategory);
            AssertResultAndClear(Types.String, Types.Object);

            //Interpolated string, LogID
            logger.Log($"{Arguments.Object}", Arguments.LogID);
            AssertResultAndClear(Types.String, Types.Object);

            //Interpolated string, Color
            logger.Log($"{Arguments.Object}", Arguments.Color);
            AssertResultAndClear(Types.InterpolatedStringHandler, Types.Color); //Color overload takes priority over object overload

            //Interpolated string, ConsoleColor
            logger.Log($"{Arguments.Object}", Arguments.ConsoleColor);
            AssertResultAndClear(Types.InterpolatedStringHandler, Types.ConsoleColor); //Color overload takes priority over object overload
            #endregion
            #region Object tests

            testGroup.Add(activeCase = new TestCase("Test: Object arguments"));

            //object, Color
            logger.Log(Arguments.Object, Arguments.Color);
            AssertResultAndClear(Types.Object, Types.Color); //Color overload takes priority over object overload

            //object, ConsoleColor
            logger.Log(Arguments.Object, Arguments.ConsoleColor);
            AssertResultAndClear(Types.Object, Types.ConsoleColor); //Color overload takes priority over object overload
            #endregion
            #region LogCategory tests

            testGroup.Add(activeCase = new TestCase("Test: LogCategory arguments"));

            //LogCategory, String
            logger.Log(Arguments.LogCategory, Arguments.String);
            AssertResultAndClear(Types.LogCategory, Types.String);

            //LogCategory, Object
            logger.Log(Arguments.LogCategory, Arguments.Object);
            AssertResultAndClear(Types.LogCategory, Types.Object);

            //LogCategory, Interpolated string
            logger.Log(Arguments.LogCategory, $"{Arguments.Object}");
            AssertResultAndClear(Types.LogCategory, Types.InterpolatedStringHandler);

            //LogCategory, Null
            logger.Log(Arguments.LogCategory, null);
            AssertResultAndClear(Types.LogCategory, Types.String);

            //LogCategory, LogCategory
            logger.Log(Arguments.LogCategory, Arguments.LogCategory);
            AssertResultAndClear(Types.LogCategory, Types.Object);

            //LogCategory, LogID
            logger.Log(Arguments.LogCategory, Arguments.LogID);
            AssertResultAndClear(Types.LogCategory, Types.Object);

            //LogCategory, Color (ambiguous)
            //logger.Log(Arguments.LogCategory, Arguments.Color);

            //LogCategory, ConsoleColor (ambiguous)
            //logger.Log(Arguments.LogCategory, Arguments.ConsoleColor);
            #endregion
            #region ILogTarget tests

            TestCaseGroup logTargetGroup = new TestCaseGroup("Test: ILogTarget arguments");
            testGroup.Add(logTargetGroup);
            logTargetGroup.Add(activeCase = new TestCase("LogID arguments"));

            //LogID, String
            logger.Log(Arguments.LogID, Arguments.String);
            AssertResultAndClear(Types.LogTarget, Types.String);

            //LogID, Object
            logger.Log(Arguments.LogID, Arguments.Object);
            AssertResultAndClear(Types.LogTarget, Types.Object);

            //LogID, Interpolated string
            logger.Log(Arguments.LogID, $"{Arguments.Object}");
            AssertResultAndClear(Types.LogTarget, Types.InterpolatedStringHandler);

            //LogID, Null
            logger.Log(Arguments.LogID, null);
            AssertResultAndClear(Types.LogTarget, Types.String);

            //LogID, LogCategory
            logger.Log(Arguments.LogID, Arguments.LogCategory);
            AssertResultAndClear(Types.LogTarget, Types.Object);

            //LogID, LogID
            logger.Log(Arguments.LogID, Arguments.LogID);
            AssertResultAndClear(Types.LogTarget, Types.Object);

            //LogID, Color (ambiguous)
            //logger.Log(Arguments.LogID, Arguments.Color);

            //LogID, ConsoleColor (ambiguous)
            //logger.Log(Arguments.LogID, Arguments.ConsoleColor);

            logTargetGroup.Add(activeCase = new TestCase("ConsoleID arguments"));

            //ConsoleID, String
            logger.Log(Arguments.ConsoleID, Arguments.String);
            AssertResultAndClear(Types.LogTarget, Types.String);

            //ConsoleID, Object
            logger.Log(Arguments.ConsoleID, Arguments.Object);
            AssertResultAndClear(Types.LogTarget, Types.Object);

            //ConsoleID, Interpolated string
            logger.Log(Arguments.ConsoleID, $"{Arguments.Object}");
            AssertResultAndClear(Types.LogTarget, Types.InterpolatedStringHandler);

            //ConsoleID, Null
            logger.Log(Arguments.ConsoleID, null);
            AssertResultAndClear(Types.LogTarget, Types.String);

            //ConsoleID, LogCategory
            logger.Log(Arguments.ConsoleID, Arguments.LogCategory);
            AssertResultAndClear(Types.LogTarget, Types.Object);

            //LogID, LogID
            logger.Log(Arguments.ConsoleID, Arguments.LogID);
            AssertResultAndClear(Types.LogTarget, Types.Object);

            //ConsoleID, Color (ambiguous)
            //logger.Log(Arguments.ConsoleID, Arguments.Color);

            //ConsoleID, ConsoleColor (ambiguous)
            //logger.Log(Arguments.ConsoleID, Arguments.ConsoleColor);

            logTargetGroup.Add(activeCase = new TestCase("ILogTarget arguments"));

            //ILogTarget, String
            logger.Log(Arguments.LogTarget, Arguments.String);
            AssertResultAndClear(Types.LogTarget, Types.String);

            //ILogTarget, Object
            logger.Log(Arguments.LogTarget, Arguments.Object);
            AssertResultAndClear(Types.LogTarget, Types.Object);

            //ILogTarget, Interpolated string
            logger.Log(Arguments.LogTarget, $"{Arguments.Object}");
            AssertResultAndClear(Types.LogTarget, Types.InterpolatedStringHandler);

            //ILogTarget, Null
            logger.Log(Arguments.LogTarget, null);
            AssertResultAndClear(Types.LogTarget, Types.String);

            //ILogTarget, LogCategory
            logger.Log(Arguments.LogTarget, Arguments.LogCategory);
            AssertResultAndClear(Types.LogTarget, Types.Object);

            //ILogTarget, LogID
            logger.Log(Arguments.LogTarget, Arguments.LogID);
            AssertResultAndClear(Types.LogTarget, Types.Object);

            //ILogTarget, Color (ambiguous)
            //logger.Log(Arguments.LogTarget, Arguments.Color);

            //ILogTarget, ConsoleColor (ambiguous)
            //logger.Log(Arguments.LogTarget, Arguments.ConsoleColor);

            activeCase = testGroup; //Subgroup has ended, reassign parent group
            #endregion
            #region Color tests

            testGroup.Add(activeCase = new TestCase("Test: Color arguments"));

            //Color, Color
            logger.Log(Arguments.Color, Arguments.Color);
            AssertResultAndClear(Types.Object, Types.Color); //Color overload takes priority over object overload

            //Color, ConsoleColor
            logger.Log(Arguments.Color, Arguments.ConsoleColor);
            AssertResultAndClear(Types.Object, Types.ConsoleColor); //Color overload takes priority over object overload
            #endregion
            #region ConsoleColor tests

            testGroup.Add(activeCase = new TestCase("Test: ConsoleColor arguments"));

            //ConsoleColor, Color
            logger.Log(Arguments.ConsoleColor, Arguments.Color);
            AssertResultAndClear(Types.Object, Types.Color); //Color overload takes priority over object overload

            //ConsoleColor, ConsoleColor
            logger.Log(Arguments.ConsoleColor, Arguments.ConsoleColor);
            AssertResultAndClear(Types.Object, Types.ConsoleColor); //Color overload takes priority over object overload
            #endregion
        }

        internal void AssertResultAndClear(params Type[] expectedTypes)
        {
            if (methodCalled == null) //Fail and return instead of throwing a null reference
            {
                UtilityLogger.Log("Expected types missing test: " + string.Join<Type>(" ,", expectedTypes));
                activeCase.AssertThat(methodCalled).IsNotNull(); //This should not happen, and can be removed when tests are complete
                return;
            }

            var parameters = methodCalled.GetParameters();

            activeCase.AssertThat(parameters.Length).IsEqualTo(expectedTypes.Length); //Parameters must be exactly equal

            int checkCount = Math.Min(parameters.Length, expectedTypes.Length);
            for (int i = 0; i < checkCount; i++)
            {
                UtilityLogger.Log($"Arg {i} actual type: " + parameters[i].ParameterType);
                UtilityLogger.Log($"Arg {i} expected type: " + expectedTypes[i]);

                var condition = activeCase.AssertThat(parameters[i].ParameterType).IsEqualTo(expectedTypes[i]); //Each type must match in the exact same order

                UtilityLogger.Log("Condition passed: " + condition.Passed);
            }

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

                if (firstParam == Types.String)
                {
                    if (tryCreateHook<string>(method, parameters, out Hook hook))
                        testHooks.Add(hook);
                }
                else if (firstParam == Types.InterpolatedStringHandler)
                {
                    if (tryCreateHook<InterpolatedStringHandler>(method, parameters, out Hook hook))
                        testHooks.Add(hook);
                }
                else if (firstParam == Types.Object)
                {
                    if (tryCreateHook<string>(method, parameters, out Hook hook))
                        testHooks.Add(hook);
                }
                else if (firstParam == Types.LogCategory)
                {
                    if (tryCreateHook<LogCategory>(method, parameters, out Hook hook))
                        testHooks.Add(hook);
                }
                else if (firstParam == Types.LogTarget)
                {
                    if (tryCreateHook<ILogTarget>(method, parameters, out Hook hook))
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

                    if (secondParam == Types.String)
                    {
                        hook = createHookWithTwoArguments<TFirst, string>(method);
                    }
                    else if (secondParam == Types.InterpolatedStringHandler)
                    {
                        hook = createHookWithTwoArguments<TFirst, InterpolatedStringHandler>(method);
                    }
                    else if (secondParam == Types.Object)
                    {
                        hook = createHookWithTwoArguments<TFirst, object>(method);
                    }
                    else if (secondParam == Types.ObjectArray)
                    {
                        hook = createHookWithTwoArguments<TFirst, object[]>(method);
                    }
                    else if (secondParam == Types.LogTarget)
                    {
                        hook = createHookWithTwoArguments<TFirst, ILogTarget>(method);
                    }
                    else if (secondParam == Types.Color)
                    {
                        hook = createHookWithTwoArguments<TFirst, Color>(method);
                    }
                    else if (secondParam == Types.ConsoleColor)
                    {
                        hook = createHookWithTwoArguments<TFirst, ConsoleColor>(method);
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

        private static class Arguments
        {
            public readonly static Color Color = Color.red;
            public readonly static ConsoleColor ConsoleColor = ConsoleColor.Red;
            public readonly static LogCategory LogCategory = LogCategory.Default;
            public readonly static LogID LogID = LogID.BepInEx;
            public readonly static ConsoleID ConsoleID = ConsoleID.BepInEx;
            public readonly static ILogTarget LogTarget = LogID.BepInEx;
            public readonly static object Object = new object();
            public readonly static object[] ObjectArray = [];
            public readonly static string String = "test";
        }

        private static class Types
        {
            public readonly static Type Color = typeof(Color);
            public readonly static Type ConsoleColor = typeof(ConsoleColor);
            public readonly static Type InterpolatedStringHandler = typeof(InterpolatedStringHandler);
            public readonly static Type LogCategory = typeof(LogCategory);
            public readonly static Type LogTarget = typeof(ILogTarget);
            public readonly static Type Object = typeof(object);
            public readonly static Type ObjectArray = typeof(object[]);
            public readonly static Type String = typeof(string);
        }
    }
}
