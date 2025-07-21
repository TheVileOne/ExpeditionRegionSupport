using System.Collections.Generic;

namespace ExpeditionRegionSupport.Filters
{
    public interface IRegionChallenge
    {
        public List<string> ApplicableRegions { get; set; }
    }
}
