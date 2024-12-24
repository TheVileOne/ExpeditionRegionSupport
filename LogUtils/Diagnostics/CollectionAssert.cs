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

        /// <summary>
        /// Asserts that the target IEnumerable<T> instance must be null or empty
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsNullOrEmpty()
        {
            var result = Assert.IsNullOrEmpty(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target IEnumerable<T> instance must have at least one entry
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool HasItems()
        {
            var result = Assert.HasItems(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target IEnumerable<T> instance must be null
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsNull()
        {
            var result = Assert.IsNull(_target);

            if (result.Failed)
                result.Response.SetDescriptors(ArrayUtils.CreateFromValues("Collection"));
            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target IEnumerable<T> instance must not be null
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
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
