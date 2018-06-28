using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MasterNodoOCB
{
    public class User
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string LLave { get; set; }
        public string LLavepriv { get; set; }
        public string Monedas { get; set; }
        public string LLavedestino { get; set; }
        public DateTime Birthday { get; set; }
    }
}
