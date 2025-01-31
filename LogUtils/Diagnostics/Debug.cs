using LogUtils.Diagnostics.Tests;
using LogUtils.Diagnostics.Tests.Utility;
using LogUtils.Enums;
using LogUtils.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReportVerbosity = LogUtils.Enums.FormatEnums.FormatVerbosity;

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

        /// <summary>
        /// Logs requests to file in a report style format
        /// </summary>
        /// <param name="reportFile">The log file to write the report to</param>
        /// <param name="requests">The objects to log</param>
        public static void LogRequestInfo(LogID reportFile, IEnumerable<LogRequest> requests)
        {
            StringBuilder report = new StringBuilder();

            report.AppendLine();

            FormatUtils.CreateHeader(report, "Log Request report");
            if (requests.Any())
            {
                foreach (LogRequest request in requests)
                    report.AppendLine(request.ToString(FormatEnums.FormatVerbosity.Verbose));
            }
            else
            {
                report.AppendLine("No requests to show");
            }
            FormatUtils.CreateHeader(report, "End of report");

            DiscreteLogger logger = new DiscreteLogger(reportFile);

            logger.LogDebug(report.ToString());
            logger.Dispose();
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
            public static ReportVerbosity ReportVerbosity = ReportVerbosity.Standard;
        }
    }
}
