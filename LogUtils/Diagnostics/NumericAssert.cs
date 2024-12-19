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

        public bool IsEqualTo(double checkValue)
        {
            var result = Assert.IsEqual(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool DoesNotEqual(double checkValue)
        {
            var result = Assert.DoesNotEqual(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsGreaterThan(double checkValue)
        {
            var result = Assert.IsGreaterThan(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsGreaterThanOrEqualTo(double checkValue)
        {
            var result = Assert.IsGreaterThanOrEqualTo(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsLessThan(double checkValue)
        {
            var result = Assert.IsLessThan(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsLessThanOrEqualTo(double checkValue)
        {
            var result = Assert.IsLessThanOrEqualTo(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsBetween(double checkValue, double checkValue2)
        {
            var result = Assert.IsBetween(_target, checkValue, checkValue2);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsZero()
        {
            var result = Assert.IsZero(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Uses the provided check condition delegate to assert a condition
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
        /// Uses the provided check condition delegate to assert a condition
        /// </summary>
        /// <param name="condition">A delegate that evaluates the assigned value</param>
        /// <param name="checkValue">A value to be used for the evaluation process</param>
        /// <param name="criteria">The expected state of the condition</param>
        /// <returns>true, if the condition state matches expectations, otherwise false</returns>
        public bool EvaluateCondition(double checkValue, Func<double, double, bool> condition, EvaluationCriteria criteria)
        {
            var result = Assert.EvaluateCondition(_target, checkValue, condition, criteria);

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
