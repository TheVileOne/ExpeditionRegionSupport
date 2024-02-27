using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public class CachedFilterStack<T>
    {
        public Stack<CachedFilterApplicator<T>> Filters { get; }

        public CachedFilterApplicator<T> BaseFilter { get; private set; }
        public CachedFilterApplicator<T> ActiveFilter { get => Filters.Count > 0 ? Filters.Peek() : null; }

        public CachedFilterStack()
        {
            Filters = new Stack<CachedFilterApplicator<T>>();
        }

        public void AssignNew()
        {
            var activeFilter = ActiveFilter;

            if (activeFilter == null)
                throw new NotImplementedException("This action requires an existing filter stored in the stack to inherit from.");

            Assign(new CachedFilterApplicator<T>(activeFilter.Cache));
        }

        public void Assign(CachedFilterApplicator<T> newFilter )
        {
            if (Filters.Count == 0)
                BaseFilter = newFilter;

            Filters.Push(newFilter);
        }

        public void AssignBase(CachedFilterApplicator<T> newBase)
        {
            BaseFilter = newBase;

            Filters.Clear();
            Filters.Push(newBase);
        }
    }
}
