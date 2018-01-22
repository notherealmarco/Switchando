using HomeAutomation.Dictionaries;
using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects;
using HomeAutomation.Objects.Lights;
using HomeAutomation.Objects.Switches;
using HomeAutomation.Users;
using HomeAutomationCore;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HomeAutomation.Rooms
{
    public class DeviceGroup : ISwitch
    {
        public string Name;
        public string Description = "";
        public string[] FriendlyNames;
        public List<string> Objects;
        public bool Hidden;
        public bool Switch;
        public string ObjectType = "DEVICE_GROUP";
        public string ObjectModel = "SWITCH";
        public string ClientName = "local";
        public DeviceGroup(string name, string description, string[] friendlyNames, bool hidden)
        {
            this.Hidden = hidden;
            this.Name = name;
            this.FriendlyNames = friendlyNames;
            this.Description = description;
            this.Objects = new List<string>();
            HomeAutomationServer.server.Objects.Add(this);
            //HomeAutomationServer.server.Objects.Add(this);

            /*foreach (NetworkInterface netInt in HomeAutomationServer.server.NetworkInterfaces)
            {
                if (netInt.Id.Equals("device_group")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("device_group", requestHandler);*/
        }
        public void AddItem(ISwitch homeAutomationObject)
        {
            this.Objects.Add(homeAutomationObject.GetName());
        }

        public void SwitchGroup(bool status)
        {
            foreach (string sitem in Objects)
            {
                IObject oitem = ObjectFactory.FromString(sitem);
                if (oitem is ISwitch)
                {
                    ISwitch item = (ISwitch)oitem;
                    if (status) ((ISwitch)item).Start(); else ((ISwitch)item).Stop();
                    Thread.Sleep(200);
                }
            }
        }
        public void Start()
        {
            SwitchGroup(true);
        }
        public void Stop()
        {
            SwitchGroup(false);
        }
        public bool IsOn()
        {
            return Switch;
        }
        public string GetName()
        {
            return Name;
        }
        public NetworkInterface GetInterface()
        {
            return NetworkInterface.FromId(ObjectType);
        }
        public string GetObjectType()
        {
            return "DEVICE_GROUP";
        }
        public string GetObjectModel()
        {
            return "";
        }
        public string[] GetFriendlyNames()
        {
            return FriendlyNames;
        }
        public void Color(uint R, uint G, uint B, int dimmer)
        {
            //HomeAutomationServer.server.Telegram.Log("Changing color of room `" + this.Name + "`.");
            foreach (string sitem in Objects)
            {
                IObject item = ObjectFactory.FromString(sitem);
                if (item is IColorableLight)
                {
                    ((IColorableLight)item).Set(R, G, B, dimmer);
                    Thread.Sleep(200);
                }
            }
        }
        public void Dimm(uint percentace, int dimmer)
        {
            //HomeAutomationServer.server.Telegram.Log("Dimming room `" + this.Name + "` to `" + percentace + "%`" + "(" + dimmer + "ms).");
            foreach (string sitem in Objects)
            {
                IObject item = ObjectFactory.FromString(sitem);
                if (item is ILight)
                {
                    ((ILight)item).Dimm(percentace, dimmer);
                    Thread.Sleep(200);
                }
            }
        }
        private static DeviceGroup FindGroupFromName(string name)
        {
            DeviceGroup myobj = null;
            foreach (IObject obj in HomeAutomationServer.server.Objects)
            {
                if (obj.GetName().ToLower().Equals(name.ToLower()))
                {
                    myobj = (DeviceGroup)obj;
                    break;
                }
                if (obj.GetFriendlyNames() == null) continue;
                if (Array.IndexOf(obj.GetFriendlyNames(), name.ToLower()) > -1)
                {
                    myobj = (DeviceGroup)obj;
                    break;
                }
            }
            return myobj;
        }
        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("createGroup"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                string[] friendlyNames = null;
                string description = null;
                bool hidden = false;
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
                        case "hidden":
                            hidden = bool.Parse(command[1]);
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
                DeviceGroup relay = new DeviceGroup(name, description, friendlyNames, hidden);
                room.AddItem(relay);
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.relay = relay;
                return data.Json();
            }
            if (method.Equals("addDevice"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                string device = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "objname":
                            name = command[1];
                            break;
                        case "device":
                            device = command[1];
                            break;
                    }
                }
                if (name == null || device == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Device not found").Json();
                IObject dvcgrp = ObjectFactory.FromString(name);
                if (dvcgrp is DeviceGroup)
                {
                    DeviceGroup dc = (DeviceGroup)dvcgrp;
                    IObject dvc = ObjectFactory.FromString(device);
                    if (dvc is ISwitch)
                    {
                        ISwitch toAdd = (ISwitch)dvc;
                        dc.AddItem(toAdd);
                        return new ReturnStatus(CommonStatus.SUCCESS).Json();
                    }
                    else return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST, "Device is not switchable").Json();
                }
            }
            if (method.Equals("changeColor/RGB"))
            {
                DeviceGroup room = null;
                uint R = 0;
                uint G = 0;
                uint B = 0;
                int dimmer = 0;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            room = FindGroupFromName(command[1]);
                            break;
                        case "R":
                            R = uint.Parse(command[1]);
                            break;
                        case "G":
                            G = uint.Parse(command[1]);
                            break;
                        case "B":
                            B = uint.Parse(command[1]);
                            break;
                        case "dimmer":
                            dimmer = int.Parse(command[1]);
                            break;
                    }
                    if (room == null) return "ADD ERROR API";
                }
                if (!login.HasAccess(room)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                room.Color(R, G, B, dimmer);
                return "ADD STATUS API";
            }

            if (method.Equals("changeColor/name"))
            {
                return "NOT IMPLEMENTED";
            }

            if (method.Equals("switch"))
            {
                DeviceGroup room = null;
                bool status = false;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            room = FindGroupFromName(command[1]);
                            break;
                        case "switch":
                            status = bool.Parse(command[1]);
                            break;
                    }
                    if (room == null) return "ADD ERROR API";
                }
                if (!login.HasAccess(room)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                room.SwitchGroup(status);
                return "";
            }
            if (method.Equals("dimm"))
            {
                DeviceGroup room = null;
                byte dimm_percentage = 255;
                int dimmer = 0;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            room = FindGroupFromName(command[1]);
                            break;
                        case "percentage":
                            dimm_percentage = byte.Parse(command[1]);
                            break;
                        case "dimmer":
                            dimmer = int.Parse(command[1]);
                            break;
                    }
                    if (room == null) return "ADD ERROR API";
                }
                if (!login.HasAccess(room)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                room.Dimm(dimm_percentage, dimmer);
                return "";
            }

            if (string.IsNullOrEmpty(method))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                DeviceGroup room = null;
                uint R = 0;
                uint G = 0;
                uint B = 0;
                int dimmer = 0;
                string color = null;
                uint dimm_percentage = 400;
                string status = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "objname":
                            foreach (DeviceGroup obj in HomeAutomationServer.server.Objects)
                            {
                                if (obj.Name.ToLower().Equals(command[1].ToLower()))
                                {
                                    room = (DeviceGroup)obj;
                                }
                                if (Array.IndexOf(obj.FriendlyNames, command[1].ToLower()) > -1)
                                {
                                    room = (DeviceGroup)obj;
                                }
                            }
                            break;

                        case "R":
                            R = uint.Parse(command[1]);
                            break;

                        case "G":
                            G = uint.Parse(command[1]);
                            break;

                        case "B":
                            B = uint.Parse(command[1]);
                            break;

                        case "dimmer":
                            dimmer = int.Parse(command[1]);
                            break;

                        case "color":
                            color = command[1];
                            break;

                        case "percentage":
                            dimm_percentage = uint.Parse(command[1]);
                            break;

                        case "switch":
                            status = command[1];
                            break;
                    }
                }
                if (status != null)
                {
                    room.SwitchGroup(bool.Parse(status));
                    return "";
                }
                if (color != null)
                {
                    uint[] vls = ColorConverter.ConvertNameToRGB(color);
                    room.Color(vls[0], vls[1], vls[2], dimmer);
                    return "";
                }
                if (dimm_percentage != 400)
                {
                    room.Dimm(dimm_percentage, dimmer);
                    return "";
                }
                room.Color(R, G, B, dimmer);
            }
            return "";
        }
        public static void Setup(Room room, dynamic device)
        {
            DeviceGroup light = new DeviceGroup(device.Name, device.Description, Array.ConvertAll(((List<object>)device.FriendlyNames).ToArray(), x => x.ToString()), device.Hidden);
            light.Switch = device.Switch;
            foreach (string dvc in device.Objects)
            {
                light.Objects.Add(dvc);
            }
            room.AddItem(light);
        }
    }
}