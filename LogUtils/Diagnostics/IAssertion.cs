using System;

namespace LogUtils.Diagnostics
{
    public interface IAssertion<T>
    {
        /// <summary>
        /// Asserts that the target value must be equal to a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsEqualTo(T checkValue);

        /// <summary>
        /// Asserts that the target value must be not equal to a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool DoesNotEqual(T checkValue);

        /// <summary>
        /// Asserts a condition by invoking a delegate using the target value as an argument
        /// </summary>
        /// <param name="condition">A delegate that evaluates the assigned value</param>
        /// <param name="criteria">The expected state of the condition</param>
        /// <returns>true, if the condition state matches expectations, otherwise false</returns>
        public bool EvaluateCondition(Func<T, bool> condition, EvaluationCriteria criteria);

        /// <summary>
        /// Asserts a condition by invoking a delegate using the target value, and a specified value as an argument
        /// </summary>
        /// <param name="conditionArg">Condition argument for delegate (used as the second argument)</param>
        /// <param name="condition">Delegate that evaluates a condition</param>
        /// <param name="criteria">The expected state of the condition</param>
        /// <returns>true, if the condition state matches expectations, otherwise false</returns>
        public bool EvaluateCondition(T conditionArg, Func<T, T, bool> condition, EvaluationCriteria criteria);

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
        public bool EvaluateCondition(Delegate dynamicCondition, EvaluationCriteria criteria, params object[] dynamicParams);
    }

    public interface IBooleanAssertion<T> : IAssertion<T>
    {
        /// <summary>
        /// Asserts that target value must be true
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsTrue();

        /// <summary>
        /// Asserts that target value must be false
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsFalse();
    }

    public interface INumericAssertion<T> : IAssertion<T>
    {
        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsGreaterThan(T checkValue);

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsGreaterThanOrEqualTo(T checkValue);

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsLessThan(T checkValue);

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsLessThanOrEqualTo(T checkValue);

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsBetween(T minimum, T maximum);

        /// <summary>
        /// Asserts that the target value is equal to zero
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsZero();
    }

    public interface INullableAssertion<T> : IAssertion<T>
    {
        /// <summary>
        /// Asserts that the target value must be null
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsNull();

        /// <summary>
        /// Asserts that the target value must not be null
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsNotNull();
    }

    public interface ICollectionAssertion<T> : INullableAssertion<T>
    {
        /// <summary>
        /// Asserts that the target collection must be null or empty
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsNullOrEmpty();

        /// <summary>
        /// Asserts that the target collection must have at least one entry
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool HasItems();

        /// <summary>
        /// Asserts that the target collection must be null
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public new bool IsNull();

        /// <summary>
        /// Asserts that the target collection must not be null
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public new bool IsNotNull();
    }
}
