using LogUtils.Diagnostics.Extensions;
using System;
using static LogUtils.UtilityConsts;
using AssertDocs = LogUtils.Documentation.AssertDocumentation;

namespace LogUtils.Diagnostics
{
    public static partial class Assert
    {
        /// <inheritdoc cref="AssertDocs.NumericAssert.IsEqualTo{T}(Condition{IComparable{T}}, T)"/>
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
                    condition.Fail(new Condition.Message(AssertResponse.MUST_BE_EQUAL, "Objects"));
                return condition;
            }

            int valueDiff = condition.Value.CompareTo(compareValue);

            conditionPassed = valueDiff == 0;

            if (conditionPassed)
                condition.Pass();
            else
                condition.Fail(new Condition.Message(valueDiff < 0 ? AssertResponse.TOO_LOW : AssertResponse.TOO_HIGH, "Value"));
            return condition;
        }

        #region Non-nullables
        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<sbyte> IsGreaterThan(this Condition<sbyte> condition, sbyte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<sbyte> IsGreaterThanOrEqualTo(this Condition<sbyte> condition, sbyte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<sbyte> IsLessThan(this Condition<sbyte> condition, sbyte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<sbyte> IsLessThanOrEqualTo(this Condition<sbyte> condition, sbyte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<sbyte> IsBetween(this Condition<sbyte> condition, sbyte minimum, sbyte maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<sbyte> IsZero(this Condition<sbyte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<sbyte> IsNotZero(this Condition<sbyte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<sbyte> IsNegative(this Condition<sbyte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<sbyte> IsPositive(this Condition<sbyte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<sbyte> IsPositiveOrZero(this Condition<sbyte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<byte> IsGreaterThan(this Condition<byte> condition, byte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<byte> IsGreaterThanOrEqualTo(this Condition<byte> condition, byte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<byte> IsLessThan(this Condition<byte> condition, byte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<byte> IsLessThanOrEqualTo(this Condition<byte> condition, byte compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<byte> IsBetween(this Condition<byte> condition, byte minimum, byte maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<byte> IsZero(this Condition<byte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<byte> IsNotZero(this Condition<byte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<byte> IsNegative(this Condition<byte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<byte> IsPositive(this Condition<byte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<byte> IsPositiveOrZero(this Condition<byte> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<short> IsGreaterThan(this Condition<short> condition, short compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<short> IsGreaterThanOrEqualTo(this Condition<short> condition, short compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<short> IsLessThan(this Condition<short> condition, short compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<short> IsLessThanOrEqualTo(this Condition<short> condition, short compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<short> IsBetween(this Condition<short> condition, short minimum, short maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<short> IsZero(this Condition<short> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<short> IsNotZero(this Condition<short> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<short> IsNegative(this Condition<short> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<short> IsPositive(this Condition<short> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<short> IsPositiveOrZero(this Condition<short> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<ushort> IsGreaterThan(this Condition<ushort> condition, ushort compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ushort> IsGreaterThanOrEqualTo(this Condition<ushort> condition, ushort compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<ushort> IsLessThan(this Condition<ushort> condition, ushort compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ushort> IsLessThanOrEqualTo(this Condition<ushort> condition, ushort compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<ushort> IsBetween(this Condition<ushort> condition, ushort minimum, ushort maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<ushort> IsZero(this Condition<ushort> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<ushort> IsNotZero(this Condition<ushort> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<ushort> IsNegative(this Condition<ushort> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<ushort> IsPositive(this Condition<ushort> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<ushort> IsPositiveOrZero(this Condition<ushort> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<int> IsGreaterThan(this Condition<int> condition, int compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<int> IsGreaterThanOrEqualTo(this Condition<int> condition, int compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<int> IsLessThan(this Condition<int> condition, int compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<int> IsLessThanOrEqualTo(this Condition<int> condition, int compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<int> IsBetween(this Condition<int> condition, int minimum, int maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<int> IsZero(this Condition<int> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<int> IsNotZero(this Condition<int> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<int> IsNegative(this Condition<int> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<int> IsPositive(this Condition<int> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<int> IsPositiveOrZero(this Condition<int> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<uint> IsGreaterThan(this Condition<uint> condition, uint compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<uint> IsGreaterThanOrEqualTo(this Condition<uint> condition, uint compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<uint> IsLessThan(this Condition<uint> condition, uint compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<uint> IsLessThanOrEqualTo(this Condition<uint> condition, uint compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<uint> IsBetween(this Condition<uint> condition, uint minimum, uint maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<uint> IsZero(this Condition<uint> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<uint> IsNotZero(this Condition<uint> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<uint> IsNegative(this Condition<uint> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<uint> IsPositive(this Condition<uint> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<uint> IsPositiveOrZero(this Condition<uint> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<long> IsGreaterThan(this Condition<long> condition, long compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<long> IsGreaterThanOrEqualTo(this Condition<long> condition, long compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<long> IsLessThan(this Condition<long> condition, long compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<long> IsLessThanOrEqualTo(this Condition<long> condition, long compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<long> IsBetween(this Condition<long> condition, long minimum, long maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<long> IsZero(this Condition<long> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<long> IsNotZero(this Condition<long> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<long> IsNegative(this Condition<long> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<long> IsPositive(this Condition<long> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<long> IsPositiveOrZero(this Condition<long> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<ulong> IsGreaterThan(this Condition<ulong> condition, ulong compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ulong> IsGreaterThanOrEqualTo(this Condition<ulong> condition, ulong compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<ulong> IsLessThan(this Condition<ulong> condition, ulong compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ulong> IsLessThanOrEqualTo(this Condition<ulong> condition, ulong compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<ulong> IsBetween(this Condition<ulong> condition, ulong minimum, ulong maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<ulong> IsZero(this Condition<ulong> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<ulong> IsNotZero(this Condition<ulong> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<ulong> IsNegative(this Condition<ulong> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<ulong> IsPositive(this Condition<ulong> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<ulong> IsPositiveOrZero(this Condition<ulong> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<float> IsGreaterThan(this Condition<float> condition, float compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<float> IsGreaterThanOrEqualTo(this Condition<float> condition, float compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<float> IsLessThan(this Condition<float> condition, float compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<float> IsLessThanOrEqualTo(this Condition<float> condition, float compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<float> IsBetween(this Condition<float> condition, float minimum, float maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<float> IsZero(this Condition<float> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<float> IsNotZero(this Condition<float> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<float> IsNegative(this Condition<float> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<float> IsPositive(this Condition<float> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<float> IsPositiveOrZero(this Condition<float> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<double> IsGreaterThan(this Condition<double> condition, double compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<double> IsGreaterThanOrEqualTo(this Condition<double> condition, double compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<double> IsLessThan(this Condition<double> condition, double compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<double> IsLessThanOrEqualTo(this Condition<double> condition, double compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<double> IsBetween(this Condition<double> condition, double minimum, double maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<double> IsZero(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<double> IsNotZero(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<double> IsNegative(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<double> IsPositive(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<double> IsPositiveOrZero(this Condition<double> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<decimal> IsGreaterThan(this Condition<decimal> condition, decimal compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<decimal> IsGreaterThanOrEqualTo(this Condition<decimal> condition, decimal compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<decimal> IsLessThan(this Condition<decimal> condition, decimal compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<decimal> IsLessThanOrEqualTo(this Condition<decimal> condition, decimal compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<decimal> IsBetween(this Condition<decimal> condition, decimal minimum, decimal maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<decimal> IsZero(this Condition<decimal> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<decimal> IsNotZero(this Condition<decimal> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<decimal> IsNegative(this Condition<decimal> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<decimal> IsPositive(this Condition<decimal> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<decimal> IsPositiveOrZero(this Condition<decimal> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }
        #endregion
        #region Nullables
        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<sbyte?> IsGreaterThan(this Condition<sbyte?> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<sbyte?> IsGreaterThanOrEqualTo(this Condition<sbyte?> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<sbyte?> IsLessThan(this Condition<sbyte?> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<sbyte?> IsLessThanOrEqualTo(this Condition<sbyte?> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<sbyte?> IsBetween(this Condition<sbyte?> condition, sbyte minimum, sbyte maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<sbyte?> IsZero(this Condition<sbyte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<sbyte?> IsNotZero(this Condition<sbyte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<sbyte?> IsNegative(this Condition<sbyte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<sbyte?> IsPositive(this Condition<sbyte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<sbyte?> IsPositiveOrZero(this Condition<sbyte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<byte?> IsGreaterThan(this Condition<byte?> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<byte?> IsGreaterThanOrEqualTo(this Condition<byte?> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<byte?> IsLessThan(this Condition<byte?> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<byte?> IsLessThanOrEqualTo(this Condition<byte?> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<byte?> IsBetween(this Condition<byte?> condition, byte minimum, byte maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<byte?> IsZero(this Condition<byte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<byte?> IsNotZero(this Condition<byte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<byte?> IsNegative(this Condition<byte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<byte?> IsPositive(this Condition<byte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<byte?> IsPositiveOrZero(this Condition<byte?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<short?> IsGreaterThan(this Condition<short?> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<short?> IsGreaterThanOrEqualTo(this Condition<short?> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<short?> IsLessThan(this Condition<short?> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<short?> IsLessThanOrEqualTo(this Condition<short?> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<short?> IsBetween(this Condition<short?> condition, short minimum, short maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<short?> IsZero(this Condition<short?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<short?> IsNotZero(this Condition<short?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<short?> IsNegative(this Condition<short?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<short?> IsPositive(this Condition<short?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<short?> IsPositiveOrZero(this Condition<short?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<ushort?> IsGreaterThan(this Condition<ushort?> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ushort?> IsGreaterThanOrEqualTo(this Condition<ushort?> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<ushort?> IsLessThan(this Condition<ushort?> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ushort?> IsLessThanOrEqualTo(this Condition<ushort?> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<ushort?> IsBetween(this Condition<ushort?> condition, ushort minimum, ushort maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<ushort?> IsZero(this Condition<ushort?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<ushort?> IsNotZero(this Condition<ushort?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<ushort?> IsNegative(this Condition<ushort?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<ushort?> IsPositive(this Condition<ushort?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<ushort?> IsPositiveOrZero(this Condition<ushort?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<int?> IsGreaterThan(this Condition<int?> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<int?> IsGreaterThanOrEqualTo(this Condition<int?> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<int?> IsLessThan(this Condition<int?> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<int?> IsLessThanOrEqualTo(this Condition<int?> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<int?> IsBetween(this Condition<int?> condition, int minimum, int maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<int?> IsZero(this Condition<int?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<int?> IsNotZero(this Condition<int?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<int?> IsNegative(this Condition<int?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<int?> IsPositive(this Condition<int?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<int?> IsPositiveOrZero(this Condition<int?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<uint?> IsGreaterThan(this Condition<uint?> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<uint?> IsGreaterThanOrEqualTo(this Condition<uint?> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<uint?> IsLessThan(this Condition<uint?> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<uint?> IsLessThanOrEqualTo(this Condition<uint?> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<uint?> IsBetween(this Condition<uint?> condition, uint minimum, uint maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<uint?> IsZero(this Condition<uint?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<uint?> IsNotZero(this Condition<uint?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<uint?> IsNegative(this Condition<uint?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<uint?> IsPositive(this Condition<uint?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<uint?> IsPositiveOrZero(this Condition<uint?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<long?> IsGreaterThan(this Condition<long?> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<long?> IsGreaterThanOrEqualTo(this Condition<long?> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<long?> IsLessThan(this Condition<long?> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<long?> IsLessThanOrEqualTo(this Condition<long?> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<long?> IsBetween(this Condition<long?> condition, long minimum, long maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<long?> IsZero(this Condition<long?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<long?> IsNotZero(this Condition<long?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<long?> IsNegative(this Condition<long?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<long?> IsPositive(this Condition<long?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<long?> IsPositiveOrZero(this Condition<long?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<ulong?> IsGreaterThan(this Condition<ulong?> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ulong?> IsGreaterThanOrEqualTo(this Condition<ulong?> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<ulong?> IsLessThan(this Condition<ulong?> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ulong?> IsLessThanOrEqualTo(this Condition<ulong?> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<ulong?> IsBetween(this Condition<ulong?> condition, ulong minimum, ulong maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<ulong?> IsZero(this Condition<ulong?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<ulong?> IsNotZero(this Condition<ulong?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<ulong?> IsNegative(this Condition<ulong?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<ulong?> IsPositive(this Condition<ulong?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<ulong?> IsPositiveOrZero(this Condition<ulong?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<float?> IsGreaterThan(this Condition<float?> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<float?> IsGreaterThanOrEqualTo(this Condition<float?> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<float?> IsLessThan(this Condition<float?> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<float?> IsLessThanOrEqualTo(this Condition<float?> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<float?> IsBetween(this Condition<float?> condition, float minimum, float maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<float?> IsZero(this Condition<float?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<float?> IsNotZero(this Condition<float?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<float?> IsNegative(this Condition<float?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<float?> IsPositive(this Condition<float?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<float?> IsPositiveOrZero(this Condition<float?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<double?> IsGreaterThan(this Condition<double?> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<double?> IsGreaterThanOrEqualTo(this Condition<double?> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<double?> IsLessThan(this Condition<double?> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<double?> IsLessThanOrEqualTo(this Condition<double?> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<double?> IsBetween(this Condition<double?> condition, double minimum, double maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<double?> IsZero(this Condition<double?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<double?> IsNotZero(this Condition<double?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<double?> IsNegative(this Condition<double?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<double?> IsPositive(this Condition<double?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<double?> IsPositiveOrZero(this Condition<double?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<decimal?> IsGreaterThan(this Condition<decimal?> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<decimal?> IsGreaterThanOrEqualTo(this Condition<decimal?> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<decimal?> IsLessThan(this Condition<decimal?> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<decimal?> IsLessThanOrEqualTo(this Condition<decimal?> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
        public static Condition<decimal?> IsBetween(this Condition<decimal?> condition, decimal minimum, decimal maximum)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeBetween(ref condition, minimum, maximum);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsZero(Condition{int})"/>
        public static Condition<decimal?> IsZero(this Condition<decimal?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNotZero(Condition{int})"/>
        public static Condition<decimal?> IsNotZero(this Condition<decimal?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustNotBeZero(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsNegative(Condition{int})"/>
        public static Condition<decimal?> IsNegative(this Condition<decimal?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeNegative(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositive(Condition{int})"/>
        public static Condition<decimal?> IsPositive(this Condition<decimal?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositive(ref condition);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsPositiveOrZero(Condition{int})"/>
        public static Condition<decimal?> IsPositiveOrZero(this Condition<decimal?> condition)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBePositiveOrZero(ref condition);
            return condition;
        }
        #endregion
        #region Non-nullables/Nullables
        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<sbyte> IsGreaterThan(this Condition<sbyte> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<sbyte> IsGreaterThanOrEqualTo(this Condition<sbyte> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<sbyte> IsLessThan(this Condition<sbyte> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<sbyte> IsLessThanOrEqualTo(this Condition<sbyte> condition, sbyte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<byte> IsGreaterThan(this Condition<byte> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<byte> IsGreaterThanOrEqualTo(this Condition<byte> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<byte> IsLessThan(this Condition<byte> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<byte> IsLessThanOrEqualTo(this Condition<byte> condition, byte? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<short> IsGreaterThan(this Condition<short> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<short> IsGreaterThanOrEqualTo(this Condition<short> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<short> IsLessThan(this Condition<short> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<short> IsLessThanOrEqualTo(this Condition<short> condition, short? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<ushort> IsGreaterThan(this Condition<ushort> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ushort> IsGreaterThanOrEqualTo(this Condition<ushort> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<ushort> IsLessThan(this Condition<ushort> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ushort> IsLessThanOrEqualTo(this Condition<ushort> condition, ushort? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<int> IsGreaterThan(this Condition<int> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<int> IsGreaterThanOrEqualTo(this Condition<int> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<int> IsLessThan(this Condition<int> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<int> IsLessThanOrEqualTo(this Condition<int> condition, int? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<uint> IsGreaterThan(this Condition<uint> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<uint> IsGreaterThanOrEqualTo(this Condition<uint> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<uint> IsLessThan(this Condition<uint> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<uint> IsLessThanOrEqualTo(this Condition<uint> condition, uint? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<long> IsGreaterThan(this Condition<long> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<long> IsGreaterThanOrEqualTo(this Condition<long> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<long> IsLessThan(this Condition<long> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<long> IsLessThanOrEqualTo(this Condition<long> condition, long? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<ulong> IsGreaterThan(this Condition<ulong> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ulong> IsGreaterThanOrEqualTo(this Condition<ulong> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<ulong> IsLessThan(this Condition<ulong> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<ulong> IsLessThanOrEqualTo(this Condition<ulong> condition, ulong? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<float> IsGreaterThan(this Condition<float> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<float> IsGreaterThanOrEqualTo(this Condition<float> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<float> IsLessThan(this Condition<float> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<float> IsLessThanOrEqualTo(this Condition<float> condition, float? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<double> IsGreaterThan(this Condition<double> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<double> IsGreaterThanOrEqualTo(this Condition<double> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<double> IsLessThan(this Condition<double> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<double> IsLessThanOrEqualTo(this Condition<double> condition, double? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
        public static Condition<decimal> IsGreaterThan(this Condition<decimal> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<decimal> IsGreaterThanOrEqualTo(this Condition<decimal> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
        public static Condition<decimal> IsLessThan(this Condition<decimal> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThan(ref condition, compareValue);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
        public static Condition<decimal> IsLessThanOrEqualTo(this Condition<decimal> condition, decimal? compareValue)
        {
            if (!condition.ShouldProcess)
                return condition;

            AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
            return condition;
        }
        #endregion
    }
}
