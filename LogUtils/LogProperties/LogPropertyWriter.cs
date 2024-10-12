using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils.Properties
{
    internal class LogPropertyWriter
    {
        private LogPropertyFile propertyFile;

        public LogPropertyWriter(LogPropertyFile file)
        {
            propertyFile = file;
        }
    }
}
