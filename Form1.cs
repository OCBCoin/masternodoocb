using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Net;
using System.Security.Policy;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;
using System.Threading;

namespace MasterNodoOCB
{
    public partial class Form1 : Form
    {
        public static int mtx = 0;
        public static int ctr = 0;
        public static int blk = 0;
        public static int blkt = 0;
        public static int blocker = 0;
        public static int start = 0;
        public static String machineName = Environment.MachineName;
        public static HttpServer httpServer;
        public static Thread thread;

        public Form1()
        {
            InitializeComponent();
            this.Text = "MasterNodoOCB Vers. " + this.ProductVersion;
            toolStripStatusLabel1.Text = "  Iniciando....";
            qrCodeImgControl2.Text = "Cargando Datos....";            
        }

        public static Uri Url;
        public static string InvokeMethod(string method, string paramString1, string paramString2, string paramString3, string paramString4, string paramString5, string paramString6, string paramString7, string paramString8, string paramString9, string paramString10)
        {
            string reply = "";
            Url = new Uri(Program.Urlc);
            try
            {
                WebClient c = new WebClient();
                reply = c.DownloadString(Url + "?id=0&method=" + method + "&params1=" + paramString1 + "&params2=" + paramString2 + "&params3=" + paramString3 + "&params4=" + paramString4 + "&params5=" + paramString5 + "&params6=" + paramString6 + "&params7=" + paramString7 + "&params8=" + paramString8 + "&params9=" + paramString9 + "&params10=" + paramString10);                
            }
            catch (Exception)
            {
                reply = "Error problemas de conexión.";
            }
            return reply;
        }

        public void MostrarCartera()
        {
            try
            {
                DbContext.Up();
                foreach (var user in UserService.GetAll())
                {
                    ctr++;
                    Program.MyWallet = user.LLave;
                    Program.MyEtiqueta = user.Label;
                    Program.MyMonedas = user.Monedas;
                    Program.MyClavedestino = user.LLavedestino;
                }
                CrearCartera();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("ERROR: " + ex.Message, "Notificación");
            }
        }

        public void CrearCartera()
        {
            if (Program.MyWallet == "")
            {
                string laip = GetLocalIPv4(NetworkInterfaceType.Ethernet);
                Program.MyWallet = "OC" + CreateMD5(machineName + laip);
                Program.MyEtiqueta = "mn1";
                String hashc = GetSHA1HashData(getHash(Program.MyWallet));
                Program.MyHash = hashc;
                Program.MyMonedas = "0.00000000";
                AgregarCartera();
            }
            else
            {
                AgregarCartera();
            }
        }

        public void AgregarCartera()
        {
            if (Program.MyWallet == "")
            {
                DbContext.Up();
                var ctx = DbContext.GetInstance();
                for (var i = 1; i <= 1; i++)
                {
                    var query = "INSERT INTO Users (Label, LLave, LLavepriv, Monedas, LLavedestino, birthday) VALUES (?, ?, ?, ?, ?, ?)";

                    using (var command = new SQLiteCommand(query, ctx))
                    {
                        command.Parameters.Add(new SQLiteParameter("Label", Program.MyEtiqueta));
                        command.Parameters.Add(new SQLiteParameter("LLave", Program.MyWallet));
                        command.Parameters.Add(new SQLiteParameter("LLavepriv", Program.MyHash));
                        command.Parameters.Add(new SQLiteParameter("Monedas", Program.MyMonedas));
                        command.Parameters.Add(new SQLiteParameter("LLavedestino", Program.MyClavedestino));
                        command.Parameters.Add(new SQLiteParameter("birthday", DateTime.Today));

                        command.ExecuteNonQuery();
                    }
                }
                EnviarCartera();
            }
            else
            {
                DbContext.Up();
                var ctx = DbContext.GetInstance();
                var query = "UPDATE Users SET Monedas=?, LLavedestino=? WHERE LLave=?";

                using (var command = new SQLiteCommand(query, ctx))
                {
                    command.Parameters.Add(new SQLiteParameter("Label", Program.MyEtiqueta));
                    command.Parameters.Add(new SQLiteParameter("LLave", Program.MyWallet));
                    command.Parameters.Add(new SQLiteParameter("LLavepriv", Program.MyHash));
                    command.Parameters.Add(new SQLiteParameter("Monedas", Program.MyMonedas));
                    command.Parameters.Add(new SQLiteParameter("LLavedestino", Program.MyClavedestino));
                    command.Parameters.Add(new SQLiteParameter("birthday", DateTime.Today));

                    command.ExecuteNonQuery();
                }
                EnviarCartera();
            }
        }

