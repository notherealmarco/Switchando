using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects;
using HomeAutomation.Objects.Blinds;
using HomeAutomation.Objects.External;
using HomeAutomation.Objects.Fans;
using HomeAutomation.Objects.Inputs;
using HomeAutomation.Objects.Lights;
using HomeAutomation.Objects.Switches;
using HomeAutomation.Rooms;
using HomeAutomation.Users;
using HomeAutomationCore;
using HomeAutomationCore.Client;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace HomeAutomation.ConfigRetriver
{
    public class ConfigRetriver
    {
        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "no permissions").Json();
            switch (method)
            {
                case "removeRoom":
                    if (RemoveRoom(request)) return new ReturnStatus(CommonStatus.SUCCESS).Json(); else return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();

                case "removeDevice":
                    if (RemoveObject(request)) return new ReturnStatus(CommonStatus.SUCCESS).Json(); else return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();

                case "removeClient":
                    if (RemoveClient(request)) return new ReturnStatus(CommonStatus.SUCCESS).Json(); else return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();

                case "updateFile":
                    return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED).Json();
        }
        private static bool RemoveRoom(string[] data)
        {
            string name = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "room":
                        name = command[1];
                        break;
                }
            }
            foreach (Room room in HomeAutomationServer.server.Rooms)
            {
                if (room.Name.Equals(name))
                {
                    HomeAutomationServer.server.Rooms.Remove(room);
                    return true;
                }
            }
            return false;
        }
        private static bool RemoveObject(string[] data)
        {
            string name = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "objname":
                        name = command[1];
                        break;
                }
            }
            foreach (Room room in HomeAutomationServer.server.Rooms)
            {
                foreach (IObject iobj in room.Objects)
                {
                    if (iobj.GetName().Equals(name))
                    {
                        room.Objects.Remove(iobj);
                    }
                }
            }
            foreach (IObject iobj in HomeAutomationServer.server.Objects)
            {
                if (iobj.GetName().Equals(name))
                {
                    HomeAutomationServer.server.Objects.Remove(iobj);
                    return true;
                }
            }
            return false;
        }
        private static bool RemoveClient(string[] data)
        {
            string name = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "client":
                        name = command[1];
                        break;
                }
            }
            foreach (Client client in HomeAutomationServer.server.Clients)
            {
                if (client.Name.Equals(name))
                {
                    HomeAutomationServer.server.Clients.Remove(client);
                    return true;
                }
            }
            return false;
        }
