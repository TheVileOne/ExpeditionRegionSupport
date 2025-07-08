using LogUtils.Diagnostics;
using LogUtils.Enums;
using System;
using System.Collections.Generic;
using static LogUtils.Diagnostics.Assert;

namespace LogUtils.Documentation
{
    internal static class AssertDocumentation
    {
        internal interface GeneralAssert
        {
            /// <summary>
            /// Asserts a condition by invoking a delegate using specified values as arguments
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="conditionArg">Condition argument for delegate</param>
            /// <param name="conditionDelegate">Callback that checks the condition state</param>
            /// <param name="criteria">The expected state of the condition</param>
            Condition<T> EvaluateCondition<T>(Condition<T> condition, T conditionArg, Func<T, bool> conditionDelegate, EvaluationCriteria criteria);

            /// <summary>
            /// Asserts a condition by invoking a delegate using specified values as arguments
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="firstArg">First condition argument</param>
            /// <param name="secondArg">Second condition argument</param>
            /// <param name="conditionDelegate">Callback that checks the condition state</param>
            /// <param name="criteria">The expected state of the condition</param>
            Condition<T> EvaluateCondition<T>(Condition<T> condition, T firstArg, T secondArg, Func<T, T, bool> conditionDelegate, EvaluationCriteria criteria);

            /// <summary>
            /// Asserts a condition by dynamically invoking a delegate
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="dynamicCondition">Delegate that evaluates a condition (must return true or false)</param>
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
            Condition<T> EvaluateCondition<T>(Condition<T> condition, Delegate dynamicCondition, EvaluationCriteria criteria, params object[] dynamicParams);
        }

        internal interface BooleanAssert
        {
            /// <summary>
            /// Asserts that target value must be false
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<bool> IsFalse(Condition<bool> condition);

            /// <summary>
            /// Asserts that target value must be true
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<bool> IsTrue(Condition<bool> condition);
        }

        internal interface ObjectAssert
        {
            /// <summary>
            /// Asserts that the target value must be not equal to a specified value
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="compareObject">The value to compare to</param>
            Condition<T> DoesNotEqual<T>(Condition<T> condition, T compareObject);

            /// <summary>
            /// Asserts that the target value must be equal to a specified value
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="compareObject">The value to compare to</param>
            Condition<T> IsEqualTo<T>(Condition<T> condition, T compareObject);

            /// <summary>
            /// Asserts that the target value must not be null
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<T> IsNotNull<T>(Condition<T> condition) where T : class;

            /// <summary>
            /// Asserts that the target object refers to a different object than a specified object
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="compareObject">The object to compare to</param>
            Condition<T> IsNotThisInstance<T>(Condition<T> condition, T compareObject) where T : class;

            /// <summary>
            /// Asserts that the target value must be null
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<T> IsNull<T>(Condition<T> condition) where T : class;

            /// <summary>
            /// Asserts that the target object refers to the same object as a specified object
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="compareObject">The object to compare to</param>
            Condition<T> IsSameInstance<T>(Condition<T> condition, T compareObject) where T : class;
        }

        internal interface CollectionAssert
        {
            /// <summary>
            /// Asserts that the target collection must have at least one entry
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<IEnumerable<T>> HasItems<T>(Condition<IEnumerable<T>> condition);

            /// <summary>
            /// Asserts that the target collection must be null or empty
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<IEnumerable<T>> IsNullOrEmpty<T>(Condition<IEnumerable<T>> condition);
        }

        internal interface NumericAssert
        {
            /// <summary>
            /// Asserts that the target value must be equal to a specified value
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="compareValue">The value to compare to</param>
            Condition<IComparable<T>> IsEqualTo<T>(Condition<IComparable<T>> condition, T compareValue);

            /// <summary>
            /// Asserts that the target value must be greater than a specified value
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="compareValue">The value to compare to</param>
            Condition<int> IsGreaterThan(Condition<int> condition, int compareValue);

            /// <summary>
            /// Asserts that the target value must be greater than or equal to a specified value
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="compareValue">The value to compare to</param>
            Condition<int> IsGreaterThanOrEqualTo(Condition<int> condition, int compareValue);

            /// <summary>
            /// Asserts that the target value must be less than a specified value
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="compareValue">The value to compare to</param>
            Condition<int> IsLessThan(Condition<int> condition, int compareValue);

            /// <summary>
            /// Asserts that the target value must be less than or equal to a specified value
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="compareValue">The value to compare to</param>
            Condition<int> IsLessThanOrEqualTo(Condition<int> condition, int compareValue);

            /// <summary>
            /// Asserts that the target value must be in a given interval
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="minimum">The lower bound</param>
            /// <param name="maximum">The upper bound</param>
            Condition<int> IsBetween(Condition<int> condition, int minimum, int maximum);

            /// <summary>
            /// Asserts that the target value must be equal to zero
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<int> IsZero(Condition<int> condition);

            /// <summary>
            /// Asserts that the target value must not be equal to zero
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<int> IsNotZero(Condition<int> condition);

            /// <summary>
            /// Asserts that the target value must be negative
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<int> IsNegative(Condition<int> condition);

            /// <summary>
            /// Asserts that the target value must be positive
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<int> IsPositive(Condition<int> condition);

            /// <summary>
            /// Asserts that the target value must not be negative
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<int> IsPositiveOrZero(Condition<int> condition);
        }

        internal interface OtherAssert
        {
            /// <summary>
            /// Asserts that the target value's IsEmpty property is set to true
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            Condition<CompositeLogCategory> IsEmpty(Condition<CompositeLogCategory> condition);

            /// <summary>
            /// Asserts that the target value contains a given value
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="flag">The flag value to evaluate</param>
            Condition<CompositeLogCategory> Contains(Condition<CompositeLogCategory> condition, LogCategory flag);

            /// <summary>
            /// Asserts that the target value only contains a given value
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="flag">The flag value to evaluate</param>
            Condition<CompositeLogCategory> ContainsOnly(Condition<CompositeLogCategory> condition, LogCategory flag);

            /// <summary>
            /// Asserts that the target value does not contain a given value
            /// </summary>
            /// <param name="condition">The data structure that checks and evaluates a result</param>
            /// <param name="flag">The flag value to evaluate</param>
            Condition<CompositeLogCategory> DoesNotContain(Condition<CompositeLogCategory> condition, LogCategory flag);
        }
    }
}
