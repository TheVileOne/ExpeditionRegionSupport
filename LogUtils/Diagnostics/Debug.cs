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

            AssertTest assertTest = new AssertTest();
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
            /// A flag that affects whether the total number of test conditions is always explictly included in the result report
            /// Default: false
            /// </summary>
            public static bool AlwaysReportResultTotal = false;
        }
    }
}
