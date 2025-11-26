using LogUtils.Diagnostics.Tests.Components;
using LogUtils.Enums;
using LogUtils.Helpers.Comparers;
using LogUtils.Helpers.FileHandling;
using LogUtils.Properties;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RainWorldPath = LogUtils.Helpers.Paths.RainWorld;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal static class LogIDTests
    {
        internal sealed class PropertyTests : TestCase, ITestable
        {
            internal const string TEST_NAME = "Test - LogID Properties";

            public PropertyTests() : base(TEST_NAME)
            {
            }

            public void Test()
            {
                testOnlyOnePropertiesInstancePerLogFile();
            }

            private void testOnlyOnePropertiesInstancePerLogFile()
            {
                LogID exampleA = TestLogID.Factory.Create("example"),
                      exampleB = TestLogID.Factory.FromTarget(exampleA);

                AssertThat(exampleA.Properties).IsSameInstance(exampleB.Properties);
                TestEnumFactory.DisposeObjects();
            }

            [PostTest]
            public void ShowResults()
            {
                TestLogger.LogDebug(CreateReport());
            }
        }

        internal sealed class ComparisonTests : TestCase, ITestable
        {
            internal const string TEST_NAME = "Test - LogID Comparison";

            public ComparisonTests() : base(TEST_NAME)
            {
            }

            public void Test()
            {
                testInstanceEquality();
                testInstanceEqualityComparer();
            }

            [PostTest]
            public void ShowResults()
            {
                TestLogger.LogDebug(CreateReport());
            }

            private void testInstanceEquality()
            {
                TestLogIDFactory factory = TestLogID.Factory;
                string rootPath = RainWorldPath.RootPath;

                LogID control = factory.Create("Test", rootPath);

                #pragma warning disable IDE0055 //Fix formatting
                LogID sameInstance = control,
                      sameLogNameDifferentReference = factory.Create("Test", rootPath),
                      sameLogNameDifferentFilePath  = factory.Create("Test", UtilityConsts.PathKeywords.STREAMING_ASSETS),
                      sameLogNameDifferentCase      = factory.Create("teSt", rootPath),
                      sameLogPathDifferentCase      = factory.Create("Test", rootPath.ToUpper()),
                      sameLogPathUsesPathKeyword    = factory.Create("Test", UtilityConsts.PathKeywords.ROOT),
                      notSameInstance               = factory.Create("NotTest", rootPath);
                #pragma warning restore IDE0055 //Fix formatting

                //Collect entries for testing
                List<ValuePair<LogID>> testEntries = new List<ValuePair<LogID>>()
                {
                    new ValuePair<LogID>(control, sameInstance),
                    new ValuePair<LogID>(control, sameLogNameDifferentReference),
                    new ValuePair<LogID>(control, sameLogNameDifferentFilePath),
                    new ValuePair<LogID>(control, sameLogNameDifferentCase), //Doesn't pass
                    new ValuePair<LogID>(control, sameLogPathDifferentCase), //Doesn't pass
                    new ValuePair<LogID>(control, sameLogPathUsesPathKeyword),
                    new ValuePair<LogID>(control, notSameInstance)
                };

                processData(testEntries);
                TestEnumFactory.DisposeObjects();
            }

            private void assertResults((bool EqualsCheck, bool NotEqualsCheck) results, bool expectEquality)
            {
                //Assert that we expect, or do not expect equality checks to pass
                if (expectEquality)
                {
                    AssertThat(results.EqualsCheck).IsTrue();
                    AssertThat(results.NotEqualsCheck).IsFalse();
                }
                else
                {
                    AssertThat(results.EqualsCheck).IsFalse();
                    AssertThat(results.NotEqualsCheck).IsTrue();
                }
            }

            private void testInstanceEqualityComparer()
            {
                CompareOptions compareOptions = CompareOptions.All;
                LogIDComparer comparer = new LogIDComparer(compareOptions);
                TestLogIDFactory factory = TestLogID.Factory;

                LogID fileLog, groupLogA, groupLogB, groupLogC;

                fileLog = factory.Create("Test");
                fileLog.Properties.AltFilename = new LogFilename("AltTest");

                groupLogA = factory.CreateLogGroup("Test");
                groupLogB = factory.CreateLogGroup("Test B");
                groupLogC = factory.CreateLogGroup("TEST");

                AssertThat(comparer.Compare(fileLog, groupLogA)).IsNotZero();   //No match
                AssertThat(comparer.Compare(groupLogA, groupLogB)).IsNotZero(); //No match
                AssertThat(comparer.Compare(groupLogA, groupLogC)).IsZero();    //Match

                AssertThat(comparer.Equals(fileLog, groupLogA)).IsFalse();   //No match
                AssertThat(comparer.Equals(groupLogA, groupLogB)).IsFalse(); //No match
                AssertThat(comparer.Equals(groupLogA, groupLogC)).IsTrue();  //Match

                testCompareMasks();
                TestEnumFactory.DisposeObjects();

                void testCompareMasks()
                {
                    int resultLengthFile, resultLengthGroup;

                    resultLengthFile = fileLog.Properties.GetValuesToCompare(compareOptions).Length;
                    resultLengthGroup = groupLogA.Properties.GetValuesToCompare(compareOptions).Length;

                    AssertThat(resultLengthFile).IsEqualTo(4);  //ID, Filename, CurrentFilename, AltFilename
                    AssertThat(resultLengthGroup).IsEqualTo(1); //ID
                }
            }

            private void processData(List<ValuePair<LogID>> data)
            {
                var resultEnumerator = data.Select(entries => getTestResults(entries.A, entries.B)).GetEnumerator();

                resultEnumerator.MoveNext();

                //SameInstance
                assertResults(resultEnumerator.Current, expectEquality: true);

                resultEnumerator.MoveNext();

                //SameLogNameDifferentReference
                assertResults(resultEnumerator.Current, expectEquality: true);

                resultEnumerator.MoveNext();

                //SameLogNameDifferentFilePath
                assertResults(resultEnumerator.Current, expectEquality: false);

                resultEnumerator.MoveNext();

                //SameLogNameDifferentCase
                assertResults(resultEnumerator.Current, expectEquality: true);

                resultEnumerator.MoveNext();

                //SameLogPathDifferentCase
                assertResults(resultEnumerator.Current, expectEquality: true);

                resultEnumerator.MoveNext();

                //SameLogPathUsesPathKeyword
                assertResults(resultEnumerator.Current, expectEquality: true);

                resultEnumerator.MoveNext();

                //NotSameInstance
                assertResults(resultEnumerator.Current, expectEquality: false);
            }

            private (bool EqualsCheck, bool NotEqualsCheck) getTestResults(LogID A, LogID B)
            {
                return (A == B && A.Equals(B),
                        A != B && !A.Equals(B));
            }

            private readonly struct ValuePair<T>(T A, T B)
            {
                public readonly T A = A;
                public readonly T B = B;
            }
        }

        internal sealed class GroupTests : TestCase, ITestable
        {
            internal const string TEST_NAME = "Test - LogID Groups";

            internal const string GROUP_NAME = "test";
            internal const string MEMBER_NAME = "member";
            internal const string MEMBER_PATH = MEMBER_NAME + " - path";

            public GroupTests() : base(TEST_NAME)
            {
            }

            public void Test()
            {
                testAssignmentIncreasesMemberCount();
                testAssignmentForcesReadOnly();
                testMemberPathCombinesWithGroupPath();
                testMemberPathIsUnaffectedWhenGroupPathIsIncompatible();
                testMemberPathInheritsGroupPathWhenEmpty();
            }

            private void testAssignmentIncreasesMemberCount()
            {
                LogGroupID testGroupEnum = TestLogID.Factory.CreateLogGroup(GROUP_NAME);
                LogID testGroupEnumMember = TestLogID.Factory.CreateLogGroupMember(testGroupEnum, MEMBER_NAME);

                AssertThat(testGroupEnum.Properties.Members).HasItems();
                TestEnumFactory.DisposeObjects();
            }

            private void testAssignmentForcesReadOnly()
            {
                LogGroupID testGroupEnum = TestLogID.Factory.CreateLogGroup(GROUP_NAME);
                LogID testGroupEnumMember = TestLogID.Factory.CreateLogGroupMember(testGroupEnum, MEMBER_NAME);

                AssertThat(testGroupEnum.Properties.ReadOnly).IsTrue();
                TestEnumFactory.DisposeObjects();
            }

            private void testMemberPathCombinesWithGroupPath()
            {
                string testGroupPath = RainWorldPath.RootPath;

                LogGroupID testGroupEnum = TestLogID.Factory.CreateLogGroup(GROUP_NAME, testGroupPath);
                LogID testGroupEnumMember = TestLogID.Factory.CreateLogGroupMember(testGroupEnum, MEMBER_NAME, MEMBER_PATH);

                assertPathsAreEqual(expectedPath: Path.Combine(testGroupPath, MEMBER_PATH),
                                      actualPath: testGroupEnumMember.Properties.FolderPath);
                TestEnumFactory.DisposeObjects();
            }

            private void testMemberPathIsUnaffectedWhenGroupPathIsIncompatible()
            {
                //Test with a null group path
                testPaths(null, MEMBER_PATH);

                //Test with two absolute paths - the member path should be used instead of the group path in this case
                testPaths(RainWorldPath.RootPath, Path.Combine(RainWorldPath.StreamingAssetsPath, MEMBER_PATH));

                void testPaths(string testGroupPath, string testMemberPath)
                {
                    LogGroupID testGroupEnum = TestLogID.Factory.CreateLogGroup(GROUP_NAME, testGroupPath);
                    LogID testGroupEnumMember = TestLogID.Factory.CreateLogGroupMember(testGroupEnum, MEMBER_NAME, testMemberPath);

                    assertPathsAreEqual(expectedPath: LogProperties.GetContainingPath(testMemberPath),
                                          actualPath: testGroupEnumMember.Properties.FolderPath);
                    TestEnumFactory.DisposeObjects();
                }
            }

            private void testMemberPathInheritsGroupPathWhenEmpty()
            {
                string testGroupPath = RainWorldPath.RootPath;

                LogGroupID testGroupEnum = TestLogID.Factory.CreateLogGroup(GROUP_NAME, testGroupPath);
                LogID testGroupEnumMember = TestLogID.Factory.CreateLogGroupMember(testGroupEnum, MEMBER_NAME);

                assertPathsAreEqual(expectedPath: testGroupPath,
                                      actualPath: testGroupEnumMember.Properties.FolderPath);
                TestEnumFactory.DisposeObjects();
            }

            private void assertPathsAreEqual(string expectedPath, string actualPath)
            {
                if (AssertThat(PathUtils.PathsAreEqual(expectedPath, actualPath)).IsTrue() == false)
                    UtilityLogger.LogWarning("EXPECTED: " + expectedPath + "\nACTUAL: " + actualPath);
            }

            [PostTest]
            public void ShowResults()
            {
                TestLogger.LogDebug(CreateReport());
            }
        }
    }
}
