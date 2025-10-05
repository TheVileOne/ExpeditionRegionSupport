using LogUtils.Collections;
using LogUtils.Enums;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Requests
{
    public class LinkedLogRequestCollection : BufferedLinkedList<LogRequest>
    {
        public LinkedLogRequestCollection(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// Returns an enumerable that sorts LogRequests by LogID (by value and path)
        /// </summary>
        public IOrderedEnumerable<LogRequest> SortRequests()
        {
            return this.OrderBy(req => req.Data.ID).ThenBy(req => req.Data.Properties.CurrentFolderPath);
        }

        /// <summary>
        /// Returns an enumerable that sorts LogRequests by LogID (by value and path) into partitioned groups for each different kind
        /// </summary>
        public IEnumerable<IGrouping<LogID, LogRequest>> GroupRequests()
        {
            return this.GroupBy(s => s.Data.ID, EqualityComparer<LogID>.Default);
        }
    }
}
