using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
