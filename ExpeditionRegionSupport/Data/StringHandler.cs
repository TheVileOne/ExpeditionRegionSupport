using System.Collections.Generic;
using System.Text;

namespace ExpeditionRegionSupport.Data
{
    public class StringHandler
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public void AddString(string data)
        {
            stringBuilder.AppendLine(data);
        }

        public void AddFrom(IEnumerable<string> enumeratedStrings)
        {
            IEnumerator<string> enumerator = enumeratedStrings.GetEnumerator();

            while (enumerator.MoveNext())
                AddString(enumerator.Current);
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}
