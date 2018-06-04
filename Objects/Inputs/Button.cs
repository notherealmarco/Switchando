using Homeautomation.GPIO;
using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects.Switches;
using HomeAutomation.Rooms;
using HomeAutomation.Users;
using HomeAutomationCore;
using HomeAutomationCore.Client;
using System;
using System.Collections.Generic;
using System.Timers;

namespace HomeAutomation.Objects.Inputs
{
    public abstract class Button : IObject
    {
        public string Name;
        bool Status;
        bool EmulatedSwitchStatus;
        public List<string> Commands;
        public List<string> Objects;
        public List<string> Actions;
        //public string ObjectType = "BUTTON";
        public string ObjectKind = "BUTTON";

        public void AddCommand(string command)
        {
            Commands.Add(command);
        }
        public void RemoveCommand(string command)
        {
            Commands.Remove(command);
        }
        public void AddObject(ISwitch obj)
        {
            if (!Objects.Contains(obj.GetName()))
                Objects.Add(obj.GetName());
        }
        public void RemoveObject(ISwitch obj)
        {
            if (Objects.Contains(obj.GetName()))
                Objects.Remove(obj.GetName());
        }
        public void AddAction(string action)
        {
            Actions.Add(action);
        }
        public void RemoveAction(string action)
        {
            Actions.Remove(action);
        }
        public void AddObject(string obj)
        {
            if (!Objects.Contains(obj))
                Objects.Add(obj);
        }
        public void RemoveObject(string obj)
        {
            if (Objects.Contains(obj))
                Objects.Remove(obj);
        }
        public void StatusChanged(bool value)
        {
            //Console.WriteLine(this.Name + " status: " + this.Status);
            if (value)
            {
                if (EmulatedSwitchStatus) EmulatedSwitchStatus = false; else EmulatedSwitchStatus = true;
                foreach (string command in Commands)
                {
                    var message = command.Replace("%EmulatedSwitchStatus%", EmulatedSwitchStatus.ToString());
                    message = message.Replace(",,,", "&");
                    message = message.Replace(",,", "=");
                    APICommand.Run(message);
                }
                foreach (string actionRaw in Actions)
                {
                    ObjectInterfaces.Action action = ObjectInterfaces.Action.FromName(actionRaw);
                    action.Run(Identity.GetAdminUser());
                }
                if (this.Objects.Count == 0) return;
                List<ISwitch> objectsList = new List<ISwitch>();
                foreach (IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (this.Objects.Contains(iobj.GetName()))
                    {
                        objectsList.Add((ISwitch)iobj);
                    }
                }
                bool allOn = true;
                bool allOff = true;
                foreach (ISwitch iobj in objectsList)
                {
                    if (iobj.IsOn())
                    {
                        allOff = false;
                    }
                    else
                    {
                        allOn = false;
                    }
                }
                if (allOn) EmulatedSwitchStatus = false;
                if (allOff) EmulatedSwitchStatus = true;

                foreach (ISwitch iobj in objectsList)
                {
                    if (EmulatedSwitchStatus)
                    {
                        iobj.Start();
                    }
                    else
                    {
                        iobj.Stop();
                    }
                }
            }
        }
        public string GetName()
        {
            return this.Name;
        }
        abstract public string GetObjectType();
        abstract public NetworkInterface GetInterface();
        public string[] GetFriendlyNames()
        {
            return new string[0];
        }
        private static Button FindButtonFromName(string name)
        {
            Button myobj = null;
            foreach (IObject obj in HomeAutomationServer.server.Objects)
            {
                if (obj.GetName().ToLower().Equals(name.ToLower()))
                {
                    myobj = (Button)obj;
                    break;
                }
                if (obj.GetFriendlyNames() == null) continue;
                if (Array.IndexOf(obj.GetFriendlyNames(), name.ToLower()) > -1)
                {
                    myobj = (Button)obj;
                    break;
                }
            }
            return myobj;
        }
        public static string SendParameters(string method, string[] request, Identity login)
        {
            /*if (method.Equals("createButton") || method.Equals("create"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                uint pin = 0;
                Client client = null;
                bool isRemote = false;

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

                        case "remote":
                            isRemote = bool.Parse(command[1]);
                            break;
                    }
                }
                if (room == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Room not found").Json();
                Button button = new Button(client, name, pin, isRemote);
                room.AddItem(button);

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.button = button;
                return data.Json();
            }*/
            if (method.Equals("addObject"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                string obj = null;

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
                            obj = command[1];
                            break;
                    }
                }
                if (name == null || obj == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                Button button = null;
                ISwitch device = null;

                foreach (IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (iobj is ISwitch)
                    {
                        if (iobj.GetName().Equals(obj))
                        {
                            device = (ISwitch)iobj;
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
                if (button == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, name + " not found").Json();
                if (button == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, obj + " not found").Json();

                button.AddObject(device);
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.button = button;
                return data.Json();
            }
            if (method.Equals("addAction"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                string obj = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "objname":
                            name = command[1];
                            break;

                        case "action":
                            obj = command[1];
                            break;
                    }
                }
                if (name == null || obj == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                Button button = null;
                ObjectInterfaces.Action action = null;

                foreach (ObjectInterfaces.Action iobj in HomeAutomationServer.server.Actions)
                {
                    if (iobj.Name.Equals(obj))
                    {
                        action = iobj;
                    }
                }
                foreach (IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (iobj.GetObjectType() == "BUTTON")
                    {
                        if (iobj.GetName().Equals(name))
                        {
                            button = (Button)iobj;
                        }
                    }
                }
                if (button == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, name + " not found").Json();
                if (button == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, obj + " not found").Json();

                button.AddAction(action.Name);
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.button = button;
                return data.Json();
            }
            if (method.Equals("addCommand"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                string newCmd = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "objname":
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

                            ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                            data.Object.button = button;
                            return data.Json();
                        }
                    }
                }
                return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, name + " not found").Json();
            }

            if (method.Equals("click"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                Button button = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            button = FindButtonFromName(command[1]);
                            break;
                    }
                }
                if (button == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                button.StatusChanged(true);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED).Json();
        }
        public static void Initialize(Button button, dynamic device)
        {
            button.Name = device.Name;
            foreach (string command in device.Commands)
            {
                button.AddCommand(command);
            }
            foreach (string objectName in device.Objects)
            {
                button.AddObject(objectName);
            }
            foreach (string action in device.Actions)
            {
                button.AddAction(action);
            }
        }
        public string CompleteRegistration(string[] request)
        {
            Room room = null;
            foreach (string cmd in request)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "objname":
                        this.Name = command[1];
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

            this.Commands = new List<string>();
            this.Objects = new List<string>();
            this.Actions = new List<string>();

            room.AddItem(this);
            HomeAutomationServer.server.Objects.Add(this);

            ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
            data.Object.button = this;
            return data.Json();
        }
    }
}