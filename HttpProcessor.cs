using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Data;
using System.Diagnostics;
using System.Xml;

namespace MasterNodoOCB
{
    public class HttpProcessor
    {

        public TcpClient socket;
        public HttpServer srv;

        public Stream inputStream;
        public StreamWriter outputStream;

        public String http_method;
        public String http_url;
        public String http_protocol_versionstring;
        public Hashtable httpHeaders = new Hashtable();

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        public HttpProcessor(TcpClient s, HttpServer srv)
        {
            this.socket = s;
            this.srv = srv;
        }


        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }
        public void process()
        {
            try
            {
                inputStream = new BufferedStream(socket.GetStream());

                outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
                try
                {
                    parseRequest();
                    readHeaders();
                    if (http_method.Equals("GET"))
                    {
                        handleGETRequest();
                    }
                    else if (http_method.Equals("POST"))
                    {
                        handlePOSTRequest();
                    }
                }
                catch (Exception e)
                {
                    writeFailure("Content-Type: application/json");
                }
                outputStream.Flush();
                inputStream = null; outputStream = null;
                socket.Close();
            }
            catch (Exception f)
            {
                string sSource = "dotNET Sample App";
                string sLog = "Application";
                string sEvent = "Sample Event";

                Debug.Listeners.Remove("Default");
                TextWriterTraceListener logFile = new TextWriterTraceListener(@"C:\temp\log.log");
                Trace.Listeners.Add(logFile);
                ConsoleTraceListener logConsole = new ConsoleTraceListener();
                Trace.Listeners.Add(logConsole);
                Debug.WriteLine(sEvent = f.Message);
                Debug.Flush();
                Trace.Flush();
            }
        }

        public void parseRequest()
        {
            try
            {
                String request = streamReadLine(inputStream);
                string[] tokens = request.Split(' ');
                if (tokens.Length != 3)
                {
                    throw new Exception("invalid http request line");
                }
                http_method = tokens[0].ToUpper();
                http_url = tokens[1];
                http_protocol_versionstring = tokens[2];

            }
            catch (Exception f)
            {
                string sSource = "dotNET Sample App";
                string sLog = "Application";
                string sEvent = "Sample Event";
                Debug.Listeners.Remove("Default");
                TextWriterTraceListener logFile = new TextWriterTraceListener(@"C:\temp\log.log");
                Trace.Listeners.Add(logFile);
                ConsoleTraceListener logConsole = new ConsoleTraceListener();
                Trace.Listeners.Add(logConsole);
                Debug.WriteLine(sEvent = f.Message);
                Debug.Flush();
                Trace.Flush();
            }
        }

        public void readHeaders()
        {
            try
            {
                String line;
                while ((line = streamReadLine(inputStream)) != null)
                {
                    if (line.Equals(""))
                    {
                        return;
                    }

                    int separator = line.IndexOf(':');
                    if (separator == -1)
                    {
                        throw new Exception("invalid http header line: " + line);
                    }
                    String name = line.Substring(0, separator);
                    int pos = separator + 1;
                    while ((pos < line.Length) && (line[pos] == ' '))
                    {
                        pos++;
                    }

                    string value = line.Substring(pos, line.Length - pos);
                    httpHeaders[name] = value;
                }
            }
            catch (Exception f)
            {
                string sSource = "dotNET Sample App";
                string sLog = "Application";
                string sEvent = "Sample Event";
                Debug.Listeners.Remove("Default");
                TextWriterTraceListener logFile = new TextWriterTraceListener(@"C:\temp\log.log");
                Trace.Listeners.Add(logFile);
                ConsoleTraceListener logConsole = new ConsoleTraceListener();
                Trace.Listeners.Add(logConsole);
                Debug.WriteLine(sEvent = f.Message);
                Debug.Flush();
                Trace.Flush();
            }
        }

        private const int BUF_SIZE = 4096;

        public void handleGETRequest()
        {
            srv.handleGETRequest(this);
        }

