using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Logging.Utils
{
    public class StringHandler
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public void AddString(string data)
        {
            stringBuilder.AppendLine(data);
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}
