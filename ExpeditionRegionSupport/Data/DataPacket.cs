using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data
{
    public class DataPacket : DataStorage
    {
        public string DataID { get; set; }
        public int HeaderID { get; set; }
        public string Data { get; set; }
        public bool Handled { get; set; }
    }

    public interface DataStorage
    {
        public string DataID { get; set; }
        public int HeaderID { get; set; }

        public string Data { get; set; }
        public bool Handled { get; set; }
    }
}
