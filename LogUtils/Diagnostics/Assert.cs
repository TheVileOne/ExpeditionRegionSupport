using LogUtils.Diagnostics.Extensions;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Asserts that target value must be true
        /// </summary>
        public static Condition<bool?> IsTrue(this Condition<bool?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = condition.Value.HasValue && condition.Value == true;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_TRUE, "Condition"));
            return condition;
        }

        /// <summary>
        /// Asserts that target value must be false
        /// </summary>
        public static Condition<bool?> IsFalse(this Condition<bool?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            //Null is considered different than false
            bool conditionPassed = condition.Value.HasValue && condition.Value == false;

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
        public static Condition<T> IsEqualTo<T>(this Condition<T> condition, T compareObject)
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
        /// Asserts that the target value must be equal to a specified value
        /// </summary>
        /// <param name="compareObject">The value to compare to</param>
        public static Condition<T> IsEqualTo<T>(this Condition<T> condition, T? compareObject) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = compareObject != null && Equals(condition.Value, compareObject.Value);

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_EQUAL, "Values"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to a specified value
        /// </summary>
        /// <param name="compareObject">The value to compare to</param>
        public static Condition<T?> IsEqualTo<T>(this Condition<T?> condition, T? compareObject) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed;
            if (condition.Value == null || compareObject == null)
                conditionPassed = condition.Value.HasValue == compareObject.HasValue;
            else
            {
                T value = condition.Value.Value;
                conditionPassed = Equals(value, compareObject.Value);
            }

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_EQUAL, "Values"));
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
        /// Asserts that the target value must be not equal to a specified value
        /// </summary>
        /// <param name="compareObject">The value to compare to</param>
        public static Condition<T> DoesNotEqual<T>(this Condition<T> condition, T? compareObject) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed = compareObject == null || !Equals(condition.Value, compareObject.Value);

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_NOT_BE_EQUAL, "Values"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be not equal to a specified value
        /// </summary>
        /// <param name="compareObject">The value to compare to</param>
        public static Condition<T?> DoesNotEqual<T>(this Condition<T?> condition, T? compareObject) where T : struct
        {
            if (!condition.ShouldProcess)
                return condition;

            bool conditionPassed;
            if (condition.Value == null || compareObject == null)
            {
                conditionPassed = !condition.Value.HasValue != compareObject.HasValue;
            }
            else
            {
                T value = condition.Value.Value;
                conditionPassed = !Equals(value, compareObject.Value);
            }

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_NOT_BE_EQUAL, "Values"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target object refers to the same object as a specified object
        /// </summary>
        /// <param name="compareObject">The object to compare to</param>
        public static Condition<T> IsSameInstance<T>(this Condition<T> condition, T compareObject) where T : class
        {
            bool conditionPassed = ReferenceEquals(condition, compareObject);

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_BE_SAME_INSTANCE, "Object"));
            return condition;
        }

        /// <summary>
        /// Asserts that the target object refers to a different object than a specified object
        /// </summary>
        /// <param name="compareObject">The object to compare to</param>
        public static Condition<T> IsNotThisInstance<T>(this Condition<T> condition, T compareObject) where T : class
        {
            bool conditionPassed = ReferenceEquals(condition, compareObject);

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(UtilityConsts.AssertResponse.MUST_NOT_BE_SAME_INSTANCE, "Object"));
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

            AssertHelper.MustNotContainItems<IEnumerable<T>, T>(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target collection must have at least one entry
        /// </summary>
        public static Condition<IEnumerable<T>> HasItems<T>(this Condition<IEnumerable<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustContainItems<IEnumerable<T>, T>(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target collection must be null or empty
        /// </summary>
        public static Condition<ICollection<T>> IsNullOrEmpty<T>(this Condition<ICollection<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotContainItems<ICollection<T>, T>(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target collection must have at least one entry
        /// </summary>
        public static Condition<ICollection<T>> HasItems<T>(this Condition<ICollection<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustContainItems<ICollection<T>, T>(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target collection must be null or empty
        /// </summary>
        public static Condition<IList<T>> IsNullOrEmpty<T>(this Condition<IList<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotContainItems<IList<T>, T>(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target collection must have at least one entry
        /// </summary>
        public static Condition<IList<T>> HasItems<T>(this Condition<IList<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustContainItems<IList<T>, T>(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target collection must be null or empty
        /// </summary>
        public static Condition<List<T>> IsNullOrEmpty<T>(this Condition<List<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotContainItems<List<T>, T>(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target collection must have at least one entry
        /// </summary>
        public static Condition<List<T>> HasItems<T>(this Condition<List<T>> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustContainItems<List<T>, T>(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target collection must be null or empty
        /// </summary>
        public static Condition<T[]> IsNullOrEmpty<T>(this Condition<T[]> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotContainItems<T[], T>(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target collection must have at least one entry
        /// </summary>
        public static Condition<T[]> HasItems<T>(this Condition<T[]> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustContainItems<T[], T>(ref condition);
            return condition;
        }
        #endregion
        #region Numerics

        /// <summary>
        /// Asserts that the target value must be equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<IComparable<T>> IsEqualTo<T>(this Condition<IComparable<T>> condition, T compareValue)
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

        #region Non-nullables
        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte> IsGreaterThan(this Condition<sbyte> condition, sbyte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte> IsGreaterThanOrEqualTo(this Condition<sbyte> condition, sbyte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte> IsLessThan(this Condition<sbyte> condition, sbyte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte> IsLessThanOrEqualTo(this Condition<sbyte> condition, sbyte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<sbyte> IsBetween(this Condition<sbyte> condition, sbyte minimum, sbyte maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<sbyte> IsZero(this Condition<sbyte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<sbyte> IsNotZero(this Condition<sbyte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<sbyte> IsNegative(this Condition<sbyte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<sbyte> IsPositive(this Condition<sbyte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<sbyte> IsPositiveOrZero(this Condition<sbyte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte> IsGreaterThan(this Condition<byte> condition, byte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte> IsGreaterThanOrEqualTo(this Condition<byte> condition, byte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte> IsLessThan(this Condition<byte> condition, byte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte> IsLessThanOrEqualTo(this Condition<byte> condition, byte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<byte> IsBetween(this Condition<byte> condition, byte minimum, byte maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<byte> IsZero(this Condition<byte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<byte> IsNotZero(this Condition<byte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<byte> IsNegative(this Condition<byte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<byte> IsPositive(this Condition<byte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<byte> IsPositiveOrZero(this Condition<byte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short> IsGreaterThan(this Condition<short> condition, short compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short> IsGreaterThanOrEqualTo(this Condition<short> condition, short compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short> IsLessThan(this Condition<short> condition, short compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short> IsLessThanOrEqualTo(this Condition<short> condition, short compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<short> IsBetween(this Condition<short> condition, short minimum, short maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<short> IsZero(this Condition<short> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<short> IsNotZero(this Condition<short> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<short> IsNegative(this Condition<short> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<short> IsPositive(this Condition<short> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<short> IsPositiveOrZero(this Condition<short> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort> IsGreaterThan(this Condition<ushort> condition, ushort compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort> IsGreaterThanOrEqualTo(this Condition<ushort> condition, ushort compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort> IsLessThan(this Condition<ushort> condition, ushort compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort> IsLessThanOrEqualTo(this Condition<ushort> condition, ushort compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<ushort> IsBetween(this Condition<ushort> condition, ushort minimum, ushort maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<ushort> IsZero(this Condition<ushort> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<ushort> IsNotZero(this Condition<ushort> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<ushort> IsNegative(this Condition<ushort> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<ushort> IsPositive(this Condition<ushort> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<ushort> IsPositiveOrZero(this Condition<ushort> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int> IsGreaterThan(this Condition<int> condition, int compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int> IsGreaterThanOrEqualTo(this Condition<int> condition, int compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int> IsLessThan(this Condition<int> condition, int compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int> IsLessThanOrEqualTo(this Condition<int> condition, int compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<int> IsBetween(this Condition<int> condition, int minimum, int maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<int> IsZero(this Condition<int> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<int> IsNotZero(this Condition<int> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<int> IsNegative(this Condition<int> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<int> IsPositive(this Condition<int> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<int> IsPositiveOrZero(this Condition<int> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint> IsGreaterThan(this Condition<uint> condition, uint compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint> IsGreaterThanOrEqualTo(this Condition<uint> condition, uint compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint> IsLessThan(this Condition<uint> condition, uint compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint> IsLessThanOrEqualTo(this Condition<uint> condition, uint compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<uint> IsBetween(this Condition<uint> condition, uint minimum, uint maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<uint> IsZero(this Condition<uint> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<uint> IsNotZero(this Condition<uint> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<uint> IsNegative(this Condition<uint> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<uint> IsPositive(this Condition<uint> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<uint> IsPositiveOrZero(this Condition<uint> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long> IsGreaterThan(this Condition<long> condition, long compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long> IsGreaterThanOrEqualTo(this Condition<long> condition, long compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long> IsLessThan(this Condition<long> condition, long compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long> IsLessThanOrEqualTo(this Condition<long> condition, long compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<long> IsBetween(this Condition<long> condition, long minimum, long maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<long> IsZero(this Condition<long> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<long> IsNotZero(this Condition<long> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<long> IsNegative(this Condition<long> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<long> IsPositive(this Condition<long> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<long> IsPositiveOrZero(this Condition<long> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong> IsGreaterThan(this Condition<ulong> condition, ulong compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong> IsGreaterThanOrEqualTo(this Condition<ulong> condition, ulong compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong> IsLessThan(this Condition<ulong> condition, ulong compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong> IsLessThanOrEqualTo(this Condition<ulong> condition, ulong compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<ulong> IsBetween(this Condition<ulong> condition, ulong minimum, ulong maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<ulong> IsZero(this Condition<ulong> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<ulong> IsNotZero(this Condition<ulong> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<ulong> IsNegative(this Condition<ulong> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<ulong> IsPositive(this Condition<ulong> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<ulong> IsPositiveOrZero(this Condition<ulong> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float> IsGreaterThan(this Condition<float> condition, float compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float> IsGreaterThanOrEqualTo(this Condition<float> condition, float compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float> IsLessThan(this Condition<float> condition, float compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float> IsLessThanOrEqualTo(this Condition<float> condition, float compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<float> IsBetween(this Condition<float> condition, float minimum, float maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<float> IsZero(this Condition<float> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<float> IsNotZero(this Condition<float> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<float> IsNegative(this Condition<float> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<float> IsPositive(this Condition<float> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<float> IsPositiveOrZero(this Condition<float> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double> IsGreaterThan(this Condition<double> condition, double compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double> IsGreaterThanOrEqualTo(this Condition<double> condition, double compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double> IsLessThan(this Condition<double> condition, double compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double> IsLessThanOrEqualTo(this Condition<double> condition, double compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<double> IsBetween(this Condition<double> condition, double minimum, double maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<double> IsZero(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<double> IsNotZero(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<double> IsNegative(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<double> IsPositive(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<double> IsPositiveOrZero(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal> IsGreaterThan(this Condition<decimal> condition, decimal compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal> IsGreaterThanOrEqualTo(this Condition<decimal> condition, decimal compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal> IsLessThan(this Condition<decimal> condition, decimal compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal> IsLessThanOrEqualTo(this Condition<decimal> condition, decimal compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<decimal> IsBetween(this Condition<decimal> condition, decimal minimum, decimal maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<decimal> IsZero(this Condition<decimal> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<decimal> IsNotZero(this Condition<decimal> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<decimal> IsNegative(this Condition<decimal> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<decimal> IsPositive(this Condition<decimal> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<decimal> IsPositiveOrZero(this Condition<decimal> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }
        #endregion
        #region Nullables
        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte?> IsGreaterThan(this Condition<sbyte?> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte?> IsGreaterThanOrEqualTo(this Condition<sbyte?> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte?> IsLessThan(this Condition<sbyte?> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte?> IsLessThanOrEqualTo(this Condition<sbyte?> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<sbyte?> IsBetween(this Condition<sbyte?> condition, sbyte minimum, sbyte maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<sbyte?> IsZero(this Condition<sbyte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<sbyte?> IsNotZero(this Condition<sbyte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<sbyte?> IsNegative(this Condition<sbyte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<sbyte?> IsPositive(this Condition<sbyte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<sbyte?> IsPositiveOrZero(this Condition<sbyte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte?> IsGreaterThan(this Condition<byte?> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte?> IsGreaterThanOrEqualTo(this Condition<byte?> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte?> IsLessThan(this Condition<byte?> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte?> IsLessThanOrEqualTo(this Condition<byte?> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<byte?> IsBetween(this Condition<byte?> condition, byte minimum, byte maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<byte?> IsZero(this Condition<byte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<byte?> IsNotZero(this Condition<byte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<byte?> IsNegative(this Condition<byte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<byte?> IsPositive(this Condition<byte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<byte?> IsPositiveOrZero(this Condition<byte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short?> IsGreaterThan(this Condition<short?> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short?> IsGreaterThanOrEqualTo(this Condition<short?> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short?> IsLessThan(this Condition<short?> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short?> IsLessThanOrEqualTo(this Condition<short?> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<short?> IsBetween(this Condition<short?> condition, short minimum, short maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<short?> IsZero(this Condition<short?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<short?> IsNotZero(this Condition<short?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<short?> IsNegative(this Condition<short?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<short?> IsPositive(this Condition<short?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<short?> IsPositiveOrZero(this Condition<short?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort?> IsGreaterThan(this Condition<ushort?> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort?> IsGreaterThanOrEqualTo(this Condition<ushort?> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort?> IsLessThan(this Condition<ushort?> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort?> IsLessThanOrEqualTo(this Condition<ushort?> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<ushort?> IsBetween(this Condition<ushort?> condition, ushort minimum, ushort maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<ushort?> IsZero(this Condition<ushort?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<ushort?> IsNotZero(this Condition<ushort?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<ushort?> IsNegative(this Condition<ushort?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<ushort?> IsPositive(this Condition<ushort?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<ushort?> IsPositiveOrZero(this Condition<ushort?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int?> IsGreaterThan(this Condition<int?> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int?> IsGreaterThanOrEqualTo(this Condition<int?> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int?> IsLessThan(this Condition<int?> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int?> IsLessThanOrEqualTo(this Condition<int?> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given int?erval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<int?> IsBetween(this Condition<int?> condition, int minimum, int maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<int?> IsZero(this Condition<int?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<int?> IsNotZero(this Condition<int?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<int?> IsNegative(this Condition<int?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<int?> IsPositive(this Condition<int?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<int?> IsPositiveOrZero(this Condition<int?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint?> IsGreaterThan(this Condition<uint?> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint?> IsGreaterThanOrEqualTo(this Condition<uint?> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint?> IsLessThan(this Condition<uint?> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint?> IsLessThanOrEqualTo(this Condition<uint?> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given int?erval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<uint?> IsBetween(this Condition<uint?> condition, uint minimum, uint maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<uint?> IsZero(this Condition<uint?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<uint?> IsNotZero(this Condition<uint?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<uint?> IsNegative(this Condition<uint?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<uint?> IsPositive(this Condition<uint?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<uint?> IsPositiveOrZero(this Condition<uint?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long?> IsGreaterThan(this Condition<long?> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long?> IsGreaterThanOrEqualTo(this Condition<long?> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long?> IsLessThan(this Condition<long?> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long?> IsLessThanOrEqualTo(this Condition<long?> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<long?> IsBetween(this Condition<long?> condition, long minimum, long maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<long?> IsZero(this Condition<long?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<long?> IsNotZero(this Condition<long?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<long?> IsNegative(this Condition<long?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<long?> IsPositive(this Condition<long?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<long?> IsPositiveOrZero(this Condition<long?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong?> IsGreaterThan(this Condition<ulong?> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong?> IsGreaterThanOrEqualTo(this Condition<ulong?> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong?> IsLessThan(this Condition<ulong?> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong?> IsLessThanOrEqualTo(this Condition<ulong?> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<ulong?> IsBetween(this Condition<ulong?> condition, ulong minimum, ulong maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<ulong?> IsZero(this Condition<ulong?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<ulong?> IsNotZero(this Condition<ulong?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<ulong?> IsNegative(this Condition<ulong?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<ulong?> IsPositive(this Condition<ulong?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<ulong?> IsPositiveOrZero(this Condition<ulong?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float?> IsGreaterThan(this Condition<float?> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float?> IsGreaterThanOrEqualTo(this Condition<float?> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float?> IsLessThan(this Condition<float?> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float?> IsLessThanOrEqualTo(this Condition<float?> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<float?> IsBetween(this Condition<float?> condition, float minimum, float maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<float?> IsZero(this Condition<float?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<float?> IsNotZero(this Condition<float?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<float?> IsNegative(this Condition<float?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<float?> IsPositive(this Condition<float?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<float?> IsPositiveOrZero(this Condition<float?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double?> IsGreaterThan(this Condition<double?> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double?> IsGreaterThanOrEqualTo(this Condition<double?> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double?> IsLessThan(this Condition<double?> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double?> IsLessThanOrEqualTo(this Condition<double?> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<double?> IsBetween(this Condition<double?> condition, double minimum, double maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<double?> IsZero(this Condition<double?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<double?> IsNotZero(this Condition<double?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<double?> IsNegative(this Condition<double?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<double?> IsPositive(this Condition<double?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<double?> IsPositiveOrZero(this Condition<double?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal?> IsGreaterThan(this Condition<decimal?> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal?> IsGreaterThanOrEqualTo(this Condition<decimal?> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal?> IsLessThan(this Condition<decimal?> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal?> IsLessThanOrEqualTo(this Condition<decimal?> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        public static Condition<decimal?> IsBetween(this Condition<decimal?> condition, decimal minimum, decimal maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be equal to zero
        /// </summary>
        public static Condition<decimal?> IsZero(this Condition<decimal?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be equal to zero
        /// </summary>
        public static Condition<decimal?> IsNotZero(this Condition<decimal?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be negative
        /// </summary>
        public static Condition<decimal?> IsNegative(this Condition<decimal?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be positive
        /// </summary>
        public static Condition<decimal?> IsPositive(this Condition<decimal?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must not be negative
        /// </summary>
        public static Condition<decimal?> IsPositiveOrZero(this Condition<decimal?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }
        #endregion
        #region Non-nullables/Nullables
        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte> IsGreaterThan(this Condition<sbyte> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte> IsGreaterThanOrEqualTo(this Condition<sbyte> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte> IsLessThan(this Condition<sbyte> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<sbyte> IsLessThanOrEqualTo(this Condition<sbyte> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte> IsGreaterThan(this Condition<byte> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte> IsGreaterThanOrEqualTo(this Condition<byte> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte> IsLessThan(this Condition<byte> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<byte> IsLessThanOrEqualTo(this Condition<byte> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short> IsGreaterThan(this Condition<short> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short> IsGreaterThanOrEqualTo(this Condition<short> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short> IsLessThan(this Condition<short> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<short> IsLessThanOrEqualTo(this Condition<short> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort> IsGreaterThan(this Condition<ushort> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort> IsGreaterThanOrEqualTo(this Condition<ushort> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort> IsLessThan(this Condition<ushort> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ushort> IsLessThanOrEqualTo(this Condition<ushort> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int> IsGreaterThan(this Condition<int> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int> IsGreaterThanOrEqualTo(this Condition<int> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int> IsLessThan(this Condition<int> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<int> IsLessThanOrEqualTo(this Condition<int> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint> IsGreaterThan(this Condition<uint> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint> IsGreaterThanOrEqualTo(this Condition<uint> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint> IsLessThan(this Condition<uint> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<uint> IsLessThanOrEqualTo(this Condition<uint> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long> IsGreaterThan(this Condition<long> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long> IsGreaterThanOrEqualTo(this Condition<long> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long> IsLessThan(this Condition<long> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<long> IsLessThanOrEqualTo(this Condition<long> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong> IsGreaterThan(this Condition<ulong> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong> IsGreaterThanOrEqualTo(this Condition<ulong> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong> IsLessThan(this Condition<ulong> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<ulong> IsLessThanOrEqualTo(this Condition<ulong> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float> IsGreaterThan(this Condition<float> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float> IsGreaterThanOrEqualTo(this Condition<float> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float> IsLessThan(this Condition<float> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<float> IsLessThanOrEqualTo(this Condition<float> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double> IsGreaterThan(this Condition<double> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double> IsGreaterThanOrEqualTo(this Condition<double> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double> IsLessThan(this Condition<double> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<double> IsLessThanOrEqualTo(this Condition<double> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal> IsGreaterThan(this Condition<decimal> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal> IsGreaterThanOrEqualTo(this Condition<decimal> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal> IsLessThan(this Condition<decimal> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="compareValue">The value to compare to</param>
        public static Condition<decimal> IsLessThanOrEqualTo(this Condition<decimal> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }
        #endregion
        #endregion

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

        public enum EvaluationCriteria
        {
            MustBeTrue,
            MustBeFalse
        }
    }

    //This namespace helps reduce noise in the form of Assert options suggested by an IDE for types that do not need those options
    namespace Extensions
    {
        public static partial class Assert
        {
            /// <summary>
            /// Asserts that the target value must be greater than a specified value
            /// </summary>
            /// <param name="compareValue">The value to compare to</param>
            public static Condition<T> IsGreaterThan<T>(this Condition<T> condition, T compareValue) where T : IComparable<T>
            {
                if (!condition.ShouldProcess)
                    return condition;

                AssertHelper.MustBeGreaterThan(ref condition, compareValue);
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

                AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
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

                AssertHelper.MustBeLessThan(ref condition, compareValue);
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

                AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
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

                AssertHelper.MustBeBetween(ref condition, minimum, maximum);
                return condition;
            }
        }
    }
}
