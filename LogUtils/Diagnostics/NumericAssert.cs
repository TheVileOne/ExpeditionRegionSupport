using System;

namespace LogUtils.Diagnostics
{
    public readonly struct NumericAssert : INumericAssertion<double>
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

        public bool IsBetween(double minimum, double maximum)
        {
            var result = Assert.IsBetween(_target, minimum, maximum);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsZero()
        {
            var result = Assert.IsZero(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool EvaluateCondition(Func<double, bool> condition, EvaluationCriteria criteria)
        {
            var result = Assert.EvaluateCondition(_target, condition, criteria);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool EvaluateCondition(double conditionArg, Func<double, double, bool> condition, EvaluationCriteria criteria)
        {
            var result = Assert.EvaluateCondition(_target, conditionArg, condition, criteria);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool EvaluateCondition(Delegate dynamicCondition, EvaluationCriteria criteria, params object[] dynamicParams)
        {
            var result = Assert.EvaluateCondition(dynamicCondition, criteria, dynamicParams);

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
