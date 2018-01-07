/*using HomeAutomationCore;
using HomeAutomationCore.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HomeAutomation.Network
{
    class TCPServer
    {
        private static TcpListener _listener;
        public static void StartListening()
        {
            System.Net.IPAddress localIPAddress = IPAddress.Any;
            IPEndPoint ipLocal = new IPEndPoint(localIPAddress, 2345);
            _listener = new TcpListener(ipLocal);
            _listener.Start();
            WaitForClientConnect();
        }
        private static void WaitForClientConnect()
        {
            object obj = new object();
            _listener.BeginAcceptTcpClient(new System.AsyncCallback(OnClientConnect), obj);
        }
        private static void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                TcpClient clientSocket = default(TcpClient);
                clientSocket = _listener.EndAcceptTcpClient(asyn);
                WaitForClientConnect();

                while (clientSocket.Connected)
                {
                    NetworkStream stream = clientSocket.GetStream();
                    var reader = new StreamReader(stream);
                    string message = reader.ReadLine();
                    if (message == null) break;
                    Console.WriteLine("TCP message from a client -> " + message);

                    if (!message.Contains("&password=" + HomeAutomationServer.server.GetPassword())) return;

                    string[] commands = message.Split('&');

                    if (message.StartsWith("client-handshake="))
                    {
                        bool clientExists = false;
                        string clientName = commands[0].Split('=')[1];
                        foreach (Client client in HomeAutomationServer.server.Clients)
                        {
                            if (client.Name.Equals(clientName))
                            {
                                clientExists = true;
                                StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
                                client.Connect(clientSocket, writer);
                                string jsonMessageDevices = JsonConvert.SerializeObject(HomeAutomationServer.server.Objects);
                                string jsonMessageRooms = JsonConvert.SerializeObject(HomeAutomationServer.server.Rooms);
                                writer.WriteLine("info_devices=" + jsonMessageDevices);
                                //return;
                                //writer.Close();
                            }
                        }
                        if (!clientExists)
                        {
                            Client client = new Client(null, 0, clientName);
                            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
                            client.Connect(clientSocket, writer);
                            string jsonMessageDevices = JsonConvert.SerializeObject(HomeAutomationServer.server.Objects);
                            string jsonMessageRooms = JsonConvert.SerializeObject(HomeAutomationServer.server.Rooms);
                            ConfigRetriver.ConfigRetriver.Update();
                            writer.WriteLine("info_devices=" + jsonMessageDevices);
                        }
                    }
                    string[] icommand = commands[0].Split('=');
                    if (icommand[0].Equals("interface"))
                    {
                        foreach (NetworkInterface networkInterface in HomeAutomationServer.server.NetworkInterfaces)
                        {
                            if (networkInterface.Id.Equals(icommand[1]))
                            {
                                networkInterface.Run(null, commands);
                            }
                        }
                    }
                    else if (icommand[0].Equals("objname"))
                    {
                        //NetworkInterface.FromId("auto").Run(commands);
                    }
                }
            }
            catch (Exception e)
            {
                WaitForClientConnect();
            }
        }
    }
}
*/