using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using WebSocketSharp;
using HomeAutomation.Utilities;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation.Network
{
    public class HTTPWebUI
    {
        private List<string> _abort;
        private Dictionary<string, AutoResetEvent> _continue;
        private readonly string[] _indexFiles = {
        "index.html",
        "index.htm",
        "default.html",
        "default.htm"
    };

        private static IDictionary<string, string> _mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".htmlfragment", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".flac", "audio/flac"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".svg", "image/svg+xml"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".json", "text/json"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };
        private Thread _serverThread;
        private string _rootDirectory;
        private HttpListener _listener;
        private int _port;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        /// <param name="port">Port of the server.</param>
        public HTTPWebUI(string path, int port)
        {
            _abort = new List<string>();
            _continue = new Dictionary<string, AutoResetEvent>();
            this.Initialize(path, port);
        }

        /// <summary>
        /// Construct server with suitable port.
        /// </summary>
        /// <param name="path">Directory path to serve.</param>
        public HTTPWebUI(string path)
        {
            _abort = new List<string>();
            _continue = new Dictionary<string, AutoResetEvent>();
            //get an empty port
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            this.Initialize(path, port);
        }

        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            //Console.WriteLine("Requested HTTP web page -> " + filename);
            filename = filename.Substring(1);

            if (string.IsNullOrEmpty(filename))
            {
                foreach (string indexFile in _indexFiles)
                {
                    if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                    {
                        filename = indexFile;
                        break;
                    }
                }
            }
            if (filename.Contains("plugins/"))
            {
                filename = filename.Substring("plugins/".Length);
                _rootDirectory = Utilities.Paths.GetServerPath() + "/plugins";
                filename = Path.Combine(_rootDirectory, filename);
                _rootDirectory = Utilities.Paths.GetServerPath() + "/web";
            }
            else if (filename.Contains("switchando-fragments/"))
            {
                filename = filename.Substring("switchando-fragments/".Length);

                string path;
                if (!HomeAutomationCore.HomeAutomationServer.server.HTMLFragments.TryGetValue(filename, out path))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.OutputStream.Close();
                    return;
                }
                filename = path;
            }
            else
            {
                filename = Path.Combine(_rootDirectory, filename);
            }
            
            
            if (File.Exists(filename))
            {
                try
                {
                    Stream input = new FileStream(filename, FileMode.Open);

                    //Adding permanent http response headers
                    string mime;
                    context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                    context.Response.ContentLength64 = input.Length;

                    byte[] buffer = new byte[1024 * 16];
                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        context.Response.OutputStream.Write(buffer, 0, nbytes);
                    input.Close();
                    
                    //context.Response.StatusCode = (int)HttpStatusCode.OK;
                    //context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    //context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));
                    context.Response.OutputStream.Flush();
                }
                catch
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            context.Response.OutputStream.Close();
        }
        public void SendPacket(string request_id, string content, byte[] buffer, int nbytes, long total_size, WebSocket ws)
        {
            List<byte> raw_msg = new List<byte>();
            byte[] rqid = Encoding.UTF8.GetBytes(request_id);
            byte[] content_type = Encoding.UTF8.GetBytes(content + "\n" + nbytes + "\n" + total_size);
            raw_msg.AddRange(rqid);
            raw_msg.Add(0);
            raw_msg.AddRange(content_type);
            raw_msg.Add(0);
            raw_msg.AddRange(buffer);
            ws.Send(raw_msg.ToArray());
        }
        public void AbortRequest(string request_id)
        {
            if (!_abort.Contains(request_id)) _abort.Add(request_id);
        }
        public async void ProcessCloud(string request_id, string request, WebSocket ws)
        {
            _continue.Add(request_id, new AutoResetEvent(true));
            string filename = request;
            string pagePrefix = "";
            //Console.WriteLine("Requested HTTP web page -> " + filename);
            filename = filename.Substring(1);

            if (string.IsNullOrEmpty(filename))
            {
                foreach (string indexFile in _indexFiles)
                {
                    if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                    {
                        filename = indexFile;
                        //pagePrefix = "<button type=\"button\" style=\"\">BETA degli account Cloud</button>";
                        break;
                    }
                }
            }
            if (filename.StartsWith("api/"))
            {
                filename = filename.Substring(4);
                byte[] packet = Encoding.UTF8.GetBytes(HTTPHandler.SendCloudResponse(filename));
                SendPacket(request_id, "application/json", packet, packet.Length, packet.Length, ws);
                ws.Send("final\n" + request_id);
                return;
            }
            if (filename.Contains("plugins/"))
            {
                filename = filename.Substring("plugins/".Length);
                _rootDirectory = Utilities.Paths.GetServerPath() + "/plugins";
                filename = Path.Combine(_rootDirectory, filename);
                _rootDirectory = Utilities.Paths.GetServerPath() + "/web";
            }
            else if (filename.Contains("switchando-fragments/"))
            {
                filename = filename.Substring("switchando-fragments/".Length);

                string path;
                if (!HomeAutomationCore.HomeAutomationServer.server.HTMLFragments.TryGetValue(filename, out path))
                {
                    //context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    //context.Response.OutputStream.Close();
                    ws.Send("httpcode\n400\n" + request_id);
                    return;
                }
                filename = path;
            }
            else
            {
                filename = Path.Combine(_rootDirectory, filename);
            }
            var fn = filename.Split('?');
            if (filename.Contains('?')) filename = filename.Substring(0, filename.Length - fn[fn.Length - 1].Length - 1);
            if (File.Exists(filename))
            {
                try
                {
                    Stream input = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    string mime;
                    string content_type = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";

                    bool safe_mode = false;
                    long lenght = input.Length;
                    if (lenght > 1e+7) safe_mode = true;

                    if (!string.IsNullOrEmpty(pagePrefix))
                    {
                        byte[] prefixRaw = Encoding.UTF8.GetBytes(pagePrefix);
                        lenght += prefixRaw.Length;
                        SendPacket(request_id, content_type, prefixRaw, prefixRaw.Length, lenght, ws);
                    }

                    byte[] buffer = new byte[1024 * 128];
                    int nbytes;
                    while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (_abort.Contains(request_id)) break;
                        _continue[request_id].WaitOne();
                        while (!ws.IsAlive)
                        {
                            Console.WriteLine("CLOUD | Connection died while sending a packet");
                            await Task.Delay(100);
                        }
                        SendPacket(request_id, content_type, buffer, nbytes, lenght, ws);
                        //Console.WriteLine(request_id + " -> sent");
                    }
                    input.Close();

                    //context.Response.StatusCode = (int)HttpStatusCode.OK;
                    //context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    //context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));
                    //context.Response.OutputStream.Flush();
                    ws.Send("final\n" + request_id);
                    if (_continue.ContainsKey(request_id)) _continue.Remove(request_id);
                    if (_abort.Contains(request_id)) _abort.Remove(request_id);
                }
                catch
                {
                    ws.Send("httpcode\n500\n" + request_id);
                    if (_continue.ContainsKey(request_id)) _continue.Remove(request_id);
                    if (_abort.Contains(request_id)) _abort.Remove(request_id);
                }

            }
            else
            {
                ws.Send("httpcode\n400\n" + request_id);
                if (_continue.ContainsKey(request_id)) _continue.Remove(request_id);
                if (_abort.Contains(request_id)) _abort.Remove(request_id);
            }
            if (_continue.ContainsKey(request_id)) _continue.Remove(request_id);
            if (_abort.Contains(request_id)) _abort.Remove(request_id);
        }
        public void SendAnother(string request_id)
        {
            if (_continue.ContainsKey(request_id))
            {
                _continue[request_id].Set();
            }
        }
        private void Initialize(string path, int port)
        {
            this._rootDirectory = path;
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }
    }
}