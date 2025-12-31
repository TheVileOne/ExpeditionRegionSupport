namespace LogUtils.Diagnostics.Tests
{
    /// <summary>
    /// Contains test values for use in unit tests
    /// </summary>
    public static class TestInput
    {
        /// <summary>
        /// Test string constants
        /// </summary>
        public static class Strings
        {
            /// <summary>
            /// Test value representing a non-empty string
            /// </summary>
            public const string FULL = "TEST";

            /// <summary>
            /// Test value representing a zero length string
            /// </summary>
            public const string EMPTY = "";

            /// <summary>
            /// Test value representing a null string
            /// </summary>
            public const string NULL = null;
        }

        /// <summary>
        /// Test value for an object
        /// </summary>
        public static readonly object OBJECT = new object();
    }
}
