namespace LogUtils.Enums
{
    public class ConsoleID : ExtEnum<ConsoleID>
    {
        public ConsoleID(string value, bool register = false) : base(value, register)
        {
        }

        static ConsoleID()
        {
            UtilityCore.EnsureInitializedState();
        }

        internal static void InitializeEnums()
        {
            BepInEx = new ConsoleID("BepInEx", true);
            RainWorld = new ConsoleID("RainWorld", true);
        }

        public static ConsoleID BepInEx;
        public static ConsoleID RainWorld;
    }
}
