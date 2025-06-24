using ReportVerbosity = LogUtils.Enums.FormatEnums.FormatVerbosity;

namespace LogUtils.Policy
{
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
        public static ReportVerbosity ReportVerbosity = ReportVerbosity.Standard;
    }
}
