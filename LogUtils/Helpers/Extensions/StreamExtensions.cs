using LogUtils.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Helpers.Extensions
{
    public static partial class ExtensionMethods
    {
        public static ResultAnalyzer GetAnalyzer(this IEnumerable<Condition.Result> results)
        {
            return new ResultAnalyzer(results);
        }

        public static StreamResumer[] InterruptAll<T>(this IEnumerable<T> handles) where T : PersistentFileHandle
        {
            //For best results, this should be treated as a critical section
            return handles.Where(handle => !handle.WaitingToResume)   //Retrieve entries that are available to interrupt
                          .Select(handle => handle.InterruptStream()) //Interrupt filestreams and collect resume handles
                          .ToArray();
        }

        public static void ResumeAll(this IEnumerable<StreamResumer> streams)
        {
            foreach (StreamResumer stream in streams)
                stream.Resume();
        }
    }
}
