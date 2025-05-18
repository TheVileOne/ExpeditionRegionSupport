using System.Collections;
using System.Collections.Generic;

namespace LogUtils.Diagnostics.Tools
{
    public class TestStringProvider : StringProvider
    {
        /// <summary>
        /// The base log string to enumerate on
        /// </summary>
        public string Template = "TEST";

        public string Format = "{0} {1}";

        public TestStringProvider() : base(new StringFactory())
        {
            //We cannot pass TestStringProvider into our factory instance and pass it to the base class at the same time, so we do it here instead
            var factory = (StringFactory)Provider;

            factory.Provider = this;
        }

        protected class StringFactory : IEnumerable<string>
        {
            internal TestStringProvider Provider;

            public IEnumerator<string> GetEnumerator()
            {
                if (Provider == null)
                    yield break;

                int counter = 0;
                while (true)
                {
                    counter++; //Count starts at 1
                    yield return string.Format(Provider.Format, Provider.Template, counter);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
