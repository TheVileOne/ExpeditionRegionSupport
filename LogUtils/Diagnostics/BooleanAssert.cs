namespace LogUtils.Diagnostics
{
    public readonly struct BooleanAssert
    {
        public readonly AssertArgs _settings;
        private readonly bool _target;

        public BooleanAssert(bool assertTarget, AssertArgs assertArgs)
        {
            _target = assertTarget;
            _settings = assertArgs;
        }

        /// <summary>
        /// Asserts that target value must be true
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsTrue()
        {
            var result = Assert.IsTrue(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }

        /// <summary>
        /// Asserts that target value must be false
        /// </summary>
        /// <returns>true if the assert passes, otherwise false</returns>
        public bool IsFalse()
        {
            var result = Assert.IsFalse(_target);

            Assert.OnResult(_settings, result);
            return result.Passed;
        }
    }
}
