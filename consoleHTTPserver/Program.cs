using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;

namespace consoleHTTPserver
{

    class HttpServer
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static HttpListener listener;
        public static string port = "8000";
        public static string url = $"http://+:" + port + "/";
        public static int pageViews = 0;
        public static int requestCount = 0;
       
        public static string pageData()
        {
            string page =
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">" +
            "    <title>DALINC</title>" +
            "  </head>" +
            "  <body style =\"text-align:center;\">" +
            "   <a href=\"/back\"> " +
            "  <input type = \"button\" style =\"width:80px; height:30px; color:red;\" value=\"<<<\" />" +
            "</a> " +
            "   <a href=\"/pause\"> " +
            "  <input type = \"button\" style =\"width:80px; height:30px; color:red;\" value=\"PAUSE\" />" +
            "</a> " +
            "   <a href=\"/forward\"> " +
            "  <input type = \"button\" style =\"width:80px; height:30px; color:red;\" value=\">>>\" />" +
            "</a><br> " +
            "<a href=\"/info\"> " +
            "  <input type = \"button\" style =\"width:40px; height:30px; color:blue; margin-top:5px;  margin-bottom:5px;\" value=\"Info\" />" +
            "</a> " +
            "<br>" +
            "   <a href=\"/volDown\"> " +
            "  <input type = \"button\" style =\"width:80px; height:30px; color:red;\" value=\"VOL-\" />" +
            "</a>" +
            "   <a href=\"/mute\"> " +
            "  <input type = \"button\" style =\"width:80px; height:30px; color:red;\" value=\"MUTE\" />" +
            "</a>" +
            "   <a href=\"/volUp\"> " +
            "  <input type = \"button\" style =\"width:80px; height:30px; color:red;\" value=\"VOL+\" />" +
            "</a><br><br>";

            string pthStr = AppDomain.CurrentDomain.BaseDirectory + "/tv.csv";
            List<string[]> rows = File.ReadAllLines(pthStr).Select(x => x.Split(',')).ToList();

            try
            {
                foreach (var item in rows)
                {
                    page += " <a href =\"/" + item[1] + "\"> " +
                "  <input type = \"button\" style =\"width:80px; height:30px; color:blue; margin:5px 5px;\" value=\"" + item[1] + "\" />" +
                "</a>";
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("No tv list load.");
            }

            page += "<br><br><br>" +
            "    <p>Page Views: {0}</p>" +
             "   <a href=\"/close\"> " +
            "  <input type = \"button\" style =\"width:80px; height:30px; color:red;\" value=\"CLOSE\" />" +
            "</a><br>" +
            "  </body>" +
            "</html>";

            return page;
        }
        
        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);

                try
                {
                    Process[] processes = Process.GetProcessesByName("firefox");

                    Array.ForEach(processes, (process) =>
                    {
                        Process pa = Process.GetProcessById(process.Id);
                        if (pa != null)
                        {
                            IntPtr h = pa.MainWindowHandle;
                            SetForegroundWindow(h);
                        }

                    });

                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/forward"))
                {
                    SendKeys.SendWait("{RIGHT}");
                    Console.WriteLine("Forward");
                }
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/back"))
                {
                    SendKeys.SendWait("{LEFT}");
                    Console.WriteLine("Back");
                }
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/volUp"))
                {
                    SendKeys.SendWait("{UP}");
                    Console.WriteLine("Volume up");
                }
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/volDown"))
                {
                    SendKeys.SendWait("{DOWN}");
                    Console.WriteLine("Volue down");
                }

                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/pause"))
                {
                    SendKeys.SendWait(" ");
                    Console.WriteLine("Pause");
                }
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/mute"))
                {
                    SendKeys.SendWait("m");
                    Console.WriteLine("mute");
                }
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/info"))
                {
                    SendKeys.SendWait("i");
                    Console.WriteLine("info");
                }
                if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/close"))
                {
                    Console.WriteLine("close");
                    Process[] processeFirefoxClose = Process.GetProcessesByName("firefox");

                    Array.ForEach(processeFirefoxClose, (process) =>
                    {
                        Process pa = Process.GetProcessById(process.Id);
                        pa.Kill();
                        runServer = false;

                    });
                }

                    string pthStr = AppDomain.CurrentDomain.BaseDirectory + "/tv.csv";
                    List<string[]> rows = File.ReadAllLines(pthStr).Select(x => x.Split(',')).ToList();

                    //MessageBox.Show(rows.Count.ToString());
                    foreach (var item in rows)
                    {
                        if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/" + item[1]))
                        {
                            SendKeys.SendWait("^w");
                            Console.WriteLine("Channel: " + item[1]);
                            Uri url = new Uri("https://eon.tv/player/" + item[0]);
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "firefox.exe",
                                Arguments = url.ToString(),
                                CreateNoWindow = false,
                                WindowStyle = ProcessWindowStyle.Maximized
                            });

                            SendKeys.SendWait("f");
                        }

                    }
                
                // Make sure we don't increment the page views counter if `favicon.ico` is requested
                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;

                // Write the response info
                string disableSubmit = !runServer ? "disabled" : "";
                byte[] data = Encoding.UTF8.GetBytes(String.Format(pageData(), pageViews, disableSubmit));
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message.ToString());
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }
        
        public static void runBrowser()
        {
            Uri url = new Uri("https://eon.tv/");
            Process.Start(new ProcessStartInfo
            {
                FileName = "firefox.exe",
                Arguments = "--kiosk " + url.ToString(),
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Maximized
            });
            Thread.Sleep(3000);
            SendKeys.SendWait("^(t)");
        }
        
        public static void Main(string[] args)
        {
            runBrowser();
            try
            {
                // Create a Http server and start listening for incoming connections
                listener = new HttpListener();
                listener.Prefixes.Add(url);
                listener.Start();

                string localIP;
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
                Console.WriteLine("Listening for connections on localhost {0}", url);
                Console.WriteLine("Listening for connections on http://" + localIP + ":" + port);
                // Handle requests
                Task listenTask = HandleIncomingConnections();
                listenTask.GetAwaiter().GetResult();

                // Close the listener
                listener.Close();
            }
            catch (Exception e)
            {

                Console.WriteLine("Error");
            }
            
            
        }
    }
}
