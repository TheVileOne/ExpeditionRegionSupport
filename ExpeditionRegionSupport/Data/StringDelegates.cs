using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data
{
    public static class StringDelegates
    {
        public delegate string Format(string data);
        public delegate bool Validate(string data);
    }
}
