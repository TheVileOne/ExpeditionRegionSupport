namespace LogUtils
{
    public class LinkedLogRequestCollection : BufferedLinkedList<LogRequest>
    {
        public LinkedLogRequestCollection(int capacity) : base(capacity)
        {
        }

        public LogRequestQueue ToQueue()
        {
            return new LogRequestQueue(this);
        }
    }
}
