using HomeAutomation.Network;
using HomeAutomation.Objects.Fans;
using HomeAutomation.Objects.Switches;
using HomeAutomationCore;
using System;
using System.Threading;

namespace HomeAutomation.Objects.Blinds
{
    class Blinds : ISwitch
    {
        //Client Client;
        //public string ClientName;

        public string Name;
        public string Description;
        public string[] FriendlyNames;

        public ISwitch OpenDevice;
        public ISwitch CloseDevice;

        public string ObjectType = "BLINDS";

        public byte TotalSteps;
        int Step;
        Thread movingThread;
        bool isMoving;

        public Blinds()
        {
            foreach (NetworkInterface netInt in HomeAutomationClient.client.NetworkInterfaces)
            {
                if (netInt.Id.Equals("blinds")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("blinds", requestHandler);
        }
        public Blinds(string name, ISwitch openDevice, ISwitch closeDevice, byte totalSteps, string description, string[] friendlyNames)
        {
            //this.Client = client;
            this.Name = name;
            this.OpenDevice = openDevice;
            this.CloseDevice = closeDevice;
            this.Description = description;
            this.FriendlyNames = friendlyNames;
            this.TotalSteps = totalSteps;
            //this.ClientName = client.Name;

            HomeAutomationClient.client.Objects.Add(this);

            foreach (NetworkInterface netInt in HomeAutomationClient.client.NetworkInterfaces)
            {
                if (netInt.Id.Equals("blinds")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("blinds", requestHandler);
        }

        public void Start()
        {
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
            if (isMoving)
            {
                isMoving = false;
                return;
            }

            movingThread = new Thread(MovingThread);
            isMoving = true;
            movingThread.Start((int)TotalSteps);
        }
        public void Move(int step)
        {
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

            if (step <= Step)
            {
                OpenDevice.Start();
                for (int pos = Step; pos >= step; pos--)
                {
                    Step = pos;
                    Thread.Sleep(1000);
                    HomeAutomationClient.client.TcpClient.writer.WriteLine("interface=blinds&objname=" + this.Name + "&update=" + this.Step + "&password=" + HomeAutomationClient.client.Password);
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
                    HomeAutomationClient.client.TcpClient.writer.WriteLine("interface=blinds&objname=" + this.Name + "&update=" + this.Step + "&password=" + HomeAutomationClient.client.Password);
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
        /*public NetworkInterface GetInterface()
        {
            return NetworkInterface.FromId("blinds");
        }*/
        public string[] GetFriendlyNames()
        {
            return this.FriendlyNames;
        }
        static void SendParameters(string[] request)
        {
            Blinds blinds = null;

            foreach (string cmd in request)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "objname":
                        foreach (IObject obj in HomeAutomationClient.client.Objects)
                        {
                            if (obj.GetName().ToLower().Equals(command[1].ToLower()))
                            {
                                blinds = (Blinds)obj;
                                break;
                            }
                        }
                        break;
                    case "percentage":
                        int percentage = int.Parse(command[1]);
                        double prestep = (percentage / 100d) * blinds.TotalSteps;
                        int step = (int)Math.Round(prestep);
                        blinds.Move(step);
                        return;
                    case "switch":
                        bool status = bool.Parse(command[1]);
                        if (status) blinds.Start(); else blinds.Stop();
                        return;

                }
            }
        }
        public static void Setup(dynamic device)
        {
            ISwitch openDevice;
            ISwitch closeDevice;

            Relay relay = new Relay();
            relay.Pin = (uint)device.OpenDevice.Pin;
            relay.Name = device.OpenDevice.Name;

            relay.Description = device.OpenDevice.Description;
            //relay.FriendlyNames = device.OpenDevice.FriendlyNames;
            relay.Enabled = device.OpenDevice.Switch;
            //relay.ClientName = client.Name;
            //relay.SetClient(client);
            openDevice = relay;

            relay = new Relay();
            relay.Pin = (uint)device.CloseDevice.Pin;
            relay.Name = device.CloseDevice.Name;
            relay.Description = device.CloseDevice.Description;
            //relay.FriendlyNames = device.CloseDevice.FriendlyNames;
            relay.Enabled = device.CloseDevice.Switch;
            //relay.ClientName = client.Name;
            //relay.SetClient(client);
            closeDevice = relay;

            Blinds blinds = new Blinds(device.Name, openDevice, closeDevice, device.TotalSteps, device.Description, device.FriendlyNames);
        }
    }
}
