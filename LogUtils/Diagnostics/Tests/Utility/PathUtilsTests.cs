using LogUtils.Helpers.FileHandling;
using System;
using System.IO;

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
            testGetPrefixLength();
            testSplitPath();
            testLocationInPath();
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
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
