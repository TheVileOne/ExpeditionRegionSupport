using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public record struct BooleanAssert(List<IConditionHandler> Handlers, bool Condition)
    {
        public bool IsTrue()
        {
            bool conditionPassed = Condition == true;

            Assert.OnResult(Handlers, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsFalse()
        {
            bool conditionPassed = Condition == false;

            Assert.OnResult(Handlers, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }
    }
}
