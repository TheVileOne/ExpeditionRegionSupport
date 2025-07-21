using System.Collections.Generic;

namespace ExpeditionRegionSupport.Filters.Utils
{
    public class CachedFilterApplicator<T> : FilterApplicator<T>
    {
        public List<T> Cache;

        public CachedFilterApplicator(List<T> target) : base(target)
        {
            //Overwrite target with a separate list reference
            Cache = Target = new List<T>(target);
        }
    }
}
