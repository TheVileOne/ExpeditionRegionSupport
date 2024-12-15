using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics
{
    public record struct CollectionAssert<T>(IConditionHandler Handler, IEnumerable<T> Enumerable)
    {
        public bool IsEmpty()
        {
            bool conditionPassed = !Enumerable.Any();

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool HasItems()
        {
            bool conditionPassed = Enumerable.Any();

            Assert.OnResult(Handler, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsNull()
        {
            return new ObjectAssert(Handler, Enumerable).IsNull();
        }

        public bool IsNotNull()
        {
            return new ObjectAssert(Handler, Enumerable).IsNotNull();
        }
    }
}
