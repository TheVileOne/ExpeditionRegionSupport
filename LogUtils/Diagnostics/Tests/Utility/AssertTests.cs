using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal class AssertTests : TestCaseGroup, ITestable
    {
        internal const string TEST_NAME = "Test - Assert System";

        public AssertTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            //Borrow the Logger instance from the current template
            AssertHandler template = AssertHandler.CurrentTemplate as AssertHandler;

            if (template == null)
                template = AssertHandler.DefaultHandler;

            var handler = new DeferredAssertHandler(template.Logger)
            {
                Behavior = AssertBehavior.LogOnFail | AssertBehavior.LogOnPass,
            };

            handler.Formatter.FailResponse = "Fail";
            handler.Formatter.PassResponse = "Pass";

            //Handler = handler; //We want this handler to be applied to children test cases

            testCollectionDetection();
            testComparisonOfNullableCombinations();
            testEqualityOfNullableCombinations();

            template.Logger.LogDebug(CreateReport());
            handler.HandleAll();
        }

        /// <summary>
        /// Demonstrate the ways collection specific asserts are accessed
        /// </summary>
        private void testCollectionDetection()
        {
            TestCaseGroup testGroup = new TestCaseGroup(this, "Test: Collection asserts");

            using (testGroup)
            {
                /*
                 * Show that the assert system can accept collections cast to different types
                 */
                ICollection<string> testDataA;
                List<string> testDataB;
                CustomCollection testDataC;

                testDataA = new List<string> { "test", "data" };
                testDataB = new List<string> { "test", "data" };
                testDataC = new CustomCollection();

                using (TestCase test = new TestCase(testGroup, "Test: ICollection support"))
                {
                    test.AssertThat(testDataA).IsNullOrEmpty().ExpectFail();
                }

                using (TestCase test = new TestCase(testGroup, "Test: List support"))
                {
                    test.AssertThat(testDataB).HasItems();
                }

                using (TestCase test = new TestCase(testGroup, "Test: Custom collection support"))
                {
                    test.AssertThat(testDataC.AsEnumerable()).IsNullOrEmpty();
                }
            }
        }

        private void testEqualityOfNullableCombinations()
        {
            TestCase test = new TestCase(this, "Test: Nullable equality asserts");

            Vector2 testValue1 = Vector2.One;
            Vector2? testValue2 = null;
            Vector2 testValue3 = Vector2.Zero;

            test.AssertThat(testValue1).IsEqualTo(testValue3).ExpectFail();
            test.AssertThat(testValue1).IsEqualTo(testValue2).ExpectFail();
            test.AssertThat(testValue2).IsEqualTo(testValue2);
            test.AssertThat(testValue1).IsEqualTo(testValue3).ExpectFail();
            test.AssertThat(testValue1).DoesNotEqual(testValue3);
            test.AssertThat(testValue1).DoesNotEqual(testValue2);
            test.AssertThat(testValue2).DoesNotEqual(testValue2).ExpectFail();
            test.AssertThat(testValue2).DoesNotEqual(testValue3);
        }

        private void testComparisonOfNullableCombinations()
        {
            TestCase test = new TestCase(this, "Test: Nullable comparison asserts");

            int testValue1 = 5,
                testValue2 = 8;

            int? testNullable1 = null,
                 testNullable2 = 6;

            /*
             * Description: Non-nullable/Non-nullable
             * Expectation: Pass
             */
            test.AssertThat(testValue1).IsLessThan(testValue2); //Is 5 less than 8?

            /*
             * Description: Non-nullable/Nullable
             * Expectation: Pass
             */
            test.AssertThat(testValue1).IsLessThan(testNullable2); //Is 5 less than 6?

            /*
             * Description: Nullable/Nullable
             * Expectation: Fail, when comparing two nullable structs, values can only be equal, or not equal
             */
            test.AssertThat(testNullable2).IsLessThan(testNullable1).ExpectFail(); //Is 6 less than null?

            /*
             * Description: Nullable/Non-nullable
             * Expectation: Fail, when comparing two nullable structs, values can only be equal, or not equal
             */
            test.AssertThat(testNullable1).IsLessThan(testValue2).ExpectFail(); //Is null less than 8?
        }

        /// <summary>
        /// Empty class used to test detection of custom collections
        /// </summary>
        private class CustomCollection : List<string>
        {
        }
    }
}
