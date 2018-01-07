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
    class SwitchButton : IObject
    {
        Client Client;
        public string ClientName;

        public string Name;

        public uint Pin;
        bool Status;
        bool IsRemote;

        Timer timer;

        public List<string> CommandsOn;
        public List<string> CommandsOff;

        public List<string> ActionsOn;
        public List<string> ActionsOff;

        public List<string> Objects;

        public string ObjectType = "SWITCH_BUTTON";
        public SwitchButton(Client client, string name, bool isRemote)
        {
            new SwitchButton(client, name, 0, IsRemote);
        }
        public SwitchButton(Client client, string name, uint pin, bool isRemote)
        {
            this.Client = client;
            this.ClientName = client.Name;
            this.Pin = pin;
            this.Name = name;
            this.IsRemote = isRemote;

            this.CommandsOn = new List<string>();
            this.CommandsOff = new List<string>();

            this.ActionsOn = new List<string>();
            this.ActionsOff = new List<string>();

            this.Objects = new List<string>();

            if (!isRemote)
            {
                if (Client.Name.Equals("local"))
                {
                    PIGPIO.set_pull_up_down(0, this.Pin, 2);
                    Console.WriteLine("PUD-UP was set on GPIO" + this.Pin);

                    timer = new Timer();

                    timer.Elapsed += Tick;
                    timer.Interval = 200;
                    timer.Start();
                }
            }
            HomeAutomationServer.server.Objects.Add(this);
        }
        public void SetClient(Client client)
        {
            this.Client = client;
        }

        public void AddCommand(string command, bool onoff)
        {
            if (onoff)
            {
                CommandsOn.Add(command);
            }
            else
            {
                CommandsOff.Add(command);
            }
        }
        public void RemoveCommand(string command, bool onoff)
        {
            if (onoff)
            {
                CommandsOn.Remove(command);
            }
            else
            {
                CommandsOff.Remove(command);
            }
        }
        public void AddAction(string action, bool onoff)
        {
            if (onoff)
            {
                ActionsOn.Add(action);
            }
            else
            {
                ActionsOff.Add(action);
            }
        }
        public void RemoveAction(string command, bool onoff)
        {
            if (onoff)
            {
                ActionsOn.Remove(command);
            }
            else
            {
                ActionsOff.Remove(command);
            }
        }
        public void RemoveObject(ISwitch obj)
        {
            Objects.Remove(obj.GetName());
        }
        public void AddObject(ISwitch obj)
        {
            Objects.Add(obj.GetName());
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

        private void Tick(object sender, ElapsedEventArgs args)
        {
            int currentStatus = PIGPIO.gpio_read(0, Pin);
            bool lStatus;
            if (currentStatus == 1) lStatus = false; else lStatus = true;

            if (lStatus == this.Status) return;
            else
            {
                this.Status = lStatus;
                StatusChanged(lStatus);
            }
        }
        public void StatusChanged(bool value)
        {
            Console.WriteLine(this.Name + " status: " + this.Status);
            if (value)
            {
                foreach (string command in CommandsOn)
                {
                    /*var message = command.Replace(",,,", "&");
                    message = message.Replace(",,", "=");
                    string[] commands = message.Split('&');

                    string[] icommand = commands[0].Split('=');
                    if (icommand[0].Equals("interface"))
                    {
                        foreach (NetworkInterface networkInterface in HomeAutomationServer.server.NetworkInterfaces)
                        {
                            if (networkInterface.Id.Equals(icommand[1]))
                            {
                                networkInterface.Run(commands);
                            }
                        }
                    }*/
                    //TODO execute command
                }
                foreach (string actionRaw in ActionsOn)
                {
                    ObjectInterfaces.Action action = ObjectInterfaces.Action.FromName(actionRaw);
                    action.Run(Identity.GetAdminUser());
                }
                List<ISwitch> objectsList = new List<ISwitch>();
                foreach (IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (this.Objects.Contains(iobj.GetName()))
                    {
                        objectsList.Add((ISwitch)iobj);
                    }
                }
                foreach (ISwitch iobj in objectsList)
                {
                    iobj.Start();
                }
            }
            else
            {
                foreach (string command in CommandsOff)
                {
                    /*var message = command.Replace(",,,", "&");
                    message = message.Replace(",,", "=");
                    string[] commands = message.Split('&');

                    string[] icommand = commands[0].Split('=');
                    if (icommand[0].Equals("interface"))
                    {
                        foreach (NetworkInterface networkInterface in HomeAutomationServer.server.NetworkInterfaces)
                        {
                            if (networkInterface.Id.Equals(icommand[1]))
                            {
                                networkInterface.Run(commands);
                            }
                        }
                    }*/
                    //TODO execute command
                }
                foreach (string actionRaw in ActionsOff)
                {
                    ObjectInterfaces.Action action = ObjectInterfaces.Action.FromName(actionRaw);
                    action.Run(Identity.GetAdminUser());
                }
                List<ISwitch> objectsList = new List<ISwitch>();
                foreach (IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (this.Objects.Contains(iobj.GetName()))
                    {
                        objectsList.Add((ISwitch)iobj);
                    }
                }
                foreach (ISwitch iobj in objectsList)
                {
                    iobj.Stop();
                }
            }
        }
        public string GetName()
        {
            return this.Name;
        }
        public string GetObjectType()
        {
            return "SWITCH_BUTTON";
        }
        public NetworkInterface GetInterface()
        {
            return NetworkInterface.FromId(ObjectType);
        }
        public string[] GetFriendlyNames()
        {
            return new string[0];
        }
        private static SwitchButton FindSwitchButtonFromName(string name)
        {
            SwitchButton myobj = null;
            foreach (IObject obj in HomeAutomationServer.server.Objects)
            {
                if (obj.GetName().ToLower().Equals(name.ToLower()))
                {
                    myobj = (SwitchButton)obj;
                    break;
                }
                if (obj.GetFriendlyNames() == null) continue;
                if (Array.IndexOf(obj.GetFriendlyNames(), name.ToLower()) > -1)
                {
                    myobj = (SwitchButton)obj;
                    break;
                }
            }
            return myobj;
        }
        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("click"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                SwitchButton switch_button = null;
                bool status = false;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            switch_button = FindSwitchButtonFromName(command[1]);
                            break;
                        case "switch":
                            status = bool.Parse(command[1]);
                            break;
                    }
                }
                if (switch_button == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                switch_button.StatusChanged(status);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }

            if (method.Equals("createSwitchButton"))
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
                if (room == null) if (client == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Room not found").Json();
                SwitchButton button = new SwitchButton(client, name, pin, isRemote);
                room.AddItem(button);
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.button = button;
                return data.Json();
            }
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

                SwitchButton button = null;
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
                    if (iobj.GetObjectType() == "SWITCH_BUTTON")
                    {
                        if (iobj.GetName().Equals(name))
                        {
                            button = (SwitchButton)iobj;
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
                string objOn = null;
                string objOff = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "objname":
                            name = command[1];
                            break;

                        case "action_on":
                            objOn = command[1];
                            break;
                        case "action_off":
                            objOff = command[1];
                            break;
                    }
                }
                if (name == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (objOff == null && objOff == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                //if (objOff == null && objOff == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                SwitchButton button = null;
                ObjectInterfaces.Action actionOn = null;
                ObjectInterfaces.Action actionOff = null;
                bool on = false;
                bool off = false;

                foreach (ObjectInterfaces.Action iobj in HomeAutomationServer.server.Actions)
                {
                    if (objOn != null)
                    {
                        if (iobj.Name.Equals(objOn))
                        {
                            actionOn = iobj;
                            on = true;
                        }
                    }
                    if (objOff != null)
                    {
                        if (iobj.Name.Equals(objOff))
                        {
                            actionOff = iobj;
                            off = false;
                        }
                    }
                }
                foreach (IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (iobj.GetObjectType() == "SWITCH_BUTTON")
                    {
                        if (iobj.GetName().Equals(name))
                        {
                            button = (SwitchButton)iobj;
                        }
                    }
                }
                if (button == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, name + " not found").Json();
                if (on && actionOn == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, objOn + " not found").Json();
                if (off && actionOff == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, objOff + " not found").Json();

                if (on) button.AddAction(actionOn.Name, true);
                if (off) button.AddAction(actionOn.Name, true);
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.button = button;
                return data.Json();
            }
            if (method.Equals("addCommand"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                string cmdOn = null;
                string cmdOff = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "objname":
                            name = command[1];
                            break;

                        case "command_on":
                            cmdOn = command[1];
                            break;
                        case "command_off":
                            cmdOff = command[1];
                            break;
                    }
                }
                if (cmdOn == null && cmdOff == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                foreach (IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (iobj.GetObjectType() == "SWITCH_BUTTON")
                    {
                        if (iobj.GetName().Equals(name))
                        {
                            SwitchButton button = (SwitchButton)iobj;
                            if (cmdOn != null) button.AddCommand(cmdOn, true);
                            if (cmdOff != null) button.AddCommand(cmdOff, true);

                            ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                            data.Object.button = button;
                            return data.Json();
                        }
                    }
                }
                return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, name + " not found").Json();
            }


            if (string.IsNullOrEmpty(method))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                SwitchButton button = null;
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
                                    button = (SwitchButton)obj;
                                }
                            }
                            break;

                        case "switch":
                            if (command[1].ToLower().Equals("true"))
                            {
                                button.StatusChanged(true);
                            }
                            else
                            {
                                button.StatusChanged(false);
                            }
                            break;
                    }
                }
            }
            return "";
        }
        public static void Setup(Room room, dynamic device)
        {
            SwitchButton button = new SwitchButton(device.Client, device.Name, (uint)device.Pin, device.IsRemote);
            foreach (string command in device.CommandsOn)
            {
                button.AddCommand(command, true);
            }
            foreach (string command in device.CommandsOff)
            {
                button.AddCommand(command, false);
            }
            foreach (string objectName in device.Objects)
            {
                button.AddObject(objectName);
            }
            foreach (string action in device.ActionsOn)
            {
                button.AddAction(action, true);
            }
            foreach (string action in device.ActionsOff)
            {
                button.AddAction(action, false);
            }
            button.ClientName = device.Client.Name;
            button.SetClient(device.Client);
            room.AddItem(button);
        }
    }
}