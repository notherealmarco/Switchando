using Homeautomation.GPIO;
using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Rooms;
using HomeAutomation.Users;
using HomeAutomationCore;
using HomeAutomationCore.Client;
using Switchando.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HomeAutomation.Objects.Lights
{
    class WLight : ILight
    {
        Client Client;
        public string ClientName;

        public uint Pin;
        public uint Value { get; set; }
        public uint Brightness { get; set; }
        public bool Switch { get; set; }

        public string Name { get; set; }
        public string[] FriendlyNames { get; set; }
        public string Description { get; set; }

        private Event OnSwitchOn;

        public string ObjectType = "LIGHT_GPIO_W";
        public string ObjectModel = "DIMMABLE_LIGHT";

        public WLight()
        {
            this.OnSwitchOn = HomeAutomationServer.server.Events.GetEvent(this, "switchon"); 
        }
        public WLight(Client client, string Name, uint pin, string description, string[] FriendlyNames)
        {
            this.Client = client;
            this.ClientName = client.Name;
            this.Switch = true;
            this.Description = description;
            this.Name = Name;
            this.Pin = pin;
            this.Value = 255;
            this.Brightness = 100;
            this.FriendlyNames = FriendlyNames;
            this.OnSwitchOn = HomeAutomationServer.server.Events.GetEvent(this, "switchon");

            if (Client.Name.Equals("local"))
            {
                PIGPIO.set_PWM_dutycycle(Client.PigpioID, Pin, Value);

                PIGPIO.set_PWM_frequency(Client.PigpioID, Pin, 4000);
            }

            HomeAutomationServer.server.Objects.Add(this);
        }
        public void SetClient(Client client)
        {
            this.Client = client;
        }

        public void Set(uint value, int dimmer, bool nolog = false)
        {
            Console.WriteLine("Setting " + this.Name + " from " + this.Value + " to " + value + " with a dimmer of " + dimmer + "ms.");
            //if (!nolog) HomeAutomationServer.server.Telegram.Log("Setting " + this.Name + " from " + this.Value + " to " + value + " with a dimmer of " + dimmer + "ms.");
            this.Brightness = 100;
            if (value == this.Value) return;

            if (value == 0) Switch = false; else Switch = true;

            if (!Client.Name.Equals("local"))
            {
                UploadValues(value, dimmer);
                this.Value = value;
                return;
            }

            if (dimmer == 0)
            {
                PIGPIO.set_PWM_dutycycle(Client.PigpioID, Pin, value);
                this.Value = value;
                return;
            }

            double[] values = new double[4];
            values[0] = this.Value;
            values[1] = value;
            values[2] = this.Pin;
            values[3] = ((dimmer / (((int)this.Value) - (int)value)));
            DimmerThread(values);

            this.Value = value;
        }

        public void Dimm(uint percentage, int dimmer)
        {
            var W = Value;

            if (Brightness == 0)
            {
                Set(255, dimmer);
            }
            else
            {
                W = W * percentage / Brightness;
                Set(W, dimmer);
            }

            this.Brightness = percentage;
        }

        public async void DimmerThread(object data)
        {
            await Task.Delay(1);
            double[] values = (double[])data;
            int led = (int)values[2];
            if (values[3] < 0) values[3] *= -1;
            if (values[0] <= values[1])
            {
                for (double i = values[0]; i <= values[1]; i = i + 1)
                {
                    PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)led, (uint)i);
                    if (values[3] == 0) values[3] = 1;
                    Thread.Sleep((int)values[3]);
                }
            }
            else
            {
                for (double i = values[0]; i >= values[1]; i = i - 1)
                {
                    PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)led, (uint)i);
                    if (values[3] == 0) values[3] = 1;
                    Thread.Sleep((int)values[3]);
                }
            }
            PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)led, (uint)values[1]);
        }

        void Block(long durationTicks)
        {
            Stopwatch sw;
            sw = Stopwatch.StartNew();
            int i = 0;

            while (sw.ElapsedTicks <= durationTicks)
            {
                if (sw.Elapsed.Ticks % 100 == 0)
                {
                    i++;
                }
            }
            sw.Stop();
        }

        public void Pause()
        {
            if (Switch)
            {
                Set(0, 1000);
                return;
            }
            else
            {
                Set(255, 1000);
            }
        }

        public void Pause(bool status)
        {
            if (!status)
            {
                Set(0, 1000);
                return;
            }
            else
            {
                Set(255, 1000);
            }
        }
        public void Start()
        {
            Pause(true);
            OnSwitchOn.Throw(this);
        }
        public void Stop()
        {
            Pause(false);
        }
        public bool IsOn()
        {
            return Switch;
        }
        public string GetObjectType()
        {
            return "LIGHT";
        }
        public string GetName()
        {
            return Name;
        }
        public string[] GetFriendlyNames()
        {
            return FriendlyNames;
        }
        public uint GetValue()
        {
            return Value;
        }
        public NetworkInterface GetInterface()
        {
            return NetworkInterface.FromId(ObjectType);
        }
        void UploadValues(uint Value, int DimmerIntervals)
        {
            Client.Sendata("LIGHT_GPIO_W/changeValue?objname=" + this.Name + "&W=" + Value + "&dimmer=" + DimmerIntervals);
        }
        private static WLight FindLightFromName(string name)
        {
            WLight light = null;
            foreach (IObject obj in HomeAutomationServer.server.Objects)
            {
                if (obj.GetName().ToLower().Equals(name.ToLower()))
                {
                    light = (WLight)obj;
                    break;
                }
                if (obj.GetFriendlyNames() == null) continue;
                if (Array.IndexOf(obj.GetFriendlyNames(), name.ToLower()) > -1)
                {
                    light = (WLight)obj;
                    break;
                }
            }
            return light;
        }
        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("changeValue"))
            {
                WLight light = null;
                uint pwm = 0;
                int dimmer = 0;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = FindLightFromName(command[1]);
                            break;
                        case "W":
                            pwm = uint.Parse(command[1]);
                            break;
                        case "value":
                            pwm = uint.Parse(command[1]);
                            break;
                        case "dimmer":
                            dimmer = int.Parse(command[1]);
                            break;
                    }
                }
                if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                if (!login.HasAccess(light)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                light.Set(pwm, dimmer);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("switch"))
            {
                WLight light = null;
                bool status = false;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = FindLightFromName(command[1]);
                            break;
                        case "switch":
                            status = bool.Parse(command[1]);
                            break;
                    }
                    if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                }
                if (!login.HasAccess(light)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                if (status) light.Start(); else light.Stop();
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("dimm"))
            {
                WLight light = null;
                byte dimm_percentage = 255;
                int dimmer = 0;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = FindLightFromName(command[1]);
                            break;
                        case "percentage":
                            dimm_percentage = byte.Parse(command[1]);
                            break;
                        case "dimmer":
                            dimmer = int.Parse(command[1]);
                            break;
                    }
                    if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                }
                if (!login.HasAccess(light)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                light.Dimm(dimm_percentage, dimmer);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("create"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                string[] friendlyNames = null;
                string description = null;

                uint pin = 0;

                Client client = null;

                Room room = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "objname":
                            name = command[1];
                            break;
                        case "description":
                            description = command[1];
                            break;
                        case "setfriendlynames":
                            string names = command[1];
                            friendlyNames = names.Split(',');
                            break;
                        case "pin":
                            string pinStr = command[1];
                            pin = uint.Parse(pinStr);
                            break;
                        case "client":
                            string clientName = command[1];
                            foreach (Client clnt in HomeAutomationServer.server.Clients)
                            {
                                if (clnt.Name.Equals(clientName))
                                {
                                    client = clnt;
                                }
                            }
                            if (client == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Raspi-Client not found").Json();
                            break;
                        case "room":
                            foreach (Room stanza in HomeAutomationServer.server.Rooms)
                            {
                                if (stanza.Name.ToLower().Equals(command[1].ToLower()))
                                {
                                    room = stanza;
                                }
                            }
                            break;
                    }
                }
                if (room == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Room not found").Json();
                WLight light = new WLight(client, name, pin, description, friendlyNames);
                room.AddItem(light);
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.light = light;
                return data.Json();
            }

            if (string.IsNullOrEmpty(method))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                WLight light = null;
                uint Value = 0;
                int dimmer = 0;
                uint dimm_percentage = 400;
                bool nolog = false;
                string status = null;
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
                                    light = (WLight)obj;
                                }
                                if (obj.GetFriendlyNames() == null) continue;
                                if (Array.IndexOf(obj.GetFriendlyNames(), command[1].ToLower()) > -1)
                                {
                                    light = (WLight)obj;
                                }
                            }
                            break;

                        case "W":
                            Value = uint.Parse(command[1]);
                            break;

                        case "dimmer":
                            dimmer = int.Parse(command[1]);
                            break;

                        case "percentage":
                            dimm_percentage = uint.Parse(command[1]);
                            break;

                        case "nolog":
                            nolog = true;
                            break;

                        case "switch":
                            status = command[1];
                            break;
                    }
                }
                if (status != null)
                {

                    light.Pause(bool.Parse(status));
                    return "";
                }
                if (dimm_percentage != 400)
                {
                    light.Dimm(dimm_percentage, dimmer);
                    return "";
                }
                light.Set(Value, dimmer, nolog);
                return "";
            }
            return "";
        }
        public void Init()
        {
            if (Client.Name.Equals("local"))
            {
                PIGPIO.set_PWM_dutycycle(0, Pin, Value);
                PIGPIO.set_PWM_frequency(0, Pin, 4000);
            }
        }
        public static void Setup(Room room, dynamic device)
        {
            WLight light = new WLight();
            light.Pin = (uint)device.Pin;
            light.Name = device.Name;
            //light.FriendlyNames = ((List<object>)device.FriendlyNames).ToArray().Where(x => x != null)
            //.Select(x => x.ToString())
            //.ToArray();
            light.FriendlyNames = Array.ConvertAll(((List<object>)device.FriendlyNames).ToArray(), x => x.ToString());
            light.Description = device.Description;
            light.Switch = device.Switch;
            light.Value = (uint)device.Value;
            light.ClientName = device.Client.Name;
            light.SetClient(device.Client);

            HomeAutomationServer.server.Objects.Add(light);
            room.AddItem(light);
            light.Init();
        }
    }
}
