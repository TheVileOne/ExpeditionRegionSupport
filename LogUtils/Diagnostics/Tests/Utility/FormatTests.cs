using LogUtils.Console;
using LogUtils.Formatting;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal sealed class FormatTests : TestCase, ITestable
    {
        internal const string TEST_NAME = "Test - Color Formatting";
        internal static readonly Color TEST_COLOR = Color.red;

        public FormatTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            Condition.Result.ResetCount();

            testEmptyFormatRemovesColorData();
            testAnsiCodeReplacesColorData();
            testFormatImplementationsProduceSameResult();
            testAnsiCodeTerminatesAtCorrectPosition();
            testAnsiCodeTerminationSkipsOverUnviewableCharacters();
            testAnsiCodeTerminationRespectsColorBoundaries();
            testFormatStringContainsPlaceholderFormatting();
        }

        private void testEmptyFormatRemovesColorData()
        {
            const string TEST_FORMAT = "{0}";

            FormattableString testDataA;
            InterpolatedStringHandler testDataB;

            createTestData();

            EmptyColorFormatProvider formatProvider = new EmptyColorFormatProvider();

            AssertThat(string.IsNullOrEmpty(testDataA.ToString(formatProvider))).IsTrue();
            AssertThat(string.IsNullOrEmpty(testDataB.ToString(formatProvider))).IsTrue();

            void createTestData()
            {
                testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR);
                testDataB = new InterpolatedStringHandler();
                testDataB.AppendFormatted(TEST_COLOR);
            }
        }

        private void testAnsiCodeReplacesColorData()
        {
            //Test data provides formatting at the beginning, middle, and end of a string
            const string TEST_FORMAT = "{0}test{0} result{0}";

            FormattableString testDataA;
            InterpolatedStringHandler testDataB;

            createTestData();

            AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

            string resultA = testDataA.ToString(formatProvider);
            string resultB = testDataB.ToString(formatProvider);

            //All three placeholder arguments should now be replaced with an Ansi color code
            testAnsiCodeAmount(resultA, resultB, expectedAnsiCodeAmount: 6);

            void createTestData()
            {
                testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR);
                testDataB = new InterpolatedStringHandler();
                testDataB.AppendFormatted(TEST_COLOR);
                testDataB.AppendLiteral("test");
                testDataB.AppendFormatted(TEST_COLOR);
                testDataB.AppendLiteral(" result");
                testDataB.AppendFormatted(TEST_COLOR);
            }
        }

        private void testFormatImplementationsProduceSameResult()
        {
            //Test data provides formatting at the beginning, middle, and end of a string
            const string TEST_FORMAT = "{0}test{0} result{0}";

            FormattableString testDataA;
            InterpolatedStringHandler testDataB;

            createTestData();

            AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

            //Both implementations are expected to convert the string into the same output
            string resultA = testDataA.ToString(formatProvider);
            string resultB = testDataB.ToString(formatProvider);

            AssertThat(resultA).IsEqualTo(resultB);

            void createTestData()
            {
                testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR);
                testDataB = new InterpolatedStringHandler();
                testDataB.AppendFormatted(TEST_COLOR);
                testDataB.AppendLiteral("test");
                testDataB.AppendFormatted(TEST_COLOR);
                testDataB.AppendLiteral(" result");
                testDataB.AppendFormatted(TEST_COLOR);
            }
        }

        private void testAnsiCodeTerminatesAtCorrectPosition()
        {
            const byte COLOR_RANGE = 4;
            const string TEST_FORMAT = "{0,4}test result";

            FormattableString testDataA;
            InterpolatedStringHandler testDataB;

            createTestData();

            AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

            //The first four characters of the result string should be followed with an Ansi color code
            string resultA = testDataA.ToString(formatProvider);
            string resultB = testDataB.ToString(formatProvider);

            AssertThat(resultA[resultA.IndexOf("test") + COLOR_RANGE]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);
            AssertThat(resultB[resultB.IndexOf("test") + COLOR_RANGE]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);

            void createTestData()
            {
                testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR);
                testDataB = new InterpolatedStringHandler();
                testDataB.AppendFormatted(TEST_COLOR, COLOR_RANGE);
                testDataB.AppendLiteral("test result");
            }
        }

        private void testAnsiCodeTerminationSkipsOverUnviewableCharacters()
        {
            const byte COLOR_RANGE = 6;
            const byte EXPECTED_SKIPPED_CHARACTER_AMOUNT = 2;
            const byte EXPECTED_COLOR_RANGE = COLOR_RANGE + EXPECTED_SKIPPED_CHARACTER_AMOUNT;
            const string TEST_FORMAT = "{0,6}t e-s\r\\t result"; //'\r' is not viewable, but `\\` is

            FormattableString testDataA;
            InterpolatedStringHandler testDataB;

            createTestData();

            AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

            //The first four characters of the result string should be followed with an Ansi color code
            string resultA = testDataA.ToString(formatProvider);
            string resultB = testDataB.ToString(formatProvider);

            AssertThat(resultA[resultA.IndexOf("t e-s\r\\t") + EXPECTED_COLOR_RANGE]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);
            AssertThat(resultB[resultB.IndexOf("t e-s\r\\t") + EXPECTED_COLOR_RANGE]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);

            void createTestData()
            {
                testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR);
                testDataB = new InterpolatedStringHandler();
                testDataB.AppendFormatted(TEST_COLOR, COLOR_RANGE);
                testDataB.AppendLiteral("t e-s\r\\t result");
            }
        }

        private void testAnsiCodeTerminationRespectsColorBoundaries()
        {
            const byte COLOR_RANGE = 4;
            Color TEST_COLOR_ALT = Color.blue;

            FormattableString testDataA;
            InterpolatedStringHandler testDataB;

            testNonTerminatingExample();
            testTerminatingExample();

            void testNonTerminatingExample()
            {
                createTestData();

                AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

                string resultA = testDataA.ToString(formatProvider);
                string resultB = testDataB.ToString(formatProvider);

                processResults(resultA, resultB, expectedAnsiCodeAmount: 4);

                void createTestData()
                {
                    const string TEST_FORMAT = "{0,4}tes{1}t result";

                    testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR, TEST_COLOR_ALT);
                    testDataB = new InterpolatedStringHandler();
                    testDataB.AppendFormatted(TEST_COLOR, COLOR_RANGE);
                    testDataB.AppendLiteral("tes");
                    testDataB.AppendFormatted(TEST_COLOR_ALT);
                    testDataB.AppendLiteral("t result");
                }
            }

            void testTerminatingExample()
            {
                createTestData();

                AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

                string resultA = testDataA.ToString(formatProvider);
                string resultB = testDataB.ToString(formatProvider);

                processResults(resultA, resultB, expectedAnsiCodeAmount: 6);

                void createTestData()
                {
                    const string TEST_FORMAT = "{0,4}tes{1,2}t result";

                    testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR, TEST_COLOR_ALT);
                    testDataB = new InterpolatedStringHandler();
                    testDataB.AppendFormatted(TEST_COLOR, COLOR_RANGE);
                    testDataB.AppendLiteral("tes");
                    testDataB.AppendFormatted(TEST_COLOR_ALT, 2);
                    testDataB.AppendLiteral("t result");
                }
            }

            void processResults(string resultA, string resultB, int expectedAnsiCodeAmount)
            {

                //Check that ANSI terminator exists where second color was included
                AssertThat(resultA[resultA.IndexOf("tes") + "tes".Length]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);
                AssertThat(resultB[resultB.IndexOf("tes") + "tes".Length]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);

                testAnsiCodeAmount(resultA, resultB, expectedAnsiCodeAmount);
            }
        }

        private void testAnsiCodeAmount(string resultA, string resultB, int expectedAnsiCodeAmount)
        {
            //Check that correct amount of ANSI escape characters are present
            int totalCodesDetected = resultA.Count(c => c == AnsiColorConverter.ANSI_ESCAPE_CHAR)
                                   + resultB.Count(c => c == AnsiColorConverter.ANSI_ESCAPE_CHAR);

            AssertThat(totalCodesDetected).IsEqualTo(expectedAnsiCodeAmount);
        }

        private void testFormatStringContainsPlaceholderFormatting()
        {
            InterpolatedStringHandler testData = $"Test format should contain {1} format arguments";
            AssertThat(testData.Format.Contains("{0}")).IsTrue();
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
        }
    }
}
