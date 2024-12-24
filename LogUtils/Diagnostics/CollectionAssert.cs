using LogUtils.Helpers;
using System;
using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public readonly struct CollectionAssert<T> : ICollectionAssertion<IEnumerable<T>>
    {
        private readonly AssertArgs _settings;
        private readonly IEnumerable<T> _target;

        public CollectionAssert(IEnumerable<T> assertTarget, AssertArgs assertArgs)
        {
            _target = assertTarget;
            _settings = assertArgs;
        }

        public bool IsNullOrEmpty()
        {
            var result = Assert.IsNullOrEmpty(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool HasItems()
        {
            var result = Assert.HasItems(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsNull()
        {
            var result = Assert.IsNull(_target);

            if (result.Failed)
                result.Response.SetDescriptors(ArrayUtils.CreateFromValues("Collection"));
            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsNotNull()
        {
            var result = Assert.IsNotNull(_target);

            if (result.Failed)
                result.Response.SetDescriptors(ArrayUtils.CreateFromValues("Collection"));
            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool IsEqualTo(IEnumerable<T> checkValue)
        {
            var result = Assert.IsEqual(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool DoesNotEqual(IEnumerable<T> checkValue)
        {
            var result = Assert.DoesNotEqual(_target, checkValue);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool EvaluateCondition(Func<IEnumerable<T>, bool> condition, EvaluationCriteria criteria)
        {
            var result = Assert.EvaluateCondition(_target, condition, criteria);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        public bool EvaluateCondition(IEnumerable<T> conditionArg, Func<IEnumerable<T>, IEnumerable<T>, bool> condition, EvaluationCriteria criteria)
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
