using LogUtils.Enums;
using LogUtils.Helpers;
using System.Collections.Generic;
using System.Linq;

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
                LogID exampleA = new LogID("example", register: false),
                      exampleB = new LogID("example", register: false);

                AssertThat(exampleA.Properties).IsSameInstance(exampleB.Properties);
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
            }

            [PostTest]
            public void ShowResults()
            {
                TestLogger.LogDebug(CreateReport());
            }

            private void testInstanceEquality()
            {
                string rootPath = Paths.GameRootPath;

                LogID control = createTestLogID("Test", rootPath);

                #pragma warning disable IDE0055 //Fix formatting
                LogID sameInstance = control,
                      sameLogNameDifferentReference = createTestLogID("Test", rootPath),
                      sameLogNameDifferentFilePath  = createTestLogID("Test", UtilityConsts.PathKeywords.STREAMING_ASSETS),
                      sameLogNameDifferentCase      = createTestLogID("teSt", rootPath),
                      sameLogPathDifferentCase      = createTestLogID("Test", rootPath.ToUpper()),
                      sameLogPathUsesPathKeyword    = createTestLogID("Test", UtilityConsts.PathKeywords.ROOT),
                      notSameInstance               = createTestLogID("NotTest", rootPath);
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

                var resultEnumerator = testEntries.Select(entries => getTestResults(entries.A, entries.B)).GetEnumerator();

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

            private LogID createTestLogID(string filename, string path)
            {
                return new LogID(filename, path, LogAccess.Private, false);
            }

            private (bool EqualsCheck, bool NotEqualsCheck) getTestResults(LogID A, LogID B)
            {
                return (A == B && A.Equals(B),
                        A != B && !A.Equals(B));
            }
        }

        internal readonly struct ValuePair<T>(T A, T B)
        {
            public readonly T A = A;
            public readonly T B = B;
        }
    }
}
