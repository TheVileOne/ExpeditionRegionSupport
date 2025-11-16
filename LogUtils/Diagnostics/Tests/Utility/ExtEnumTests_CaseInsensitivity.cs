using LogUtils.Diagnostics.Tests.Components;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal sealed partial class ExtEnumTests
    {
        private void testCaseInsensitivity()
        {
            testCaseInsensitiveValuesPropagateRegistrationStatus();
            testCaseInsensitiveValuesHaveTheSameIndex();
        }

        private void testCaseInsensitiveValuesPropagateRegistrationStatus()
        {
            TestEnum testEnumA = TestEnum.Factory.Create("test", register: false),
                     testEnumB = TestEnum.Factory.Create("teSt", register: false);

            testEnumB.Register();
            AssertThat(testEnumA.Registered).IsTrue()
                                            .IsEqualTo(testEnumB.Registered);

            testEnumA.Unregister();
            AssertThat(testEnumA.Registered).IsFalse()
                                            .IsEqualTo(testEnumB.Registered);
            TestEnumFactory.DisposeObjects();
        }

        private void testCaseInsensitiveValuesHaveTheSameIndex()
        {
            TestEnum testEnumA = TestEnum.Factory.Create("test", register: true),
                     testEnumB = TestEnum.Factory.Create("teSt", register: true),
                     testEnumC = TestEnum.Factory.Create("TEst", register: false); //Confirm that registration status is unimportant

            AssertThat(testEnumA.Index).IsEqualTo(testEnumB.Index);
            AssertThat(testEnumC.Index).IsEqualTo(testEnumB.Index);

            TestEnumFactory.DisposeObjects();

            testEnumA = TestEnum.Factory.Create("another " + testEnumA.Value, register: false);
            testEnumB = TestEnum.Factory.Create("another " + testEnumB.Value, register: true);

            AssertThat(testEnumA.Index).IsEqualTo(testEnumB.Index); //Registered index of B should tranfer to A
            TestEnumFactory.DisposeObjects();
        }
    }
}
