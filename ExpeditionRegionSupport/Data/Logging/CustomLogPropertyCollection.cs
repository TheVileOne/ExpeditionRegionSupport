using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class CustomLogPropertyCollection : IEnumerable<CustomLogProperty>
    {
        protected List<CustomLogProperty> InnerList = new List<CustomLogProperty>();

        public void AddProperty(CustomLogProperty property)
        {
            InnerList.Add(property);
        }

        public bool RemoveProperty(CustomLogProperty property)
        {
            return InnerList.Remove(property);
        }

        public IEnumerator<CustomLogProperty> GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }
    }
}
