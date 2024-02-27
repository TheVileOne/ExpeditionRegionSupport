using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public class CachedFilterStack<T>
    {
        protected Stack<CachedFilterApplicator<T>> Filters { get; }

        /// <summary>
        /// The FilterApplicator at the bottom of the stack
        /// </summary>
        public CachedFilterApplicator<T> BaseFilter { get; private set; }
        
        /// <summary>
        /// The FilterApplicator at the top of the stack
        /// </summary>
        public CachedFilterApplicator<T> CurrentFilter { get => Filters.Count > 0 ? Filters.Peek() : null; }

        /// <summary>
        /// Returns the cache of the latest filter if one exists
        /// </summary>
        public List<T> Cache => CurrentFilter?.Cache;

        /// <summary>
        /// A flag that indicates whether the first filter on the stack can be removed. Invoking Clear overrides this flag.
        /// </summary>
        public bool AllowBaseRemoval = true;

        public CachedFilterStack()
        {
            Filters = new Stack<CachedFilterApplicator<T>>();
        }

        public CachedFilterApplicator<T> AssignNew()
        {
            if (BaseFilter == null)
                throw new NotImplementedException("This action requires an existing filter stored in the stack to inherit from.");

            var filter = new CachedFilterApplicator<T>(CurrentFilter.Cache);

            Push(filter);
            return filter;
        }

        public void Assign(CachedFilterApplicator<T> newFilter )
        {
            Push(newFilter);
        }

        public void AssignBase(CachedFilterApplicator<T> newBase)
        {
            BaseFilter = newBase;

            Filters.Clear();
            Filters.Push(newBase);
        }

        public void Push(CachedFilterApplicator<T> newFilter)
        {
            if (Filters.Count == 0)
                BaseFilter = newFilter;

            Filters.Push(newFilter);
        }

        public CachedFilterApplicator<T> Pop()
        {
            if (Filters.Count == 0) return null;

            //Only handle the base when allowed
            if (Filters.Count == 1)
            {
                if (!AllowBaseRemoval)
                    return BaseFilter;
                BaseFilter = null;
            }

            return Filters.Pop();
        }

        public void Clear(bool keepBase = false)
        {
            if (keepBase) //Ensures that everything on the stack except for the first element are removed 
            {
                for (int i = Filters.Count - 1; i > 0; i--)
                    Filters.Pop();
                return;
            }

            BaseFilter = null;
            Filters.Clear();
        }
    }
}
