using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterNodoOCB
{
    class blocktransaction
    {
        public int track { get; set; }
        public int height { get; set; }
        public string tracking { get; set; }
        public string addressinput { get; set; }
        public string addressoutput { get; set; }
        public string type { get; set; }
        public string typeo { get; set; }
        public string status { get; set; }
        public string amountoutput { get; set; }
        public string amountinput { get; set; }
        public string commission { get; set; }
        public DateTime datetimetrack { get; set; }
        public int confirmation { get; set; }
    }
}