        public void handlePOSTRequest()
        {
            try
            {
                int content_len = 0;
                MemoryStream ms = new MemoryStream();
                if (this.httpHeaders.ContainsKey("Content-Length"))
                {
                    content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
                    if (content_len > MAX_POST_SIZE)
                    {
                        throw new Exception(
                            String.Format("POST Content-Length({0}) too big for this simple server",
                              content_len));
                    }
                    byte[] buf = new byte[BUF_SIZE];
                    int to_read = content_len;
                    while (to_read > 0)
                    {
                        int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                        if (numread == 0)
                        {
                            if (to_read == 0)
                            {
                                break;
                            }
                            else
                            {
                                throw new Exception("client disconnected during post");
                            }
                        }
                        to_read -= numread;
                        ms.Write(buf, 0, numread);
                    }
                    ms.Seek(0, SeekOrigin.Begin);
                }
                srv.handlePOSTRequest(this, new StreamReader(ms));
            }
            catch (Exception f)
            {
                string sSource = "dotNET Sample App";
                string sLog = "Application";
                string sEvent = "Sample Event";
                Debug.Listeners.Remove("Default");
                TextWriterTraceListener logFile = new TextWriterTraceListener(@"C:\temp\log.log");
                Trace.Listeners.Add(logFile);
                ConsoleTraceListener logConsole = new ConsoleTraceListener();
                Trace.Listeners.Add(logConsole);
                Debug.WriteLine(sEvent = f.Message);
                Debug.Flush();
                Trace.Flush();
            }
        }

        public void writeSuccess(string content_type)
        {
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Content-Type: " + content_type);
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }

