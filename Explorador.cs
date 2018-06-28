using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MasterNodoOCB
{
    public partial class Explorador : Form
    {
        public Explorador()
        {
            InitializeComponent();
            webBrowser1.Navigate("https://www.ocbcoin.org/cadenadebloques/carteras/?c=" + Program.cartera);
        }

    }
}
