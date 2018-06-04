using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects;
using HomeAutomation.Objects.Lights;
using HomeAutomation.Rooms;
using HomeAutomation.Users;
using HomeAutomationCore;
using System;
using System.Collections.Generic;

namespace Switchando.Objects.Lights
{
    public abstract class WLight : ILight
    {
        public uint Value { get; set; }
        public uint Brightness { get; set; }

        public bool Switch { get; set; }

        public string Name;
        public string DisplayName;
        public string[] FriendlyNames;
        public string Description;

        public bool nolog = false;

        public string ObjectModel = "DIMMABLE_LIGHT";

        public string ClientName = "local"; //Compatibility with older versions

        public void Create(string Name, string DisplayedName, string Description, string[] FriendlyNames)
        {
            this.Brightness = 100;
            this.Switch = true;
            this.Value = 0;
            this.Name = Name;
            this.DisplayName = DisplayedName;
            this.Description = Description;
            this.FriendlyNames = FriendlyNames;
        }
        public void Set(uint Value, int DimmerIntervals)
        {
            UpdateStatus(Value, DimmerIntervals);
            this.Value = Value;
        }
        abstract public void UpdateStatus(uint Value, int DimmerIntervals);
        abstract public void Dimm(uint percentage, int dimmer);

        public void Pause()
        {
            if (Switch)
            {
                UpdateSwitch(false);
                Switch = false;
            }
            else
            {
                UpdateSwitch(true);
                Switch = true;
            }
        }

        public void Pause(bool status)
        {
            if (status)
            {
                UpdateSwitch(true);
                Switch = true;
            }
            else
            {
                UpdateSwitch(false);
                Switch = false;
            }
        }

        abstract public void UpdateSwitch(bool status);

        public void Start()
        {
            Pause(true);
        }
        public void Stop()
        {
            Pause(false);
        }
        public bool IsOn()
        {
            return Switch;
        }
        abstract public string GetObjectType();
        /*public string GetObjectType()
        {
            return ObjectType;
        }*/
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
        abstract public NetworkInterface GetInterface();
        private static IObject FindLightFromName(string name)
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
        public string CompleteRegistration(string[] request)
        {
            string name = null;
            string dname = null;
            string[] friendlyNames = null;
            string description = null;
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
                    case "display_name":
                        dname = command[1];
                        break;
                    case "description":
                        description = command[1];
                        break;
                    case "setfriendlynames":
                        string names = command[1];
                        friendlyNames = names.Split(',');
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

            this.Brightness = 100;
            this.Switch = true;
            this.Value = 0;
            this.Name = name;
            this.DisplayName = dname;
            this.Description = description;
            this.FriendlyNames = friendlyNames;
            room.AddItem(this);
            HomeAutomationServer.server.Objects.Add(this);

            ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
            data.Object.light = this;
            return data.Json();
        }
        public static string APIRequest(string method, string[] request, Identity login)
        {
            if (method.Equals("changeBrightness") || method.Equals("changeIntensity") || method.Equals("changeValue"))
            {
                WLight light = null;
                uint W = 0;
                int dimmer = 0;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = (WLight)FindLightFromName(command[1]);
                            break;
                        case "W":
                            W = uint.Parse(command[1]);
                            break;
                        case "value":
                            W = uint.Parse(command[1]);
                            break;
                        case "dimmer":
                            dimmer = int.Parse(command[1]);
                            break;
                    }
                }
                if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                if (!login.HasAccess(light)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                light.Set(W, dimmer);
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
                            light = (WLight)FindLightFromName(command[1]);
                            break;
                        case "switch":
                            status = bool.Parse(command[1]);
                            break;
                    }
                }
                if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
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
                            light = (WLight)FindLightFromName(command[1]);
                            break;
                        case "percentage":
                            dimm_percentage = byte.Parse(command[1]);
                            break;
                        case "dimmer":
                            dimmer = int.Parse(command[1]);
                            break;
                    }
                }
                if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                if (!login.HasAccess(light)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                light.Dimm(dimm_percentage, dimmer);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("create"))
            {
                return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST, "This is an abstract device and can only be implemented").Json();
            }
            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED).Json();
        }
        public static void Initialize(WLight light, dynamic device)
        {
            light.Name = device.Name;
            light.DisplayName = device.DisplayName;
            light.FriendlyNames = Array.ConvertAll(((List<object>)device.FriendlyNames).ToArray(), x => x.ToString());
            light.Description = device.Description;
            light.Switch = device.Switch;
            light.Value = (uint)device.Value;

            //HomeAutomationServer.server.Objects.Add(light);
            //room.AddItem(light);
        }
    }
}
