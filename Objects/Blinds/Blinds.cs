using HomeAutomation.Application.ConfigRetriver;
using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects.External;
using HomeAutomation.Objects.Fans;
using HomeAutomation.Objects.Switches;
using HomeAutomation.Rooms;
using HomeAutomation.Users;
using HomeAutomationCore;
using HomeAutomationCore.Client;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HomeAutomation.Objects.Blinds
{
    class Blinds : ISwitch
    {
        Client Client;
        public string ClientName;

        public string Name;
        public string Description;
        public string[] FriendlyNames;

        public ISwitch OpenDevice;
        public ISwitch CloseDevice;

        public string ObjectType = "BLINDS";
        public string ObjectModel = "BLINDS";

        public int TotalSteps;
        public int Step;
        Thread movingThread;
        bool isMoving;

        public Blinds()
        {
        }
        public Blinds(Client client, string name, ISwitch openDevice, ISwitch closeDevice, int totalSteps, string description, string[] friendlyNames)
        {
            this.Client = client;
            this.Name = name;
            this.OpenDevice = openDevice;
            this.CloseDevice = closeDevice;
            this.Description = description;
            this.FriendlyNames = friendlyNames;
            this.TotalSteps = totalSteps;
            this.ClientName = client.Name;

            //quella roba

            HomeAutomationServer.server.Objects.Add(this);
        }

        public void Start()
        {
            if (!Client.Name.Equals("local"))
            {
                Client.Sendata("BLINDS/switch?objname=" + this.Name + "&switch=true");
                return;
            }
            if (isMoving)
            {
                isMoving = false;
                return;
            }

            movingThread = new Thread(MovingThread);
            isMoving = true;
            movingThread.Start(0);
        }
        public void Stop()
        {
            if (!Client.Name.Equals("local"))
            {
                Client.Sendata("BLINDS/switch?objname=" + this.Name + "&switch=false");
                return;
            }
            if (isMoving)
            {
                isMoving = false;
                return;
            }

            movingThread = new Thread(MovingThread);
            isMoving = true;
            movingThread.Start(TotalSteps);
        }
        public void Move(int step)
        {
            //Console.WriteLine(step);
            if (!Client.Name.Equals("local"))
            {
                double d = (double)step / (double)TotalSteps;
                Console.WriteLine(d);
                int percentage = (int)Math.Round(d * 100d);
                Console.WriteLine(percentage);
                Client.Sendata("BLINDS/move?objname=" + this.Name + "&value=" + percentage);
                return;
            }
            if (isMoving)
            {
                isMoving = false;
                Thread.Sleep(1500);
            }

            movingThread = new Thread(MovingThread);
            isMoving = true;
            movingThread.Start(step);
        }
        void MovingThread(object data)
        {
            int step = (int)data;

            if (step < Step)
            {
                OpenDevice.Start();
                for (int pos = Step; pos >= step; pos--)
                {
                    Step = pos;
                    Thread.Sleep(1000);
                    if (isMoving == false)
                    {
                        OpenDevice.Stop();
                        return;
                    }
                }
                OpenDevice.Stop();
                isMoving = false;
            }
            else
            {
                CloseDevice.Start();
                for (int pos = Step; pos <= step; pos++)
                {
                    Step = pos;
                    Thread.Sleep(1000);
                    if (isMoving == false)
                    {
                        CloseDevice.Stop();
                        return;
                    }
                }
                CloseDevice.Stop();
                isMoving = false;
            }
        }
        public bool IsOn()
        {
            int percentage = Step / TotalSteps * 100;
            if (percentage <= 90) return false; else return true;
        }
        public string GetName()
        {
            return this.Name;
        }
        public string GetObjectType()
        {
            return "BLINDS";
        }
        public NetworkInterface GetInterface()
        {
            return NetworkInterface.FromId(ObjectType);
        }
        public string[] GetFriendlyNames()
        {
            return this.FriendlyNames;
        }
        private static Blinds FindBlindsFromName(string name)
        {
            Blinds blinds = null;
            foreach (IObject obj in HomeAutomationServer.server.Objects)
            {
                if (obj.GetName().ToLower().Equals(name.ToLower()))
                {
                    blinds = (Blinds)obj;
                    break;
                }
                if (obj.GetFriendlyNames() == null) continue;
                if (Array.IndexOf(obj.GetFriendlyNames(), name.ToLower()) > -1)
                {
                    blinds = (Blinds)obj;
                    break;
                }
            }
            return blinds;
        }
        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("switch"))
            {
                Blinds blinds = null;
                bool status = false;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            blinds = FindBlindsFromName(command[1]);
                            break;
                        case "switch":
                            status = bool.Parse(command[1]);
                            break;
                    }
                }
                if (blinds == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                if (!login.HasAccess(blinds)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                if (status) blinds.Start(); else blinds.Stop();
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }

            if (method.Equals("move"))
            {
                Blinds blinds = null;
                int percentage = 255;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            blinds = FindBlindsFromName(command[1]);
                            break;
                        case "value":
                            percentage = int.Parse(command[1]);
                            break;
                    }
                }
                if (blinds == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                double prestep = (percentage / 100d) * blinds.TotalSteps;
                int step = (int)Math.Round(prestep);
                if (!login.HasAccess(blinds)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                blinds.Move(step);

                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }

            if (method.Equals("internal/updateStep"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                Blinds blinds = null;
                int percentage = 255;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            blinds = FindBlindsFromName(command[1]);
                            break;
                        case "value":
                            percentage = int.Parse(command[1]);
                            break;
                    }
                }
                if (blinds == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                blinds.Step = percentage;

                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("createBlinds"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                string[] friendlyNames = null;
                string description = null;
                Client client = null;
                Room room = null;
                string openDevice = null;
                string closeDevice = null;
                int totalSteps = 0;

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
                        case "open_device":
                            openDevice = command[1];
                            break;
                        case "close_device":
                            closeDevice = command[1];
                            break;
                        case "totalsteps":
                            totalSteps = int.Parse(command[1]);
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
                if (name == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (description == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (openDevice == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (closeDevice == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (room == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (totalSteps == 0) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                IObject openObj = ObjectFactory.FromString(openDevice);
                IObject closeObj = ObjectFactory.FromString(closeDevice);

                if (openObj == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Open device was not found").Json();
                if (closeObj == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Close device was not found").Json();
                if (openObj is ISwitch) { } else return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Open device is not on/off switchable").Json();
                if (closeObj is ISwitch) { } else return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Close device is not on/off switchable").Json();

                /*if (((dynamic)openObj).ClientName != null && ((dynamic)closeObj).ClientName != null)
                {
                    if ((((dynamic)openObj).ClientName).Equals((((dynamic)closeObj).ClientName)))
                    {
                        client = Client.GetCreateClient((((dynamic)openObj).ClientName));
                    }
                    else
                    {
                        client = Client.GetCreateClient("local");
                        
                    }
                }
                else
                {
                    client = Client.GetCreateClient("local");
                }*/
                client = Client.GetCreateClient("local"); //tmp

                if (client == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST, "Internal error: client is null").Json();

                foreach (Room iRoom in HomeAutomationServer.server.Rooms)
                {
                    if (iRoom.Objects.Contains(openObj))
                    {
                        iRoom.Objects.Remove(openObj);
                    }
                    if (iRoom.Objects.Contains(closeObj))
                    {
                        iRoom.Objects.Remove(closeObj);
                    }
                }
                //HomeAutomationServer.server.Objects.Remove(openObj);
                //HomeAutomationServer.server.Objects.Remove(closeObj);

                Blinds blinds = new Blinds(client, name, (ISwitch)openObj, (ISwitch)closeObj, totalSteps, description, friendlyNames);
                room.AddItem(blinds);
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.blinds = blinds;
                return data.Json();
            }
            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED).Json();
        }
        public static void Setup(Room room, dynamic device)
        {
            string openDeviceId = device.OpenDevice.Name;
            string closeDeviceId = device.CloseDevice.Name;
            if (device.OpenDevice.ClientName != null)
            {
                device.OpenDevice.Client = Client.GetCreateClient(device.OpenDevice.ClientName);
            }
            if (device.CloseDevice.ClientName != null)
            {
                device.CloseDevice.Client = Client.GetCreateClient(device.CloseDevice.ClientName);
            }
            SetupTool.FromId((string)device.OpenDevice.ObjectType).Run(room, device.OpenDevice);
            SetupTool.FromId((string)device.CloseDevice.ObjectType).Run(room, device.CloseDevice);
            ISwitch openDevice = (ISwitch)ObjectFactory.FromString(openDeviceId);
            ISwitch closeDevice = (ISwitch)ObjectFactory.FromString(closeDeviceId);
            closeDevice.Stop();
            openDevice.Stop();

            foreach (Room iRoom in HomeAutomationServer.server.Rooms)
            {
                if (iRoom.Objects.Contains(openDevice))
                {
                    iRoom.Objects.Remove(openDevice);
                }
                if (iRoom.Objects.Contains(closeDevice))
                {
                    iRoom.Objects.Remove(closeDevice);
                }
            }

            Client client = device.Client;
            Blinds blinds = new Blinds(client, device.Name, openDevice, closeDevice, (int)device.TotalSteps, device.Description, Array.ConvertAll(((List<object>)device.FriendlyNames).ToArray(), x => x.ToString()));
            room.AddItem(blinds);
        }
    }
}