/*using Homeautomation.GPIO;
using HomeAutomation.Network;
using HomeAutomation.Objects.Switches;
using HomeAutomationCore;
using HomeAutomationCore.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation.Objects.Fans
{
    class SimpleFan : ISwitch
    {
        Client Client;
        public string ClientName;

        public uint Pin;
        public string Name;
        public string[] FriendlyNames;
        public bool Enabled;
        public string Description;

        public string ObjectType = "SIMPLE_FAN";
        public string ObjectModel = "FAN";

        public SimpleFan()
        {
            foreach (NetworkInterface netInt in HomeAutomationServer.server.NetworkInterfaces)
            {
                if (netInt.Id.Equals("fan_simple")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("fan_simple", requestHandler);
        }
        public SimpleFan(Client client, string name, uint pin, string description, string[] friendlyNames)
        {
            this.Client = client;
            this.ClientName = client.Name;
            this.FriendlyNames = friendlyNames;

            this.Description = description;
            this.Pin = pin;
            this.Name = name;
            HomeAutomationServer.server.Objects.Add(this);

            foreach (NetworkInterface netInt in HomeAutomationServer.server.NetworkInterfaces)
            {
                if (netInt.Id.Equals("fan_simple")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("fan_simple", requestHandler);
        }
        public void SetClient(Client client)
        {
            this.Client = client;
        }

        public void Start()
        {
            Console.WriteLine("Fan `" + this.Name + "` has been turned on.");
            //HomeAutomationServer.server.Telegram.Log("Fan `" + this.Name + "` has been turned on.");
            if (Client.Name.Equals("local"))
            {
                PIGPIO.gpio_write(0, Pin, 1);
            }
            else
            {
                UploadValues(true);
            }
            Enabled = true;
        }
        public void Stop()
        {
            Console.WriteLine("Fan `" + this.Name + "` has been turned off.");
            //HomeAutomationServer.server.Telegram.Log("Fan `" + this.Name + "` has been turned off.");
            if (Client.Name.Equals("local"))
            {
                PIGPIO.gpio_write(0, Pin, 0);
            }
            else
            {
                UploadValues(false);
            }
            Enabled = false;
        }
        public bool IsOn()
        {
            return Enabled;
        }
        public string GetName()
        {
            return Name;
        }
        public string GetId()
        {
            return Name;
        }
        public string GetObjectType()
        {
            return "SIMPLE_FAN";
        }
        public string[] GetFriendlyNames()
        {
            return FriendlyNames;
        }
        void UploadValues(bool Value)
        {
            Client.Sendata("interface=fan_simple&objname=" + this.Name + "&enabled=" + Value.ToString());
        }
        public NetworkInterface GetInterface()
        {
            return NetworkInterface.FromId("fan_simple");
        }
        public static string SendParameters(string[] request)
        {
            SimpleFan fan = null;
            foreach (string cmd in request)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "objname":
                        foreach (IObject obj in HomeAutomationServer.server.Objects)
                        {
                            if (obj.GetName().Equals(command[1]))
                            {
                                fan = (SimpleFan)obj;
                            }
                            if (obj.GetFriendlyNames() == null) continue;
                            if (Array.IndexOf(obj.GetFriendlyNames(), command[1].ToLower()) > -1)
                            {
                                fan = (SimpleFan)obj;
                            }
                        }
                        break;

                    case "switch":
                        if (command[1].Equals("true"))
                        {
                            fan.Start();
                        }
                        else
                        {
                            fan.Stop();
                        }
                        break;
                }
            }
            return "";
        }
    }
}*/