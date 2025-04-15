using LogUtils.Diagnostics.Tests.Components;
using LogUtils.Requests;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal sealed class LoggerTests : TestCaseGroup, ITest
    {
        internal const string TEST_NAME = "Test - Logging System";

        private TestCaseGroup singleLoggerTests, multiLoggerTests;

        private TestCaseGroup activeTestGroup;

        public LoggerTests() : base(TEST_NAME)
        {
            singleLoggerTests = new TestCaseGroup(this, "Test - Single Logger");
            multiLoggerTests = new TestCaseGroup(this, "Test - Multiple Loggers");
        }

        public void PreTest()
        {
        }

        public void Test()
        {
            runSingleLoggerTests();
            runMultiLoggerTests();
        }

        public void PostTest()
        {
            TestLogger.LogDebug(CreateReport());
            activeTestGroup = null;
        }

        private void runSingleLoggerTests()
        {
            activeTestGroup = singleLoggerTests;
            testLogRequestSubmissionFromLogger();
        }

        private void runMultiLoggerTests()
        {
            activeTestGroup = multiLoggerTests;
        }

        private void testLogRequestSubmissionFromLogger()
        {
            var testWriter = new FakeLogWriter();
            var testLogID = new TestLogID();
            Logger logger = new Logger(testLogID)
            {
                Writer = testWriter
            };

            string fullStringInput = "test",
                   emptyStringInput = string.Empty,
                   nullInput = null;
            object objectInput = logger;

            LogRequest resultFromFullString,
                       resultFromEmptyString,
                       resultFromNullParam,
                       resultFromObjectParam;

            //Non-empty string test
            logger.Log(fullStringInput);
            resultFromFullString = testWriter.LatestRequest;
            
            //Empty string test
            logger.Log(emptyStringInput);
            resultFromEmptyString = testWriter.LatestRequest;

            //Null test
            logger.Log(nullInput);
            resultFromNullParam = testWriter.LatestRequest;
            
            //Object test
            logger.Log(objectInput);
            resultFromObjectParam = testWriter.LatestRequest;

            AssertThat(resultFromFullString.Data.Message).IsEqualTo(fullStringInput);
            AssertThat(resultFromEmptyString.Data.Message).IsEqualTo(string.Empty);
            AssertThat(resultFromNullParam.Data.Message).IsEqualTo(string.Empty); //Null translates to string.Empty
            AssertThat(resultFromObjectParam.Data.Message).IsEqualTo(objectInput.ToString());

            foreach (var request in testWriter.ReceivedRequests)
            {
                AssertThat(request.Submitted).IsTrue();
                AssertThat(request.Status).IsEqualTo(RequestStatus.Pending);
                AssertThat(request.Type).IsEqualTo(RequestType.Local);
                AssertThat(request.UnhandledReason).IsEqualTo(RejectionReason.None);
            }
        }
    }
}
