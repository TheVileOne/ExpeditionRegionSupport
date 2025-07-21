using ExpeditionRegionSupport.Data;
using System.Collections.Generic;

namespace ExpeditionRegionSupport.Regions.Data
{
    public interface IHaveEnumeratedSections
    {
        /// <summary>
        /// Associates a section with its index position in the enumerated list
        /// </summary>
        public Dictionary<string, Range> SectionMap { get; }

        public List<string> ReadNextSection();
    }
}
