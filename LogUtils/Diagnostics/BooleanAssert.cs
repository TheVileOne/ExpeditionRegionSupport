using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public readonly struct BooleanAssert
    {
        private readonly bool _target;
        private readonly List<IConditionHandler> _handlers;

        public BooleanAssert(List<IConditionHandler> handlers, bool assertTarget)
        {
            _target = assertTarget;
            _handlers = handlers;
        }

        public bool IsTrue()
        {
            var result = Assert.IsTrue(_target);

            Assert.OnResult(_handlers, result);
            return result.Passed;
        }

        public bool IsFalse()
        {
            var result = Assert.IsFalse(_target);

            Assert.OnResult(_handlers, result);
            return result.Passed;
        }
    }
}
