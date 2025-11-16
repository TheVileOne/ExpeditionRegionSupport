using LogUtils.Diagnostics.Tests.Components;
using LogUtils.Enums;

namespace LogUtils.Diagnostics.Tests.Utility
{
    internal sealed partial class ExtEnumTests : TestCase, ITestable
    {
        internal const string TEST_NAME = "Test - ExtEnums";

        public ExtEnumTests() : base(TEST_NAME)
        {
        }

        public void Test()
        {
            //Index must be set to zero on register and negative one on unregister
            testIndexIsSetCorrectlyOnRegisterAndUnregister_Construction();
            testIndexIsSetCorrectlyOnRegisterAndUnregister_Method();

            //ManagedReference should always be set on initialization
            testManagedReferenceIsInitialized();
            testManagedReferenceIsSharedBetweenInstances();
            testManagedReferenceIsDifferentPerLogPath();

            testCaseInsensitivity();
        }

        [PostTest]
        public void ShowResults()
        {
            TestLogger.LogDebug(CreateReport());
        }

        private void testIndexIsSetCorrectlyOnRegisterAndUnregister_Construction()
        {
            TestEnum testEnum = TestEnum.Factory.Create("test", register: true);

            AssertThat(testEnum.Index).IsPositiveOrZero();

            testEnum.Unregister();
            AssertThat(testEnum.Index).IsNegative();
            TestEnumFactory.DisposeObjects();
        }

        private void testIndexIsSetCorrectlyOnRegisterAndUnregister_Method()
        {
            TestEnum testEnum = TestEnum.Factory.Create("test", register: false);

            testEnum.Register();
            AssertThat(testEnum.Index).IsPositiveOrZero();

            testEnum.Unregister();
            AssertThat(testEnum.Index).IsNegative();
            TestEnumFactory.DisposeObjects();
        }

        private void testManagedReferenceIsInitialized()
        {
            TestEnum testEnum = TestEnum.Factory.Create("test-managed", register: false); //It shouldn't matter if we register the enum, this should always work

            AssertThat(testEnum.ManagedReference).IsSameInstance(testEnum);
            TestEnumFactory.DisposeObjects();
        }

        private void testManagedReferenceIsSharedBetweenInstances()
        {
            TestEnum testEnumA = TestEnum.Factory.Create("test-managed", register: false), //It shouldn't matter if we register the enum, this should always work
                     testEnumB = TestEnum.Factory.FromTarget(testEnumA);

            AssertThat(testEnumA.ManagedReference).IsSameInstance(testEnumB.ManagedReference);
            TestEnumFactory.DisposeObjects();
        }

        /// <summary>
        /// Test that a log filename can exist at two different paths without sharing the same managed reference
        /// </summary>
        private void testManagedReferenceIsDifferentPerLogPath()
        {
            LogID testEnumA = TestLogID.Factory.FromPath(UtilityConsts.PathKeywords.ROOT),
                  testEnumB = TestLogID.Factory.FromTarget(testEnumA, UtilityConsts.PathKeywords.STREAMING_ASSETS);

            AssertThat(testEnumA.ManagedReference).IsNotThisInstance(testEnumB.ManagedReference);
            TestEnumFactory.DisposeObjects();
        }
    }
}
