using System;
using System.Collections;
using System.Collections.Generic;

namespace LogUtils
{
    public class StringProvider : IEnumerable<string>
    {
        protected IEnumerable<string> Provider;

        public StringProvider(IEnumerable<string> provider)
        {
            Provider = provider;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return Provider.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
