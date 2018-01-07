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
        public string[] FriendlyNames;
        public List<ISwitch> Objects;
        public bool Hidden;
        public bool Switch;
        public string ObjectType = "DEVICE_GROUP";
        public string ObjectModel = "SWITCH";
        public DeviceGroup(string name, string[] friendlyNames, bool hidden)
        {
            this.Hidden = hidden;
            this.Name = name;
            this.FriendlyNames = friendlyNames;
            this.Objects = new List<ISwitch>();
            //HomeAutomationServer.server.Objects.Add(this);

            foreach (NetworkInterface netInt in HomeAutomationServer.server.NetworkInterfaces)
            {
                if (netInt.Id.Equals("device_group")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("device_group", requestHandler);
        }
        public void AddItem(ISwitch homeAutomationObject)
        {
            this.Objects.Add(homeAutomationObject);

            foreach(ISwitch obj in Objects)
            {
                
            }
        }

        public void SwitchGroup(bool status)
        {
            foreach (ISwitch item in Objects)
            {
                if (status) ((ISwitch)item).Start(); else ((ISwitch)item).Stop();
                Thread.Sleep(200);
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
            return NetworkInterface.FromId("device_group");
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
            foreach (ISwitch item in Objects)
            {
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
            foreach (ISwitch item in Objects)
            {
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
    }
}