using LogUtils.Diagnostics.Tests;
using LogUtils.Diagnostics.Tests.Utility;
using LogUtils.Enums;
using LogUtils.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReportVerbosity = LogUtils.Enums.FormatEnums.FormatVerbosity;

namespace LogUtils.Diagnostics
{
    public static class Debug
    {
        /// <summary>
        /// The maximum amount of time (in milliseconds) that a logging thread update can experience without triggering a slow update report message
        /// Set to 25 by default (time windows shorter than this will be subject to context switching delays)
        /// </summary>
        public static int LogFrameReportThreshold = 25;

        /// <summary>
        /// When set to true, ignores the <see cref="DebugContext"/> information when determining whether an assert is considered a test condition
        /// </summary>
        public static bool AcceptTestResultsFromAnyContext;

        /// <summary>
        /// When set to true, assert results will not be assigned an identifier
        /// </summary>
        public static bool SuppressTestResultCount;

        /// <summary>
        /// Context flag, typically used for assert processing
        /// </summary>
        public static DebugContext LastKnownContext;

        internal static TestSuite UtilityTests;

        internal static void InitializeTestSuite()
        {
            UtilityTests = new TestSuite();
            UtilityTests.AddTests(UtilityCore.Assembly);
        }

        internal static void RunTests()
        {
            UtilityTests.RunAllTests();
            //Task.Run(StressTests.TestMultithreadedLogging);
            //StressTests.TestLoggerDisposal();
            //StressTests.LogEveryFrame(LogID.Unity, messageFrequency: 1, logUntilThisFrame: 100000, messagesPerFrame: 100);
            //TestLogsFolder();
            //StressTests.TestThreadSafeWorker();
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

            report.AppendHeader("Log Request report");
            if (requests.Any())
            {
                foreach (LogRequest request in requests)
                    report.AppendLine(request.ToString(ReportVerbosity.Verbose));
            }
            else
            {
                report.AppendLine("No requests to show");
            }
            report.AppendHeader("End of report");

            DiscreteLogger logger = new DiscreteLogger(reportFile);

            logger.LogDebug(report.ToString());
            logger.Dispose();
        }

        internal static void TestMultipleWritersOneFile()
        {
            Logger t1, t2, t3;

            LogID testLogID = new LogID("test.log", UtilityConsts.PathKeywords.ROOT, LogAccess.FullAccess, false);

            t1 = new Logger(LoggingMode.Timed, testLogID);
            t2 = new Logger(LoggingMode.Queue, testLogID);
            t3 = new Logger(LoggingMode.Normal, testLogID);

            //These logs will not log to file properly. Writers are not aware of when other writers are flushing to the stream
            t1.Log("Message logged using timed writer");
            t2.Log("Message logged using queue writer");
            t3.Log("Message logged using standard writer");
            t1.Log("This message will break LogUtils");
        }

        internal static void TestLogsFolder()
        {
            UtilityCore.Scheduler.Schedule(() =>
            {
                try
                {
                    //Alternate between having log files in the Logs folder, and having them at their original location
                    if (!LogsFolder.IsManagingFiles)
                    {
                        UtilityLogger.DebugLog("Enabled");
                        LogsFolder.MoveFilesToFolder();
                    }
                    else
                    {
                        UtilityLogger.DebugLog("Disabled");
                        LogsFolder.RestoreFiles();
                    }
                }
                catch (Exception ex)
                {
                    UtilityLogger.DebugLog(ex);
                }
            }, frameInterval: 200);
        }
    }
}
