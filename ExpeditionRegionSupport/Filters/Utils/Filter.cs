using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public class Filter
    {
        public FilterCriteria Criteria = FilterCriteria.None;

        private bool _enabled = true;
        /// <summary>
        /// Flag that prevents filter logic from being applied
        /// </summary>
        public virtual bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        /// <summary>
        /// Applies any constraints necessary for comparing sets of data
        /// </summary>
        public virtual void Apply()
        {
        }

        /// <summary>
        /// Used when there is a need to check a certain game condition is true
        /// </summary>
        public virtual bool ConditionMet()
        {
            return true;
        }
    }

    public class ListFilter<T> : Filter
    {
        public List<T> CompareValues;
        public Func<T, T> ValueModifier;

        public ListFilter(List<T> compareValues, FilterCriteria criteria = FilterCriteria.MustInclude)
        {
            CompareValues = compareValues ?? new List<T>();
            Criteria = criteria;
        }

        /// <summary>
        /// Checks a list against a criteria, and removes items that don't meet that criteria/>
        /// </summary>
        /// <param name="allowedItems">The list to check</param>
        /// <param name="valueModifier">A function used to apply formatting to a value</param>
        public virtual void Apply(List<T> allowedItems)
        {
            if (!Enabled) return;

            //Plugin.Logger.LogDebug(ChallengeFilterSettings.FilterTarget.ChallengeName() + " filter applied");

            //Determines if we check if Compare reference contains or does not contain an item
            bool compareCondition = Criteria == FilterCriteria.MustInclude;

            allowedItems.RemoveAll(item =>
            {
                return !Evaluate(item, compareCondition); //Evaluate needs to return false for items to be kept in the list
            });

            /*
            if (RainWorld.ShowLogs)
            {
                Plugin.Logger.LogDebug("Remaining regions");
                foreach (var item in allowedItems)
                    Plugin.Logger.LogDebug(item);
            }
            */
        }

        public virtual bool Evaluate(T item, bool condition)
        {
            item = ValueModifier == null ? item : ValueModifier(item);

            //if (RainWorld.ShowLogs)
            //    Plugin.Logger.LogDebug("Comparing region: " + item + " Must be in list: " + condition);
            return CompareValues.Contains(item) == condition;
        }
    }

    public enum FilterCriteria
    {
        None,
        MustInclude,
        MustExclude
    }
}
