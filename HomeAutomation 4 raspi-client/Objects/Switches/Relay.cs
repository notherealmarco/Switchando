using Homeautomation.GPIO;
using HomeAutomation.Network;
using HomeAutomation.Objects.Switches;
using HomeAutomationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation.Objects.Fans
{
    class Relay : ISwitch
    {
        //Client Client;
        public uint Pin;
        public string Name;
        public bool Enabled;
        public string Description;

        public HomeAutomationObject ObjectType = HomeAutomationObject.GENERIC_SWITCH;
        public Relay(string name, uint pin, string description)
        {
            //this.Client = client;

            this.Description = description;
            this.Pin = pin;
            this.Name = name;
            HomeAutomationClient.client.Objects.Add(this);

            foreach (NetworkInterface netInt in HomeAutomationClient.client.NetworkInterfaces)
            {
                if (netInt.Id.Equals("relay")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("relay", requestHandler);
        }

        public Relay()
        {
            foreach (NetworkInterface netInt in HomeAutomationClient.client.NetworkInterfaces)
            {
                if (netInt.Id.Equals("relay")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("relay", requestHandler);
        }

        public void Start()
        {
            PIGPIO.gpio_write(0, Pin, 1);
            Enabled = true;
        }
        public void Stop()
        {
            PIGPIO.gpio_write(0, Pin, 0);
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
        public new HomeAutomationObject GetObjectType()
        {
            return HomeAutomationObject.GENERIC_SWITCH;
        }
        /*void UploadValues(bool Value, int DimmerIntervals)
        {
            Client.Sendata("update=" + this.Name + "&enabled=" + Value.ToString());
        }*/
        public static void SendParameters(string[] request)
        {
            Relay fan = null;
            foreach (string cmd in request)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "objname":
                        foreach (IObject obj in HomeAutomationClient.client.Objects)
                        {
                            if (obj.GetName().Equals(command[1]))
                            {
                                fan = (Relay)obj;
                            }
                        }
                        break;

                    case "enabled":
                        if (command[1].ToLower().Equals("true"))
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
        }
    }
}