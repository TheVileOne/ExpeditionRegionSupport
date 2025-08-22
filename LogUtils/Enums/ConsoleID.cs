using LogUtils.Requests;

namespace LogUtils.Enums
{
    public class ConsoleID : SharedExtEnum<ConsoleID>, ILogTarget
    {
        /// <inheritdoc/>
        public bool IsEnabled => true;

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

        public RequestType GetRequestType(ILogFileHandler handler)
        {
            return RequestType.Console;
        }

        public static ConsoleID BepInEx;
        public static ConsoleID RainWorld;

        public static CompositeLogTarget operator |(ConsoleID a, ILogTarget b)
        {
            return LogTarget.Combiner.Combine(a, b);
        }
    }
}
