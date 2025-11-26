using LogUtils.Enums;

namespace LogUtils.Diagnostics.Tests.Components
{
    /// <summary>
    /// A bare-structured implementation of a SharedExtEnum
    /// </summary>
    public class TestEnum : SharedExtEnum<TestEnum>
    {
        /// <summary>
        /// Access factory methods for creating specific kinds of <see cref="TestEnum"/> instances 
        /// </summary>
        public static TestEnumFactory Factory = new TestEnumFactory();

        public TestEnum(string value, bool register = false) : base(value, register)
        {
        }
    }
}
