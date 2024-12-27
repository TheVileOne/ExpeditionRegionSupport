using System;

namespace LogUtils.Diagnostics.Extensions
{
    internal static class AssertHelper
    {
        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        internal static void MustBeGreaterThan<T>(ref Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            int valueDiff = compareValues(condition.Value, compareValue);
            bool conditionPassed = valueDiff > 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.TOO_LOW, "Value"));
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        internal static void MustBeGreaterThanOrEqualTo<T>(ref Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            int valueDiff = compareValues(condition.Value, compareValue);
            bool conditionPassed = valueDiff >= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.TOO_LOW, "Value"));
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        internal static void MustBeLessThan<T>(ref Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            int valueDiff = compareValues(condition.Value, compareValue);
            bool conditionPassed = valueDiff < 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.TOO_HIGH, "Value"));
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        internal static void MustBeLessThanOrEqualTo<T>(ref Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            int valueDiff = compareValues(condition.Value, compareValue);
            bool conditionPassed = valueDiff <= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.TOO_HIGH, "Value"));
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        internal static void MustBeBetween<T>(ref Condition<T> condition, T minimum, T maximum) where T : IComparable<T>
        {
            //Just in case the values are out of order
            if (compareValues(minimum, maximum) > 0)
            {
                T swapValue = minimum;

                minimum = maximum;
                maximum = swapValue;
            }

            bool conditionPassed = compareValues(condition.Value, minimum) > 0
                                && compareValues(condition.Value, maximum) < 0;

            if (conditionPassed)
                condition.Pass();
            else
            {
                string reportDescriptorMin = minimum?.ToString() ?? "NULL";
                string reportDescriptorMax = maximum?.ToString() ?? "NULL";

                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_IN_RANGE, "Value", reportDescriptorMin, reportDescriptorMax));
            }
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        internal static void MustBeZero<T>(ref Condition<T> condition) where T : IComparable<T>, IComparable
        {
            int valueDiff = condition.Value.CompareTo(0);
            bool conditionPassed = valueDiff == 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(valueDiff < 0 ? UtilityConsts.AssertResponse.TOO_LOW : UtilityConsts.AssertResponse.TOO_HIGH, "Value"));
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        internal static void MustNotBeZero<T>(ref Condition<T> condition) where T : IComparable<T>, IComparable
        {
            int valueDiff = condition.Value.CompareTo(0);
            bool conditionPassed = valueDiff != 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_NOT_BE_ZERO, "Value"));
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        internal static void MustBeNegative<T>(ref Condition<T> condition) where T : IComparable<T>, IComparable
        {
            bool conditionPassed = condition.Value.CompareTo(0) < 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_NEGATIVE, "Value"));
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        internal static void MustBePositive<T>(ref Condition<T> condition) where T : IComparable<T>, IComparable
        {
            bool conditionPassed = condition.Value.CompareTo(0) > 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_POSITIVE, "Value"));
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        internal static void MustBePositiveOrZero<T>(ref Condition<T> condition) where T : IComparable<T>, IComparable
        {
            bool conditionPassed = condition.Value.CompareTo(0) >= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_NOT_BE_NEGATIVE, "Value"));
        }

        private static int compareValues<T>(in T val, in T val2) where T : IComparable<T>
        {
            if (val != null)
                return val.CompareTo(val2);

            if (val2 != null)
                return -val2.CompareTo(val);
            return 0; //Both values must be null
        }
    }
}