        public void EnviarCartera()
        {
            try
            {
                string mydata = InvokeMethod("getblocktrack", Program.MyWallet, Program.MyEtiqueta, Program.MyHash, Program.MyHash, "masternodo@ocbcoin.org", "", "", "", "", "");
                if (mydata.Length > 0)
                {
                    string searchWithinThis = mydata;
                    string searchForThis = "track";
                    int firstCharacter = searchWithinThis.IndexOf(searchForThis);
                    if (firstCharacter >= 0)
                    {
                        JObject results = JObject.Parse(mydata);
                        string track = results["track"].ToString();
                        if (track.Length > 0)
                        {
                            EnviarIPN();
                        }
                        else
                        {
                            EnviarCartera();
                        }
                    }
                    else
                    {
                        EnviarCartera();
                    }
                }
                else
                {
                    MessageBox.Show(mydata, "Notificación EC");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message, "Notificación");
            }
        }

        public void EnviarIPN()
        {
            string ipn = La_IP();
            InvokeMethod("newred", ipn, "1", this.ProductVersion, Program.MyWallet, "", "", "", "", "", "");            
            Mostrarmatrix();
        }

        public void Mostrarmatrix()
        {
            try
            {
                DbContext.Up();

                foreach (var mmatrix in matrixServices.GetAll())
                {
                    mtx++;
                    Program.Matrix = mmatrix.matrixg;
                }
                AgregaMatrix();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("ERROR: " + ex.Message, "Notificación");
            }

        }

