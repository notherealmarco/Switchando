using HomeAutomation.Network;
using HomeAutomation.Utilities;
using HomeAutomationCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Switchando.Cloud
{
    public class SwitchandoCloud
    {
        private WebSocket _websocket;
        private bool _enable;
        private string _endpoint;
        private string _key;
        private string _prefix;
        private string _wsprefix;
        private bool _secure;
        private int _wsport = -1;
        private byte _failcount;
        public void SaveConfig()
        {
            var file = "enable: " + _enable.ToString().ToLower() + "\nsecure: " + _secure.ToString().ToLower() + "\nkey: " + _key + "\nendpoint: " + _endpoint;
            File.WriteAllText(Paths.GetServerPath() + "/cloud.config", file);
        }
        private void ReadConfig()
        {
            Console.WriteLine("CLOUD | Attempting to read config file...");
            _enable = false;
            _secure = true;
            _key = "";
            _endpoint = "";
            if (!File.Exists(Paths.GetServerPath() + "/cloud.config"))
            {
                return;
            }
            string file = File.ReadAllText(Paths.GetServerPath() + "/cloud.config");
            string[] lines = file.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string[] s = line.Split(':');
                string value = line.Substring(s[0].Length + 1);
                if (value.StartsWith(" ")) value = value.Substring(1);
                if (s[0].Equals("enable")) _enable = bool.Parse(value);
                if (s[0].Equals("secure"))
                {
                    if (bool.Parse(value))
                    {
                        _secure = true;
                        _prefix = "https://";
                        _wsprefix = "wss://";
                    }
                    else
                    {
                        _secure = false;
                        _prefix = "http://";
                        _wsprefix = "ws://";
                    }
                }
                if (s[0].Equals("endpoint")) _endpoint = value;
                if (s[0].Equals("wsport")) _wsport = int.Parse(value);
                if (s[0].Equals("key")) _key = value;
            }
        }
        public SwitchandoCloud()
        {
            ReadConfig();
            Init();
        }
        public SwitchandoCloud(bool partialInit, string[] args)
        {
            ReadConfig();
            if (args[1].Equals("set"))
            {
                if (args[2].Equals("collector"))
                {
                    var status = bool.Parse(args[3]);
                    if (status)
                    {
                        if (File.Exists(Paths.GetServerPath() + "/collector.disabled")) File.Delete(Paths.GetServerPath() + "/collector.disabled");
                    }
                    else
                    {
                        File.WriteAllText(Paths.GetServerPath() + "/collector.disabled", "Switchando Collector has been disabled :(\nYou can always change your mind and enable it again!");
                    }
                }
                if (args[2].Equals("enable"))
                {
                    var status = bool.Parse(args[3]);
                    HomeAutomationServer.server.Cloud.SetEnable(status);
                }
                if (args[2].Equals("secure") || args[2].Equals("security"))
                {
                    bool secure = true;
                    var secure_string = args[3];
                    if (secure_string.ToLower().Equals("high")) secure = true;
                    else if (secure_string.ToLower().Equals("low")) secure = false;
                    else if (secure_string.ToLower().Equals("true")) secure = true;
                    else if (secure_string.ToLower().Equals("false")) secure = false;
                    HomeAutomationServer.server.Cloud.SetSecurity(secure);
                }
                if (args[2].Equals("key"))
                {
                    var key = args[3];
                    HomeAutomationServer.server.Cloud.SetKey(key);
                }
                if (args[2].Equals("endpoint"))
                {
                    var endpoint = args[3];
                    HomeAutomationServer.server.Cloud.SetEndpoint(endpoint);
                }
            }
            if (args[1].Equals("register"))
            {
                Console.WriteLine();
                if (string.IsNullOrEmpty(HomeAutomationServer.server.Cloud.GetEndpoint()))
                {
                    Console.WriteLine("Please, make sure you have setted up a Cloud Endpoint properly.");
                    return;
                }
                Console.WriteLine("Welcome to Switchando Cloud\nCurrent Cloud Endpoint -> " + HomeAutomationServer.server.Cloud.GetEndpoint());
                Console.Write("\nType in your invitation key: ");
                var invKey = Console.ReadLine();
                Console.Write("\nSelect your nickname: ");
                var nickname = Console.ReadLine();
                Console.Write("\nType in your full name: ");
                var fullName = Console.ReadLine();
                HomeAutomationServer.server.Cloud.GetNewAccount(invKey, nickname, fullName);
            }
            HomeAutomationServer.server.Cloud.SaveConfig();
            Console.WriteLine("\n>> Switchando is done <<");
            return;
        }
        private void Init()
        {
            if (_enable)
            {
                Console.WriteLine("CLOUD | Contacting the remote endpoint... (" + _prefix + _endpoint + ")");
                try
                {
                    NameValueCollection nvc = new NameValueCollection();
                    nvc.Add("cloud_key", _key);
                    WebClient client = new WebClient();
                    string response_string = Encoding.UTF8.GetString(client.UploadValues(_prefix + _endpoint + "/swcapi/account/handshake", nvc));
                    var response = JsonConvert.DeserializeObject<MainResponse>(response_string);
                    if (response.status == 0)
                    {
                        string addr = response.address;
                        if (response.address.StartsWith("http://")) addr = response.address.Substring(7);
                        if (response.address.StartsWith("https://")) addr = response.address.Substring(8);
                        string port = ":" + _wsport;
                        if (_wsport == -1) port = "";
                        _websocket = new WebSocket(_wsprefix + addr + port + "/ws/swc?safe_mode=true&cloud_key=" + _key);
                        //_websocket = new WebSocket("ws://localhost:4649/swc?cloud_key=" + _key);
                        var sslProtocolHack = (System.Security.Authentication.SslProtocols)(3072 | 768 | 192);
                        
                        _websocket.SslConfiguration.EnabledSslProtocols = sslProtocolHack;

                        Console.WriteLine("CLOUD | Connecting... (" + addr + ")");
                        _websocket.OnMessage += (sender, e) =>
                        {
                            if (e.Data.StartsWith(@"\"))
                            {
                                //Console.WriteLine(e.Data);
                                var data = e.Data.Split('\n');
                                if (data[0].Equals("\\abort")) HomeAutomationServer.server.Web.AbortRequest(data[1]);
                                if (data[0].Equals("\\continue")) HomeAutomationServer.server.Web.SendAnother(data[1]);
                                return;
                            }
                            var id = e.Data.Split('\n')[0];
                            var request = e.Data.Split('\n')[1];
                            Task.Factory.StartNew(() => HomeAutomationServer.server.Web.ProcessCloud(id, request, _websocket));
                            //HomeAutomationServer.server.Web.ProcessCloud(id, request, _websocket);
                        };
                        _websocket.OnClose += (sender, e) =>
                        {
                            Console.WriteLine("CLOUD | Connection lost");
                            if (_failcount > 10)
                            {
                                Init();
                                return;
                            }
                            _failcount += 1;
                            _websocket.Connect();
                        };
                        _websocket.OnOpen += (sender, e) =>
                        {
                            _failcount = 0;
                            if (_websocket.IsAlive) Console.WriteLine("CLOUD | Connected");
                        };

                        _websocket.SslConfiguration.ServerCertificateValidationCallback =
                        (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            return true;
                        }; //remove it's insecure

                        _websocket.Connect();
                        }
                    else
                    {
                        Console.WriteLine("CLOUD | Switchando Cloud returned an internal server error");
                        Thread.Sleep(10000);
                        Init();
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("CLOUD | An exception occurred while contacting the endpoint -> " + e.ToString());
                    Thread.Sleep(10000);
                    Init();
                }
            } else Console.WriteLine("CLOUD | disabled");
        }
        public void GetNewAccount(string invKey, string nickname, string fullName)
        {
            if (_enable)
            {
                try
                {
                    //NameValueCollection nvc = new NameValueCollection();
                    //nvc.Add("cloud_key", _key);
                    WebClient client = new WebClient();
                    string response_string = client.DownloadString(_prefix + _endpoint + "/swcapi/account/register/" + invKey + "/" + nickname + "/" + fullName);
                    var response = JsonConvert.DeserializeObject<MainResponse>(response_string);
                    if (response.status == 0)
                    {
                        Console.WriteLine("CLOUD | Your account has been successfully registred");
                        Console.WriteLine("CLOUD | Your Cloud Key is -> " + response.key);
                        SetKey(response.key);
                        SaveConfig();
                    }
                    else
                    {
                        Console.WriteLine("CLOUD | The remote endpoint didn't accept your application, is the invite wrong or expired?");
                    }
                }
                catch
                {
                    Console.WriteLine("CLOUD | An exception occurred while contacting the remote endpoint");
                }
            }
        }
        public void SetEnable(bool status)
        {
            _enable = status;
        }
        public void SetKey(string key)
        {
            _key = key;
        }
        public void SetEndpoint(string endpoint)
        {
            _endpoint = endpoint;
        }
        public string GetEndpoint()
        {
            return _endpoint;
        }
        public void SetSecurity(bool secure)
        {
            _secure = secure;
        }
    }
    class MainResponse
    {
        public int status;
        public string description;
        public string address;
        public string key;
    }
}