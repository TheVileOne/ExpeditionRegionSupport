namespace LogUtils.Enums
{
    public class BufferContext : ExtEnum<BufferContext>
    {
        public BufferContext(string value, bool register = false) : base(value, register)
        {
        }

        public static BufferContext HighVolume = new BufferContext("High Volume", true);
        public static BufferContext RequestConsolidation = new BufferContext("Consolidation", true);
        public static BufferContext WriteFailure = new BufferContext("Write Failure", true);
        public static BufferContext Debug = new BufferContext("Debug", true);
    }
}
