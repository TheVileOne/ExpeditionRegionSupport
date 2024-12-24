using System;

namespace LogUtils.Diagnostics
{
    public readonly struct BooleanAssert : IBooleanAssertion<bool>
    {
        public readonly AssertArgs _settings;
        private readonly bool _target;

        public BooleanAssert(bool assertTarget, AssertArgs assertArgs)
        {
            _target = assertTarget;
            _settings = assertArgs;
        }

        public bool IsTrue()
        {
            var result = Assert.IsTrue(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsFalse()
        {
            var result = Assert.IsFalse(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsEqualTo(bool checkValue)
        {
            var result = Assert.IsTrue(_target == checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool DoesNotEqual(bool checkValue)
        {
            var result = Assert.IsTrue(_target != checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool EvaluateCondition(Func<bool, bool> condition, EvaluationCriteria criteria)
        {
            var result = Assert.EvaluateCondition(_target, condition, criteria);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool EvaluateCondition(bool conditionArg, Func<bool, bool, bool> condition, EvaluationCriteria criteria)
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
}
