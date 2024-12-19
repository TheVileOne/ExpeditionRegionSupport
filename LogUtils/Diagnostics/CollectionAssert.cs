using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public readonly struct CollectionAssert<T>
    {
        private readonly IEnumerable<T> _target;
        private readonly List<IConditionHandler> _handlers;

        public CollectionAssert(List<IConditionHandler> handlers, IEnumerable<T> assertTarget)
        {
            _target = assertTarget;
            _handlers = handlers;
        }

        public bool IsNullOrEmpty()
        {
            var result = Assert.IsNullOrEmpty(_target);

            Assert.OnResult(_handlers, result);
            return result.Passed;
        }

        public bool HasItems()
        {
            var result = Assert.HasItems(_target);

            Assert.OnResult(_handlers, result);
            return result.Passed;
        }

        public bool IsNull()
        {
            var result = Assert.IsNull(_target);
            result.Response.SetDescriptors(new string[] { "Collection" });

            Assert.OnResult(_handlers, result);
            return result.Passed;
        }

        public bool IsNotNull()
        {
            var result = Assert.IsNotNull(_target);
            result.Response.SetDescriptors(new string[] { "Collection" });

            Assert.OnResult(_handlers, result);
            return result.Passed;
        }
    }
}
