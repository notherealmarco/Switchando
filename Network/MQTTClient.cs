using uPLibrary.Networking.M2Mqtt;
using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Collections.Generic;
using HomeAutomationCore.Client;
using HomeAutomationCore;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HomeAutomation.Network
{
    public class MQTTClient
    {
        private MqttClient client;
        public delegate void Delegate(MqttClient sender, MqttMsgPublishEventArgs e);
        private List<KeyValuePair<string, Delegate>> CustomTopics;
        private List<string> Subscriptions;
        private List<string> Ignore;
        public string Username;
        public string Password;
        public string Address;
        public MQTTClient()
        {
            Ignore = new List<string>();
            Subscriptions = new List<string>();
            this.CustomTopics = new List<KeyValuePair<string, Delegate>>();
        }
        public MQTTClient(string address, string username, string password)
        {
            client = new MqttClient(address);
            Ignore = new List<string>();
            Subscriptions = new List<string>();
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            client.MqttMsgSubscribed += client_MqttMsgSubscribed;
            client.MqttMsgUnsubscribed += client_MqttMsgUnsubscribed;
            client.MqttMsgPublished += client_MqttMsgPublished;
            client.ConnectionClosed += OnConnectionClosed;

            this.Username = username;
            this.Password = password;
            this.Address = address;

            string[] topic = { "switchando", "switchando/main", "switchando/clients" };
            Subscriptions.Add("switchando"); Subscriptions.Add("switchando/main"); Subscriptions.Add("switchando/clients");
            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };
            client.Subscribe(topic, qosLevels);
            Publish("switchando/clients", "server-online");
            this.CustomTopics = new List<KeyValuePair<string, Delegate>>();
        }
        public MQTTClient(string address)
        {
            client = new MqttClient(address);
            Ignore = new List<string>();
            Subscriptions = new List<string>();
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            client.MqttMsgSubscribed += client_MqttMsgSubscribed;
            client.MqttMsgUnsubscribed += client_MqttMsgUnsubscribed;
            client.MqttMsgPublished += client_MqttMsgPublished;
            client.ConnectionClosed += OnConnectionClosed;

            this.Username = null;
            this.Password = null;

            string[] topic = { "switchando", "switchando/main", "switchando/clients" };
            Subscriptions.Add("switchando"); Subscriptions.Add("switchando/main"); Subscriptions.Add("switchando/clients");
            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };
            client.Subscribe(topic, qosLevels);
            Publish("switchando/clients", "server-online");
            this.CustomTopics = new List<KeyValuePair<string, Delegate>>();
        }
        public void Init()
        {
            client = new MqttClient(Address);
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            client.MqttMsgSubscribed += client_MqttMsgSubscribed;
            client.MqttMsgUnsubscribed += client_MqttMsgUnsubscribed;
            client.MqttMsgPublished += client_MqttMsgPublished;
            string[] topic = { "switchando", "switchando/main", "switchando/clients" };
            Subscriptions.Add("switchando"); Subscriptions.Add("switchando/main"); Subscriptions.Add("switchando/clients");
            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };
            client.Subscribe(topic, qosLevels);
            Publish("switchando/clients", "server-online");
        }
        public void Connect()
        {
            if (this.Username == null)
            {
                client.Connect("a-switchando-server");
            }
            else
            {
                client.Connect("a-switchando-server", Username, Password);
            }
        }
        public void Disconnect()
        {

        }
        public void Subscribe(string topic)
        {
            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };
            client.Subscribe(new string[]{ topic }, qosLevels);
            if (!Subscriptions.Contains(topic)) Subscriptions.Add(topic);
        }
        public void Subscribe(string topic, Delegate method)
        {
            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };
            bool addTopic = true;
            foreach(KeyValuePair<string, Delegate> kvp in CustomTopics)
            {
                if (kvp.Key.Equals(topic)) addTopic = false;
            }
            if (addTopic) client.Subscribe(new string[] { topic }, qosLevels);
            this.CustomTopics.Add(new KeyValuePair<string, Delegate>(topic, method));
            if (!Subscriptions.Contains(topic)) Subscriptions.Add(topic);
        }
        public void Unsubscribe(string topic)
        {
            client.Unsubscribe(new string[] { topic });
        }
        public void Publish(string topic, string message)
        {
            client.Publish(topic, Encoding.UTF8.GetBytes(message));
            if (Subscriptions.Contains(topic)) Ignore.Add(topic + message);
        }
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if (Ignore.Contains(e.Topic + Encoding.UTF8.GetString(e.Message)))
            {
                Ignore.Remove(e.Topic + Encoding.UTF8.GetString(e.Message));
                return;
            }
            Console.Write(e.Topic + " -> ");
            Console.WriteLine(Encoding.UTF8.GetChars(e.Message));

            foreach (KeyValuePair<string, Delegate> kvp in CustomTopics)
            {
                if (kvp.Key.Equals(e.Topic))
                {
                    kvp.Value((MqttClient)sender, e);
                    return;
                }
                if (kvp.Key.Contains("#"))
                {
                    string topic = kvp.Key.Substring(0, kvp.Key.Length - 1);
                    if (e.Topic.Contains(topic))
                    {
                        kvp.Value((MqttClient)sender, e);
                        return;
                    }
                }
            }
            string message = Encoding.UTF8.GetString(e.Message);
            if (message.StartsWith("client_handshake"))
            {
                bool clientExists = false;
                string[] data = message.Split('/');
                string clientName = data[1];
                string serverPassword = data[2];
                foreach (Client client in HomeAutomationServer.server.Clients)
                {
                    if (client.Name.Equals(clientName))
                    {
                        clientExists = true;
                        client.Connect("switchando/client/" + clientName);
                        string jsonMessageDevices = JsonConvert.SerializeObject(HomeAutomationServer.server.Objects);
                        Publish("switchando/client/" + clientName + "/init", jsonMessageDevices);
                    }
                }
                if (!clientExists)
                {
                    Client client = new Client(clientName);
                    client.Connect("switchando/client/" + clientName);
                    string jsonMessageDevices = JsonConvert.SerializeObject(HomeAutomationServer.server.Objects);
                    Publish("switchando/client/" + clientName + "/init", jsonMessageDevices);
                }
                Subscribe("switchando/client" + clientName);
                return;
            }
            APICommand.Run(Encoding.UTF8.GetString(e.Message));
        }
        private void OnConnectionClosed(object sender, EventArgs e)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            Task T = Task.Run(() => TryReconnectAsync(source.Token));
            T.Wait();
        }
        private async Task TryReconnectAsync(CancellationToken cancellationToken)
        {
            var connected = client.IsConnected;
            while (!connected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Connect();
                    Publish("switchando/clients", "server-online");
                }
                catch
                {
                    Console.WriteLine("MQTT> Can't connect to the broker, retrying in 10 seconds...");
                }
                connected = client.IsConnected;
                await Task.Delay(10000, cancellationToken);
            }
        }
        void client_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
        {

        }

        void client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {

        }
        void client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {

        }
    }
}