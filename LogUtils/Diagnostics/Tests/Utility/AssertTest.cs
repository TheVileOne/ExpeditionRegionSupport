using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal class AssertTest : ITestable
    {
        public string Name => "Test - Assert System";

        public void Test()
        {
            testCollectionDetection();
            testComparisonOfNullableCombinations();
            testEqualityOfNullableCombinations();
        }

        /// <summary>
        /// Demonstrate the ways collection specific asserts are accessed
        /// </summary>
        private void testCollectionDetection()
        {
            //The implementation is sensitive to the type it stored as.
            ICollection<string> strings = new List<string> { "test", "data" };
            List<string> strings2 = new List<string> { "test", "data" };
            CustomCollection customCollection = new CustomCollection();

            Assert.That(strings).IsNullOrEmpty();
            Assert.That(strings2).HasItems();
            Assert.That(customCollection.AsEnumerable()).IsNullOrEmpty();
        }

        private void testEqualityOfNullableCombinations()
        {
            UnityEngine.Vector2 testValue1 = UnityEngine.Vector2.one;
            UnityEngine.Vector2? testValue2 = null;
            UnityEngine.Vector2 testValue3 = UnityEngine.Vector2.zero;

            Assert.That(testValue1).IsEqualTo(testValue3);    //Expect fail
            Assert.That(testValue1).IsEqualTo(testValue2);    //Expect fail
            Assert.That(testValue2).IsEqualTo(testValue2);
            Assert.That(testValue1).IsEqualTo(testValue3);    //Expect fail
            Assert.That(testValue1).DoesNotEqual(testValue3);
            Assert.That(testValue1).DoesNotEqual(testValue2);
            Assert.That(testValue2).DoesNotEqual(testValue2); //Expect fail
            Assert.That(testValue2).DoesNotEqual(testValue3);
        }

        private void testComparisonOfNullableCombinations()
        {
            int testValue1 = 5,
                testValue2 = 8;

            int? testNullable1 = null,
                 testNullable2 = 6;

            /*
             * Description: Non-nullable/Non-nullable
             * Expectation: Pass
             */
            Assert.That(testValue1).IsLessThan(testValue2); //Is 5 less than 8?

            /*
             * Description: Non-nullable/Nullable
             * Expectation: Pass
             */
            Assert.That(testValue1).IsLessThan(testNullable2); //Is 5 less than 6?

            /*
             * Description: Nullable/Nullable
             * Expectation: Fail, when comparing two nullable structs, values can only be equal, or not equal
             */
            Assert.That(testNullable2).IsLessThan(testNullable1); //Is 6 less than null?

            /*
             * Description: Nullable/Non-nullable
             * Expectation: Fail, when comparing two nullable structs, values can only be equal, or not equal
             */
            Assert.That(testNullable1).IsLessThan(testValue2); //Is null less than 8?
        }

        /// <summary>
        /// Empty class used to test detection of custom collections
        /// </summary>
        private class CustomCollection : List<string>
        {
        }
    }
}
