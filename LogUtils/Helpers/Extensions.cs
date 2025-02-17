using LogUtils.Diagnostics;
using LogUtils.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogUtils.Helpers
{
    public static class Extensions
    {
        /// <summary>
        /// Evaluates whether a string is equal to any of the provided values
        /// </summary>
        /// <param name="str">The string to evaluate</param>
        /// <param name="comparer">An IEqualityComparer to use for the evaluation</param>
        /// <param name="values">The values to compare the string against</param>
        /// <returns>Whether a match was found</returns>
        public static bool MatchAny(this string str, IEqualityComparer<string> comparer, params string[] values)
        {
            return values.Contains(str, comparer);
        }

        internal static void AppendComments(this StringBuilder sb, string commentOwner, List<CommentEntry> comments)
        {
            var applicableComments = comments.Where(entry => entry.Owner == commentOwner);

            foreach (string comment in applicableComments.Select(entry => entry.Message))
                sb.AppendLine(comment);
        }

        internal static void AppendPropertyString(this StringBuilder sb, string name, string value = "")
        {
            sb.AppendLine(LogProperties.ToPropertyString(name, value));
        }

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
