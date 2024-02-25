using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters
{
    public class FilterApplicator<T>
    {
        protected List<T> Backup;
        protected List<T> Target;

        protected List<T> ItemsRemoved = new List<T>();
        public List<T> ItemsToRemove = new List<T>();

        public FilterApplicator(List<T> target)
        {
            Backup = new List<T>(target);
            Target = target;
        }

        public void Apply()
        {
            foreach (var item in ItemsToRemove)
            {
                if (Target.Remove(item)) //Makes sure items belong to the target
                    ItemsRemoved.Add(item);
            }

            ItemsToRemove.Clear();
        }

        public List<T> ApplyTemp(IEnumerable<T> itemsNotWanted)
        {
            return Target.Except(itemsNotWanted).ToList();
        }

        public List<T> ApplyTemp(Func<T, bool> excludeCondition)
        {
            return Target.Where(item => !excludeCondition(item)).ToList();
        }

        public List<T> ApplyTemp<T2>(IEnumerable<T2> items, Func<T2, bool> getItem, Func<T2, T> getValue)
        {
            //Filter IEnumerable by the condition getItem
            var enumerator = items.Where(getItem).GetEnumerator();

            //Retrieve a T value from each item. These values are what we want to filter from the Target
            List<T> values = new List<T>();
            while (enumerator.MoveNext())
                values.Add(getValue(enumerator.Current));

            return ApplyTemp(values);
        }

        /// <summary>
        /// Restores items from Backup in the exact order they were int he target list
        /// </summary>
        public void Restore()
        {
            ItemsRemoved.Clear();
            ItemsToRemove.Clear();
            Target.Clear();
            Target.AddRange(Backup);
        }
    }
}