        public void writeFailure(string content_type)
        {
            outputStream.WriteLine("HTTP/1.0 404 File not found");
            outputStream.WriteLine("Content-Type: " + content_type);
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }
    }

    public abstract class HttpServer
    {

        protected int port;
        TcpListener listener;
        bool is_active = true;

        public HttpServer(int port)
        {
            this.port = port;
        }

        public void listen()
        {
            try
            {
                listener = new TcpListener(port);
                listener.Start();
                while (is_active)
                {
                    TcpClient s = listener.AcceptTcpClient();
                    HttpProcessor processor = new HttpProcessor(s, this);
                    Thread thread = new Thread(new ThreadStart(processor.process));
                    thread.Start();
                    Thread.Sleep(1);
                }
            }
            catch (Exception f)
            {
                string sSource = "dotNET Sample App";
                string sLog = "Application";
                string sEvent = "Sample Event";
                Debug.Listeners.Remove("Default");
                TextWriterTraceListener logFile = new TextWriterTraceListener(@"C:\temp\log.log");
                Trace.Listeners.Add(logFile);
                ConsoleTraceListener logConsole = new ConsoleTraceListener();
                Trace.Listeners.Add(logConsole);
                Debug.WriteLine(sEvent = f.Message);
                Debug.Flush();
                Trace.Flush();
            }
        }

        public abstract void handleGETRequest(HttpProcessor p);
        public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);
    }

    public class MyHttpServer : HttpServer
    {
        public static Uri Url;
        public static string User;
        public static string Password;
        public static XmlDocument documento = new XmlDocument();

        public MyHttpServer(int port)
            : base(port)
        {

        }

        public static string La_IP()
        {
            string a4 = "";
            try
            {
                string url = "http://ocbcoin.org/ip_public.php";
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                string response = sr.ReadToEnd().Trim();
                string[] a = response.Split(':');
                string a2 = a[1].Substring(1);
                string[] a3 = a2.Split('<');
                a4 = a3[0];
            }
            catch (WebException)
            {
                a4 = "";
            }
            return a4;
        }

        public override void handleGETRequest(HttpProcessor p)
        {
            try
            {
                char comi = '"';
                string dataj = "";
                string parametro1 = "";
                string parametro2 = "";
                string parametro3 = "";
                string parametro4 = "";
                string parametro5 = "";
                string parametro6 = "";
                string parametro7 = "";
                string parametro8 = "";
                string parametro9 = "";
                string parametro10 = "";
                string data = p.http_url;

                if (data.Length > 0)
                {
                    string ValHashLW = data.Replace("/", "");
                    String[] elements = ValHashLW.Split(new Char[] { ',' });
                    int cv = 0;
                    foreach (var element in elements)
                    {
                        if (cv == 0)
                        {
                            dataj = element;
                        }
                        if (cv == 1)
                        {
                            parametro1 = element;
                        }
                        if (cv == 2)
                        {
                            parametro2 = element;
                        }
                        if (cv == 3)
                        {
                            parametro3 = element;
                        }
                        if (cv == 4)
                        {
                            parametro4 = element;
                        }
                        if (cv == 5)
                        {
                            parametro5 = element;
                        }
                        if (cv == 6)
                        {
                            parametro6 = element;
                        }
                        if (cv == 7)
                        {
                            parametro7 = element;
                        }
                        if (cv == 8)
                        {
                            parametro8 = element;
                        }
                        if (cv == 9)
                        {
                            parametro9 = element;
                        }
                        if (cv == 10)
                        {
                            parametro10 = element;
                        }
                        cv++;
                    }
                }

                p.writeSuccess("Content-Type: application/json");
                p.outputStream.WriteLine("{" + comi + "data" + comi + ":" + "[" + "{" + comi + "nodo" + comi + ":" + comi + Program.MyWallet + comi + "," + comi + "destino" + comi + ":" + comi + Program.MyClavedestino + comi + "," + comi + "monedas" + comi + ":" + comi + Program.MyMonedas + comi + "," + comi + "bloques" + comi + ":" + comi + Convert.ToString(Program.lastblock) + comi + "}" + "]}");
            }
            catch (Exception e)
            {
                string sSource = "dotNET Sample App";
                string sLog = "Application";
                string sEvent = "Sample Event";
                Debug.Listeners.Remove("Default");
                TextWriterTraceListener logFile = new TextWriterTraceListener(@"C:\temp\log.log");
                Trace.Listeners.Add(logFile);
                ConsoleTraceListener logConsole = new ConsoleTraceListener();
                Trace.Listeners.Add(logConsole);
                Debug.WriteLine(sEvent = e.Message);
                Debug.Flush();
                Trace.Flush();
            }
        }

        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            try
            {
                char comi = '"';
                string dataj = "";
                string parametro1 = "";
                string parametro2 = "";
                string parametro3 = "";
                string parametro4 = "";
                string parametro5 = "";
                string parametro6 = "";
                string parametro7 = "";
                string parametro8 = "";
                string parametro9 = "";
                string parametro10 = "";
                string data = inputData.ReadToEnd();

                if (data.Length > 0)
                {
                    string searchWithinThis = data;
                    string searchForThis = "method";
                    int firstCharacter = searchWithinThis.IndexOf(searchForThis);
                    if (firstCharacter >= 0)
                    {
                        JObject ocb = JObject.Parse(data);
                        dataj = ocb["method"].ToString();
                        parametro1 = ocb["params1"].ToString();
                        parametro2 = ocb["params2"].ToString();
                        parametro3 = ocb["params3"].ToString();
                        parametro4 = ocb["params4"].ToString();
                        parametro5 = ocb["params5"].ToString();
                        parametro6 = ocb["params6"].ToString();
                        parametro7 = ocb["params7"].ToString();
                        parametro8 = ocb["params8"].ToString();
                        parametro9 = ocb["params9"].ToString();
                        parametro10 = ocb["params10"].ToString();
                    }
                }

                p.writeSuccess("Content-Type: application/json");
                p.outputStream.WriteLine("{" + comi + "data" + comi + ":" + "[" + "{" + comi + "nodo" + comi + ":" + comi + Program.MyWallet + comi + "," + comi + "destino" + comi + ":" + comi + Program.MyClavedestino + comi + "," + comi + "monedas" + comi + ":" + comi + Program.MyMonedas + comi + "," + comi + "bloques" + comi + ":" + comi + Convert.ToString(Program.lastblock) + comi + "}" + "]}");
            }
            catch (Exception e)
            {
                string sSource = "dotNET Sample App";
                string sLog = "Application";
                string sEvent = "Sample Event";
                Debug.Listeners.Remove("Default");
                TextWriterTraceListener logFile = new TextWriterTraceListener(@"C:\temp\log.log");
                Trace.Listeners.Add(logFile);
                ConsoleTraceListener logConsole = new ConsoleTraceListener();
                Trace.Listeners.Add(logConsole);
                Debug.WriteLine(sEvent = e.Message);
                Debug.Flush();
                Trace.Flush();
            }
        }


    }

    public class TestMain
    {
        public int Mainn(String[] args)
        {
            HttpServer httpServer;
            if (args.GetLength(0) > 0)
            {
                httpServer = new MyHttpServer(Convert.ToInt16(args[0]));
            }
            else
            {
                httpServer = new MyHttpServer(5993);
            }
            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
            return 0;
        }
    }
}