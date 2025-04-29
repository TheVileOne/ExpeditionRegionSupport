using LogUtils.Diagnostics.Tests.Components;
using LogUtils.Enums;
using LogUtils.Requests;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal sealed class LoggerTests : TestCaseGroup, ITestable
    {
        internal const string TEST_NAME = "Test - Logging System";

        private TestCaseGroup singleLoggerTests, multiLoggerTests;

        private TestCaseGroup activeTestGroup;

        public LoggerTests() : base(TEST_NAME)
        {
            singleLoggerTests = new TestCaseGroup(this, "Test - Single Logger");
            multiLoggerTests = new TestCaseGroup(this, "Test - Multiple Loggers");
        }

        public void Test()
        {
            runSingleLoggerTests();
            runMultiLoggerTests();
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
            activeTestGroup = null;
        }

        private void runSingleLoggerTests()
        {
            activeTestGroup = singleLoggerTests;
            testLogRequestSubmissionFromLogger();
            testLogRequestsObeyEnabledState();
        }

        private void runMultiLoggerTests()
        {
            activeTestGroup = multiLoggerTests;
            testConflictResolution();
        }

        private void testLogRequestSubmissionFromLogger()
        {
            Logger logger = createLogger(new TestLogID());
            var testWriter = (FakeLogWriter)logger.Writer;

            LogRequest resultFromFullString,
                       resultFromEmptyString,
                       resultFromNullParam,
                       resultFromObjectParam;

            //Non-empty string test
            logger.Log(TestInput.Strings.FULL);
            resultFromFullString = testWriter.LatestRequest;
            
            //Empty string test
            logger.Log(TestInput.Strings.EMPTY);
            resultFromEmptyString = testWriter.LatestRequest;

            //Null test
            logger.Log(TestInput.Strings.NULL);
            resultFromNullParam = testWriter.LatestRequest;
            
            //Object test
            logger.Log(TestInput.OBJECT);
            resultFromObjectParam = testWriter.LatestRequest;

            activeTestGroup.AssertThat(resultFromFullString.Data.Message).IsEqualTo(TestInput.Strings.FULL);
            activeTestGroup.AssertThat(resultFromEmptyString.Data.Message).IsEqualTo(TestInput.Strings.EMPTY);
            activeTestGroup.AssertThat(resultFromNullParam.Data.Message).IsEqualTo(TestInput.Strings.EMPTY); //Null translates to string.Empty
            activeTestGroup.AssertThat(resultFromObjectParam.Data.Message).IsEqualTo(TestInput.OBJECT.ToString());

            foreach (var request in testWriter.ReceivedRequests)
            {
                activeTestGroup.AssertThat(request.Submitted).IsTrue();
                activeTestGroup.AssertThat(request.Status).IsEqualTo(RequestStatus.Pending);
                activeTestGroup.AssertThat(request.Type).IsEqualTo(RequestType.Local);
                activeTestGroup.AssertThat(request.UnhandledReason).IsEqualTo(RejectionReason.None);
            }

            logger.Dispose();
        }

        public void testLogRequestsObeyEnabledState()
        {
            TestLogID testLogID = new TestLogID();

            testLogID.Properties.AllowLogging = false;

            Logger logger = createLogger(testLogID);
            var testWriter = (FakeLogWriter)logger.Writer;

            logger.Log("test");

            //Enabled state should prevent any LogRequests from making it to the writer
            activeTestGroup.AssertThat(testWriter.LatestRequest).IsNull();
            logger.Dispose();
        }

        /// <summary>
        /// Tests related to ensuring that LogIDs with equivalent filename, but different paths make it to the correct Logger instances
        /// </summary>
        public void testConflictResolution()
        {
            //Represent two log files with the same filename, but different paths
            TestLogID targetA = TestLogID.FromPath(UtilityConsts.PathKeywords.ROOT),
                      targetB = TestLogID.FromTarget(targetA, UtilityConsts.PathKeywords.STREAMING_ASSETS);

            Logger loggerA = createLogger(targetA),
                   loggerB = createLogger(targetB);

            FakeLogWriter writerA = (FakeLogWriter)loggerA.Writer,
                          writerB = (FakeLogWriter)loggerB.Writer;

            string messageA = "A",
                   messageB = "B";

            testCorrectLoggerIsChosen_SelfTarget();
            testCorrectLoggerIsChosen_CrossTarget();

            loggerA.Dispose();
            loggerB.Dispose();

            void testCorrectLoggerIsChosen_SelfTarget()
            {
                loggerA.Log(messageA);
                loggerB.Log(messageB);

                activeTestGroup.AssertThat(writerA.LatestRequest.Data.Message).IsEqualTo(messageA);
                activeTestGroup.AssertThat(writerB.LatestRequest.Data.Message).IsEqualTo(messageB);

                //Test that specifying the same target has the same behavior
                loggerA.Log(targetA, messageA);
                loggerB.Log(targetB, messageB);

                activeTestGroup.AssertThat(writerA.LatestRequest.Data.Message).IsEqualTo(messageA);
                activeTestGroup.AssertThat(writerB.LatestRequest.Data.Message).IsEqualTo(messageB);

                writerA.ReceivedRequests.Clear();
                writerB.ReceivedRequests.Clear();
            }

            void testCorrectLoggerIsChosen_CrossTarget()
            {
                loggerA.Log(targetB, messageB);
                loggerB.Log(targetA, messageA);

                activeTestGroup.AssertThat(writerA.LatestRequest.Data.Message).IsEqualTo(messageA);
                activeTestGroup.AssertThat(writerB.LatestRequest.Data.Message).IsEqualTo(messageB);

                writerA.ReceivedRequests.Clear();
                writerB.ReceivedRequests.Clear();
            }
        }

        private Logger createLogger(params LogID[] logTargets)
        {
            var testWriter = new FakeLogWriter();

            Logger logger = new Logger(logTargets)
            {
                Writer = testWriter
            };
            return logger;
        }
    }
}
