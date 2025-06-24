using LogUtils.Enums;

namespace LogUtils.Diagnostics.Tests.Components
{
    /// <summary>
    /// A bare-structured implementation of a SharedExtEnum
    /// </summary>
    public class TestEnum : SharedExtEnum<TestEnum>
    {
        public TestEnum(string value, bool register = false) : base(value, register)
        {
        }
    }
}
