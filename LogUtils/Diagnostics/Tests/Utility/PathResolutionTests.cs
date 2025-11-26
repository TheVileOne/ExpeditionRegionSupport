using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;
using System.IO;
using System.Text;
using BepInExPath = LogUtils.Helpers.Paths.BepInEx;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal sealed class PathResolutionTests : TestCaseGroup, ITestable
    {
        internal const string TEST_NAME = "Test - Path Resolution";

        private TestCaseGroup pathConversionTests, pathDetectionTests;

        private TestCaseGroup activeTestGroup;

        public PathResolutionTests() : base(TEST_NAME)
        {
            pathConversionTests = new TestCaseGroup(this, "Test - Path Conversion");
            pathDetectionTests = new TestCaseGroup(this, "Test - Path Detection");
        }

        public void Test()
        {
            testRootFolderExistenceChecks();
            testSubrootFolderExistenceChecks();
            testPathConversion();
            testPathDetection();
            activeTestGroup = null;
        }

        private void testPathConversion()
        {
            Condition.Result.ResetCount();

            activeTestGroup = pathConversionTests;
            testKeywordsReturnCorrectPath();
            testEmptyPathsReturnCorrectPath();
            testAbsolutePathReturnsCorrectPath();
            testRelativePaths();
            testRelativePathsCaseInsensitive();
        }

        private void testRootFolderExistenceChecks()
        {
            //Confirm that the root directory itself fails the directory exists check
            AssertThat(Directory.Exists(RainWorldPath.ROOT_DIRECTORY)).IsFalse();
            AssertThat(Directory.Exists(PathUtils.PrependWithSeparator(RainWorldPath.ROOT_DIRECTORY))).IsFalse();
        }

        private void testSubrootFolderExistenceChecks()
        {
            //Confirm that the root directory itself fails the directory exists check
            AssertThat(Directory.Exists("StreamingAssets")).IsFalse();
            AssertThat(Directory.Exists(PathUtils.PrependWithSeparator("StreamingAssets"))).IsFalse();
        }

        private void testPathDetection()
        {
            Condition.Result.ResetCount();
            activeTestGroup = pathDetectionTests;
        }

        private void testKeywordsReturnCorrectPath()
        {
            string pathResult = getPathConversion(UtilityConsts.PathKeywords.ROOT);

            AssertPathsAreEqual(expectedPath: RainWorldPath.RootPath,
                                  actualPath: pathResult);

            pathResult = getPathConversion(UtilityConsts.PathKeywords.STREAMING_ASSETS);

            AssertPathsAreEqual(expectedPath: RainWorldPath.StreamingAssetsPath,
                                  actualPath: pathResult);
        }

        private void testEmptyPathsReturnCorrectPath()
        {
            string[] pathInputs = [null, string.Empty, " "]; //All three of these should return the same result

            foreach (string input in pathInputs)
            {
                string pathResult = getPathConversion(input);

                AssertPathsAreEqual(expectedPath: RainWorldPath.StreamingAssetsPath,
                                      actualPath: pathResult);
            }
        }

        private void testAbsolutePathReturnsCorrectPath()
        {
            //All absolute paths should be treated similarly - but lets test inside and outside of the root directory to be safe
            testPath(RainWorldPath.StreamingAssetsPath);
            testPath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

            void testPath(string testInput)
            {
                string pathResult = getPathConversion(testInput); //Output should be the same as the input
                AssertPathsAreEqual(expectedPath: testInput,
                                      actualPath: pathResult);
            }
        }

        private void testRelativePaths()
        {
            TestCase testCase = new TestCase(activeTestGroup, "Test: Relative paths");

            string relativeRoot = "Rain World",
                   relativeCustomRoot = "StreamingAssets",
                   relativeBepInExRoot = "BepInEx"; //This is actually RainWorld/BepInEx

            string testFolderName = "SomeFolder"; //Value represents any arbitrary directory that does not exist

            testPath(relativeRoot, RainWorldPath.RootPath);
            testPath(relativeCustomRoot, RainWorldPath.StreamingAssetsPath);
            testPath(relativeBepInExRoot, BepInExPath.RootPath);

            void testPath(string testInput, string expectedOutput)
            {
                //Root
                string pathResult = getPathConversion(testInput);
                testCase.AssertPathsAreEqual(expectedPath: expectedOutput,
                                               actualPath: pathResult);
                //Root/Subdirectory
                testInput = Path.Combine(testInput, testFolderName);
                expectedOutput = Path.Combine(expectedOutput, testFolderName);

                pathResult = getPathConversion(testInput);
                testCase.AssertPathsAreEqual(expectedPath: expectedOutput,
                                               actualPath: pathResult);
            }
        }

        private void testRelativePathsCaseInsensitive()
        {
            TestCase testCase = new TestCase(activeTestGroup, "Test: Relative paths (Case insensitive)");

            string relativeRoot = "Rain World",
                   relativeCustomRoot = "StreamingAssets",
                   relativeBepInExRoot = "BepInEx"; //This is actually RainWorld/BepInEx

            string testFolderName = "SomeFolder"; //Value represents any arbitrary directory that does not exist

            //This will create a case difference in each of these root directory names
            relativeRoot = relativeRoot.ToLower();
            relativeCustomRoot = relativeCustomRoot.ToLower();
            relativeBepInExRoot = relativeBepInExRoot.ToLower();

            testPath(relativeRoot, RainWorldPath.RootPath);
            testPath(relativeCustomRoot, RainWorldPath.StreamingAssetsPath);
            testPath(relativeBepInExRoot, BepInExPath.RootPath);

            void testPath(string testInput, string expectedOutput)
            {
                //Root
                string pathResult = getPathConversion(testInput);
                testCase.AssertPathsAreEqual(expectedPath: expectedOutput,
                                               actualPath: pathResult);
                //Root/Subdirectory
                testInput = Path.Combine(testInput, testFolderName);
                expectedOutput = Path.Combine(expectedOutput, testFolderName);

                pathResult = getPathConversion(testInput);
                testCase.AssertPathsAreEqual(expectedPath: expectedOutput,
                                               actualPath: pathResult);
            }
        }

        internal static void LogPaths()
        {
            string[] dirNames = ["Rain World", "BepInEx", "StreamingAssets"];

            StringBuilder builder = new StringBuilder();

            builder.AppendHeader("Full Path - Base");

            foreach (string dir in dirNames)
                builder.AppendLine(Path.GetFullPath(dir));

            builder.AppendHeader("Full Path - Rooted");

            foreach (string dir in dirNames)
                builder.AppendLine(Path.GetFullPath(PathUtils.PrependWithSeparator(dir)));

            builder.AppendHeader("Full Path - Relative");

            foreach (string dir in dirNames)
                builder.AppendLine(Path.GetFullPath("./" + dir)); //Repeating ./ synbols doesn't appear to change the path output

            builder.AppendHeader("Full Path - One Parent");

            foreach (string dir in dirNames)
                builder.AppendLine(Path.GetFullPath("../" + dir));

            builder.AppendHeader("Full Path - Two Parents");

            foreach (string dir in dirNames)
                builder.AppendLine(Path.GetFullPath("../../" + dir));

            builder.AppendHeader("Full Path - Three Parents");

            foreach (string dir in dirNames)
                builder.AppendLine(Path.GetFullPath("../../../" + dir));

            UtilityLogger.Log(builder.ToString());
        }

        private string getPathConversion(string pathInput)
        {
            return LogProperties.GetContainingPath(pathInput);
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
        }

        private static class Helper
        {

        }
    }
}
