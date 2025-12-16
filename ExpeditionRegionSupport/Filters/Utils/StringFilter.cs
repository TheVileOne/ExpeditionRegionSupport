using System.Collections.Generic;
using System.Linq;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public class StringFilter
    {
        /// <summary>
        /// Unfiltered list of strings
        /// </summary>
        public List<string> Entries;

        /// <summary>
        /// A list of collection modifier functions that apply the filter logic
        /// </summary>
        public event FilterCondition Criteria;

        public StringFilter(IEnumerable<string> values)
        {
            Entries = new List<string>(values);
        }

        public List<string> Apply()
        {
            IEnumerable<string> filteredStrings = Entries;
            foreach (var filter in getFilterEvents())
                filteredStrings = filter.Invoke(filteredStrings);
            return filteredStrings.ToList();
        }

        private IEnumerable<FilterCondition> getFilterEvents()
        {
            if (Criteria == null)
                return Enumerable.Empty<FilterCondition>();

            return Criteria.GetInvocationList().Cast<FilterCondition>();
        }

        public delegate IEnumerable<string> FilterCondition(IEnumerable<string> entries);
    }
}
