using LogUtils.Collections;
using LogUtils.Helpers.Comparers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Properties.Formatting
{
    public class LogRuleCollection : ValueSet<LogRule>, IOrderedEnumerable<LogRule>
    {
        internal IOrderedEnumerable<LogRule> Enumerable => Values.OrderBy(r => r.Priority);

        private static readonly NameComparer nameComparer = new NameComparer();

        /// <summary>
        /// Tracks LogRule state changes
        /// </summary>
        public Stack<ChangeState> ChangeRecord;

        /// <summary>
        /// Enables/disables change state system
        /// </summary>
        public bool TrackChanges
        {
            get => ChangeRecord != null;
            set
            {
                if (value)
                {
                    if (ChangeRecord == null)
                        ChangeRecord = new Stack<ChangeState>();
                }
                else
                {
                    ResetRecord();
                }
            }
        }

        /// <inheritdoc/>
        public override bool IsReadOnly => false; //The collection itself is never ReadOnly, only its contained rules may be

        /// <summary>
        /// Gets a value indicating whether rules belonging to this collection should protect their persistent state
        /// </summary>
        internal bool AllowRuleChanges => !base.IsReadOnly;

        /// <summary>
        /// Creates a new <see cref="LogRuleCollection"/> instance
        /// </summary>
        /// <param name="logRuleReadOnlySource">The binding source for determining the ReadOnly state of  a <see cref="LogRule"/> that belongs to this collection</param>
        public LogRuleCollection(ReadOnlyProvider logRuleReadOnlySource = null) : base(logRuleReadOnlySource)
        {
        }

        /// <summary>
        /// Notify that <see cref="LogRule"/> changes are ready to be tracked
        /// </summary>
        public void ChangeAlert()
        {
            //Rule changes should be tracked when the initialization period has ended
            if (AllowRuleChanges)
            {
                UtilityLogger.Log("LogRule change in progress");
                TrackChanges = true;
            }
        }

        public void ResetRecord()
        {
            if (TrackChanges)
            {
                ChangeRecord.Clear();
                ChangeRecord = null;
            }
        }

        public void RestoreRecord()
        {
            UtilityLogger.Log("Attempting to restore rule modifications");
            if (TrackChanges)
            {
                while (ChangeRecord.Any())
                    ChangeRecord.Pop().Restore();
                ResetRecord();
            }
            else
            {
                UtilityLogger.Log("Nothing to restore...");
            }
        }

        /// <summary>
        /// Adds a LogRule instance to the collection of Rules
        /// </summary>
        /// <remarks>Do not use this for temporary rule changes, use <see cref="SetTemporaryRule"/> instead</remarks>
        public override bool Add(LogRule rule)
        {
            bool ruleAdded = base.Add(rule);

            if (ruleAdded)
                rule.Owner = this;
            return ruleAdded;
        }

        /// <summary>
        /// Replaces an existing rule with another instance
        /// </summary>
        /// <remarks>
        /// <para>Be warned, running this each time your mod runs will overwrite data being saved, and read from file</para>
        /// <para>Do not replace existing property data values in a way that might break the parse logic</para>
        /// <para>Consider using temporary rules instead, and handle saving of the property values through your mod</para>
        /// <para>In either case, you may want to inherit from the existing property in case a user has changed the property through the file</para>
        /// </remarks>
        public bool Replace(LogRule rule)
        {
            LogRule existingRule = FindByName(rule.Name);

            if (existingRule != null)
            {
                //Transfer over temporary rules as long as replacement rule doesn't have one already
                if (rule.TemporaryOverride == null)
                    rule.TemporaryOverride = existingRule.TemporaryOverride;

                existingRule.Owner = null;
                Remove(existingRule);
            }
            return Add(rule); //Add rule when there is no existing rule match
        }

        /// <inheritdoc cref="ICollection{LogRule}.Remove(LogRule)"/>
        public override bool Remove(LogRule rule)
        {
            bool ruleRemoved = base.Remove(rule);

            if (ruleRemoved)
                rule.Owner = null;
            return ruleRemoved;
        }

        public bool Remove(string name)
        {
            LogRule existingRule = FindByName(name);
            return Remove(existingRule);
        }

        /// <inheritdoc/>
        protected override void Reset()
        {
            if (Values != null)
            {
                Values.Clear();
                return;
            }
            Values = new HashSet<LogRule>(nameComparer);
        }

        public void SetTemporaryRule(LogRule rule)
        {
            LogRule targetRule = FindByName(rule.Name);

            if (targetRule != null)
                targetRule.TemporaryOverride = rule;
            else
            {
                rule.IsTemporary = true;
                Add(rule); //No associated rule was found, treat temporary rule as a normal rule
            }
        }

        public void RemoveTemporaryRule(LogRule rule)
        {
            LogRule targetRule = Find(r => r.TemporaryOverride == rule || r == rule);

            if (targetRule != null)
            {
                targetRule.IsTemporary = false;
                targetRule.TemporaryOverride = null;
            }
        }

        public LogRule Find(Predicate<LogRule> match)
        {
            return this.FirstOrDefault(match.Invoke);
        }

        public LogRule FindByName(string name)
        {
            return Find(r => nameComparer.Equals(r, name));
        }

        public LogRule FindByType<T>() where T : LogRule
        {
            return Find(r => r is T);
        }

        public LogRule FindByType(Type type)
        {
            return Find(r => r.GetType() == type);
        }

        /// <inheritdoc/>
        public IOrderedEnumerable<LogRule> CreateOrderedEnumerable<TKey>(Func<LogRule, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return Enumerable;
        }

        /// <inheritdoc/>
        public override IEnumerator<LogRule> GetEnumerator()
        {
            return Enumerable.GetEnumerator();
        }

        private class NameComparer : IEqualityComparer<LogRule>
        {
            public bool Equals(LogRule logRule, LogRule logRuleOther)
            {
                if (logRule == null || logRuleOther == null)
                    return logRule == logRuleOther;
                return ComparerUtils.StringComparerIgnoreCase.Equals(logRule.Name, logRuleOther.Name);
            }

            public bool Equals(LogRule logRule, string name)
            {
                if (logRule == null)
                    return false; //Do not equate null values if the types are different
                return ComparerUtils.StringComparerIgnoreCase.Equals(logRule.Name, name);
            }

            public int GetHashCode(LogRule logRule)
            {
                return logRule?.Name != null ? logRule.Name.GetHashCode() : 0;
            }
        }
    }
}
