using System;

namespace LogUtils.Diagnostics
{
    public readonly struct NumericAssert
    {
        public readonly AssertArgs _settings;
        private readonly double _target;

        public NumericAssert(double assertTarget, AssertArgs assertArgs)
        {
            _target = assertTarget;
            _settings = assertArgs;
        }

        /// <summary>
        /// Asserts that the target value must be equal to a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsEqualTo(double checkValue)
        {
            var result = Assert.IsEqual(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target value must be not equal to a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool DoesNotEqual(double checkValue)
        {
            var result = Assert.DoesNotEqual(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target value must be greater than a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsGreaterThan(double checkValue)
        {
            var result = Assert.IsGreaterThan(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target value must be greater than or equal to a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsGreaterThanOrEqualTo(double checkValue)
        {
            var result = Assert.IsGreaterThanOrEqualTo(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target value must be less than a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsLessThan(double checkValue)
        {
            var result = Assert.IsLessThan(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target value must be less than or equal to a specified value
        /// </summary>
        /// <param name="checkValue">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsLessThanOrEqualTo(double checkValue)
        {
            var result = Assert.IsLessThanOrEqualTo(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target value must be in a given interval
        /// </summary>
        /// <param name="minimum">The lower bound</param>
        /// <param name="maximum">The upper bound</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsBetween(double minimum, double maximum)
        {
            var result = Assert.IsBetween(_target, minimum, maximum);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target value is equal to zero
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsZero()
        {
            var result = Assert.IsZero(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts a condition by invoking a delegate using the target value as an argument
        /// </summary>
        /// <param name="condition">A delegate that evaluates the assigned value</param>
        /// <param name="criteria">The expected state of the condition</param>
        /// <returns>true, if the condition state matches expectations, otherwise false</returns>
        public bool EvaluateCondition(Func<double, bool> condition, EvaluationCriteria criteria)
        {
            var result = Assert.EvaluateCondition(_target, condition, criteria);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts a condition by invoking a delegate using the target value, and a specified value as an argument
        /// </summary>
        /// <param name="conditionArg">Condition argument for delegate (used as the second argument)</param>
        /// <param name="condition">Delegate that evaluates a condition</param>
        /// <param name="criteria">The expected state of the condition</param>
        /// <returns>true, if the condition state matches expectations, otherwise false</returns>
        public bool EvaluateCondition(double conditionArg, Func<double, double, bool> condition, EvaluationCriteria criteria)
        {
            var result = Assert.EvaluateCondition(_target, conditionArg, condition, criteria);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }
    }

    public enum EvaluationCriteria
    {
        MustBeTrue,
        MustBeFalse
    }
}
