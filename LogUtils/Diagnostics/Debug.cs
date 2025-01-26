using LogUtils.Diagnostics.Tests;
using LogUtils.Diagnostics.Tests.Utility;

namespace LogUtils.Diagnostics
{
    public static class Debug
    {
        public static bool AssertsEnabled = true;

        internal static TestSuite UtilityTests;

        internal static void InitializeTestSuite()
        {
            UtilityTests = new TestSuite();

            AssertTests assertTest = new AssertTests();
            UtilityTests.Add(assertTest);
        }

        public static class TestCasePolicy
        {
            /// <summary>
            /// A flag that affects whether failed expectations qualify as a failure result
            /// Default: true
            /// </summary>
            public static bool PreferExpectationsAsFailures = true;

            /// <summary>
            /// A flag that affects whether all failure results are reported, or only the unexpected ones
            /// Default: false
            /// </summary>
            public static bool FailuresAreAlwaysReported = false;

            /// <summary>
            /// This field affects the level of detail revealed in the test case report
            /// Default: Standard
            /// </summary>
            public static TestCase.ReportVerbosity ReportVerbosity = TestCase.ReportVerbosity.Standard;
        }
    }
}
