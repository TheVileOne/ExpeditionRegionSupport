using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Enums
{
    public sealed class LogTargetCollection : IReadOnlyCollection<ILogTarget>, ICloneable
    {
        /// <summary>
        /// Creates an independent collection instances containing the same collection items as this instance
        /// </summary>
        public LogTargetCollection AllTargets => (LogTargetCollection)Clone();

        /// <inheritdoc/>
        public int Count => LogIDs.Count + ConsoleIDs.Count;

        public List<LogID> LogIDs { get; }
        public List<ConsoleID> ConsoleIDs { get; }

        /// <summary>
        /// Constructs a new target collection containing no targets
        /// </summary>
        public LogTargetCollection()
        {
            LogIDs = new List<LogID>();
            ConsoleIDs = new List<ConsoleID>();
        }

        /// <summary>
        /// Constructs a new target collection containing the elements of the provided composite object
        /// </summary>
        public LogTargetCollection(CompositeLogTarget targets) : this()
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

        /// <summary>
        /// Removes all elements from the collection
        /// </summary>
        public void Clear()
        {
            LogIDs.Clear();
            ConsoleIDs.Clear();
        }

        /// <summary>
        /// Checks whether the collection contains a given log target
        /// </summary>
        public bool Contains(ILogTarget target) => LogIDs.Contains(target) || ConsoleIDs.Contains(target);

        /// <inheritdoc/>
        public IEnumerator<ILogTarget> GetEnumerator() => AllTargets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => AllTargets.GetEnumerator();

        /// <summary>
        /// Creates a new object that is a member deep copy of this instance
        /// </summary>
        public object Clone()
        {
            LogTargetCollection clone = new LogTargetCollection();

            clone.AddRange(LogIDs);
            clone.AddRange(ConsoleIDs);
            return clone;
        }
    }
}
