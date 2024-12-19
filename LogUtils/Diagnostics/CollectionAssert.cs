using LogUtils.Helpers;
using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public readonly struct CollectionAssert<T>
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
    }
}
