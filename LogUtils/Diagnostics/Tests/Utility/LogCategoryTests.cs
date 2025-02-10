using LogUtils.Enums;
using LogUtils.Helpers.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal class LogCategoryTests : TestCase, ITestable
    {
        internal const string TEST_NAME = "Test - LogCategory";
        internal const string CATEGORY_TEST_NAME = "UTILITY_TEST";

        public LogCategoryTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            testSubmission();
            testComposition();
            testEquality();
            testErrorCategoryDoesNotIncludeCategoryAll();
        }

        private void testSubmission()
        {
            //Register/Unregister
            testRegistration();
            testUnregistration();

            //Transfer state across multiple entries
            testRegistrationTransfer();
            testUnregistrationTransfer();

            //Multiple registrations
            testRegisteringMultipleTimesDoesNotCreateDuplicateSubmissions();
        }

        private void testComposition()
        {
            testLogicalOR();
            testLogicalAND();
            testLogicalXOR();
            testComplementaryComposition();
        }

        #region Submission Tests
        private void testRegistration()
        {
            LogCategory testEntry = new LogCategory(CATEGORY_TEST_NAME, true);

            AssertThat(testEntry.Registered).IsTrue();
            AssertThat(testEntry.ManagedReference).IsEqualTo(testEntry);
            AssertThat(LogCategory.RegisteredEntries.Contains(testEntry)).IsTrue();

            testEntry.Unregister();
        }

        private void testUnregistration()
        {
            LogCategory testEntry = new LogCategory(CATEGORY_TEST_NAME, true);

            testEntry.Unregister();
            AssertThat(testEntry.Registered).IsFalse();
            AssertThat(LogCategory.RegisteredEntries.Contains(testEntry)).IsFalse();
        }

        private void testRegistrationTransfer()
        {
            testCase(TransferMode.ParentToChild);
            testCase(TransferMode.ChildToParent);

            void testCase(TransferMode mode)
            {
                LogCategory parent, child;

                parent = new LogCategory(CATEGORY_TEST_NAME, mode == TransferMode.ParentToChild);
                child = new LogCategory(CATEGORY_TEST_NAME, mode == TransferMode.ChildToParent);

                //Registration should propagate to this entry
                LogCategory registrationRecipient = mode == TransferMode.ParentToChild ? child : parent;

                //Show that registration is tranferred from one entry to another
                AssertThat(registrationRecipient.Registered).IsTrue();

                parent.Unregister();
                child.Unregister();
            }
        }

        private void testUnregistrationTransfer()
        {
            testCase(TransferMode.ParentToChild);
            testCase(TransferMode.ChildToParent);

            void testCase(TransferMode mode)
            {
                LogCategory parent, child;

                parent = new LogCategory(CATEGORY_TEST_NAME, true);
                child = new LogCategory(CATEGORY_TEST_NAME, true);

                LogCategory unregisterTarget, transferTarget;

                switch (mode)
                {
                    case TransferMode.ParentToChild:
                        unregisterTarget = parent;
                        transferTarget = child;
                        break;
                    case TransferMode.ChildToParent:
                        unregisterTarget = child;
                        transferTarget = parent;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                unregisterTarget.Unregister();

                //Show that unregistering one entry unregisters all entries
                AssertThat(transferTarget.Registered).IsFalse();
            }
        }

        private void testRegisteringMultipleTimesDoesNotCreateDuplicateSubmissions()
        {
            LogCategory testEntry = new LogCategory(CATEGORY_TEST_NAME, false);

            testEntry.Register();
            testEntry.Register();

            var comparer = new ExtEnumValueComparer<LogCategory>();

            //There should only be one entry per LogCategory, and this should be enforced by ExtEnum class itself
            int entryCount = LogCategory.RegisteredEntries.Count(entry => comparer.Equals(entry, testEntry));

            AssertThat(entryCount).IsEqualTo(1);
            testEntry.Unregister();
        }
        #endregion
        #region Composition Tests
        private void testLogicalOR()
        {
            LogCategory a = new LogCategory("A", true),
                        b = new LogCategory("B", true),
                        c = new LogCategory("C", true);

            var result = a | b;

            result |= c;

            AssertThat(result).Contains(a);
            AssertThat(result).Contains(b);
            AssertThat(result).Contains(c);

            //Test that All cannot be combined with any other category
            result = LogCategory.All | a;
            AssertThat(result).DoesNotContain(a);

            //Test that order doesn't matter
            result = a | LogCategory.All;
            AssertThat(result).DoesNotContain(a);

            //Test that None cannot be combined with any other category, and does not affect assignment
            result = LogCategory.None | a;
            AssertThat(result).ContainsOnly(a);

            //Test that order doesn't matter
            result = a | LogCategory.None;
            AssertThat(result).ContainsOnly(a);

            //Test that an empty composite will be created
            result = null | LogCategory.None;
            AssertThat(result).IsEmpty();

            //Test that order doesn't matter
            result = LogCategory.None | null;
            AssertThat(result).IsEmpty();

            a.Unregister();
            b.Unregister();
            c.Unregister();
        }

        private void testLogicalAND()
        {
            LogCategory a = new LogCategory("A", true),
                        b = new LogCategory("B", true),
                        c = new LogCategory("C", true);

            var composite = a | b;

            //Test that the operator produces a composite with only common values
            var result = composite & b;
            AssertThat(result).ContainsOnly(b);

            //Test that the operator produces an empty composite when there are no values in common
            result = b & c;
            AssertThat(result).IsEmpty();

            //Test that All is interpreted as containing all other flags
            result = LogCategory.All & a;
            AssertThat(result).ContainsOnly(a);

            //Test that order doesn't matter
            result = a & LogCategory.All;
            AssertThat(result).ContainsOnly(a);

            //Test that None produces an empty composite 
            result = a & LogCategory.None;
            AssertThat(result).IsEmpty();

            //Test that order doesn't matter
            result = LogCategory.None & a;
            AssertThat(result).IsEmpty();

            //Test that null produces an empty composite
            result = a & null;
            AssertThat(result).IsEmpty();

            //Test that order doesn't matter
            result = null & a;
            AssertThat(result).IsEmpty();

            a.Unregister();
            b.Unregister();
            c.Unregister();
        }

        private void testLogicalXOR()
        {
            LogCategory a = new LogCategory("A", true),
                        b = new LogCategory("B", true),
                        c = new LogCategory("C", true);

            var composite = a | b | c;
            var compositeMask = a | c;

            //Test that operator applies the mask correctly
            var result = composite ^ compositeMask;
            AssertThat(result).ContainsOnly(b);

            //Test that None behaves like an empty mask
            result = composite ^ LogCategory.None;
            AssertThat(result).Contains(a);
            AssertThat(result).Contains(b);
            AssertThat(result).Contains(c);

            //Test that order doesn't matter
            result = LogCategory.None ^ composite;
            AssertThat(result).Contains(a);
            AssertThat(result).Contains(b);
            AssertThat(result).Contains(c);

            //TODO: Test LogCategory.All
            //result = composite ^ LogCategory.All;
            //result = LogCategory.All ^ composite;

            a.Unregister();
            b.Unregister();
            c.Unregister();
        }

        private void testComplementaryComposition()
        {
            LogCategory a = new LogCategory("A", true),
                        b = new LogCategory("B", true),
                        c = new LogCategory("C", false);

            var composite = a | b;

            LogCategory result = ~composite;

            //Test that two registered entries can swapped, and then swapped back again to get back to the initial state
            result = ~result;
            AssertThat(result).IsEqualTo(composite);

            //Test that unregistered entries translate to All as the complementary output
            result = ~c;
            AssertThat(result).IsEqualTo(LogCategory.All);

            //Test that All translates to None as the complementary output
            result = ~LogCategory.All;
            AssertThat(result).IsEqualTo(LogCategory.None);

            //Test that None translates to All as the complementary output
            result = ~LogCategory.None;
            AssertThat(result).IsEqualTo(LogCategory.All);

            a.Unregister();
            b.Unregister();
        }

        #endregion
        #region Equality Tests

        private void testEquality()
        {
            var emptyComposite = new CompositeLogCategory(new HashSet<LogCategory>()
            {
                LogCategory.None
            });

            var valuedComposite = new CompositeLogCategory(new HashSet<LogCategory>()
            {
                LogCategory.Info
            });

            //Test that an empty composite is valued the same as LogCategory.None
            testEquality(emptyComposite, LogCategory.None);

            //Test that a valued composite is values the same as its component elements
            testEquality(valuedComposite, LogCategory.Info);
        }

        void testEquality(LogCategory category, LogCategory categoryOther)
        {
            //Test that compare value is correct
            AssertThat(category.CompareTo(categoryOther)).IsZero();

            //Test that equality check is correct
            AssertThat(category).IsEqualTo(categoryOther);

            //Test that hashcodes match
            int hash = category.GetHashCode(),
                hashOther = categoryOther.GetHashCode();

            AssertThat(hash).IsEqualTo(hashOther);
        }
        #endregion
        #region Miscellaneous Tests
        private void testErrorCategoryDoesNotIncludeCategoryAll()
        {
            var composite = new CompositeLogCategory(new HashSet<LogCategory>()
            {
                LogCategory.All
            });
            AssertThat(LogCategory.IsErrorCategory(composite)).IsFalse();
        }
        #endregion
        /// <summary>
        /// TransferMode is primarily useful for checking that registration status propagates correctly to all instances
        /// </summary>
        private enum TransferMode
        {
            ParentToChild,
            ChildToParent
        }
    }
}
