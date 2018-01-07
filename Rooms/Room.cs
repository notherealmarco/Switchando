using HomeAutomation.Dictionaries;
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
    public class Room
    {
        public string Name;
        public string[] FriendlyNames;
        public List<IObject> Objects;
        public bool Hidden;
        public Room(string name, string[] friendlyNames, bool hidden)
        {
            this.Hidden = hidden;
            this.Name = name;
            this.FriendlyNames = friendlyNames;
            this.Objects = new List<IObject>();
            HomeAutomationServer.server.Rooms.Add(this);
        }
        public void AddItem(IObject homeAutomationObject)
        {
            this.Objects.Add(homeAutomationObject);
        }
        public void Switch(bool status, Identity login)
        {
            foreach (IObject item in Objects)
            {
                if (!login.HasAccess(item)) continue;
                if (item is ISwitch)
                {
                    if (status) ((ISwitch)item).Start(); else ((ISwitch)item).Stop();
                }
                /*//Console.WriteLine("switching_pre");
                if (item.GetObjectType().Equals("LIGHT_GPIO_RGB"))
                {
                    //Console.WriteLine("switching");
                    ((ILight)item).Pause(status);
                    Thread.Sleep(1000);
                 }
                else if (item.GetObjectType().Equals("GENERIC_SWITCH"))
                {
                    if (status) ((ISwitch)item).Start(); else ((ISwitch)item).Stop();
                }*/
            }
        }
        public void Color(uint R, uint G, uint B, int dimmer, Identity login)
        {
            foreach (IObject item in Objects)
            {
                if (!login.HasAccess(item)) continue;
                if (item.GetObjectType().Equals("LIGHT_GPIO_RGB"))
                {
                    ((IColorableLight)item).Set(R, G, B, dimmer);
                    Thread.Sleep(1000);
                }
            }
        }
        public void Dimm(uint percentace, int dimmer, Identity login)
        {
            foreach (IObject item in Objects)
            {
                if (!login.HasAccess(item)) continue;
                if (item.GetObjectType().Equals("LIGHT_GPIO_RGB") || item.GetObjectType().Equals("LIGHT_GPIO_W"))
                {
                    ((ILight)item).Dimm(percentace, dimmer);
                    Thread.Sleep(1000);
                }
            }
        }
        private static Room FindRoomFromName(string name)
        {
            Room room = null;
            foreach (Room obj in HomeAutomationServer.server.Rooms)
            {
                if (obj.Name.ToLower().Equals(name.ToLower()))
                {
                    room = (Room)obj;
                }
                if (Array.IndexOf(obj.FriendlyNames, name.ToLower()) > -1)
                {
                    room = (Room)obj;
                }
            }
            return room;
        }
        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("changeColor/RGB"))
            {
                Room room = null;
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
                            room = FindRoomFromName(command[1]);
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
                    if (room == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Room not found").Json();
                }
                room.Color(R, G, B, dimmer, login);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("changeColor/name"))
            {
                return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED, "Switch from color name is not implemented yet, but it's in queue").Json();
            }
            if (method.Equals("switch"))
            {
                Room room = null;
                bool status = false;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            room = FindRoomFromName(command[1]);
                            break;
                        case "switch":
                            status = bool.Parse(command[1]);
                            break;
                    }
                    if (room == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Room not found").Json();
                }
                room.Switch(status, login);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("dimm"))
            {
                Room room = null;
                byte dimm_percentage = 255;
                int dimmer = 0;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            room = FindRoomFromName(command[1]);
                            break;
                        case "percentage":
                            dimm_percentage = byte.Parse(command[1]);
                            break;
                        case "dimmer":
                            dimmer = int.Parse(command[1]);
                            break;
                    }
                    if (room == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Room not found").Json();
                }
                room.Dimm(dimm_percentage, dimmer, login);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("createRoom"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                bool hiddenRoom = false;
                string[] friendlyNames = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "objname":
                            name = command[1];
                            break;

                        case "setfriendlynames":
                            string names = command[1];
                            friendlyNames = names.Split(',');
                            break;

                        case "hiddenroom":
                            string hiddenroomString = command[1];
                            hiddenRoom = bool.Parse(hiddenroomString);
                            break;
                    }
                }
                Room editRoom = null;
                foreach (Room area in HomeAutomationServer.server.Rooms)
                {
                    if (area.Name.ToLower().Equals(name.ToLower()))
                    {
                        editRoom = area;
                    }
                }
                if (editRoom != null)
                {
                    editRoom.Name = name;
                    if (friendlyNames != null) editRoom.FriendlyNames = friendlyNames;
                    editRoom.Hidden = hiddenRoom;
                    ReturnStatus return_data = new ReturnStatus(CommonStatus.SUCCESS);
                    return_data.Object.room = editRoom;
                    return return_data.Json();
                }
                Room room = new Room(name, friendlyNames, hiddenRoom);
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.room = room;
                return data.Json();
            }
            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED).Json();
        }
    }
}