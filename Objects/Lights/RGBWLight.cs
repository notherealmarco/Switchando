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
    public abstract class RGBWLight : IColorableLight
    {
        public RGBWType Type { get; set; }
        public uint ValueR { get; set; }
        public uint ValueG { get; set; }
        public uint ValueB { get; set; }
        public uint ValueW { get; set; }
        public uint Brightness { get; set; }
        public bool HUEMode { get; set; }
        public byte Ambiance { get; set; }

        public bool Switch { get; set; }

        public string Name;
        public string DisplayName;
        public string[] FriendlyNames;
        public string Description;

        public bool nolog = false;

        //public string ObjectType = "A_RGBW_LIGHT";
        public string ObjectModel = "COLOR_AMBIANCE_LIGHT";

        public string ClientName = "local"; //Compatibility with older versions

        public void Create(string Name, string DisplayedName, string Description, string[] FriendlyNames, RGBWType type)
        {
            this.Brightness = 100;
            this.Switch = true;

            this.ValueR = 0;
            this.ValueG = 0;
            this.ValueB = 0;
            this.ValueW = 255;
            this.HUEMode = false;
            this.Ambiance = 255;

            this.Name = Name;
            this.Type = type;
            this.DisplayName = DisplayedName;
            this.Description = Description;
            this.FriendlyNames = FriendlyNames;
        }
        public void SetMode(string mode)
        {
            if (mode.ToLower().Equals("hue"))
            {
                HUEMode = true;
                Set(ValueR, ValueG, ValueB, ValueW, 300);
                Brightness = 100;
            }
            else
            {
                HUEMode = false;
                SetAmbiance(Ambiance);
                //MyMessage.Send(NodeID, Actuator, Command.SET, true, (byte)SetReq.V_PERCENTAGE, Brightness.ToString());
            }
        }
        public void SetAmbiance(byte amb)
        {
            uint r = 255, g = 255, b = 255, w = 255;

            if (Type == RGBWType.RGBA || Type == RGBWType.RGBWW)
            {
                if (amb == 127)
                {
                    Set(255, 255, 255, 255, 300);
                    Ambiance = amb;
                    return;
                }
                if (amb > 127)
                {
                    var diff = amb - 127;
                    r -= ((uint)diff * 2) - 1;
                    g -= ((uint)diff * 2) - 1;
                    b -= ((uint)diff * 2) - 1;
                    w = 255;
                    Set(r, g, b, w, 300);
                    Ambiance = amb;
                    return;
                }
                if (amb < 127)
                {
                    var diff = 127 - amb;
                    w -= ((uint)diff * 2) + 1;
                    r = 255;
                    g = 255;
                    b = 255;
                    Set(r, g, b, w, 300);
                    Ambiance = amb;
                    return;
                }
            }
            else if (Type == RGBWType.RGBW)
            {
                //not implemented (yet)
            }
        }
        public void Set(uint ValueR, uint ValueG, uint ValueB, uint ValueW, int DimmerIntervals)
        {
            UpdateStatus(ValueR, ValueG, ValueB, ValueW, DimmerIntervals);
            if (HUEMode)
            {
                this.ValueR = ValueR;
                this.ValueG = ValueG;
                this.ValueB = ValueB;
                this.ValueW = ValueW;
            }
        }
        public void Set(uint ValueR, uint ValueG, uint ValueB, int DimmerIntervals)
        {
            byte w = GetWhite(ValueR, ValueG, ValueB);
            UpdateStatus(ValueR, ValueG, ValueB, w, DimmerIntervals);
            if (HUEMode)
            {
                this.ValueR = ValueR;
                this.ValueG = ValueG;
                this.ValueB = ValueB;
                this.ValueW = w;
            }
        }
        abstract public void UpdateStatus(uint ValueR, uint ValueG, uint ValueB, uint ValueW, int DimmerIntervals);
        public byte GetWhite(uint Ri, uint Gi, uint Bi)
        {
            float tM = Math.Max(Ri, Math.Max(Gi, Bi));
            if (tM == 0) return 0;
            float multiplier = 255.0f / tM;
            float hR = Ri * multiplier;
            float hG = Gi * multiplier;
            float hB = Bi * multiplier;
            float M = Math.Max(hR, Math.Max(hG, hB));
            float m = Math.Min(hR, Math.Min(hG, hB));
            float Luminance = ((M + m) / 2.0f - 127.5f) * (255.0f / 127.5f) / multiplier;
            return Convert.ToByte(Luminance);
        }
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
        public double[] GetValues()
        {
            return new double[3] { ValueR, ValueG, ValueB };
        }
        abstract public NetworkInterface GetInterface();
        private static IObject FindLightFromName(string name)
        {
            RGBWLight light = null;
            foreach (IObject obj in HomeAutomationServer.server.Objects)
            {
                if (obj.GetName().ToLower().Equals(name.ToLower()))
                {
                    light = (RGBWLight)obj;
                    break;
                }
                if (obj.GetFriendlyNames() == null) continue;
                if (Array.IndexOf(obj.GetFriendlyNames(), name.ToLower()) > -1)
                {
                    light = (RGBWLight)obj;
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
            string rgbwtype = null;
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
                    case "rgbw_type":
                        rgbwtype = command[1];
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
            RGBWType type = (RGBWType)Enum.Parse(typeof(RGBWType), rgbwtype);

            this.Brightness = 100;
            this.Switch = true;
            this.ValueR = 0;
            this.ValueG = 0;
            this.ValueB = 0;
            this.ValueW = 255;
            this.HUEMode = false;
            this.Ambiance = 255;
            this.Name = name;
            this.Type = type;
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
            if (method.Equals("changeColor/RGB"))
            {
                RGBWLight light = null;
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
                            light = (RGBWLight)FindLightFromName(command[1]);
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
                }
                if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                if (!login.HasAccess(light)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                light.Set(R, G, B, dimmer);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("changeColor/RGBW"))
            {
                RGBWLight light = null;
                uint R = 0;
                uint G = 0;
                uint B = 0;
                uint W = 0;
                int dimmer = 0;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = (RGBWLight)FindLightFromName(command[1]);
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
                        case "W":
                            B = uint.Parse(command[1]);
                            break;
                        case "dimmer":
                            dimmer = int.Parse(command[1]);
                            break;
                    }
                }
                if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                if (!login.HasAccess(light)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                light.Set(R, G, B, W, dimmer);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("switch"))
            {
                RGBWLight light = null;
                bool status = false;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = (RGBWLight)FindLightFromName(command[1]);
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
            if (method.Equals("setColorMode"))
            {
                RGBWLight light = null;
                string cmode = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = (RGBWLight)FindLightFromName(command[1]);
                            break;
                        case "color_mode":
                            cmode = command[1];
                            break;
                        case "mode":
                            cmode = command[1];
                            break;
                    }
                }
                if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                if (!login.HasAccess(light)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                light.SetMode(cmode);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("setWhiteTemperature") || method.Equals("setAmbiance"))
            {
                RGBWLight light = null;
                byte temp = 255;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = (RGBWLight)FindLightFromName(command[1]);
                            break;
                        case "temperature":
                            temp = byte.Parse(command[1]);
                            break;
                        case "white_temperature":
                            temp = byte.Parse(command[1]);
                            break;
                        case "ambiance":
                            temp = byte.Parse(command[1]);
                            break;
                        case "white_ambiance":
                            temp = byte.Parse(command[1]);
                            break;
                    }
                }
                if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                if (!login.HasAccess(light)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                light.SetAmbiance(temp);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("dimm"))
            {
                RGBWLight light = null;
                byte dimm_percentage = 255;
                int dimmer = 0;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = (RGBWLight)FindLightFromName(command[1]);
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
        public static void Initialize(RGBWLight light, dynamic device)
        {
            light.Name = device.Name;
            light.DisplayName = device.DisplayName;
            light.FriendlyNames = Array.ConvertAll(((List<object>)device.FriendlyNames).ToArray(), x => x.ToString());
            light.Description = device.Description;
            light.Switch = device.Switch;
            light.ValueR = (uint)device.ValueR;
            light.ValueG = (uint)device.ValueG;
            light.ValueB = (uint)device.ValueB;
            light.ValueW = (uint)device.ValueW;
            light.Ambiance = (byte)device.Ambiance;
            light.HUEMode = (bool)device.HUEMode;

            //HomeAutomationServer.server.Objects.Add(light);
            //room.AddItem(light);
        }
    }
}
