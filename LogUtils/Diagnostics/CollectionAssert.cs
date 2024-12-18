using System.Collections.Generic;

namespace LogUtils.Diagnostics
{
    public record struct CollectionAssert<T>(List<IConditionHandler> Handlers, IEnumerable<T> Enumerable)
    {
        public bool IsNullOrEmpty()
        {
            var result = Assert.IsNullOrEmpty(Enumerable);

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
            result.Response.SetDescriptors(new string[] { "Collection" });

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }

        public bool IsNotNull()
        {
            var result = Assert.IsNotNull(Enumerable);
            result.Response.SetDescriptors(new string[] { "Collection" });

            Assert.OnResult(Handlers, result);
            return result.Passed;
        }
    }
}
