using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics
{
    public record struct CollectionAssert<T>(List<IConditionHandler> Handlers, IEnumerable<T> Enumerable)
    {
        public bool IsEmpty()
        {
            var result = Assert.IsEmpty(Enumerable);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool HasItems()
        {
            var result = Assert.HasItems(Enumerable);

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsNull()
        {
            var result = Assert.IsNull(Enumerable);
            result.SetDescriptors("Collection");

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsNotNull()
        {
            var result = Assert.IsNotNull(Enumerable);
            result.SetDescriptors("Collection");

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }
    }
}
