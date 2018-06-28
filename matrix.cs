using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterNodoOCB
{
    public class matrix
    {
        public int id { get; set; }
        public string coin { get; set; }
        public string matrixg { get; set; }
        public string price { get; set; }
        public string stock { get; set; }
        public string stop { get; set; }
        public string difficulty { get; set; }
        public DateTime datestart { get; set; }
        public DateTime lastblock { get; set; }
        public int block { get; set; }
        public string reward { get; set; }
        public int nodos { get; set; }
    }
}
