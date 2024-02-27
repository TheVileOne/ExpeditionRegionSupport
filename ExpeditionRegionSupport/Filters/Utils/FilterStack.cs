using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public class FilterStack<T, Ttarget> where T : FilterApplicator<Ttarget>
    {
        public Stack<FilterApplicator<Ttarget>> Filters { get; }

        public FilterApplicator<Ttarget> BaseFilter { get; private set; }
        public FilterApplicator<Ttarget> ActiveFilter { get => Filters.Count > 0 ? Filters.Peek() : null; }

        public FilterStack()
        {
            Filters = new Stack<FilterApplicator<Ttarget>>();
        }

        public void AssignNew()
        {
            var activeFilter = ActiveFilter as CachedFilterApplicator<Ttarget>;

            if (activeFilter == null)
                throw new NotImplementedException();

            Assign(new FilterApplicator<Ttarget>(activeFilter.Cache));
        }

        public void Assign(FilterApplicator<Ttarget> newFilter )
        {
            if (Filters.Count == 0)
                BaseFilter = newFilter;

            Filters.Push(newFilter);
        }

        public void AssignBase(FilterApplicator<Ttarget> newBase)
        {
            BaseFilter = newBase;

            Filters.Clear();
            Filters.Push(newBase);
        }
    }
}
