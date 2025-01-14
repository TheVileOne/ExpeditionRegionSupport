using System;
using System.Linq;

namespace LogUtils
{
    public class LinkedLogRequestCollection : BufferedLinkedList<LogRequest>
    {
        public LinkedLogRequestCollection(int capacity) : base(capacity)
        {
        }

        public LogRequest[] GetRequestsSorted()
        {
            if (Count == 0)
                return Array.Empty<LogRequest>();

            return this.OrderBy(req => req.Data.ID).ThenBy(req => req.Data.Properties.CurrentFolderPath).ToArray();
        }
    }
}
