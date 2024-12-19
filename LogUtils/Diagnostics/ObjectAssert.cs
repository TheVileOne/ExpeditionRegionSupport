using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public readonly struct ObjectAssert
    {
        private readonly object _target;
        private readonly List<IConditionHandler> _handlers;

        public ObjectAssert(List<IConditionHandler> handlers, object assertTarget)
        {
            _target = assertTarget;
            _handlers = handlers;
        }

        public bool IsEqualTo(object checkData)
        {
            var result = Assert.IsEqual(_target, checkData);

            Assert.OnResult(_handlers, result);
            return result.Passed;
        }

        public bool DoesNotEqual(object checkData)
        {
            var result = Assert.DoesNotEqual(_target, checkData);

            Assert.OnResult(_handlers, result);
            return result.Passed;
        }

        public bool IsNull()
        {
            var result = Assert.IsNull(_target);

            Assert.OnResult(_handlers, result);
            return result.Passed;
        }

        public bool IsNotNull()
        {
            var result = Assert.IsNotNull(_target);

            Assert.OnResult(_handlers, result);
            return result.Passed;
        }
    }
}
