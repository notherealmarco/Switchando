using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomationCore.Client
{
    public class Client
    {
        private string IP;
        public int PigpioID;
        public string Name;
        //public TcpClient TcpClient = null;
        private StreamWriter Writer = null;
        string Topic = null;
        bool IsMQTT = false;
        public bool Connected { get; set; }

        public Client(string ip, int pigpioID, string name)
        {
            this.Name = name;
            this.IP = ip;
            this.PigpioID = pigpioID;
            HomeAutomationServer.server.Clients.Add(this);
        }
        public Client(string name)
        {
            this.Name = name;
            HomeAutomationServer.server.Clients.Add(this);
        }
        public void Connect(TcpClient client, StreamWriter writer)
        {
            //this.TcpClient = client;
            this.Writer = writer;
            this.Connected = true;
        }
        public void Connect(string topic)
        {
            this.Topic = topic;
            this.IsMQTT = true;
        }
        public void Sendata(string message)
        {
            if (IsMQTT)
            {
                HomeAutomationServer.server.MQTTClient.Publish("switchando/client/" + this.Name, message);
                return;
            }
            if (!Connected) return;
            //var writer = new StreamWriter(TcpClient.GetStream()) { AutoFlush = true };
            try
            {
                Writer.WriteLine(message);
                Writer.Flush();
            }
            catch
            {
                Connected = false;
            }
            //writer.Close();
        }
        /// <summary>
        ///  Returns the client object from the given name if exists, else it returns a new client
        /// </summary>
        public static Client GetCreateClient(string name)
        {
            Client client = null;
            bool toAdd = true;
            foreach (Client clnt in HomeAutomationServer.server.Clients)
            {
                if (clnt.Name.Equals(name))
                {
                    client = clnt;
                    toAdd = false;
                }
            }
            if (toAdd) client = new Client(null, 0, name);

            if (HomeAutomationServer.server.Clients.Count == 0)
            {
                client = new Client(null, 0, name);
            }
            return client;
        }
    }
}