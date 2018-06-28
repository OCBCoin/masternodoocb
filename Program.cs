using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MasterNodoOCB
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        public static string MyWallet = "";
        public static string MyClave = "";
        public static string MyEtiqueta = "";
        public static string MyHash = "";
        public static string Urlc = "http://zzz/work/";
        public static string MyMonedas = "";
        public static string MyClavedestino = "";
        public static string Matrix = "";
        public static string cartera = "";
        public static string resultw = "";
        public static int lastblock = 0;
        public static int lasttrack = 0;
        public static int procep = 0;
        public static double ganancia = 0;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
