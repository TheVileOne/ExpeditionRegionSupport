using System;
using AssertDocs = LogUtils.Documentation.AssertDocumentation;
using AssertResponse = LogUtils.UtilityConsts.AssertResponse;

namespace LogUtils.Diagnostics
{
    /// <summary>
    /// This class provides methods for making assert statements
    /// </summary>
    public static partial class Assert
    {
        /// <inheritdoc cref="AssertDocs.GeneralAssert.EvaluateCondition{T}(Condition{T}, T, Func{T, bool}, EvaluationCriteria)"/>
        public static Condition<T> EvaluateCondition<T>(this Condition<T> condition, T conditionArg, Func<T, bool> conditionDelegate, EvaluationCriteria criteria)
        {
            if (condition.ShouldProcess)
                processCondition(ref condition, conditionDelegate.Invoke(conditionArg), criteria);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.GeneralAssert.EvaluateCondition{T}(Condition{T}, T, T, Func{T, T, bool}, EvaluationCriteria)"/>
        public static Condition<T> EvaluateCondition<T>(this Condition<T> condition, T firstArg, T secondArg, Func<T, T, bool> conditionDelegate, EvaluationCriteria criteria)
        {
            if (condition.ShouldProcess)
                processCondition(ref condition, conditionDelegate.Invoke(firstArg, secondArg), criteria);
            return condition;
        }

        /// <inheritdoc cref="AssertDocs.GeneralAssert.EvaluateCondition{T}(Condition{T}, Delegate, EvaluationCriteria, object[])"/>
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
                    failMessage = AssertResponse.MUST_BE_TRUE;
                else if (criteria == EvaluationCriteria.MustBeFalse)
                    failMessage = AssertResponse.MUST_BE_FALSE;

                condition.Fail(new Condition.Message(failMessage, "Condition"));
            }
        }

#pragma warning disable CS1591 //Missing XML comment for publicly visible type or member
        /// <summary>
        /// Represents the expected condition state
        /// </summary>
        public enum EvaluationCriteria
        {
            MustBeTrue,
            MustBeFalse
        }
#pragma warning restore CS1591 //Missing XML comment for publicly visible type or member
    }

    //This namespace helps reduce noise in the form of Assert options suggested by an IDE for types that do not need those options
    namespace Extensions
    {
        /// <summary>
        /// This class provides methods for making assert statements to additional types
        /// </summary>
        public static class Assert
        {
            /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThan(Condition{int}, int)"/>
            public static Condition<T> IsGreaterThan<T>(this Condition<T> condition, T compareValue) where T : IComparable<T>
            {
                if (!condition.ShouldProcess)
                    return condition;

                AssertHelper.MustBeGreaterThan(ref condition, compareValue);
                return condition;
            }

            /// <inheritdoc cref="AssertDocs.NumericAssert.IsGreaterThanOrEqualTo(Condition{int}, int)"/>
            public static Condition<T> IsGreaterThanOrEqualTo<T>(this Condition<T> condition, T compareValue) where T : IComparable<T>
            {
                if (!condition.ShouldProcess)
                    return condition;

                AssertHelper.MustBeGreaterThanOrEqualTo(ref condition, compareValue);
                return condition;
            }

            /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThan(Condition{int}, int)"/>
            public static Condition<T> IsLessThan<T>(this Condition<T> condition, T compareValue) where T : IComparable<T>
            {
                if (!condition.ShouldProcess)
                    return condition;

                AssertHelper.MustBeLessThan(ref condition, compareValue);
                return condition;
            }

            /// <inheritdoc cref="AssertDocs.NumericAssert.IsLessThanOrEqualTo(Condition{int}, int)"/>
            public static Condition<T> IsLessThanOrEqualTo<T>(this Condition<T> condition, T compareValue) where T : IComparable<T>
            {
                if (!condition.ShouldProcess)
                    return condition;

                AssertHelper.MustBeLessThanOrEqualTo(ref condition, compareValue);
                return condition;
            }

            /// <inheritdoc cref="AssertDocs.NumericAssert.IsBetween(Condition{int}, int, int)"/>
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
