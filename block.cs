using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterNodoOCB
{
    public class block
    {
        public int height { get; set; }
        public DateTime dateblock { get; set; }
        public string hash { get; set; }
        public string sizeblock { get; set; }
        public string shared { get; set; }
        public string resolvedby { get; set; }
        public string difficulty { get; set; }
        public string rewardblock { get; set; }
        public int status { get; set; }
        public string raizmerkle { get; set; }
    }
}
