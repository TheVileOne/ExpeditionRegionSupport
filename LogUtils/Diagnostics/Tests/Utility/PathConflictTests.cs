using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils.Diagnostics.Tests.Utility
{
    public class PathConflictTests : TestCase, ITestable
    {
        internal const string TEST_NAME = "Test - Path Conflicts";

        public PathConflictTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            testConflictDetectionRenamesFile();
        }

        private void testConflictDetectionRenamesFile()
        {
            LogProperties propertiesA, propertiesB;

            propertiesA = new LogProperties("Test");
            LogProperties.PropertyManager.SetProperties(propertiesA);

            //Change filename to something other than the original
            propertiesA.ChangeFilename("Test-A");

            //When a second LogProperties instance is created, the filename should resolve the conflict for us
            propertiesB = new LogProperties("Test-A");

            string expectedFilename = string.Format(FileUtils.BRACKET_FORMAT, propertiesB.Filename, "1", string.Empty);

            AssertThat(propertiesB.CurrentFilename).IsEqualTo(expectedFilename);
            LogProperties.PropertyManager.RemoveProperties(propertiesA);

            UtilityLogger.Log("Current: " + propertiesB.CurrentFilename);
            UtilityLogger.Log("Expected: " + expectedFilename);
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
        }
    }
}
