namespace LogUtils.Diagnostics
{
    public readonly struct ObjectAssert
    {
        public readonly AssertArgs _settings;
        private readonly object _target;

        public ObjectAssert(object assertTarget, AssertArgs assertArgs)
        {
            _target = assertTarget;
            _settings = assertArgs;
        }

        /// <summary>
        /// Asserts that the target value must be equal to a specified value
        /// </summary>
        /// <param name="checkData">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsEqualTo(object checkData)
        {
            var result = Assert.IsEqual(_target, checkData);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target value must be not equal to a specified value
        /// </summary>
        /// <param name="checkData">The input value to check</param>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool DoesNotEqual(object checkData)
        {
            var result = Assert.DoesNotEqual(_target, checkData);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target value must be null
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsNull()
        {
            var result = Assert.IsNull(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that the target value must not be null
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsNotNull()
        {
            var result = Assert.IsNotNull(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }
    }
}
