using LogUtils.Diagnostics.Tests.Components;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;

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
            testConflictDetailsArePathDependent();
            testConflictDetailsAreNumeric();
            testNewConflictDetailsAreProducedWhenPathIsChanged();
            testReserveFilename();
        }

        private void testConflictDetectionRenamesFile()
        {
            LogPropertyFactory factory = new LogPropertyFactory("example");

            //Create a registered instance to test against - A conflict wont be triggered unless an entry is registered
            LogProperties target = factory.Create(UtilityConsts.PathKeywords.ROOT, register: true);

            //Change filename to something other than the original
            target.ChangeFilename("conflict");

            //When a second LogProperties instance is created, the filename should resolve the conflict for us
            LogProperties conflictTrigger = new LogProperties("conflict", target.FolderPath);

            AssertThat(conflictTrigger.CurrentFilename).DoesNotEqual(target.CurrentFilename);

            //Clean up resources
            LogProperties.PropertyManager.RemoveProperties(target);
            factory.Dispose();
        }

        private void testConflictDetailsArePathDependent()
        {
            LogPropertyFactory factory = new LogPropertyFactory("example");

            //Create a registered instance to test against - A conflict wont be triggered unless an entry is registered
            LogProperties target = factory.Create(UtilityConsts.PathKeywords.ROOT, register: true);

            //Change filename to something other than the original
            target.ChangeFilename("conflict");

            //When a second LogProperties instance is created, the filename should resolve the conflict for us
            LogProperties conflictTrigger = new LogProperties("conflict", target.FolderPath);

            //When we try to move to a path without a conflict - the expectation is that the bracket info will be removed
            conflictTrigger.ChangePath(UtilityConsts.PathKeywords.STREAMING_ASSETS);

            AssertThat(conflictTrigger.CurrentFilename).IsEqualTo(target.CurrentFilename);

            //Clean up resources
            LogProperties.PropertyManager.RemoveProperties(target);
            factory.Dispose();
        }

        private void testConflictDetailsAreNumeric()
        {
            LogPropertyFactory factory = new LogPropertyFactory("example");

            string testPath = UtilityConsts.PathKeywords.ROOT;

            LogProperties exampleA = factory.Create(testPath, true);
            LogProperties exampleB = factory.Create(testPath, true);
            LogProperties exampleC = factory.Create(testPath, true);

            exampleB.ChangeFilename(exampleA.CurrentFilename);
            exampleC.ChangeFilename(exampleA.CurrentFilename);

            string conflictDetails = FileUtils.GetBracketInfo(exampleB.CurrentFilename);

            int firstValue, secondValue;

            //Expected output should be numeric
            AssertThat(conflictDetails).IsNotNull();
            AssertThat(int.TryParse(conflictDetails, out firstValue)).IsTrue();

            conflictDetails = FileUtils.GetBracketInfo(exampleC.CurrentFilename);

            AssertThat(conflictDetails).IsNotNull();
            AssertThat(int.TryParse(conflictDetails, out secondValue)).IsTrue();

            //For each additional conflict, the designation should increase by one
            int valueDiff = secondValue - firstValue;
            AssertThat(valueDiff).IsEqualTo(1);

            //Clean up resources
            LogProperties.PropertyManager.RemoveProperties(exampleA);
            LogProperties.PropertyManager.RemoveProperties(exampleB);
            LogProperties.PropertyManager.RemoveProperties(exampleC);
            factory.Dispose();
        }

        private void testNewConflictDetailsAreProducedWhenPathIsChanged()
        {
            LogPropertyFactory factoryA = new LogPropertyFactory("example-A");
            LogPropertyFactory factoryB = new LogPropertyFactory("example-B");

            string testPathA = UtilityConsts.PathKeywords.ROOT;
            string testPathB = UtilityConsts.PathKeywords.STREAMING_ASSETS;

            //Create conflicts for two different paths
            LogProperties exampleA = factoryA.Create(testPathA, true);
            LogProperties exampleB = factoryA.Create(testPathA, true);
            LogProperties exampleC = factoryB.Create(testPathB, true);
            LogProperties exampleD = factoryB.Create(testPathB, true);

            //These examples should now have path conflicts
            exampleB.ChangeFilename(exampleA.CurrentFilename);
            exampleD.ChangeFilename(exampleC.CurrentFilename);

            //Move a conflicted example from one path to the other
            exampleD.ChangePath(exampleA.CurrentFilePath);

            //Moved file should reevaluate its bracket info example[1] should become example[2], not example[1][1]
            string conflictDetails = FileUtils.GetBracketInfo(exampleD.CurrentFilename);

            int value = int.Parse(conflictDetails);
            AssertThat(value).IsEqualTo(2);

            //Clean up resources
            LogProperties.PropertyManager.RemoveProperties(exampleA);
            LogProperties.PropertyManager.RemoveProperties(exampleB);
            LogProperties.PropertyManager.RemoveProperties(exampleC);
            LogProperties.PropertyManager.RemoveProperties(exampleD);
            factoryA.Dispose();
            factoryB.Dispose();
        }

        private void testReserveFilename()
        {
            LogProperties example = new LogProperties("example");

            example.AltFilename = "alt";
            example.ChangeFilename("example-A");         //Set name again to establish that the reserve is not the same as the initial filename
            example.ChangeFilename(example.AltFilename); //Name is no longer example-A

            //Reserve should be example-A here
            string reserveFilename = example.GetUnusedFilename();
            AssertThat(reserveFilename).DoesNotEqual(example.Filename);
            AssertThat(reserveFilename).IsEqualTo(example.ReserveFilename);
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
        }
    }
}
