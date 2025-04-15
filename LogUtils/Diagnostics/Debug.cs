using LogUtils.Diagnostics.Tests;
using LogUtils.Diagnostics.Tests.Utility;
using LogUtils.Enums;
using LogUtils.Helpers;
using LogUtils.Requests;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReportVerbosity = LogUtils.Enums.FormatEnums.FormatVerbosity;

namespace LogUtils.Diagnostics
{
    public static class Debug
    {
        public static bool AssertsEnabled = true;

        /// <summary>
        /// The maximum amount of time (in milliseconds) that a logging thread update can experience without triggering a slow update report message
        /// Set to 25 by default (time windows shorter than this will be subject to context switching delays)
        /// </summary>
        public static int LogFrameReportThreshold = 25;

        internal static TestSuite UtilityTests;

        internal static void InitializeTestSuite()
        {
            UtilityTests = new TestSuite();

            UtilityTests.Add(new AssertTests());
            UtilityTests.Add(new LoggerTests());
            UtilityTests.Add(new LogCategoryTests());
            UtilityTests.Add(new ExceptionComparerTests());
            UtilityTests.Add(new FrameTimerTests());
            UtilityTests.Add(new ExtEnumTests());
            UtilityTests.Add(new LogIDTests.ComparisonTests());
        }

        internal static void RunTests()
        {
            UtilityTests.RunAllTests();
            //StressTests.LogEveryFrame(LogID.Unity, messageFrequency: 1, logUntilThisFrame: 100000, messagesPerFrame: 100);
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
                    report.AppendLine(request.ToString(ReportVerbosity.Verbose));
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
