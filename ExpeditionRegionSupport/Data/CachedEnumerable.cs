using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data
{
    public class CachedEnumerable<T> : IEnumerable<T>
    {
        protected IEnumerable<T> InnerEnumerable;

        /// <summary>
        /// Enumerated values are stored here when values are returned from InnerEnumerable
        /// </summary>
        public readonly List<T> EnumeratedValues = new List<T>();

        public CachedEnumerable(IEnumerable<T> innerEnumerable)
        {
            InnerEnumerable = innerEnumerable;
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            if (InnerEnumerable != null)
            {
                foreach (var item in InnerEnumerable)
                {
                    EnumeratedValues.Add(item);
                    yield return item;
                }
                InnerEnumerable = null;
            }
            else
            {
                foreach (var item in EnumeratedValues)
                    yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
