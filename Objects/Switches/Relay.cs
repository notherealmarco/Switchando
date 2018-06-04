using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects;
using HomeAutomation.Objects.Switches;
using HomeAutomation.Rooms;
using HomeAutomation.Users;
using HomeAutomationCore;
using System;
using System.Collections.Generic;

namespace Switchando.Objects.Lights
{
    public abstract class Relay : ISwitch
    {
        public bool Switch { get; set; }

        public string Name;
        public string DisplayName;
        public string[] FriendlyNames;
        public string Description;

        public bool nolog = false;

        public string ObjectModel = "SWITCH";

        public string ClientName = "local"; //Compatibility with older versions

        public void Create(string Name, string DisplayedName, string Description, string[] FriendlyNames)
        {
            this.Switch = true;
            this.Name = Name;
            this.DisplayName = DisplayedName;
            this.Description = Description;
            this.FriendlyNames = FriendlyNames;
        }

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
        abstract public NetworkInterface GetInterface();
        private static IObject FindLightFromName(string name)
        {
            Relay light = null;
            foreach (IObject obj in HomeAutomationServer.server.Objects)
            {
                if (obj.GetName().ToLower().Equals(name.ToLower()))
                {
                    light = (Relay)obj;
                    break;
                }
                if (obj.GetFriendlyNames() == null) continue;
                if (Array.IndexOf(obj.GetFriendlyNames(), name.ToLower()) > -1)
                {
                    light = (Relay)obj;
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

            this.Switch = true;
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
            if (method.Equals("switch"))
            {
                Relay light = null;
                bool status = false;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = (Relay)FindLightFromName(command[1]);
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
            if (method.Equals("create"))
            {
                return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST, "This is an abstract device and can only be implemented").Json();
            }
            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED).Json();
        }
        public static void Initialize(Relay light, dynamic device)
        {
            light.Name = device.Name;
            light.DisplayName = device.DisplayName;
            light.FriendlyNames = Array.ConvertAll(((List<object>)device.FriendlyNames).ToArray(), x => x.ToString());
            light.Description = device.Description;
            light.Switch = device.Switch;

            //HomeAutomationServer.server.Objects.Add(light);
            //room.AddItem(light);
        }
    }
}
