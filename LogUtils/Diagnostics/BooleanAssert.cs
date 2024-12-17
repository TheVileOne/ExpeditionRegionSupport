using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public record struct BooleanAssert(List<IConditionHandler> Handlers, bool Condition)
    {
        public bool IsTrue()
        {
            var result = Assert.IsTrue(Condition);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsFalse()
        {
            var result = Assert.IsFalse(Condition);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }
    }
}
