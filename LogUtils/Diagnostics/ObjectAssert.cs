using System;

namespace LogUtils.Diagnostics
{
    public readonly struct ObjectAssert<T> : IObjectAssertion<T>
    {
        public readonly AssertArgs _settings;
        private readonly T _target;

        public ObjectAssert(T assertTarget, AssertArgs assertArgs)
        {
            _target = assertTarget;
            _settings = assertArgs;
        }

        public bool IsEqualTo(T checkValue)
        {
            var result = Assert.IsEqual(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool DoesNotEqual(T checkValue)
        {
            var result = Assert.DoesNotEqual(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsNull()
        {
            var result = Assert.IsNull(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsNotNull()
        {
            var result = Assert.IsNotNull(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool EvaluateCondition(Func<T, bool> condition, EvaluationCriteria criteria)
        {
            var result = Assert.EvaluateCondition(_target, condition, criteria);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool EvaluateCondition(T conditionArg, Func<T, T, bool> condition, EvaluationCriteria criteria)
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
