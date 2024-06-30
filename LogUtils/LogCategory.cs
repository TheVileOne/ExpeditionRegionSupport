namespace LogUtils
{
    public class LogCategory : ExtEnum<LogCategory>
    {
        public LogCategory(string value, bool register = false) : base(value, register)
        {
        }

        public static readonly LogCategory All = new LogCategory("All", true);
        public static readonly LogCategory None = new LogCategory("None", true);
        public static readonly LogCategory Debug = new LogCategory("Debug", true);
        public static readonly LogCategory Info = new LogCategory("Info", true);
        public static readonly LogCategory Message = new LogCategory("Message", true);
        public static readonly LogCategory Important = new LogCategory("Important", true);
        public static readonly LogCategory Warning = new LogCategory("Warning", true);
        public static readonly LogCategory Error = new LogCategory("Error", true);
        public static readonly LogCategory Fatal = new LogCategory("Fatal", true);
    }
}
