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

        public LogTargetCollection(CompositeLogTarget targets)
        {
            AddRange(targets.Set);
        }

        /// <summary>
        /// Constructs a new target collection containing the elements of the provided IEnumerable
        /// </summary>
        public LogTargetCollection(IEnumerable<ILogTarget> targets) : this()
        {
            AddRange(targets);
        }

        public void AddRange(IEnumerable<ILogTarget> targets)
        {
            foreach (var target in targets)
            {
                LogID fileTarget = target as LogID;

                if (fileTarget != null)
                {
                    LogIDs.Add(fileTarget);
                    continue;
                }

                ConsoleID consoleTarget = target as ConsoleID;

                if (consoleTarget != null)
                {
                    ConsoleIDs.Add(consoleTarget);
                    continue;
                }

                CompositeLogTarget compositeTarget = target as CompositeLogTarget;

                if (compositeTarget != null)
                    AddRange(compositeTarget.Set);
            }
        }

        public bool Contains(ILogTarget target) => LogIDs.Contains(target) || ConsoleIDs.Contains(target);

        public IEnumerator<ILogTarget> GetEnumerator() => AllTargets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => AllTargets.GetEnumerator();
    }
}
