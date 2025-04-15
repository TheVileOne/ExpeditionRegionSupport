namespace LogUtils.Diagnostics.Tests
{
    /// <summary>
    /// Simple interface to make an object compatible with a TestSuite instance
    /// </summary>
    public interface ITestable
    {
        /// <summary>
        /// A descriptive name for the test
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Processes test logic, and assertions
        /// </summary>
        public abstract void Test();
    }
}
