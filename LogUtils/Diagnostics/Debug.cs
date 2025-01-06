using LogUtils.Diagnostics.Tests;
using LogUtils.Diagnostics.Tests.Utility;

namespace LogUtils.Diagnostics
{
    public static class Debug
    {
        public static bool AssertsEnabled = true;

        internal static TestSuite UtilityTests;

        internal static void InitializeTestSuite()
        {
            UtilityTests = new TestSuite();

            AssertTest assertTest = new AssertTest();
            UtilityTests.Add(assertTest);
        }
    }
}
