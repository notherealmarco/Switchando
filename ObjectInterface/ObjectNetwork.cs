using HomeAutomation.Network;
using HomeAutomation.Users;
using HomeAutomationCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HomeAutomation.ObjectInterfaces
{
    public class ObjectNetwork
    {
        public List<MethodInterface> MethodInterfaces { get; set; }
        public List<ObjectInterface> ObjectInterfaces { get; set; }
        //public ObjectNetwork.NetworkObjects Objects { get; set; }

        /*public void Load()
        {
            if (File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/configuration_objectnetwork.json"))
            {
                string json = File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/configuration_objectnetwork.json");
                HomeAutomationServer.server.ObjectNetwork.Objects = JsonConvert.DeserializeObject<NetworkObjects>(json);
            }
            if (Objects == null) Objects = new NetworkObjects();
            if (Objects.Actions == null) Objects.Actions = new List<Action>();
            if (Objects.Identities == null) Objects.Identities = new List<Identity>();
        }
        public void Save()
        {
            File.WriteAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/configuration_objectnetwork.json", JsonConvert.SerializeObject(HomeAutomationServer.server.ObjectNetwork.Objects));
        }
        public class NetworkObjects
        {
            /*public List<Action> Actions { get; set; }
            public List<Identity> Identities { get; set; }
            public MQTTClient MQTTClient { get; set; }
        }*/
    }
}
