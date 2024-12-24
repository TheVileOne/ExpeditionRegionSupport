using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public static class Condition
    {
        public static readonly List<IConditionHandler> AssertHandlers = new List<IConditionHandler>();

        /// <summary>
        /// Asserts that the input value is true. Default handle behavior is to log a message when the asserted value is not true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>true, when the condition is true, otherwise false</returns>
        public static bool Assert(bool condition)
        {
            BooleanAssert assert = new BooleanAssert(condition, assertArgs: default);
            return assert.IsTrue();
        }

        /// <summary>
        /// Asserts that the input value is true. Default handle behavior is to log a message when the asserted value is not true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="assertArgs">A consolidated group of arguments</param>
        /// <returns>true, when the condition is true, otherwise false</returns>
        public static bool Assert(bool condition, AssertArgs assertArgs)
        {
            BooleanAssert assert = new BooleanAssert(condition, assertArgs);
            return assert.IsTrue();
        }

        /// <summary>
        /// Asserts that the input value is true. Default handle behavior is to log a message when the asserted value is not true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        /// <returns>true, when the condition is true, otherwise false</returns>
        public static bool Assert(bool condition, IConditionHandler handler)
        {
            BooleanAssert assert = new BooleanAssert(condition, new AssertArgs(handler));
            return assert.IsTrue();
        }

        private static BooleanAssert assert;

        /// <summary>
        /// Asserts that the input value is false. Default handle behavior is to log a message when the asserted value is true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>true, when the condition is false, otherwise false</returns>
        public static bool AssertFalse(bool condition)
        {
            BooleanAssert assert = new BooleanAssert(condition, assertArgs: default);
            return assert.IsFalse();
        }

        /// <summary>
        /// Asserts that the input value is false. Default handle behavior is to log a message when the asserted value is true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="assertArgs">A consolidated group of arguments</param>
        /// <returns>true, when the condition is false, otherwise false</returns>
        public static bool AssertFalse(bool condition, AssertArgs assertArgs)
        {
            BooleanAssert assert = new BooleanAssert(condition, assertArgs);
            return assert.IsFalse();
        }

        /// <summary>
        /// Asserts that the input value is false. Default handle behavior is to log a message when the asserted value is true
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        /// <returns>true, when the condition is false, otherwise false</returns>
        public static bool AssertFalse(bool condition, IConditionHandler handler)
        {
            BooleanAssert assert = new BooleanAssert(condition, new AssertArgs(handler));
            return assert.IsFalse();
        }
    }
}
