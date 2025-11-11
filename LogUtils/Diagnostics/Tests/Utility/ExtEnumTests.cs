using LogUtils.Diagnostics.Tests.Components;
using LogUtils.Enums;
using System;

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
            TestEnum testEnum = new TestEnum("test", register: true);

            AssertThat(testEnum.Index).IsPositiveOrZero();

            testEnum.Unregister();
            AssertThat(testEnum.Index).IsNegative();

            clearSharedData();
        }

        private void testIndexIsSetCorrectlyOnRegisterAndUnregister_Method()
        {
            TestEnum testEnum = new TestEnum("test", register: false);

            testEnum.Register();
            AssertThat(testEnum.Index).IsPositiveOrZero();

            testEnum.Unregister();
            AssertThat(testEnum.Index).IsNegative();

            clearSharedData();
        }

        private void testManagedReferenceIsInitialized()
        {
            TestEnum testEnum = new TestEnum("test-managed", register: false); //It shouldn't matter if we register the enum, this should always work

            AssertThat(testEnum.ManagedReference).IsSameInstance(testEnum);

            testEnum.Unregister();
            clearSharedData();
        }

        private void testManagedReferenceIsSharedBetweenInstances()
        {
            TestEnum testEnumA = new TestEnum("test-managed", register: false), //It shouldn't matter if we register the enum, this should always work
                     testEnumB = new TestEnum("test-managed", register: false);

            AssertThat(testEnumA.ManagedReference).IsSameInstance(testEnumB.ManagedReference);

            testEnumA.Unregister();
            clearSharedData();
        }

        /// <summary>
        /// Test that a log filename can exist at two different paths without sharing the same managed reference
        /// </summary>
        private void testManagedReferenceIsDifferentPerLogPath()
        {
            LogID testA = createTestLogID("test-managed", UtilityConsts.PathKeywords.ROOT);
            LogID testB = createTestLogID("test-managed", UtilityConsts.PathKeywords.STREAMING_ASSETS);

            AssertThat(testA.ManagedReference).IsNotThisInstance(testB.ManagedReference);
        }

        private static LogID createTestLogID(string filename, string path = null)
        {
            //TODO: LogID instances need to be cleaned up through SharedDataHandler
            return new LogID(filename, path, LogAccess.Private, false);
        }

        /// <summary>
        /// This should be called after each test that sets shared data to ensure that state transfer isn't a factor in between tests
        /// </summary>
        private void clearSharedData()
        {
            Type testEnumType = typeof(TestEnum);
            UtilityCore.DataHandler.DataCollection[testEnumType].Clear();
        }
    }
}