/*        private static void CreateRoom(string[] data)
        {
            string name = null;
            bool hiddenRoom = false;
            string[] friendlyNames = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "addroom":
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
                return;
            }
            Room room = new Room(name, friendlyNames, hiddenRoom);
        }
        private static void CreateLightRGB(string[] data)
        {
            string name = null;
            string[] friendlyNames = null;
            string description = null;

            uint pinR = 0;
            uint pinG = 0;
            uint pinB = 0;

            Client client = null;

            Room room = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "addlightrgb":
                        name = command[1];
                        break;
                    case "description":
                        description = command[1];
                        break;
                    case "setfriendlynames":
                        string names = command[1];
                        friendlyNames = names.Split(',');
                        break;

                    case "addpinr":
                        string pinr = command[1];
                        pinR = uint.Parse(pinr);
                        break;
                    case "addping":
                        string ping = command[1];
                        pinG = uint.Parse(ping);
                        break;
                    case "addpinb":
                        string pinb = command[1];
                        pinB = uint.Parse(pinb);
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
                        if (client == null) return;
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
            if (room == null) return;
            RGBLight light = new RGBLight(client, name, pinR, pinG, pinB, description, friendlyNames);
            room.AddItem(light);
        }
        private static void CreateLightW(string[] data)
        {
            string name = null;
            string[] friendlyNames = null;
            string description = null;

            uint pin = 0;

            Client client = null;

            Room room = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "addlightw":
                        name = command[1];
                        break;
                    case "description":
                        description = command[1];
                        break;
                    case "setfriendlynames":
                        string names = command[1];
                        friendlyNames = names.Split(',');
                        break;

                    case "addpin":
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
                        if (client == null) return;
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
            if (room == null) return;
            WLight light = new WLight(client, name, pin, description, friendlyNames);
            room.AddItem(light);
        }
        private static void CreateRelay(string[] data)
        {
            string name = null;
            string[] friendlyNames = null;
            string description = null;

            uint pin = 0;

            Client client = null;

            Room room = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "addrelay":
                        name = command[1];
                        break;
                    case "description":
                        description = command[1];
                        break;
                    case "setfriendlynames":
                        string names = command[1];
                        friendlyNames = names.Split(',');
                        break;

                    case "addpin":
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
                        if (client == null) return;
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
            if (room == null) return;
            Relay relay = new Relay(client, name, pin, description, friendlyNames);
            room.AddItem(relay);
        }
        private static void CreateWebRelay(string[] data)
        {
            string name = null;
            string id = null;
            string[] friendlyNames = null;
            string description = null;
            string createButton = "none";

            Room room = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "addwebrelay":
                        name = command[1];
                        break;
                    case "id":
                        id = command[1];
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
                    case "createbutton":
                        createButton = command[1];
                        break;
                }
            }
            if (room == null) return;
            WebRelay relay = new WebRelay(name, id, description, friendlyNames);
            room.AddItem(relay);
            if (createButton.Equals("button"))
            {
                relay.AddButton(room);
            }
            else if (createButton.Equals("switch_button"))
            {
                relay.AddSwitchButton(room);
            }
        }
        private static void CreateBlinds(string[] data)
        {
            string name = null;
            string[] friendlyNames = null;
            string description = null;
            Client client = null;
            Room room = null;

            uint openPin = 0;
            uint closePin = 0;

            byte totalsteps = 0;

            string webrelayOpenId = null;
            string webrelayCloseId = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "addblinds":
                        name = command[1];
                        break;
                    case "description":
                        description = command[1];
                        break;
                    case "setfriendlynames":
                        string names = command[1];
                        friendlyNames = names.Split(',');
                        break;

                    case "openpin":
                        string pinStr = command[1];
                        openPin = uint.Parse(pinStr);
                        break;
                    case "closepin":
                        pinStr = command[1];
                        closePin = uint.Parse(pinStr);
                        break;
                    case "webrelayopenid":
                        webrelayOpenId = command[1];
                        break;
                    case "webrelaycloseid":
                        webrelayCloseId = command[1];
                        break;

                    case "totalsteps":
                        totalsteps = byte.Parse(command[1]);
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
                        if (client == null) return;
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
            if (room == null) return;
            if (openPin == closePin && webrelayOpenId != null && webrelayCloseId != null)
            {
                WebRelay openRelay = new WebRelay(name + "_openrelay", webrelayOpenId, "", new string[0]);
                WebRelay closeRelay = new WebRelay(name + "_closerelay", webrelayCloseId, "", new string[0]);
                Blinds blinds = new Blinds(client, name, openRelay, closeRelay, totalsteps, description, friendlyNames);
                room.AddItem(blinds);
            }
            else
            {
                Relay openRelay = new Relay(client, name + "_openrelay", openPin, "", new string[0]);
                Relay closeRelay = new Relay(client, name + "_closerelay", closePin, "", new string[0]);
                Blinds blinds = new Blinds(client, name, openRelay, closeRelay, totalsteps, description, friendlyNames);
                room.AddItem(blinds);
            }
        }
        /*private static void CreateSimpleFan(string[] data)
        {
            string name = null;
            string[] friendlyNames = null;
            string description = null;

            uint pin = 0;

            Client client = null;

            Room room = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "addsimplefan":
                        name = command[1];
                        break;
                    case "description":
                        description = command[1];
                        break;
                    case "setfriendlynames":
                        string names = command[1];
                        friendlyNames = names.Split(',');
                        break;

                    case "addpin":
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
                        if (client == null) return;
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
            if (room == null) return;
            SimpleFan relay = new SimpleFan(client, name, pin, description, friendlyNames);
            room.AddItem(relay);
        }
        private static void CreateButton(string[] data) // a posto
        {
            string name = null;
            uint pin = 0;
            Client client = null;
            bool isRemote = false;

            Room room = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "addbutton":
                        name = command[1];
                        break;

                    case "addpin":
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
                        if (client == null) return;
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

                    case "remote":
                        isRemote = bool.Parse(command[1]);
                        break;
                }
            }
            if (room == null) return;
            Button button = new Button(client, name, pin, isRemote);
            room.AddItem(button);
        }
        private static void CreateSwitchButton(string[] data)
        {
            string name = null;
            uint pin = 0;
            Client client = null;
            bool isRemote = false;

            Room room = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "addswitchbutton":
                        name = command[1];
                        break;

                    case "addpin":
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
                        if (client == null) return;
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

                    case "remote":
                        isRemote = bool.Parse(command[1]);
                        break;
                }
            }
            if (room == null) return;
            SwitchButton button = new SwitchButton(client, name, pin, isRemote);
            room.AddItem(button);
        }
        private static void SwitchButtonAddCommand(string[] data)
        {
            string name = null;
            bool type = false;
            string newCmd = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "switchbuttonaddcommand":
                        name = command[1];
                        break;

                    case "type":
                        string typeStr = command[1];
                        if (typeStr.ToLower().Equals("on")) type = true; else type = false;
                        break;

                    case "command":
                        newCmd = command[1];
                        break;
                }
            }

            newCmd = newCmd.Replace(",,,", "&");
            newCmd = newCmd.Replace(",,", "=");

            foreach (IObject iobj in HomeAutomationServer.server.Objects)
            {
                if (iobj.GetObjectType() == "SWITCH_BUTTON")
                {
                    if (iobj.GetName().Equals(name))
                    {
                        SwitchButton button = (SwitchButton)iobj;
                        button.AddCommand(newCmd, type);
                    }
                }
            }
        }
        private static void ButtonAddCommand(string[] data)
        {
            string name = null;
            string newCmd = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "buttonaddcommand":
                        name = command[1];
                        break;

                    case "command":
                        newCmd = command[1];
                        break;
                }
            }

            foreach (IObject iobj in HomeAutomationServer.server.Objects)
            {
                if (iobj.GetObjectType() == "BUTTON")
                {
                    if (iobj.GetName().Equals(name))
                    {
                        Button button = (Button)iobj;
                        button.AddCommand(newCmd);
                    }
                }
            }
        }
        private static void ButtonAddObject(string[] data)
        {
            string name = null;
            string obj = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "buttonaddobject":
                        name = command[1];
                        break;

                    case "object":
                        obj = command[1];
                        break;
                }
            }

            if (name == null || obj == null) return;

            Button button = null;
            ISwitch switchable = null;

            foreach (IObject iobj in HomeAutomationServer.server.Objects)
            {
                if (iobj is ISwitch)
                {
                    if (iobj.GetName().Equals(obj))
                    {
                        switchable = (ISwitch)iobj;
                    }
                }
                if (iobj.GetObjectType() == "BUTTON")
                {
                    if (iobj.GetName().Equals(name))
                    {
                        button = (Button)iobj;
                    }
                }
            }

            if (button == null || switchable == null) return;

            button.AddObject(switchable);
        }
        private static void SwitchButtonAddObject(string[] data)
        {
            string name = null;
            string obj = null;

            foreach (string cmd in data)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "switchbuttonaddobject":
                        name = command[1];
                        break;

                    case "object":
                        obj = command[1];
                        break;
                }
            }

            if (name == null || obj == null) return;

            SwitchButton button = null;
            ISwitch switchable = null;

            foreach (IObject iobj in HomeAutomationServer.server.Objects)
            {
                if (iobj is ISwitch)
                {
                    if (iobj.GetName().Equals(name))
                    {
                        switchable = (ISwitch)iobj;
                    }
                }
                if (iobj.GetObjectType() == "SWITCH_BUTTON")
                {
                    if (iobj.GetName().Equals(name))
                    {
                        button = (SwitchButton)iobj;
                    }
                }
            }

            if (button == null || switchable == null) return;

            button.AddObject(switchable);
        }*/
    }
}