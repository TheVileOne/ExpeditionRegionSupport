using LogUtils.Helpers.FileHandling;
using System;
using System.IO;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal sealed class FileReplaceTests : TestCase, ITestable
    {
        internal const string TEST_NAME = "Test - File Replace";

        public FileReplaceTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            //Setup
            string testPath = "LogsUtils.UnitTests";
            string testFileA = Path.Combine(testPath, "FolderA, test.txt");
            string testFileB = Path.Combine(testPath, "FolderB", "test.txt");

            //Create identical named files in two directories
            Directory.CreateDirectory(Path.GetDirectoryName(testFileA));
            Directory.CreateDirectory(Path.GetDirectoryName(testFileB));
            File.Create(testFileA).Close();
            File.Create(testFileB).Close();

            bool success = FileUtils.TryReplace(testFileA, testFileB);
            AssertThat(success).IsTrue();
            try
            {
                Directory.Delete(testPath, true);
            }
            catch (Exception ex)
            {
                UtilityLogger.LogError("Unable to delete test directory", ex);
            }
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
        }
    }
}
