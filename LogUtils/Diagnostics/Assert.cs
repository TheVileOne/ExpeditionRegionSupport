using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics
{
    public static partial class Assert
    {
        #region Boolean
        /// <summary>
        /// Asserts that target value must be true
        /// </summary>
        public static Condition<bool> IsTrue(this Condition<bool> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value == true;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_TRUE, "Condition"));
            return condition;
        }

        /// <summary>
        /// Asserts that target value must be false
        /// </summary>
        public static Condition<bool> IsFalse(this Condition<bool> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value == false;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_FALSE, "Condition"));
            return condition;
        }
        #endregion
        #region Objects and Structs

        /// <summary>
        /// Asserts that the target value must be equal to a specified value
        /// </summary>
        /// <param name="compareObject">The value to compare to</param>
        public static Condition<T> IsEqual<T>(this Condition<T> condition, T compareObject)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = Equals(condition.Value, compareObject);

            if (conditionPassed)
                condition.Pass();
            else
            {
                string reportDescriptor = "Objects";

                if (typeof(T).IsValueType)
                    reportDescriptor = "Values";

                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_EQUAL, reportDescriptor));
            }
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be not equal to a specified value
        /// </summary>
        /// <param name="compareObject">The value to compare to</param>
        public static Condition<T> DoesNotEqual<T>(this Condition<T> condition, T compareObject)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = !Equals(condition.Value, compareObject);

            if (conditionPassed)
                condition.Pass();
            else
            {
                string reportDescriptor = "Objects";

                if (typeof(T).IsValueType)
                    reportDescriptor = "Values";

                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_NOT_BE_EQUAL, reportDescriptor));
            }
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be null
        /// </summary>
        public static Condition<T> IsNull<T>(this Condition<T> condition) where T : class
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value == null;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_NULL, "Object"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be null
        /// </summary>
        public static Condition<T?> IsNull<T>(this Condition<T?> condition) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value == null;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_NULL, "Nullable value"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be null
        /// </summary>
        public static Condition<T> IsNotNull<T>(this Condition<T> condition) where T : class
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value != null;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_NOT_BE_NULL, "Object"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be null
        /// </summary>
        public static Condition<T?> IsNotNull<T>(this Condition<T?> condition) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value != null;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_NOT_BE_NULL, "Nullable value"));
            return condition;
        }
        #endregion
        #region Collections

        /// <summary>
        /// Asserts that the target collection must be null or empty
        /// </summary>
        public static Condition<IEnumerable<T>> IsNullOrEmpty<T>(this Condition<IEnumerable<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value == null || !condition.Value.Any();

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_EMPTY, "Collection"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target collection must have at least one entry
        /// </summary>
        public static Condition<IEnumerable<T>> HasItems<T>(this Condition<IEnumerable<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value != null && condition.Value.Any();

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_HAVE_ITEMS, "Collection"));
            return condition;
        }
        #endregion
        #region Numerics

        /// <summary>
        /// Asserts that the target value must be equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<IComparable<T>> IsEqual<T>(this Condition<IComparable<T>> condition, T compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed;

            //Null values suggest we should do an object comparison rather than a value comparison
            if (condition.Value == null || compareValue == null)
            {
                conditionPassed = Equals(condition.Value, compareValue);

                if (conditionPassed)
                    condition.Pass();
                else
                    condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_EQUAL, "Objects"));
                return condition;
            }

            int valueDiff = condition.Value.CompareTo(compareValue);

            conditionPassed = valueDiff == 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(valueDiff < 0 ? UtilityConsts.AssertResponse.TOO_LOW : UtilityConsts.AssertResponse.TOO_HIGH, "Value"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<T> IsGreaterThan<T>(this Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            if (!condition.ShouldProcess)
                return condition;

            int valueDiff = Comparer<T>.Default.Compare(condition.Value, compareValue);

            bool conditionPassed = valueDiff > 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.TOO_LOW, "Value"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<T> IsGreaterThanOrEqualTo<T>(this Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            if (!condition.ShouldProcess)
                return condition;

            int valueDiff = Comparer<T>.Default.Compare(condition.Value, compareValue);

            bool conditionPassed = valueDiff >= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.TOO_LOW, "Value"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<T> IsLessThan<T>(this Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            if (!condition.ShouldProcess)
                return condition;

            int valueDiff = Comparer<T>.Default.Compare(condition.Value, compareValue);

            bool conditionPassed = valueDiff < 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.TOO_HIGH, "Value"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<T> IsLessThanOrEqualTo<T>(this Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            if (!condition.ShouldProcess)
                return condition;

            int valueDiff = Comparer<T>.Default.Compare(condition.Value, compareValue);

            bool conditionPassed = valueDiff <= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.TOO_HIGH, "Value"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<T> IsBetween<T>(this Condition<T> condition, T minimum, T maximum) where T : IComparable<T>
        {
            if (!condition.ShouldProcess)
                return condition;

            var comparer = Comparer<T>.Default;

            //Just in case the values are out of order
            if (comparer.Compare(minimum, maximum) > 0)
            {
                T swapValue = minimum;

                minimum = maximum;
                maximum = swapValue;
            }

            bool conditionPassed = comparer.Compare(condition.Value, minimum) > 0
                                && comparer.Compare(condition.Value, maximum) < 0;

            if (conditionPassed)
                condition.Pass();
            else
            {
                string reportDescriptorMin = minimum?.ToString() ?? "NULL";
                string reportDescriptorMax = maximum?.ToString() ?? "NULL";

                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_IN_RANGE, "Value", reportDescriptorMin, reportDescriptorMax));
            }
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<double> IsZero(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            int valueDiff = condition.Value.CompareTo(0);
            bool conditionPassed = valueDiff == 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(valueDiff < 0 ? UtilityConsts.AssertResponse.TOO_LOW : UtilityConsts.AssertResponse.TOO_HIGH, "Value"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<double> IsNotZero(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            int valueDiff = condition.Value.CompareTo(0);
            bool conditionPassed = valueDiff != 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_NOT_BE_ZERO, "Value"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<double> IsNegative(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value < 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_NEGATIVE, "Value"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<double> IsPositive(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value > 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_POSITIVE, "Value"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<double> IsPositiveOrZero(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value >= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_NOT_BE_NEGATIVE, "Value"));
            return condition;
        }

        /// <summary>
        /// Asserts a condition by invoking a delegate using specified values as arguments
        /// </summary>
        /// <param name="conditionArg">Condition argument for delegate</param>
        /// <param name="condition">Delegate that evaluates a condition</param>
        /// <param name="criteria">The expected state of the condition</param>
        public static Condition<T> EvaluateCondition<T>(this Condition<T> condition, T conditionArg, Func<T, bool> conditionDelegate, EvaluationCriteria criteria)
        {
            if (condition.ShouldProcess)
                processCondition(ref condition, conditionDelegate.Invoke(conditionArg), criteria);
            return condition;
        }

        /// <summary>
        /// Asserts a condition by invoking a delegate using specified values as arguments
        /// </summary>
        /// <param name="firstArg">First condition argument</param>
        /// <param name="secondArg">Second condition argument</param>
        /// <param name="condition">Delegate that evaluates a condition</param>
        /// <param name="criteria">The expected state of the condition</param>
        public static Condition<T> EvaluateCondition<T>(this Condition<T> condition, T firstArg, T secondArg, Func<T, T, bool> conditionDelegate, EvaluationCriteria criteria)
        {
            if (condition.ShouldProcess)
                processCondition(ref condition, conditionDelegate.Invoke(firstArg, secondArg), criteria);
            return condition;
        }

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
        public static Condition<T> EvaluateCondition<T>(this Condition<T> condition, Delegate dynamicCondition, EvaluationCriteria criteria, params object[] dynamicParams)
        {
            if (condition.ShouldProcess)
                processCondition(ref condition, (bool)dynamicCondition.DynamicInvoke(dynamicParams), criteria);
            return condition;
        }

        private static void processCondition<T>(ref Condition<T> condition, bool conditionIsTrue, EvaluationCriteria criteria)
        {
            conditionIsTrue =
                  (criteria == EvaluationCriteria.MustBeTrue && conditionIsTrue)
               || (criteria == EvaluationCriteria.MustBeFalse && !conditionIsTrue);

            if (conditionIsTrue)
                condition.Pass();
            else
            {
                string failMessage = null;
                if (criteria == EvaluationCriteria.MustBeTrue)
                    failMessage = UtilityConsts.AssertResponse.MUST_BE_TRUE;
                else if (criteria == EvaluationCriteria.MustBeFalse)
                    failMessage = UtilityConsts.AssertResponse.MUST_BE_FALSE;

                condition.Fail(new Condition.Message(failMessage, "Condition"));
            }
        }
        #endregion

        public enum EvaluationCriteria
        {
            MustBeTrue,
            MustBeFalse
        }
    }
}
