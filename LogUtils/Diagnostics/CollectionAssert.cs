using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics
{
    public record struct CollectionAssert<T>(List<IConditionHandler> Handlers, IEnumerable<T> Enumerable)
    {
        public bool IsEmpty()
        {
            bool conditionPassed = !Enumerable.Any();

            Assert.OnResult(Handlers, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool HasItems()
        {
            bool conditionPassed = Enumerable.Any();

            Assert.OnResult(Handlers, new ConditionResults(null, conditionPassed));
            return conditionPassed;
        }

        public bool IsNull()
        {
            return new ObjectAssert(Handlers, Enumerable).IsNull();
        }

        public bool IsNotNull()
        {
            return new ObjectAssert(Handlers, Enumerable).IsNotNull();
        }
    }
}
