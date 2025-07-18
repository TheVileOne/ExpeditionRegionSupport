﻿using System;
using System.Collections.Generic;
using System.Linq;
using AssertDocs = LogUtils.Documentation.AssertDocumentation;
using AssertResponse = LogUtils.UtilityConsts.AssertResponse;

namespace LogUtils.Diagnostics.Extensions
{
    internal static class AssertHelper
    {
        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        internal static void MustBeGreaterThan<T>(ref Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            int valueDiff = compareValues(condition.Value, compareValue);
            bool conditionPassed = valueDiff > 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_LOW, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        internal static void MustBeGreaterThanOrEqualTo<T>(ref Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            int valueDiff = compareValues(condition.Value, compareValue);
            bool conditionPassed = valueDiff >= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_LOW, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        internal static void MustBeLessThan<T>(ref Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            int valueDiff = compareValues(condition.Value, compareValue);
            bool conditionPassed = valueDiff < 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_HIGH, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        internal static void MustBeLessThanOrEqualTo<T>(ref Condition<T> condition, T compareValue) where T : IComparable<T>
        {
            int valueDiff = compareValues(condition.Value, compareValue);
            bool conditionPassed = valueDiff <= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_HIGH, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
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

                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_IN_RANGE, "Value", reportDescriptorMin, reportDescriptorMax));
            }
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        internal static void MustBeZero<T>(ref Condition<T> condition) where T : IComparable<T>, IComparable
        {
            int valueDiff = compareToZero(condition.Value);
            bool conditionPassed = valueDiff == 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(valueDiff < 0 ? AssertResponse.TOO_LOW : AssertResponse.TOO_HIGH, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        internal static void MustNotBeZero<T>(ref Condition<T> condition) where T : IComparable<T>, IComparable
        {
            int valueDiff = compareToZero(condition.Value);
            bool conditionPassed = valueDiff != 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_ZERO, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        internal static void MustBeNegative<T>(ref Condition<T> condition) where T : IComparable<T>, IComparable
        {
            bool conditionPassed = compareToZero(condition.Value) < 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_NEGATIVE, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        internal static void MustBePositive<T>(ref Condition<T> condition) where T : IComparable<T>, IComparable
        {
            bool conditionPassed = compareToZero(condition.Value) > 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_POSITIVE, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        internal static void MustBePositiveOrZero<T>(ref Condition<T> condition) where T : IComparable
        {
            bool conditionPassed = compareToZero(condition.Value) >= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NEGATIVE, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        internal static void MustBeGreaterThan<T>(ref Condition<T> condition, T? compareValue) where T : struct, IComparable<T>
        {
            if (compareValue == null)
            {
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NULL, "Value"));
                return;
            }

            int valueDiff = compareValues(condition.Value, compareValue.Value);
            bool conditionPassed = valueDiff > 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_LOW, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        internal static void MustBeGreaterThanOrEqualTo<T>(ref Condition<T> condition, T? compareValue) where T : struct, IComparable<T>
        {
            if (compareValue == null)
            {
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NULL, "Value"));
                return;
            }

            int valueDiff = compareValues(condition.Value, compareValue.Value);
            bool conditionPassed = valueDiff >= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_LOW, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        internal static void MustBeLessThan<T>(ref Condition<T> condition, T? compareValue) where T : struct, IComparable<T>
        {
            if (compareValue == null)
            {
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NULL, "Value"));
                return;
            }

            int valueDiff = compareValues(condition.Value, compareValue.Value);
            bool conditionPassed = valueDiff < 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_HIGH, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        internal static void MustBeLessThanOrEqualTo<T>(ref Condition<T> condition, T? compareValue) where T : struct, IComparable<T>
        {
            if (compareValue == null)
            {
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NULL, "Value"));
                return;
            }

            int valueDiff = compareValues(condition.Value, compareValue.Value);
            bool conditionPassed = valueDiff <= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_HIGH, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        internal static void MustBeGreaterThan<T>(ref Condition<T?> condition, T? compareValue) where T : struct, IComparable<T>
        {
            if (condition.Value == null || compareValue == null)
            {
                condition.Fail(new Condition.Message(AssertResponse.NO_COMPARISON, "Nullable values"));
                return;
            }

            int valueDiff = compareValues(condition.Value, compareValue.Value);
            bool conditionPassed = valueDiff > 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_LOW, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        internal static void MustBeGreaterThanOrEqualTo<T>(ref Condition<T?> condition, T? compareValue) where T : struct, IComparable<T>
        {
            if (condition.Value == null || compareValue == null)
            {
                //Two null values passes this assertion
                if (!condition.Value.HasValue && !compareValue.HasValue)
                    condition.Pass();
                else
                    condition.Fail(new Condition.Message(AssertResponse.NO_COMPARISON, "Nullable values"));
                return;
            }

            int valueDiff = compareValues(condition.Value, compareValue.Value);
            bool conditionPassed = valueDiff >= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_LOW, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        internal static void MustBeLessThan<T>(ref Condition<T?> condition, T? compareValue) where T : struct, IComparable<T>
        {
            if (condition.Value == null || compareValue == null)
            {
                condition.Fail(new Condition.Message(AssertResponse.NO_COMPARISON, "Nullable values"));
                return;
            }

            int valueDiff = compareValues(condition.Value, compareValue.Value);
            bool conditionPassed = valueDiff < 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_HIGH, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        internal static void MustBeLessThanOrEqualTo<T>(ref Condition<T?> condition, T? compareValue) where T : struct, IComparable<T>
        {
            if (condition.Value == null || compareValue == null)
            {
                //Two null values passes this assertion
                if (!condition.Value.HasValue && !compareValue.HasValue)
                    condition.Pass();
                else
                    condition.Fail(new Condition.Message(AssertResponse.NO_COMPARISON, "Nullable values"));
                return;
            }

            int valueDiff = compareValues(condition.Value, compareValue.Value);
            bool conditionPassed = valueDiff <= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.TOO_HIGH, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        internal static void MustBeBetween<T>(ref Condition<T?> condition, T minimum, T maximum) where T : struct, IComparable<T>
        {
            if (condition.Value == null)
            {
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NULL, "Value"));
                return;
            }

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
                string reportDescriptorMin = minimum.ToString() ?? "NULL";
                string reportDescriptorMax = maximum.ToString() ?? "NULL";

                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_IN_RANGE, "Value", reportDescriptorMin, reportDescriptorMax));
            }
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        internal static void MustBeZero<T>(ref Condition<T?> condition) where T : struct, IComparable
        {
            if (condition.Value == null) //Null is not the same as zero in this context
            {
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NULL, "Value"));
                return;
            }

            int valueDiff = compareToZero(condition.Value);
            bool conditionPassed = valueDiff == 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(valueDiff < 0 ? AssertResponse.TOO_LOW : AssertResponse.TOO_HIGH, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        internal static void MustNotBeZero<T>(ref Condition<T?> condition) where T : struct, IComparable
        {
            if (condition.Value == null)
            {
                condition.Pass(); //Null is not the same as zero in this context
                return;
            }

            int valueDiff = compareToZero(condition.Value);
            bool conditionPassed = valueDiff != 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_ZERO, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        internal static void MustBeNegative<T>(ref Condition<T?> condition) where T : struct, IComparable
        {
            if (condition.Value == null)
            {
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NULL, "Value"));
                return;
            }

            bool conditionPassed = compareToZero(condition.Value) < 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_NEGATIVE, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        internal static void MustBePositive<T>(ref Condition<T?> condition) where T : struct, IComparable
        {
            if (condition.Value == null)
            {
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NULL, "Value"));
                return;
            }

            bool conditionPassed = compareToZero(condition.Value) > 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_POSITIVE, "Value"));
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        internal static void MustBePositiveOrZero<T>(ref Condition<T?> condition) where T : struct, IComparable
        {
            if (condition.Value == null)
            {
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NULL, "Value"));
                return;
            }

            bool conditionPassed = compareToZero(condition.Value) >= 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_NOT_BE_NEGATIVE, "Value"));
        }

        internal static void MustContainItems<T, U>(ref Condition<T> condition) where T : IEnumerable<U>
        {
            bool conditionPassed = condition.Value != null && condition.Value.Any();

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_HAVE_ITEMS, "Collection"));
        }

        internal static void MustNotContainItems<T, U>(ref Condition<T> condition) where T : IEnumerable<U>
        {
            bool conditionPassed = condition.Value == null || !condition.Value.Any();

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(AssertResponse.MUST_BE_EMPTY, "Collection"));
        }

        private static int compareValues<T>(in T val, in T val2) where T : IComparable<T>
        {
            if (val != null)
                return val.CompareTo(val2);

            if (val2 != null)
                return -val2.CompareTo(val);
            return 0; //Both values must be null
        }

        private static int compareValues<T>(in T? val, in T val2) where T : struct, IComparable<T>
        {
            return compareValues(val.Value, val2);
        }

        private static int compareToZero<T>(in T val) where T : IComparable
        {
            return val.CompareTo(0);
        }

        private static int compareToZero<T>(in T? nullable) where T : struct, IComparable
        {
            return compareToZero(nullable.Value);
        }
    }
}
