using System;
using System.Collections.Generic;
using System.Linq;
using Message = LogUtils.Diagnostics.ConditionResults.Message;

namespace LogUtils.Diagnostics
{
    public static class Assert
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
        public static ObjectAssert Make(object data) => new ObjectAssert(data, assertArgs: default);

        /// <summary>
        /// Creates an assert structure designed to evaluate an object
        /// </summary>
        /// <param name="data">The object to evaluate</param>
        /// <param name="assertArgs">A consolidated group of arguments</param>
        /// <returns>The assert structure is returned</returns>
        public static ObjectAssert Make(object data, AssertArgs assertArgs) => new ObjectAssert(data, assertArgs);

        /// <summary>
        /// Creates an assert structure designed to evaluate an object
        /// </summary>
        /// <param name="data">The object to evaluate</param>
        /// <param name="handler">The exclusive handler to receive the assert result</param>
        /// <returns>The assert structure is returned</returns>
        public static ObjectAssert Make(object data, IConditionHandler handler) => new ObjectAssert(data, new AssertArgs(handler));
        #endregion

        internal static void OnResult(AssertArgs assertArgs, ConditionResults result)
        {
            foreach (var handler in assertArgs.Handlers)
                handler.Handle(assertArgs, result);
        }

        #region Boolean
        public static ConditionResults IsTrue(bool condition)
        {
            bool conditionPassed = condition == true;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_BE_TRUE, "Condition");
            return result;
        }

        public static ConditionResults IsFalse(bool condition)
        {
            bool conditionPassed = condition == false;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_BE_FALSE, "Condition");
            return result;
        }
        #endregion
        #region Objects and Structs
        public static ConditionResults IsEqual(object obj, object obj2)
        {
            bool conditionPassed = object.Equals(obj, obj2);

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_BE_EQUAL, "Objects");
            return result;
        }

        public static ConditionResults DoesNotEqual(object obj, object obj2)
        {
            bool conditionPassed = !object.Equals(obj, obj2);

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_NOT_BE_EQUAL, "Objects");
            return result;
        }

        public static ConditionResults IsEqual(ValueType value, ValueType value2)
        {
            bool conditionPassed = value.Equals(value2);

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_BE_EQUAL, "Value types");
            return result;
        }

        public static ConditionResults DoesNotEqual(ValueType value, ValueType value2)
        {
            bool conditionPassed = !value.Equals(value2);

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_NOT_BE_EQUAL, "Value types");
            return result;
        }

        public static ConditionResults IsNull(object obj)
        {
            bool conditionPassed = obj == null;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_BE_NULL, "Object");
            return result;
        }

        public static ConditionResults IsNotNull(object obj)
        {
            bool conditionPassed = obj != null;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_NOT_BE_NULL, "Object");
            return result;
        }
        #endregion
        #region Collections
        public static ConditionResults IsNullOrEmpty<T>(IEnumerable<T> collection)
        {
            bool conditionPassed = collection == null || !collection.Any();

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_BE_EMPTY, "Collection");
            return result;
        }

        public static ConditionResults HasItems<T>(IEnumerable<T> collection)
        {
            bool conditionPassed = collection != null && collection.Any();
            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_HAVE_ITEMS, "Collection");
            return result;
        }
        #endregion
        #region Numerics
        public static ConditionResults IsEqual(double value, double value2)
        {
            int valueDiff = value.CompareTo(value2);

            bool conditionPassed = valueDiff == 0;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(valueDiff < 0 ? UtilityConsts.AssertResponse.TOO_LOW : UtilityConsts.AssertResponse.TOO_HIGH, "Value");
            return result;
        }

        public static ConditionResults IsGreaterThan(double value, double value2)
        {
            bool conditionPassed = value > value2;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.TOO_LOW, "Value");
            return result;
        }

        public static ConditionResults IsGreaterThanOrEqualTo(double value, double value2)
        {
            bool conditionPassed = value >= value2;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.TOO_LOW, "Value");
            return result;
        }

        public static ConditionResults IsLessThan(double value, double value2)
        {
            bool conditionPassed = value < value2;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.TOO_HIGH, "Value");
            return result;
        }

        public static ConditionResults IsLessThanOrEqualTo(double value, double value2)
        {
            bool conditionPassed = value >= value2;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.TOO_HIGH, "Value");
            return result;
        }

        public static ConditionResults IsBetween(double value, double minimum, double maximum)
        {
            //Just in case the values are out of order
            if (minimum > maximum)
            {
                double swapValue = minimum;

                minimum = maximum;
                maximum = swapValue;
            }

            bool conditionPassed = value > minimum && value < maximum;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_BE_IN_RANGE, "Value", minimum.ToString(), maximum.ToString());
            return result;
        }

        public static ConditionResults IsZero(double value)
        {
            int valueDiff = value.CompareTo(0);
            bool conditionPassed = valueDiff == 0;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(valueDiff < 0 ? UtilityConsts.AssertResponse.TOO_LOW : UtilityConsts.AssertResponse.TOO_HIGH, "Value");
            return result;
        }

        public static ConditionResults IsNotZero(double value)
        {
            int valueDiff = value.CompareTo(0);

            bool conditionPassed = valueDiff != 0;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_NOT_BE_ZERO, "Value");
            return result;
        }

        public static ConditionResults IsNegative(double value)
        {
            bool conditionPassed = value < 0;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_BE_NEGATIVE, "Value");
            return result;
        }

        public static ConditionResults IsPositive(double value)
        {
            bool conditionPassed = value > 0;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_BE_POSITIVE, "Value");
            return result;
        }

        public static ConditionResults IsPositiveOrZero(double value)
        {
            bool conditionPassed = value >= 0;

            if (conditionPassed)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            result.Response = new Message(UtilityConsts.AssertResponse.MUST_NOT_BE_NEGATIVE, "Value");
            return result;
        }

        /// <summary>
        /// Asserts a condition by invoking a delegate using specified values as arguments
        /// </summary>
        /// <param name="conditionArg">Condition argument for delegate</param>
        /// <param name="condition">Delegate that evaluates a condition</param>
        /// <param name="criteria">The expected state of the condition</param>
        public static ConditionResults EvaluateCondition(double conditionArg, Func<double, bool> condition, EvaluationCriteria criteria)
        {
            bool conditionIsTrue = condition.Invoke(conditionArg);

            conditionIsTrue =
                   (criteria == EvaluationCriteria.MustBeTrue && conditionIsTrue)
                || (criteria == EvaluationCriteria.MustBeFalse && !conditionIsTrue);

            if (conditionIsTrue)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            string failMessage = null;
            if (criteria == EvaluationCriteria.MustBeTrue)
                failMessage = UtilityConsts.AssertResponse.MUST_BE_TRUE;
            else if (criteria == EvaluationCriteria.MustBeFalse)
                failMessage = UtilityConsts.AssertResponse.MUST_BE_FALSE;

            result.Response = new Message(failMessage, "Condition");
            return result;
        }

        /// <summary>
        /// Asserts a condition by invoking a delegate using specified values as arguments
        /// </summary>
        /// <param name="firstArg">First condition argument</param>
        /// <param name="secondArg">Second condition argument</param>
        /// <param name="condition">Delegate that evaluates a condition</param>
        /// <param name="criteria">The expected state of the condition</param>
        public static ConditionResults EvaluateCondition<T>(T firstArg, T secondArg, Func<T, T, bool> condition, EvaluationCriteria criteria)
        {
            return processCondition(condition.Invoke(firstArg, secondArg), criteria);
        }
        #endregion

        /// <summary>
        /// Asserts a condition by dynamically invoking a delegate
        /// </summary>
        /// <param name="dynamicCondition">Delegate that evaluates a condition (must return a Boolean)</param>
        /// <param name="criteria">The expected state of the condition</param>
        /// <param name="dynamicParams">Parameters for evaluating a condition</param>
        /// <exception cref="MemberAccessException">
        ///    The caller does not have access to the method represented by the delegate (for
        ///    example, if the method is private). -or- The number, order, or type of parameters
        ///    listed in args is invalid.</exception>
        /// <exception cref="ArgumentException">
        ///     The method represented by the delegate is invoked on an object or a class that
        ///     does not support it.</exception>
        /// <exception cref="System.Reflection.TargetInvocationException">
        ///     The method represented by the delegate is an instance method and the target object
        ///     is null. -or- One of the encapsulated methods throws an exception.</exception>
        public static ConditionResults EvaluateCondition(Delegate dynamicCondition, EvaluationCriteria criteria, params object[] dynamicParams)
        {
            return processCondition((bool)dynamicCondition.DynamicInvoke(dynamicParams), criteria);
        }

        private static ConditionResults processCondition(bool conditionIsTrue, EvaluationCriteria criteria)
        {
            conditionIsTrue =
                  (criteria == EvaluationCriteria.MustBeTrue && conditionIsTrue)
               || (criteria == EvaluationCriteria.MustBeFalse && !conditionIsTrue);

            if (conditionIsTrue)
                return ConditionResults.Pass;

            var result = ConditionResults.Fail;

            string failMessage = null;
            if (criteria == EvaluationCriteria.MustBeTrue)
                failMessage = UtilityConsts.AssertResponse.MUST_BE_TRUE;
            else if (criteria == EvaluationCriteria.MustBeFalse)
                failMessage = UtilityConsts.AssertResponse.MUST_BE_FALSE;

            result.Response = new Message(failMessage, "Condition");
            return result;
        }
    }
}
