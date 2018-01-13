using HomeAutomation.Application.ConfigRetriver;
using HomeAutomation.ConfigRetriver;
using HomeAutomation.Core;
using HomeAutomation.Events;
using HomeAutomation.Network;
using HomeAutomation.ObjectInterfaces;
using HomeAutomation.Objects;
using HomeAutomation.Rooms;
using HomeAutomation.Scenarios;
using HomeAutomation.Users;
using HomeAutomation.Utilities;
using System;
using System.Collections.Generic;

namespace HomeAutomationCore
{
    public class HomeAutomationServer
    {
        public static HomeAutomationServer server;

        public List<Client.Client> Clients { get; set; }
        public List<NetworkInterface> NetworkInterfaces { get; set; }
        public List<SetupTool> Setups { get; set; }
        public List<Configuration> Configs { get; set; }

        public ConfigManager Database { get; set; }

        public List<IObject> Objects { get; set; }
        public List<Room> Rooms { get; set; }
        public List<Scenario> Scenarios { get; set; }
        public List<HomeAutomation.ObjectInterfaces.Action> Actions { get; set; }
        public EventsManager Events { get; set; }
        public List<Identity> Identities { get; set; }
        public MQTTClient MQTTClient { get; set; }


        //public TelegramBot Telegram { get; set; }
        public ObjectNetwork ObjectNetwork { get; set; }
        public Dictionary<string, string> HTMLFragments { get; set; }

        public string House;
        private string Password;

        public HomeAutomationServer(string house, string password)
        {
            server = this;
            this.House = house;
            this.Password = password;
            Database = new ConfigManager(Paths.GetServerPath() + "/switchando.json");
            Clients = new List<Client.Client>();
            Objects = new List<IObject>();
            Rooms = new List<Room>();

            NetworkInterfaces = new List<NetworkInterface>();
            Setups = new List<SetupTool>();
            Configs = new List<Configuration>();

            HTMLFragments = new Dictionary<string, string>();

            ObjectNetwork = new ObjectNetwork();
            ObjectNetwork.MethodInterfaces = new List<MethodInterface>();
            ObjectNetwork.ObjectInterfaces = new List<ObjectInterface>();
            Events = new EventsManager();
            
            Actions = Database.Get<dynamic>("switchando.actions").ToObject<List<HomeAutomation.ObjectInterfaces.Action>>();
            Identities = Database.Get<dynamic>("switchando.identities").ToObject<List<Identity>>();
            MQTTClient = Database.Get<dynamic>("switchando.mqttclient").ToObject<MQTTClient>();

            if (Actions == null) Actions = new List<HomeAutomation.ObjectInterfaces.Action>();
            if (Identities == null) Identities = new List<Identity>();

        }
        public void LoadCore()
        {
            Console.WriteLine("Loading Switchando Core...");
            CoreLoader.LoadFromJson();
        }
        public void SaveData()
        {
            Database.Put("switchando.rooms", Rooms);
            Database.Put("switchando.actions", Actions);
            Database.Put("switchando.identities", Identities);
            Database.Put("switchando.mqttclient", MQTTClient);
            Events.Save();
            Database.Save();
        }
        public string GetPassword()
        {
            return this.Password;
        }
        public void SetPassword(string password)
        {
            this.Password = password;
        }
    }
}