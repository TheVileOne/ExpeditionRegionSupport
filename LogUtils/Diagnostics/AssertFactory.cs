using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils.Diagnostics
{
    public static partial class Assert
    {
        #region BooleanAssert
        /// <summary>
        /// Creates an assert structure designed to evaluate a boolean input type
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <returns>The assert structure is returned</returns>
        public static BooleanAssert Make(bool condition) => new BooleanAssert(condition, assertArgs: default);

        /// <summary>
        /// Creates an assert structure designed to evaluate a boolean input type
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="assertArgs">A consolidated group of arguments</param>
        /// <returns>The assert structure is returned</returns>
        public static BooleanAssert Make(bool condition, AssertArgs assertArgs) => new BooleanAssert(condition, assertArgs);

        /// <summary>
        /// Creates an assert structure designed to evaluate a boolean input type
        /// </summary>
        /// <param name="condition">The condition to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        /// <returns>The assert structure is returned</returns>
        public static BooleanAssert Make(bool condition, IConditionHandler handler) => new BooleanAssert(condition, new AssertArgs(handler));
        #endregion
        #region NumericAssert
        /// <summary>
        /// Creates an assert structure designed to evaluate a numeric input type
        /// </summary>
        /// <param name="value">The input value to evalauate</param>
        /// <returns>The assert structure is returned</returns>
        public static NumericAssert Make(double value) => new NumericAssert(value, assertArgs: default);

        /// <summary>
        /// Creates an assert structure designed to evaluate a numeric input type
        /// </summary>
        /// <param name="value">The input value to evalauate</param>
        /// <param name="assertArgs">A consolidated group of arguments</param>
        /// <returns>The assert structure is returned</returns>
        public static NumericAssert Make(double value, AssertArgs assertArgs) => new NumericAssert(value, assertArgs);

        /// <summary>
        /// Creates an assert structure designed to evaluate a numeric input type
        /// </summary>
        /// <param name="value">The input value to evalauate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        /// <returns>The assert structure is returned</returns>
        public static NumericAssert Make(double value, IConditionHandler handler) => new NumericAssert(value, new AssertArgs(handler));
        #endregion
        #region CollectionAssert
        /// <summary>
        /// Creates an assert structure designed to evaluate an IEnumerable
        /// </summary>
        /// <param name="enumerable">The IEnumerable to evaluate</param>
        /// <returns>The assert structure is returned</returns>
        public static CollectionAssert<T> Make<T>(IEnumerable<T> enumerable) => new CollectionAssert<T>(enumerable, assertArgs: default);

        /// <summary>
        /// Creates an assert structure designed to evaluate an IEnumerable
        /// </summary>
        /// <param name="enumerable">The IEnumerable to evaluate</param>
        /// <param name="assertArgs">A consolidated group of arguments</param>
        /// <returns>The assert structure is returned</returns>
        public static CollectionAssert<T> Make<T>(IEnumerable<T> enumerable, AssertArgs assertArgs) => new CollectionAssert<T>(enumerable, assertArgs);

        /// <summary>
        /// Creates an assert structure designed to evaluate an IEnumerable
        /// </summary>
        /// <param name="enumerable">The IEnumerable to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        /// <returns>The assert structure is returned</returns>
        public static CollectionAssert<T> Make<T>(IEnumerable<T> enumerable, IConditionHandler handler) => new CollectionAssert<T>(enumerable, new AssertArgs(handler));
        #endregion
        #region ObjectAssert
        /// <summary>
        /// Creates an assert structure designed to evaluate an object
        /// </summary>
        /// <param name="data">The object to evaluate</param>
        /// <returns>The assert structure is returned</returns>
        public static ObjectAssert<T> MakeRef<T>(T data) => new ObjectAssert<T>(data, assertArgs: default);

        /// <summary>
        /// Creates an assert structure designed to evaluate an object
        /// </summary>
        /// <param name="data">The object to evaluate</param>
        /// <param name="assertArgs">A consolidated group of arguments</param>
        /// <returns>The assert structure is returned</returns>
        public static ObjectAssert<T> MakeRef<T>(T data, AssertArgs assertArgs) => new ObjectAssert<T>(data, assertArgs);

        /// <summary>
        /// Creates an assert structure designed to evaluate an object
        /// </summary>
        /// <param name="data">The object to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        /// <returns>The assert structure is returned</returns>
        public static ObjectAssert<T> MakeRef<T>(T data, IConditionHandler handler) => new ObjectAssert<T>(data, new AssertArgs(handler));
        #endregion
    }
}
