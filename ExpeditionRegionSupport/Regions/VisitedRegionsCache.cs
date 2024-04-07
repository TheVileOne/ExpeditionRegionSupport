using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Regions
{
    public class VisitedRegionsCache //Cannot be a struct due to challenge filters requiring an unchanging list to reference
    {
        public SlugcatStats.Name LastAccessed;

        private readonly List<string> _regionsVisited = new List<string>();

        public List<string> RegionsVisited
        {
            get => _regionsVisited;
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                //This reference should not be overwritten. Transfer values to existing reference instead.
                _regionsVisited.Clear();
                _regionsVisited.AddRange(value);
            }
        }
    }
}
