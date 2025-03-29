using LogUtils.Diagnostics.Tests.Components;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal partial class ExtEnumTests
    {
        private void testCaseInsensitivity()
        {
            testCaseInsensitiveValuesPropagateRegistrationStatus();
            testCaseInsensitiveValuesHaveTheSameIndex();
        }

        private void testCaseInsensitiveValuesPropagateRegistrationStatus()
        {
            TestEnum testEnumA = new TestEnum("test", register: false),
                     testEnumB = new TestEnum("teSt", register: false);

            testEnumB.Register();
            AssertThat(testEnumA.Registered).IsTrue()
                                            .IsEqualTo(testEnumB.Registered);

            testEnumA.Unregister();
            AssertThat(testEnumA.Registered).IsFalse()
                                            .IsEqualTo(testEnumB.Registered);

            clearSharedData();
        }

        private void testCaseInsensitiveValuesHaveTheSameIndex()
        {
            TestEnum testEnumA = new TestEnum("test", register: true),
                     testEnumB = new TestEnum("teSt", register: true),
                     testEnumC = new TestEnum("TEst", register: false); //Confirm that registration status is unimportant

            AssertThat(testEnumA.Index).IsEqualTo(testEnumB.Index);
            AssertThat(testEnumC.Index).IsEqualTo(testEnumB.Index);

            testEnumA.Unregister();
            clearSharedData();

            testEnumA = new TestEnum("another " + testEnumA.value, register: false);
            testEnumB = new TestEnum("another " + testEnumB.value, register: true);

            AssertThat(testEnumA.Index).IsEqualTo(testEnumB.Index); //Registered index of B should tranfer to A

            testEnumA.Unregister();
            clearSharedData();
        }
    }
}
