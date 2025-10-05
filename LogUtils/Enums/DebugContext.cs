namespace LogUtils.Enums
{
    public class DebugContext : ExtEnum<DebugContext>
    {
        public DebugContext(string value, bool register = false) : base(value, register)
        {
        }

        static DebugContext()
        {
            UtilityCore.EnsureInitializedState();
        }

        internal static void InitializeEnums()
        {
            Normal = new DebugContext("Normal", true);
            TestCondition = new DebugContext("TestCondition", true);
        }

        public static DebugContext Normal;
        public static DebugContext TestCondition;
    }
}
