using LogUtils.Helpers.Comparers;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal class ExceptionComparerTests : TestCase, ITestable
    {
        internal const string TEST_NAME = "Test - Exception Comparing";

        internal const int TEST_CHAR_LIMIT = 150;
        internal const int TEST_LINE_LIMIT = 5;
        internal const int TEST_INSERT_POSITION_MATCH = TEST_CHAR_LIMIT + 40; //This offset has to account for removed \r\n characters from the test data
        internal const int TEST_INSERT_POSITION_NO_MATCH = 40;

        public ExceptionComparerTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            testStringMatch_NoRestrictions();
            testStringMatch_CharLimit();
            testStringMatch_LineLimit();
            testStringMatch_BothCharAndLineLimits();
            testStringDoesNotMatch_NoRestrictions();
            testStringDoesNotMatch_CharLimit();
            testStringDoesNotMatch_LineLimit();
            testStringDoesNotMatch_BothCharAndLineLimits();

            TestLogger.LogDebug(CreateReport());
        }

        private void testStringMatch_NoRestrictions()
        {
            ExceptionComparer comparer = new ExceptionComparer(0, 0);

            var compareData = createTestData();
            createAndAssertResults(comparer, compareData, true);
        }

        private void testStringMatch_CharLimit()
        {
            ExceptionComparer comparer = new ExceptionComparer(0, TEST_CHAR_LIMIT);

            var compareData = createTestData(TEST_INSERT_POSITION_MATCH);
            createAndAssertResults(comparer, compareData, true);
        }

        private void testStringMatch_LineLimit()
        {
            ExceptionComparer comparer = new ExceptionComparer(TEST_LINE_LIMIT, 0);

            var compareData = createTestData(TEST_INSERT_POSITION_MATCH);
            createAndAssertResults(comparer, compareData, true);
        }

        private void testStringMatch_BothCharAndLineLimits()
        {
            ExceptionComparer comparer = new ExceptionComparer(TEST_LINE_LIMIT, TEST_CHAR_LIMIT);

            var compareData = createTestData(TEST_INSERT_POSITION_MATCH);
            createAndAssertResults(comparer, compareData, true);
        }

        private void testStringDoesNotMatch_NoRestrictions()
        {
            ExceptionComparer comparer = new ExceptionComparer(0, 0);

            var compareData = createTestData(TEST_INSERT_POSITION_NO_MATCH);
            createAndAssertResults(comparer, compareData, false);
        }

        private void testStringDoesNotMatch_CharLimit()
        {
            ExceptionComparer comparer = new ExceptionComparer(0, TEST_CHAR_LIMIT);

            var compareData = createTestData(TEST_INSERT_POSITION_NO_MATCH);
            createAndAssertResults(comparer, compareData, false);
        }

        private void testStringDoesNotMatch_LineLimit()
        {
            ExceptionComparer comparer = new ExceptionComparer(TEST_LINE_LIMIT, 0);

            var compareData = createTestData(TEST_INSERT_POSITION_NO_MATCH);
            createAndAssertResults(comparer, compareData, false);
        }

        private void testStringDoesNotMatch_BothCharAndLineLimits()
        {
            ExceptionComparer comparer = new ExceptionComparer(TEST_LINE_LIMIT, TEST_CHAR_LIMIT);

            var compareData = createTestData(TEST_INSERT_POSITION_NO_MATCH);
            createAndAssertResults(comparer, compareData, false);
        }

        private void createAndAssertResults(IEqualityComparer<ExceptionInfo> comparer, ExceptionInfo[] compareData, bool assertExpectation)
        {
            ExceptionInfo exceptionInfo, exceptionInfoOther;

            exceptionInfo = compareData[0];
            exceptionInfoOther = compareData[1];

            bool valuesEqual = comparer.Equals(exceptionInfo, exceptionInfoOther);

            int hashCode = comparer.GetHashCode(exceptionInfo),
                hashCodeOther = comparer.GetHashCode(exceptionInfoOther);

            bool hashCodesEqual = hashCode == hashCodeOther;

            AssertThat(valuesEqual).IsEqualTo(assertExpectation);
            AssertThat(hashCodesEqual).IsEqualTo(assertExpectation);
        }

        private ExceptionInfo[] createTestData(int insertPosition = -1)
        {
            string valueA, valueB;

            valueA = createLongString();
            valueB = insertPosition < 0 ? valueA : valueA.Insert(insertPosition, "TEST");

            ExceptionInfo[] testData = new ExceptionInfo[2];

            testData[0] = new ExceptionInfo(valueA, valueA);
            testData[1] = new ExceptionInfo(valueB, valueB);
            return testData;
        }

        private string createLongString()
        {
            StringBuilder sb = new StringBuilder(1000);

            sb.AppendLine("------------------------LONG-----")
              .AppendLine("--------------------LONG------------------------------")
              .AppendLine("-----LONG----")
              .AppendLine("--------------------LONG-----------------")
              .AppendLine("------------------------LONG-----")
              .AppendLine("-------------------------------------------LONG------------------------")
              .AppendLine("--------------------------------LONG----------------------------")
              .AppendLine("------------------------LONG-----")
              .AppendLine("--------------------LONG------------------------------")
              .AppendLine("-----LONG----")
              .AppendLine("------------------------LONG-----")
              .AppendLine("--------------------LONG------------------------------")
              .AppendLine("-------------------------------------LONG----")
              .AppendLine("------------------------LONG-------------------")
              .AppendLine("--------------------LONG------------------------------")
              .AppendLine("-----LONG-----------------------")
              .AppendLine("------------------------LONG-----")
              .AppendLine("--------------------LONG----------------------------------------")
              .AppendLine("-----LONG------")
              .AppendLine("------------------------LONG-----")
              .AppendLine("--------------------LONG------------------------------")
              .AppendLine("-------------------------------------LONG----")
              .AppendLine("------------------------LONG-------------------")
              .AppendLine("--------------------LONG------------------------------")
              .AppendLine("------------------------LONG-----")
              .AppendLine("-------LONG------------------------------")
              .AppendLine("-------------------------------------LONG----")
              .AppendLine("-------------------LONG-------------------")
              .AppendLine("--------------------LONG------------------------------")
              .AppendLine("------------------------LONG-----")
              .AppendLine("---------LONG---------------------")
              .AppendLine("-------------------------------------LONG----")
              .AppendLine("------------------------LONG-------------------")
              .AppendLine("-----------LONG-----------------");
            return sb.ToString();
        }
    }
}
