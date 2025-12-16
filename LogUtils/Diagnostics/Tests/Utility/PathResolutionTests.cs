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
            /*
             * Test Notes
             * A path must be a full path for an exists check to pass.
             * Path.GetFullPath expands relative paths if they exist and always appends working directory when not an absolute path. It will not fill in the missing gaps in the path. 
             */
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
            testPartialPaths();
            testPartialPathsCaseInsensitive();
        }

        private void testPathDetection()
        {
            Condition.Result.ResetCount();
            activeTestGroup = pathDetectionTests;

            //Test that we can walk along the dir tree and find the correct node
            testPositionLookup("AssetBundles", Path.Combine(RainWorldPath.StreamingAssetsPath, "AssetBundles"));
            testPositionLookup("StreamingAssets/music", Path.Combine(RainWorldPath.StreamingAssetsPath, "music"));
            testPositionLookup("StreamingAssets/world/ds", Path.Combine(RainWorldPath.StreamingAssetsPath, "world", "ds"));
            testPositionLookup("Rain World/SomeFolder", RainWorldPath.RootPath);
            testPositionLookup("BepInEx", BepInExPath.RootPath);

            //Game path detection
            testDirectoryCategory("./", PathCategory.Game);
            testDirectoryCategory("Rain World", PathCategory.Game);
            testDirectoryCategory("StreamingAssets", PathCategory.Game);
            testDirectoryCategory("scenes/dream - iggy", PathCategory.Game);     //Located in StreamingAssets
            testDirectoryCategory("BepInEx", PathCategory.Game);
            testDirectoryCategory(RainWorldPath.RootPath, PathCategory.Game);
            testDirectoryCategory(RainWorldPath.StreamingAssetsPath, PathCategory.Game);
            testDirectoryCategory(BepInExPath.RootPath, PathCategory.Game);

            //Arbitrary folder within one of the designated root directories
            testDirectoryCategory("Rain World/SomeFolder", PathCategory.ModSourced);
            testDirectoryCategory("StreamingAssets/SomeFolder", PathCategory.ModSourced);
            testDirectoryCategory("BepInEx/SomeFolder", PathCategory.ModSourced);
            testDirectoryCategory(Path.Combine(RainWorldPath.RootPath, "SomeFolder"), PathCategory.ModSourced);
            testDirectoryCategory(Path.Combine(RainWorldPath.StreamingAssetsPath, "SomeFolder"), PathCategory.ModSourced);
            testDirectoryCategory(Path.Combine(BepInExPath.RootPath, "SomeFolder"), PathCategory.ModSourced);

            //Confirm that certain folders such as scenes are always considered a game folder even when it is unrecognized
            testDirectoryCategory("scenes/SomeFolder", PathCategory.Game);

            //A similar folder without such a restriction will be detected as Modsourced as expected
            testDirectoryCategory("decals/SomeFolder", PathCategory.ModSourced);

            //Mod path detection
            testDirectoryCategory("mods", PathCategory.Game);
            testDirectoryCategory("mods/SomeFolder", PathCategory.ModRequiredFolder);

            //Existing mod path
            string testModPath = "mods/expeditionregionsupport";

            testDirectoryCategory(testModPath, PathCategory.ModRequiredFolder);
            testDirectoryCategory(Path.Combine(testModPath, "plugins"), PathCategory.ModRequiredFolder);
            testDirectoryCategory(Path.Combine(testModPath, "newest"), PathCategory.ModRequiredFolder);
            testDirectoryCategory(Path.Combine(testModPath, "newest/modify"), PathCategory.ModRequiredFolder);

            //Arbitrary folder inside mod path that isn't a recognized mod folder path
            testDirectoryCategory(Path.Combine(testModPath, "SomeFolder"), PathCategory.ModSourced);
            testDirectoryCategory(Path.Combine(testModPath, "plugins/SomeFolder"), PathCategory.ModSourced);
            testDirectoryCategory(Path.Combine(testModPath, "newest/SomeFolder"), PathCategory.ModSourced);
            testDirectoryCategory(Path.Combine(testModPath, "newest/modify/SomeFolder"), PathCategory.ModSourced);

            //External path
            testDirectoryCategory(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), PathCategory.NotRooted);

            void testPositionLookup(string path, string expectation)
            {
                var dirNode = RainWorldDirectory.FolderTree.FindPositionInTree(path);
                activeTestGroup.AssertPathsAreEqual(expectation, dirNode.DirPath);
            }

            void testDirectoryCategory(string path, PathCategory expectation)
            {
                PathCategory category = RainWorldDirectory.GetDirectoryCategory(path);
                var condition = activeTestGroup.AssertThat(category).IsEqualTo(expectation);

                if (!condition)
                    UtilityLogger.LogWarning($"Path category unexpected EXPECTED ({expectation}) ACTUAL ({condition.Value})");
            }
        }

        private void testKeywordsReturnCorrectPath()
        {
            string pathResult = getPathConversion(UtilityConsts.PathKeywords.ROOT);
            activeTestGroup.AssertPathsAreEqual(expectedPath: RainWorldPath.RootPath,
                                                  actualPath: pathResult);

            pathResult = getPathConversion(UtilityConsts.PathKeywords.STREAMING_ASSETS);
            activeTestGroup.AssertPathsAreEqual(expectedPath: RainWorldPath.StreamingAssetsPath,
                                                  actualPath: pathResult);
        }

        private void testEmptyPathsReturnCorrectPath()
        {
            string[] pathInputs = [null, string.Empty, " "]; //All three of these should return the same result

            foreach (string input in pathInputs)
            {
                string pathResult = getPathConversion(input);
                activeTestGroup.AssertPathsAreEqual(expectedPath: RainWorldPath.StreamingAssetsPath,
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
                activeTestGroup.AssertPathsAreEqual(expectedPath: testInput,
                                                      actualPath: pathResult);
            }
        }

        private void testRelativePaths()
        {
            Condition.Result.ResetCount();
            TestCase testCase = new TestCase(activeTestGroup, "Test: Relative paths");

            string testFolderName = "SomeFolder"; //Value represents any arbitrary directory that does not exist
            string expectedTestFolderPath = Path.Combine(RainWorldPath.RootPath, testFolderName);

            testPath(@"./", RainWorldPath.RootPath);
            testPath(@".\", RainWorldPath.RootPath);
            testPath(@"././", RainWorldPath.RootPath);

            testPath(@"./" + testFolderName, expectedTestFolderPath);
            testPath(@".\" + testFolderName, expectedTestFolderPath);
            testPath(@"././" + testFolderName, expectedTestFolderPath);

            testPath(@"/./", @"C:/");

            void testPath(string testInput, string expectedOutput)
            {
                UtilityLogger.Log("Input: " + testInput);
                string pathResult = getPathConversion(testInput);
                testCase.AssertPathsAreEqual(expectedPath: expectedOutput,
                                               actualPath: pathResult);
            }
        }

        private void testPartialPaths()
        {
            Condition.Result.ResetCount();
            TestCase testCase = new TestCase(activeTestGroup, "Test: Partial paths");

            string[] testData = createTestData();

            #pragma warning disable IDE0055 //Fix formatting
            string relativeRoot        = testData[(int)PathIndex.RAIN_WORLD],
                   relativeCustomRoot  = testData[(int)PathIndex.STREAMING_ASSETS],
                   relativeBepInExRoot = testData[(int)PathIndex.BEPINEX];
            #pragma warning restore IDE0055 //Fix formatting

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

        private void testPartialPathsCaseInsensitive()
        {
            TestCase testCase = new TestCase(activeTestGroup, "Test: Relative paths (Case insensitive)");

            string[] testData = createTestData();

            #pragma warning disable IDE0055 //Fix formatting
            string relativeRoot        = testData[(int)PathIndex.RAIN_WORLD],
                   relativeCustomRoot  = testData[(int)PathIndex.STREAMING_ASSETS],
                   relativeBepInExRoot = testData[(int)PathIndex.BEPINEX];
            #pragma warning restore IDE0055 //Fix formatting

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

        private static string[] createTestData() => ["Rain World", "StreamingAssets", "BepInEx"];

        private enum PathIndex
        {
            RAIN_WORLD = 0,
            STREAMING_ASSETS = 1,
            BEPINEX
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
    }
}
