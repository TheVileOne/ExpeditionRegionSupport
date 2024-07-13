using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace LogUtils
{
    public class LogRuleCollection : IOrderedEnumerable<LogRule>
    {
        public bool ReadOnly;

        protected List<LogRule> InnerList = new List<LogRule>();
        protected IOrderedEnumerable<LogRule> InnerEnumerable => InnerList.OrderBy(r => r.Priority);

        /// <summary>
        /// Adds a LogRule instance to the collection of Rules
        /// Do not use this for temporary rule changes, use SetTemporaryRule instead 
        /// </summary>
        public void Add(LogRule rule)
        {
            if (InnerList.Exists(r => LogProperties.CompareNames(r.Name, rule.Name))) //Don't allow more than one rule concept to be added with the same name
                return;
            InnerList.Add(rule);
        }

        /// <summary>
        /// Replaces an existing rule with another instance
        /// Be warned, running this each time your mod runs will overwrite data being saved, and read from file
        /// Do not replace existing property data values in a way that might break the parse logic
        /// Consider using temporary rules instead, and handle saving of the property values through your mod
        /// In either case, you may want to inherit from the existing property in case a user has changed the property through the file
        /// </summary>
        public void Replace(LogRule rule)
        {
            int ruleIndex = InnerList.FindIndex(r => LogProperties.CompareNames(r.Name, rule.Name));

            if (ruleIndex != -1)
            {
                LogRule replacedRule = InnerList[ruleIndex];

                //Transfer over temporary rules as long as replacement rule doesn't have one already
                if (rule.TemporaryOverride == null)
                    rule.TemporaryOverride = replacedRule.TemporaryOverride;
                InnerList.RemoveAt(ruleIndex);
            }
            Add(rule); //Add rule when there is no existing rule match
        }

        public bool Remove(LogRule rule)
        {
            return InnerList.Remove(rule);
        }

        public bool Remove(string name)
        {
            int ruleIndex = InnerList.FindIndex(r => LogProperties.CompareNames(r.Name, name));

            if (ruleIndex != -1)
            {
                InnerList.RemoveAt(ruleIndex);
                return true;
            }
            return false;
        }

        public void SetTemporaryRule(LogRule rule)
        {
            LogRule targetRule = FindByName(rule.Name);

            if (targetRule != null)
                targetRule.TemporaryOverride = rule;
            else
                Add(rule); //No associated rule was found, treat temporary rule as a normal rule
        }

        public void RemoveTemporaryRule(LogRule rule)
        {
            LogRule targetRule = InnerList.Find(r => r.TemporaryOverride == rule);

            if (targetRule != null)
                targetRule.TemporaryOverride = null;
        }

        public LogRule Find(Predicate<LogRule> match)
        {
            return InnerList.Find(match);
        }

        public LogRule FindByName(string name)
        {
            return Find(r => LogProperties.CompareNames(r.Name, name));
        }

        public LogRule FindByType<T>() where T : LogRule
        {
            return Find(r => r is T);
        }

        public LogRule FindByType(Type type)
        {
            return Find(r => r.GetType() == type);
        }

        public bool Contains(LogRule rule)
        {
            return InnerList.Contains(rule);
        }

        public IOrderedEnumerable<LogRule> CreateOrderedEnumerable<TKey>(Func<LogRule, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return InnerEnumerable;
        }

        public IEnumerator<LogRule> GetEnumerator()
        {
            return InnerEnumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InnerEnumerable.GetEnumerator();
        }
    }
}
