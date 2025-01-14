using System.Collections.Generic;
using System.Linq;

namespace LogUtils
{
    public class LogRequestQueue
    {
        protected List<Queue<LogRequest>> QueueList = new List<Queue<LogRequest>>();
        protected IEnumerable<LogRequest> Requests;

        public LogRequestQueue(IEnumerable<LogRequest> source)
        {
            Requests = source;
        }

        public IEnumerable<LogRequest> GetRequests()
        {
            //Return all requests for the first identified LogID in the collection
            LogRequest compareRef = null;
            foreach (var request in Requests)
            {
                if (compareRef == null || compareRef.Data.ID.Properties.HasID(request.Data.ID))
                {
                    compareRef = request;
                    yield return request;
                }
                else
                {
                    var existingQueue = FindQueue(request);

                    if (existingQueue == null)
                    {
                        existingQueue = new Queue<LogRequest>();
                        QueueList.Add(existingQueue);
                    }
                    existingQueue.Enqueue(request);
                }
            }

            //Return any queued requests that were detected while processing the first identified LogID
            while (QueueList.Count > 0)
            {
                var currentQueue = QueueList[0];

                while (currentQueue.Any())
                    yield return currentQueue.Dequeue();

                //Don't leave an empty queue in the list
                QueueList.RemoveAt(0);
            }
            yield break;
        }

        protected Queue<LogRequest> FindQueue(LogRequest request)
        {
            return QueueList.Find(queue => queue.Any() && queue.Peek().Data.Properties.HasID(request.Data.ID));
        }
    }
}
