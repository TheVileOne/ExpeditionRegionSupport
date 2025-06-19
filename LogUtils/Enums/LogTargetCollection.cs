using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Enums
{
    public class LogTargetCollection : IReadOnlyCollection<ILogTarget>
    {
        public IReadOnlyCollection<ILogTarget> AllTargets
        {
            get
            {
                List<ILogTarget> collection = new List<ILogTarget>();

                collection.AddRange(LogIDs);
                collection.AddRange(ConsoleIDs);
                return collection;
            }
        }

        public int Count => LogIDs.Count + ConsoleIDs.Count;

        public List<LogID> LogIDs = new List<LogID>();
        public List<ConsoleID> ConsoleIDs = new List<ConsoleID>();

        /// <summary>
        /// Constructs a new target collection containing no targets
        /// </summary>
        public LogTargetCollection()
        {
        }

        /// <summary>
        /// Constructs a new target collection containing the elements of the provided IEnumerable
        /// </summary>
        public LogTargetCollection(IEnumerable<ILogTarget> targets) : this()
        {
            LogIDs.AddRange(targets.OfType<LogID>());
            ConsoleIDs.AddRange(targets.OfType<ConsoleID>());
        }

        public bool Contains(ILogTarget target) => LogIDs.Contains(target) || ConsoleIDs.Contains(target);

        public IEnumerator<ILogTarget> GetEnumerator() => AllTargets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => AllTargets.GetEnumerator();
    }
}
