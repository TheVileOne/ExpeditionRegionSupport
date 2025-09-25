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

        internal readonly Color TEST_COLOR = Color.red;

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
        }

        private void testEmptyFormatRemovesColorData()
        {
            const string TEST_FORMAT = "{0}";

            FormattableString testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR);
            InterpolatedStringHandler testDataB = new InterpolatedStringHandler(literalLength: 0, formattedCount: 1);
            testDataB.AppendFormatted(TEST_COLOR);

            EmptyColorFormatProvider formatProvider = new EmptyColorFormatProvider();

            AssertThat(string.IsNullOrEmpty(testDataA.ToString(formatProvider))).IsTrue();
            AssertThat(string.IsNullOrEmpty(testDataB.ToString(formatProvider))).IsTrue();
        }

        private void testAnsiCodeReplacesColorData()
        {
            //Test data provides formatting at the beginning, middle, and end of a string
            const string TEST_FORMAT = "{0}test{0} result{0}";

            FormattableString testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR);
            InterpolatedStringHandler testDataB = new InterpolatedStringHandler(literalLength: 11, formattedCount: 3);
            testDataB.AppendFormatted(TEST_COLOR);
            testDataB.AppendLiteral("test");
            testDataB.AppendFormatted(TEST_COLOR);
            testDataB.AppendLiteral(" result");
            testDataB.AppendFormatted(TEST_COLOR);

            AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

            string resultA = testDataA.ToString(formatProvider);
            string resultB = testDataB.ToString(formatProvider);

            //All three placeholder arguments should now be replaced with an Ansi color code
            int totalCodesDetected = resultA.Count(c => c == AnsiColorConverter.ANSI_ESCAPE_CHAR)
                                   + resultB.Count(c => c == AnsiColorConverter.ANSI_ESCAPE_CHAR);

            AssertThat(totalCodesDetected).IsEqualTo(6); //3 x 2
        }

        private void testFormatImplementationsProduceSameResult()
        {
            //Test data provides formatting at the beginning, middle, and end of a string
            const string TEST_FORMAT = "{0}test{0} result{0}";

            FormattableString testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR);
            InterpolatedStringHandler testDataB = new InterpolatedStringHandler(literalLength: 11, formattedCount: 3);
            testDataB.AppendFormatted(TEST_COLOR);
            testDataB.AppendLiteral("test");
            testDataB.AppendFormatted(TEST_COLOR);
            testDataB.AppendLiteral(" result");
            testDataB.AppendFormatted(TEST_COLOR);

            AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

            //Both implementations are expected to convert the string into the same output
            string resultA = testDataA.ToString(formatProvider);
            string resultB = testDataB.ToString(formatProvider);

            AssertThat(resultA).IsEqualTo(resultB);
        }

        private void testAnsiCodeTerminatesAtCorrectPosition()
        {
            const byte COLOR_RANGE = 4;
            const string TEST_FORMAT = "{0,4}test result";

            FormattableString testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR);
            InterpolatedStringHandler testDataB = new InterpolatedStringHandler(literalLength: 11, formattedCount: 1);
            testDataB.AppendFormatted(TEST_COLOR, COLOR_RANGE);
            testDataB.AppendLiteral("test result");

            AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

            //The first four characters of the result string should be followed with an Ansi color code
            string resultA = testDataA.ToString(formatProvider);
            string resultB = testDataB.ToString(formatProvider);

            //UtilityLogger.Log("TEST INDEX: " + (resultB.LastIndexOf("test") + 1));
            //UtilityLogger.Log("TEST STRING: " + (resultB));

            //for (int i = 0; i < resultB.Length; i++)
            //    UtilityLogger.Log("ch " + resultB[i]);

            AssertThat(resultA[resultA.IndexOf("test") + COLOR_RANGE]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);
            AssertThat(resultB[resultB.IndexOf("test") + COLOR_RANGE]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);
        }

        private void testAnsiCodeTerminationSkipsOverUnviewableCharacters()
        {
            const byte COLOR_RANGE = 6;
            const byte EXPECTED_SKIPPED_CHARACTER_AMOUNT = 2;
            const byte EXPECTED_COLOR_RANGE = COLOR_RANGE + EXPECTED_SKIPPED_CHARACTER_AMOUNT;
            const string TEST_FORMAT = "{0,6}t e-s\r\\t result"; //'\r' is not viewable, but `\\` is

            FormattableString testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR);
            InterpolatedStringHandler testDataB = new InterpolatedStringHandler(literalLength: 15, formattedCount: 1);
            testDataB.AppendFormatted(TEST_COLOR, COLOR_RANGE);
            testDataB.AppendLiteral("t e-s\r\\t result");

            AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

            //The first four characters of the result string should be followed with an Ansi color code
            string resultA = testDataA.ToString(formatProvider);
            string resultB = testDataB.ToString(formatProvider);

            //UtilityLogger.Log("TEST INDEX: " + (resultA.LastIndexOf("t e-s\r\\t") + 1));
            //UtilityLogger.Log("TEST STRING: " + (resultA));

            //for (int i = 0; i < resultA.Length; i++)
            //    UtilityLogger.Log("ch " + resultA[i]);

            AssertThat(resultA[resultA.IndexOf("t e-s\r\\t") + EXPECTED_COLOR_RANGE]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);
            AssertThat(resultB[resultB.IndexOf("t e-s\r\\t") + EXPECTED_COLOR_RANGE]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);
        }

        private void testAnsiCodeTerminationRespectsColorBoundaries()
        {
            const byte COLOR_RANGE = 4;
            const string TEST_FORMAT = "{0,4}tes{1}t result";
            const string TEST_FORMAT_ALT = "{0,4}tes{1,2}t result";

            Color testColorAlt = Color.blue;

            testNonTerminatingExample();
            testTerminatingExample();

            void testNonTerminatingExample()
            {
                FormattableString testDataA = FormattableStringFactory.Create(TEST_FORMAT, TEST_COLOR, testColorAlt);
                InterpolatedStringHandler testDataB = new InterpolatedStringHandler(literalLength: 11, formattedCount: 2);
                testDataB.AppendFormatted(TEST_COLOR, COLOR_RANGE);
                testDataB.AppendLiteral("tes");
                testDataB.AppendFormatted(testColorAlt);
                testDataB.AppendLiteral("t result");

                AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

                //The first four characters of the result string should be followed with an Ansi color code
                string resultA = testDataA.ToString(formatProvider);
                string resultB = testDataB.ToString(formatProvider);

                //UtilityLogger.Log("TEST INDEX: " + (resultB.LastIndexOf("test") + 1));
                //UtilityLogger.Log("TEST STRING: " + (resultB));

                //for (int i = 0; i < resultB.Length; i++)
                //    UtilityLogger.Log("ch " + resultB[i]);

                //Check that ANSI terminator exists where second color was included
                AssertThat(resultA[resultA.IndexOf("tes") + "tes".Length]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);
                AssertThat(resultB[resultB.IndexOf("tes") + "tes".Length]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);

                //Check that correct amount of ANSI escape characterttts are present

                //All three placeholder arguments should now be replaced with an Ansi color code
                int totalCodesDetected = resultA.Count(c => c == AnsiColorConverter.ANSI_ESCAPE_CHAR)
                                       + resultB.Count(c => c == AnsiColorConverter.ANSI_ESCAPE_CHAR);

                AssertThat(totalCodesDetected).IsEqualTo(4); //2 x 2 - There should be no termination when there is an interception by another color
            }

            void testTerminatingExample()
            {
                FormattableString testDataA = FormattableStringFactory.Create(TEST_FORMAT_ALT, TEST_COLOR, testColorAlt);
                InterpolatedStringHandler testDataB = new InterpolatedStringHandler(literalLength: 11, formattedCount: 2);
                testDataB.AppendFormatted(TEST_COLOR, COLOR_RANGE);
                testDataB.AppendLiteral("tes");
                testDataB.AppendFormatted(testColorAlt, 2);
                testDataB.AppendLiteral("t result");

                AnsiColorFormatProvider formatProvider = new AnsiColorFormatProvider();

                //The first four characters of the result string should be followed with an Ansi color code
                string resultA = testDataA.ToString(formatProvider);
                string resultB = testDataB.ToString(formatProvider);

                //UtilityLogger.Log("TEST INDEX: " + (resultB.LastIndexOf("test") + 1));
                //UtilityLogger.Log("TEST STRING: " + (resultB));

                //for (int i = 0; i < resultB.Length; i++)
                //    UtilityLogger.Log("ch " + resultB[i]);

                //Check that ANSI terminator exists where second color was included
                AssertThat(resultA[resultA.IndexOf("tes") + "tes".Length]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);
                AssertThat(resultB[resultB.IndexOf("tes") + "tes".Length]).IsEqualTo(AnsiColorConverter.ANSI_ESCAPE_CHAR);

                //Check that correct amount of ANSI escape characterttts are present

                //All three placeholder arguments should now be replaced with an Ansi color code
                int totalCodesDetected = resultA.Count(c => c == AnsiColorConverter.ANSI_ESCAPE_CHAR)
                                       + resultB.Count(c => c == AnsiColorConverter.ANSI_ESCAPE_CHAR);

                AssertThat(totalCodesDetected).IsEqualTo(6); //3 x 2
            }
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
        }
    }
}
