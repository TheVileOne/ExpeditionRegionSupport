using LogUtils.Helpers.FileHandling;
using System;
using System.IO;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal sealed class PathUtilsTests : TestCase, ITestable
    {
        internal const string TEST_NAME = "Test - Path Utils";

        public PathUtilsTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            testFindCommonRoot();
            testGetPrefixLength();
            testSplitPath();
            testLocationInPath();
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
        }

        private void testFindCommonRoot()
        {
            //Two partial paths
            testFindCommonRoot("one", "two", RainWorldPath.StreamingAssetsPath);

            //Two partial paths - subfolder matches other path 
            testFindCommonRoot("one/two", "two", RainWorldPath.StreamingAssetsPath);

            //Two partial paths - leading folder matches other path
            testFindCommonRoot("one", "one/two", Path.Combine(RainWorldPath.StreamingAssetsPath, "one"));

            //One partial path, and one fully qualified path
            testFindCommonRoot("one", Path.Combine(RainWorldPath.RootPath, "one"), RainWorldPath.RootPath); //Rain World\RainWorld_Data\StreamingAssets\one | Rain World\one

            //Two fully qualified matching paths
            testFindCommonRoot(Path.Combine(RainWorldPath.RootPath, "one"),
                               Path.Combine(RainWorldPath.RootPath, "one"),
                               Path.Combine(RainWorldPath.RootPath, "one")); //Rain World\one | Rain World\one

            //One relative path, and one fully qualified path
            testFindCommonRoot(@"./", RainWorldPath.RootPath, RainWorldPath.RootPath);

            //Test case insensitivity

            //Two partial paths - leading folder matches other path
            testFindCommonRoot("one", "ONE/two", Path.Combine(RainWorldPath.StreamingAssetsPath, "one")); //Expectation: First case choice will be selected

            //Two fully qualified matching paths
            testFindCommonRoot(Path.Combine(RainWorldPath.RootPath, "one"),
                               Path.Combine(RainWorldPath.RootPath, "ONE"),
                               Path.Combine(RainWorldPath.RootPath, "one")); //Rain World\one | Rain World\one

            void testFindCommonRoot(string path, string pathOther, string expectation)
            {
                string result = PathUtils.FindCommonRoot(path, pathOther);
                if (!AssertPathsAreEqual(expectation, result))
                {
                    UtilityLogger.Log("Common root doesn't match:" +
                        "\nPATH " + path +
                        "\nPATH OTHER " + pathOther +
                        "\nEXPECTATION " + expectation +
                        "\nACTUAL " + result);
                }
            }
        }

        private void testGetPrefixLength()
        {
            testEmptyPathForms();

            void testEmptyPathForms()
            {
                testNullPrefix();           //Should be zero
                testPrefixUnmodified(@"");  //Should be zero
                testPrefixUnmodified(@" "); //Should be one
            }

            testPrefix(@"/");
            testPrefix(@"//");
            testPrefix(@"\");
            testPrefix(@"\\");
            testPrefix(@"\/");
            testPrefix(@"./");
            testPrefix(@"././");
            testPrefix(@".\");
            testPrefix(@".\.\");
            testPrefix(@"/./");
            testPrefix(@"C:/");

            void testPrefix(string value)
            {
                int length = PathUtils.GetPrefixLength(value + "test");
                AssertThat(length).IsEqualTo(value.Length);
            }

            void testPrefixUnmodified(string value)
            {
                int length = PathUtils.GetPrefixLength(value);
                AssertThat(length).IsEqualTo(value.Length);
            }

            void testNullPrefix()
            {
                int length = PathUtils.GetPrefixLength(null);
                AssertThat(length).IsZero();
            }
        }

        private void testSplitPath()
        {
            string[] testData = ["one", "two", "three"];
            string testPath = Path.Combine(testData);

            testSplitPath(@"");
            testSplitPath(@"/");
            testSplitPath(@"//");
            testSplitPath(@"\");
            testSplitPath(@"\\");
            testSplitPath(@"\/");
            testSplitPath(@"./");
            testSplitPath(@"././");
            testSplitPath(@".\");
            testSplitPath(@".\.\");
            testSplitPath(@"/./");
            testSplitPath(@"C:/");

            void testSplitPath(string prefix)
            {
                string[] results = PathUtils.SplitPath(prefix + testPath);
                AssertThat(results.Length).IsEqualTo(testData.Length);

                for (int i = 0; i < Math.Min(results.Length, testData.Length); i++)
                    AssertThat(results[i]).IsEqualTo(testData[i]);
            }
        }

        private void testLocationInPath()
        {
            int searchIndex = 1;
            string[] testData = ["one", "two", "three"];
            string testPath = Path.Combine(testData);

            testLocationInPath(@"");
            testLocationInPath(@"/");
            testLocationInPath(@"//");
            testLocationInPath(@"\");
            testLocationInPath(@"\\");
            testLocationInPath(@"\/");
            testLocationInPath(@"./");
            testLocationInPath(@"././");
            testLocationInPath(@".\");
            testLocationInPath(@".\.\");
            testLocationInPath(@"/./");
            testLocationInPath(@"C:/");

            void testLocationInPath(string prefix)
            {
                const int EXPECTED_DIR_INDEX = 4; //"one/"

                string prefixedTestPath = prefix + testPath;

                int dirIndex = DirectoryUtils.GetLocationInPath(prefixedTestPath, testData[searchIndex]);
                AssertThat(dirIndex).IsEqualTo(PathUtils.GetPrefixLength(prefixedTestPath) + EXPECTED_DIR_INDEX);
            }
        }
    }
}