        public void AgregaMatrix()
        {
            string mydata = InvokeMethod("getmatrix", Program.MyWallet, "", "", "", "", "", "", "", "", "");
            if (mydata.Length > 0)
            {
                string searchWithinThis = mydata;
                string searchForThis = "id";
                int firstCharacter = searchWithinThis.IndexOf(searchForThis);
                if (firstCharacter >= 0)
                {
                    JObject results = JObject.Parse(mydata);
                    string id = results["id"].ToString();
                    string coin = results["coin"].ToString();
                    string matrixg = results["matrix"].ToString();
                    string price = results["price"].ToString();
                    string stock = results["stock"].ToString();
                    string stop = results["stop"].ToString();
                    string difficulty = results["difficulty"].ToString();
                    string datestart = results["datestart"].ToString();
                    string lastblock = results["lastblock"].ToString();
                    string block = results["block"].ToString();
                    string reward = results["reward"].ToString();
                    string nodos = results["nodos"].ToString();

                    if (Program.Matrix == "")
                    {
                        DbContext.Up();
                        var ctx = DbContext.GetInstance();
                        for (var i = 1; i <= 1; i++)
                        {
                            var query = "INSERT INTO matrix (id, coin, matrix, price, stock, stop, difficulty, datestart, lastblock, block, reward, nodos) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                            using (var command = new SQLiteCommand(query, ctx))
                            {
                                command.Parameters.Add(new SQLiteParameter("id", id));
                                command.Parameters.Add(new SQLiteParameter("coin", coin));
                                command.Parameters.Add(new SQLiteParameter("matrix", matrixg));
                                command.Parameters.Add(new SQLiteParameter("price", price));
                                command.Parameters.Add(new SQLiteParameter("stock", stock));
                                command.Parameters.Add(new SQLiteParameter("stop", stop));
                                command.Parameters.Add(new SQLiteParameter("difficulty", difficulty));
                                command.Parameters.Add(new SQLiteParameter("datestart", Convert.ToDateTime(datestart)));
                                command.Parameters.Add(new SQLiteParameter("lastblock", Convert.ToDateTime(lastblock)));
                                command.Parameters.Add(new SQLiteParameter("block", block));
                                command.Parameters.Add(new SQLiteParameter("reward", reward));
                                command.Parameters.Add(new SQLiteParameter("nodos", nodos));

                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        DbContext.Up();
                        var ctx = DbContext.GetInstance();
                        for (var i = 1; i <= 1; i++)
                        {
                            var query = "UPDATE matrix SET coin=?, matrix=?, price=?, stock=?, stop=?, difficulty=?, datestart=?, lastblock=?, block=?, reward=?, nodos=? WHERE id=?";

                            using (var command = new SQLiteCommand(query, ctx))
                            {
                                command.Parameters.Add(new SQLiteParameter("id", id));
                                command.Parameters.Add(new SQLiteParameter("coin", coin));
                                command.Parameters.Add(new SQLiteParameter("matrix", matrixg));
                                command.Parameters.Add(new SQLiteParameter("price", price));
                                command.Parameters.Add(new SQLiteParameter("stock", stock));
                                command.Parameters.Add(new SQLiteParameter("stop", stop));
                                command.Parameters.Add(new SQLiteParameter("difficulty", difficulty));
                                command.Parameters.Add(new SQLiteParameter("datestart", Convert.ToDateTime(datestart)));
                                command.Parameters.Add(new SQLiteParameter("lastblock", Convert.ToDateTime(lastblock)));
                                command.Parameters.Add(new SQLiteParameter("block", block));
                                command.Parameters.Add(new SQLiteParameter("reward", reward));
                                command.Parameters.Add(new SQLiteParameter("nodos", nodos));

                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    Mostrarblock();
                }
            }
        }

        public void Mostrarblock()
        {
            try
            {
                DbContext.Up();
                foreach (var mblock in blockServices.GetAll())
                {
                    blk++;
                    Program.lastblock = Convert.ToInt32(mblock.height);
                }
                AgregaBlock();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("ERROR: " + ex.Message, "Notificación");
            }

        }

        public void AgregaBlock()
        {
            string mydata = InvokeMethod("getblockinfo", Program.MyWallet, Program.lastblock.ToString(), "", "", "", "", "", "", "", "");
            if (mydata.Length > 0)
            {
                string searchWithinThis = mydata;
                string searchForThis = "height";
                int firstCharacter = searchWithinThis.IndexOf(searchForThis);
                if (firstCharacter >= 0)
                {
                    JObject results = JObject.Parse(mydata);
                    foreach (var ocb in results["data"])
                    {
                        string height = ocb["height"].ToString();
                        string dateblock = ocb["dateblock"].ToString();
                        string hash = ocb["hash"].ToString();
                        string sizeblock = ocb["sizeblock"].ToString();
                        string shared = ocb["shared"].ToString();
                        string resolvedby = ocb["resolvedby"].ToString();
                        string difficulty = ocb["difficulty"].ToString();
                        string rewardblock = ocb["rewardblock"].ToString();
                        string status = ocb["status"].ToString();
                        string raizmerkle = ocb["raizmerkle"].ToString();

                        if (Convert.ToInt32(height) > Program.lastblock)
                        {
                            DbContext.Up();
                            var ctx = DbContext.GetInstance();
                            for (var i = 1; i <= 1; i++)
                            {
                                var query = "INSERT INTO block (height,dateblock,hash,sizeblock,shared,resolvedby,difficulty,rewardblock,status,raizmerkle) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                                using (var command = new SQLiteCommand(query, ctx))
                                {
                                    command.Parameters.Add(new SQLiteParameter("height", Convert.ToInt32(height)));
                                    command.Parameters.Add(new SQLiteParameter("dateblock", Convert.ToDateTime(dateblock)));
                                    command.Parameters.Add(new SQLiteParameter("hash", hash));
                                    command.Parameters.Add(new SQLiteParameter("sizeblock", sizeblock));
                                    command.Parameters.Add(new SQLiteParameter("shared", shared));
                                    command.Parameters.Add(new SQLiteParameter("resolvedby", resolvedby));
                                    command.Parameters.Add(new SQLiteParameter("difficulty", difficulty));
                                    command.Parameters.Add(new SQLiteParameter("rewardblock", rewardblock));
                                    command.Parameters.Add(new SQLiteParameter("status", Convert.ToInt32(status)));
                                    command.Parameters.Add(new SQLiteParameter("raizmerkle", raizmerkle));

                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }                    
                }
                Mostrarblocktransaction();
            }
        }

        public void Mostrarblocktransaction()
        {
            try
            {
                double mone = 0;
                DbContext.Up();
                foreach (var mblockt in blocktransactionServices.GetAll())
                {
                    blkt++;
                    Program.lasttrack = Convert.ToInt32(mblockt.track);
                    if (mblockt.addressinput == Program.MyWallet)
                    {
                        mone = mone + Convert.ToDouble(mblockt.amountinput);
                        if (mblockt.amountoutput == "1000.00000000")
                        {
                            Program.MyClavedestino = mblockt.addressoutput;
                        }
                    }
                    if (mblockt.addressoutput == Program.MyWallet)
                    {
                        mone = mone - Convert.ToDouble(mblockt.amountoutput);
                    }
                }
                Program.MyMonedas = String.Format("{0:#######0.00000000}", Convert.ToDecimal(mone));
                AgregaBlocktransaction();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("ERROR: " + ex.Message, "Notificación");
            }

        }

        public void AgregaBlocktransaction()
        {
            string mydata = InvokeMethod("getblocktransaction", Program.MyWallet, Program.lasttrack.ToString(), "", "", "", "", "", "", "", "");
            if (mydata.Length > 0)
            {
                string searchWithinThis = mydata;
                string searchForThis = "track";
                int firstCharacter = searchWithinThis.IndexOf(searchForThis);
                if (firstCharacter >= 0)
                {
                    JObject results = JObject.Parse(mydata);
                    foreach (var ocb in results["data"])
                    {
                        string track = ocb["track"].ToString();
                        string height = ocb["height"].ToString();
                        string tracking = ocb["tracking"].ToString();
                        string addressinput = ocb["addressinput"].ToString();
                        string addressoutput = ocb["addressoutput"].ToString();
                        string type = ocb["type"].ToString();
                        string typeo = ocb["typeo"].ToString();
                        string status = ocb["status"].ToString();
                        string amountoutput = ocb["amountoutput"].ToString();
                        string amountinput = ocb["amountinput"].ToString();
                        string commission = ocb["commission"].ToString();
                        string datetimetrack = ocb["datetimetrack"].ToString();
                        string confirmation = ocb["confirmation"].ToString();

                        if (Convert.ToInt32(track) > Program.lasttrack)
                        {
                            DbContext.Up();
                            var ctx = DbContext.GetInstance();
                            for (var i = 1; i <= 1; i++)
                            {
                                var query = "INSERT INTO blocktransaction (track,height,tracking,addressinput,addressoutput,type,typeo,status,amountoutput,amountinput,commission,datetimetrack,confirmation) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                                using (var command = new SQLiteCommand(query, ctx))
                                {
                                    command.Parameters.Add(new SQLiteParameter("track", Convert.ToInt32(track)));
                                    command.Parameters.Add(new SQLiteParameter("height", Convert.ToInt32(height)));
                                    command.Parameters.Add(new SQLiteParameter("tracking", tracking));
                                    command.Parameters.Add(new SQLiteParameter("addressinput", addressinput));
                                    command.Parameters.Add(new SQLiteParameter("addressoutput", addressoutput));
                                    command.Parameters.Add(new SQLiteParameter("type", type));
                                    command.Parameters.Add(new SQLiteParameter("typeo", typeo));
                                    command.Parameters.Add(new SQLiteParameter("status", status));
                                    command.Parameters.Add(new SQLiteParameter("amountoutput", amountoutput));
                                    command.Parameters.Add(new SQLiteParameter("amountinput", amountinput));
                                    command.Parameters.Add(new SQLiteParameter("commission", commission));
                                    command.Parameters.Add(new SQLiteParameter("datetimetrack", Convert.ToDateTime(datetimetrack)));
                                    command.Parameters.Add(new SQLiteParameter("confirmation", Convert.ToInt32(confirmation)));

                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
        }

        public static string getHash(string cartera)
        {
            RIPEMD160 r160 = RIPEMD160Managed.Create();
            byte[] myByte = System.Text.Encoding.ASCII.GetBytes(cartera);
            byte[] encrypted = r160.ComputeHash(myByte);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < encrypted.Length; i++)
            {
                sb.Append(encrypted[i].ToString("x2"));
            }
            return sb.ToString().ToLower();
        }

        public static string GetSHA1HashData(string data)
        {
            SHA256 sha1 = SHA256.Create();
            string hashg;
            byte[] temp;
            byte[] hashData = sha1.ComputeHash(Encoding.Default.GetBytes(data));
            StringBuilder returnValue = new StringBuilder();
            for (int i = 0; i < hashData.Length; i++)
            {
                returnValue.Append(hashData[i].ToString());
            }
            SHA256 sha = new SHA256CryptoServiceProvider();
            temp = sha.ComputeHash(Encoding.UTF8.GetBytes(returnValue.ToString()));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < temp.Length; i++)
            {
                sb.Append(temp[i].ToString("x2"));
            }
            hashg = sb.ToString();
            return hashg;
        }

        public static string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }

        public static string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static string La_IP()
        {
            string a2 = "";
            try
            {
                string url = "http://ocbcoin.org/ip_public.php";
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                string response = sr.ReadToEnd().Trim();
                string[] a = response.Split(':');
                a2 = a[0];
                Program.Urlc = Program.Urlc.Replace("zzz",a[1]);                
            }
            catch (WebException)
            {
                a2 = "";
            }
            return a2;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;

            int arg = (int)e.Argument;

            e.Result = ServiceWeb(bw, arg, e);

            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Application.DoEvents();
                thread.IsBackground = false;
                thread.Abort();
            }
            else if (e.Error != null)
            {
                Application.DoEvents();
                thread.IsBackground = false;
                thread.Abort();
            }
            else
            {
                toolStripStatusLabel1.Text = "  Conectado";
            }
        }

        public int ServiceWeb(BackgroundWorker bw, int sleepPeriod, DoWorkEventArgs e)
        {
            int result = 0;

            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                httpServer = new MyHttpServer(5993);
                thread = new Thread(new ThreadStart(httpServer.listen));
                thread.IsBackground = true;
                thread.Start();
            }
            return result;
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;

            int arg = (int)e.Argument;

            e.Result = ServiceData(bw, arg, e);

            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
            }
            else if (e.Error != null)
            {
            }
            else
            {
                qrCodeImgControl2.Text = "ocbcoin: " + Program.MyWallet;
                label1.Text = "Billetera Nodo: " + Program.MyWallet;
                label2.Text = "Billetera Destino: " + Program.MyClavedestino;
                label3.Text = "Monedas: " + Program.MyMonedas + " OCB";
                label4.Text = "IP Nodo: " + La_IP() + ":5993";
                label5.Text = "Bloques: " + Program.lastblock.ToString();
                start++;
                if (start == 1)
                {
                    mensaje.Text = "Esperando.";
                }
                if (start == 2)
                {
                    mensaje.Text = "Esperando..";
                }
                if (start == 3)
                {
                    mensaje.Text = "Esperando...";
                }
                if (start == 4)
                {
                    mensaje.Text = "Esperando....";
                }
                if (start == 5)
                {
                    mensaje.Text = "Esperando.....";
                    start = 0;
                }
                Program.procep = 0;
                if (timer2.Enabled == false)
                {
                    timer2.Enabled = true;
                }
                this.backgroundWorker2.RunWorkerAsync(2000);
            }
        }

        public int ServiceData(BackgroundWorker bw, int sleepPeriod, DoWorkEventArgs e)
        {
            int result = 0;

            if (bw.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                try
                {
                    MostrarCartera();
                }
                catch (Exception ec)
                {
                    MessageBox.Show("ERROR: " + ec.Message, "Notificación");
                }
            }
            return result;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            La_IP();
            timer1.Enabled = false;
            this.backgroundWorker1.RunWorkerAsync(2000);
            this.backgroundWorker2.RunWorkerAsync(2000);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (blocker == 0)
            {
                string caption = "Confirmación";
                string mensage = "¿ Seguro que desea salir de la Aplicación ?";
                MessageBoxButtons botones = MessageBoxButtons.YesNo;
                MessageBoxIcon icono = MessageBoxIcon.Question;

                DialogResult resultado = MessageBox.Show(mensage, caption, botones, icono);
                if (resultado == DialogResult.Yes)
                {
                    this.backgroundWorker1.CancelAsync();
                    blocker = 1;
                    Application.Exit();
                }
                else
                {
                    switch (e.CloseReason)
                    {
                        case CloseReason.UserClosing:
                            e.Cancel = true;
                            break;
                    }
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Program.cartera = Program.MyWallet;
            Explorador frm1 = new Explorador();
            frm1.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Program.cartera = Program.MyClavedestino;
            Explorador frm2 = new Explorador();
            frm2.Show();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://" + La_IP() + ":5993"); 
        }

        public static int hr = 23;
        public static int mr = 59;
        public static int sr = 60;
        public static string indica = "";
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (Convert.ToDouble(Program.MyMonedas) >= 1000)
            {
                if (sr == 0)
                {
                    sr = 60;
                    if (mr == 0)
                    {
                        mr = 59;
                        if (hr > 0)
                        {
                            hr--;
                        }
                        mensaje.Text = "Procesando Ganancias.....";
                        notifyIcon1.Text = "MasterNodoOCB | " + "Procesando Ganancias.....";
                        Program.ganancia = (1000 * 0.000416666666666667);
                        Program.procep = 1;
                        CreaG();
                    }
                    else
                    {
                        mr--;
                    }
                }
                else
                {
                    sr--;
                }
                if (hr > 0)
                {
                    indica = " horas";
                    contador.Text = "Tiempo Restante: " + String.Format("{0:00}", hr) + ":" + String.Format("{0:00}", mr) + ":" + String.Format("{0:00}", sr) + indica;
                    notifyIcon1.Text = "MasterNodoOCB | " + "Tiempo Restante: " + String.Format("{0:00}", hr) + ":" + String.Format("{0:00}", mr) + ":" + String.Format("{0:00}", sr) + indica;
                    if (mr == 58)
                    {
                        label6.Text = "";
                    }
                }
                else
                {
                    if (mr > 0)
                    {
                        indica = " minutos";
                        contador.Text = "Tiempo Restante: " + String.Format("{0:00}", hr) + ":" + String.Format("{0:00}", mr) + ":" + String.Format("{0:00}", sr) + indica;
                        notifyIcon1.Text = "MasterNodoOCB | " + "Tiempo Restante: " + String.Format("{0:00}", hr) + ":" + String.Format("{0:00}", mr) + ":" + String.Format("{0:00}", sr) + indica;
                    }
                    else
                    {
                        if (sr > 0)
                        {
                            indica = " segundos";
                            contador.Text = "Tiempo Restante: " + String.Format("{0:00}", hr) + ":" + String.Format("{0:00}", mr) + ":" + String.Format("{0:00}", sr) + indica;
                            notifyIcon1.Text = "MasterNodoOCB | " + "Tiempo Restante: " + String.Format("{0:00}", hr) + ":" + String.Format("{0:00}", mr) + ":" + String.Format("{0:00}", sr) + indica;
                        }
                        else
                        {
                            notifyIcon1.Text = "MasterNodoOCB | " + "Enviando Pago.....";
                            mensaje.Text = "Procesando el pago de sus ganancias.....";
                            contador.Text = "Enviando Pago.....";
                            EnviarPago();
                        }
                    }
                }
            }
            else
            {
                contador.Text = "Nodo inactivo....";
            }
        }

        public void EnviarPago()
        {
            string result = "";
            string resu = "";
            double enviar = Convert.ToDouble(Program.MyMonedas) - 1000;
            result = InvokeMethod("sendocb", Program.MyWallet, Program.MyClavedestino, String.Format("{0:#######0.00000000}", Convert.ToDecimal(enviar)), "", "", "", "", "", "", "");
            if (result.Length > 0)
            {
                string searchWithinThis = result;
                string searchForThis = "Result";
                int firstCharacter = searchWithinThis.IndexOf(searchForThis);
                if (firstCharacter >= 0)
                {
                    JObject results = JObject.Parse(result);
                    resu = results["Result"].ToString();
                }
            }
            if (resu == "true")
            {
                Program.ganancia = 0;
                mensaje.Text = "Esperando.";
                label6.Text = "Transacción enviada a la red por el monto de: " + String.Format("{0:#######0.00000000}", Convert.ToDecimal(enviar)) + " OCB y será procesada en breve.";
                hr = 23;
                mr = 59;
                sr = 60;
            }
            else
            {
                EnviarPago();
                mensaje.Text = "Procesando el pago de sus ganancias.....";
                label6.Text = "Transacción no aceptada, intentando nuevamente.";
            }
        }

        public void CreaG()
        {
            string result = "";
            string resu = "";
            result = InvokeMethod("sendocb", Program.Matrix, Program.MyWallet, String.Format("{0:#######0.00000000}", Convert.ToDecimal(Program.ganancia)), "", "", "", "", "", "", "");
            if (result.Length > 0)
            {
                string searchWithinThis = result;
                string searchForThis = "Result";
                int firstCharacter = searchWithinThis.IndexOf(searchForThis);
                if (firstCharacter >= 0)
                {
                    JObject results = JObject.Parse(result);
                    resu = results["Result"].ToString();
                }
            }
            if (resu == "false")
            {
                CreaG();
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon1.Visible = false;
                this.ShowInTaskbar = true;
            }
            catch (Exception)
            {
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            try
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.Visible = false;
                    this.ShowInTaskbar = false;
                    notifyIcon1.Visible = true;
                }
            }
            catch (Exception)
            {
            }
        }

    }
}
