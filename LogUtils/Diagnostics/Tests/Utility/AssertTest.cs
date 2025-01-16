using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal class AssertTest : ITestable
    {
        public string Name => "Test - Assert System";

        private DeferredAssertHandler handler;

        public void Test()
        {
            //Borrow the Logger instance from the current template
            AssertHandler template = AssertHandler.CurrentTemplate as AssertHandler;

            if (template == null)
                template = AssertHandler.DefaultHandler;

            handler = new DeferredAssertHandler(template.Logger)
            {
                Behavior = AssertBehavior.LogOnFail | AssertBehavior.LogOnPass,
                FailResponse = "Fail",
                PassResponse = "Pass"
            };

            testCollectionDetection();
            testComparisonOfNullableCombinations();
            testEqualityOfNullableCombinations();
        }

        /// <summary>
        /// Demonstrate the ways collection specific asserts are accessed
        /// </summary>
        private void testCollectionDetection()
        {
            handler.Logger.Log("Testing: Collection asserts");

            //The implementation is sensitive to the type it stored as.
            ICollection<string> strings = new List<string> { "test", "data" };
            List<string> strings2 = new List<string> { "test", "data" };
            CustomCollection customCollection = new CustomCollection();

            handler.Logger.Log("Test 1");
            Assert.That(strings, handler).IsNullOrEmpty();
            handler.Logger.Log("Test 2");
            Assert.That(strings2, handler).HasItems();
            handler.Logger.Log("Test 3");
            Assert.That(customCollection.AsEnumerable(), handler).IsNullOrEmpty();

            //The first is expected to fail, but the next few are not
            handler.HandleCurrent(expectation: Condition.State.Fail);
            handler.HandleAll();
        }

        private void testEqualityOfNullableCombinations()
        {
            handler.Logger.Log("Testing: Nullable equality asserts");

            UnityEngine.Vector2 testValue1 = UnityEngine.Vector2.one;
            UnityEngine.Vector2? testValue2 = null;
            UnityEngine.Vector2 testValue3 = UnityEngine.Vector2.zero;

            Assert.That(testValue1, handler).IsEqualTo(testValue3);    //Expect fail
            Assert.That(testValue1, handler).IsEqualTo(testValue2);    //Expect fail
            Assert.That(testValue2, handler).IsEqualTo(testValue2);
            Assert.That(testValue1, handler).IsEqualTo(testValue3);    //Expect fail
            Assert.That(testValue1, handler).DoesNotEqual(testValue3);
            Assert.That(testValue1, handler).DoesNotEqual(testValue2);
            Assert.That(testValue2, handler).DoesNotEqual(testValue2); //Expect fail
            Assert.That(testValue2, handler).DoesNotEqual(testValue3);

            handler.HandleCurrent(expectation: Condition.State.Fail)
                   .HandleCurrent(expectation: Condition.State.Fail)
                   .HandleCurrent()
                   .HandleCurrent(expectation: Condition.State.Fail)
                   .HandleCurrent()
                   .HandleCurrent()
                   .HandleCurrent(expectation: Condition.State.Fail)
                   .HandleCurrent();
        }

        private void testComparisonOfNullableCombinations()
        {
            handler.Logger.Log("Testing: Nullable comparison asserts");

            int testValue1 = 5,
                testValue2 = 8;

            int? testNullable1 = null,
                 testNullable2 = 6;

            /*
             * Description: Non-nullable/Non-nullable
             * Expectation: Pass
             */
            Assert.That(testValue1, handler).IsLessThan(testValue2); //Is 5 less than 8?

            /*
             * Description: Non-nullable/Nullable
             * Expectation: Pass
             */
            Assert.That(testValue1, handler).IsLessThan(testNullable2); //Is 5 less than 6?

            /*
             * Description: Nullable/Nullable
             * Expectation: Fail, when comparing two nullable structs, values can only be equal, or not equal
             */
            Assert.That(testNullable2, handler).IsLessThan(testNullable1); //Is 6 less than null?

            /*
             * Description: Nullable/Non-nullable
             * Expectation: Fail, when comparing two nullable structs, values can only be equal, or not equal
             */
            Assert.That(testNullable1, handler).IsLessThan(testValue2); //Is null less than 8?

            handler.HandleCurrent()
                   .HandleCurrent()
                   .HandleCurrent(expectation: Condition.State.Fail)
                   .HandleCurrent(expectation: Condition.State.Fail);
        }

        /// <summary>
        /// Empty class used to test detection of custom collections
        /// </summary>
        private class CustomCollection : List<string>
        {
        }
    }
}
