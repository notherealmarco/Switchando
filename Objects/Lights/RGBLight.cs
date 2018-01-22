using Homeautomation.GPIO;
using HomeAutomation.Dictionaries;
using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Rooms;
using HomeAutomation.Users;
using HomeAutomationCore;
using HomeAutomationCore.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace HomeAutomation.Objects.Lights
{
    class RGBLight : IColorableLight
    {
        Client Client;
        public string ClientName;

        public uint PinR, PinG, PinB;
        public uint ValueR { get; set; }
        public uint ValueG { get; set; }
        public uint ValueB { get; set; }
        public uint Brightness { get; set; }

        public bool Switch { get; set; }

        public string Name;
        public string[] FriendlyNames;
        public string Description;

        public bool nolog = false;

        Semaphore Semaphore;

        public string ObjectType = "LIGHT_GPIO_RGB";
        public string ObjectModel = "COLORABLE_LIGHT";

        public RGBLight()
        {
            this.Semaphore = new Semaphore(0, 1);
            Semaphore.Release();
        }
        public RGBLight(Client Client, string Name, uint PinR, uint PinG, uint PinB, string Description, string[] FriendlyNames)
        {
            this.Client = Client;
            this.ClientName = Client.Name;
            this.PinR = PinR;
            this.PinG = PinG;
            this.PinB = PinB;
            this.Brightness = 100;
            this.Switch = true;

            this.ValueR = 255;
            this.ValueG = 255;
            this.ValueB = 255;

            this.Name = Name;
            this.Description = Description;
            this.FriendlyNames = FriendlyNames;

            this.Semaphore = new Semaphore(0, 1);
            Semaphore.Release();

            HomeAutomationServer.server.Objects.Add(this);

            if (Client.Name.Equals("local"))
            {
                PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)PinR, (uint)ValueR);
                PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)PinG, (uint)ValueG);
                PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)PinB, (uint)ValueB);

                PIGPIO.set_PWM_frequency(Client.PigpioID, PinR, 4000);
                PIGPIO.set_PWM_frequency(Client.PigpioID, PinG, 4000);
                PIGPIO.set_PWM_frequency(Client.PigpioID, PinB, 4000);
            }
        }
        public void SetClient(Client client)
        {
            this.Client = client;
        }
        public void Set(uint ValueR, uint ValueG, uint ValueB, int DimmerIntervals)
        {
            Set(ValueR, ValueG, ValueB, DimmerIntervals, false);
        }
        public void Set(uint ValueR, uint ValueG, uint ValueB, int DimmerIntervals, bool nolog = false)
        {
            Console.WriteLine("Changing color of " + this.Name + " with a dimmer of " + DimmerIntervals + "ms.");
            //if (!nolog) HomeAutomationServer.server.Telegram.Log("Changing color of " + this.Name + " with a dimmer of " + DimmerIntervals + "ms.");
            this.Brightness = 100;
            if (ValueR == this.ValueR && ValueG == this.ValueG && ValueB == this.ValueB)
            {
                return;
            }

            if (ValueR == 0 && ValueG == 0 && ValueB == 0) this.Switch = false;
            else this.Switch = true;

            if (!Client.Name.Equals("local"))
            {
                UploadValues(ValueR, ValueG, ValueB, DimmerIntervals);
                this.ValueR = ValueR;
                this.ValueG = ValueG;
                this.ValueB = ValueB;
                return;
            }

            if (DimmerIntervals == 0)
            {
                PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)PinR, (uint)ValueR);
                PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)PinG, (uint)ValueG);
                PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)PinB, (uint)ValueB);
                this.ValueR = ValueR;
                this.ValueG = ValueG;
                this.ValueB = ValueB;
                return;
            }

            if (this.ValueR != ValueR)
            {
                Thread thread = new Thread(DimmerThread);
                int subtract = (int)this.ValueR - (int)ValueR;
                double[] values = new double[4];
                values[0] = this.ValueR;
                values[1] = ValueR;
                values[2] = PinR;
                values[3] = (DimmerIntervals / subtract);
                if (((this.ValueR) - ValueR) == 0) values[3] = 0;
                
                thread.Start(values);
            }

            if (this.ValueG != ValueG)
            {
                Thread thread = new Thread(DimmerThread);
                double[] values2 = new double[4];
                values2[0] = this.ValueG;
                values2[1] = ValueG;
                values2[2] = PinG;
                values2[3] = ((DimmerIntervals / (((int)this.ValueG) - (int)ValueG)));
                if (((this.ValueG) - ValueG) == 0) values2[3] = 0;
                
                thread.Start(values2);
            }

            if (this.ValueB != ValueB)
            {
                Thread thread = new Thread(DimmerThread);
                double[] values3 = new double[4];
                values3[0] = this.ValueB;
                values3[1] = ValueB;
                values3[2] = PinB;
                values3[3] = ((DimmerIntervals / (((int)this.ValueB) - (int)ValueB)));
                if (((this.ValueB) - ValueB) == 0) values3[3] = 0;
                
                thread.Start(values3);

                Console.WriteLine("Waiting for dimmer...");
                while (thread.IsAlive)
                {
                    Thread.Sleep(10);
                }
            }

            this.ValueR = ValueR;
            this.ValueG = ValueG;
            this.ValueB = ValueB;
        }

        public void Dimm(uint percentage, int dimmer)
        {
            uint R = ValueR;
            uint G = ValueG;
            uint B = ValueB;

            if (Brightness == 0)
            {
                Set(255, 255, 255, dimmer);
            }
            else
            {
                R = R * percentage / Brightness;
                G = G * percentage / Brightness;
                B = B * percentage / Brightness;
                Set(R, G, B, dimmer);
            }

            this.Brightness = percentage;
        }

        public void DimmerThread(object data)
        {
            //await Task.Delay(1);
            double[] values = (double[])data;
            int led = (int)values[2];
            if (values[3] < 0) values[3] *= -1;
            if (values[0] <= values[1])
            {
                for (double i = values[0]; i <= values[1]; i = i + 1)
                {
                    Semaphore.WaitOne();
                    PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)led, (uint)i);
                    Semaphore.Release();
                    if (values[3] == 0) values[3] = 1;
                    Thread.Sleep((int)values[3]);
                }
            }
            else
            {
                for (double i = values[0]; i >= values[1]; i = i - 1)
                {
                    Semaphore.WaitOne();
                    PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)led, (uint)i);
                    Semaphore.Release();
                    if (values[3] == 0) values[3] = 1;
                    Thread.Sleep((int)values[3]);
                }
            }
            PIGPIO.set_PWM_dutycycle(Client.PigpioID, (uint)led, (uint)values[1]);
        }

        public void Pause()
        {
            if (Switch)
            {
                //PauseR = ValueR;
                //PauseG = ValueG;
                //PauseB = ValueB;
                Set(0, 0, 0, 1000);
            }
            else
            {
                Set(255, 255, 255, 1000);
                /*if (PauseR == 0 && PauseG == 0 && PauseB == 0)
                    Set(255, 255, 255, 1000);
                else
                    Set(PauseR, PauseG, PauseB, 1000);*/
            }
        }

        public void Pause(bool status)
        {
            if (!status)
            {
                Set(0, 0, 0, 1000);
            }
            else
            {
                Set(255, 255, 255, 1000);
            }
        }

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

        public string GetObjectType()
        {
            return "LIGHT_GPIO_RGB";
        }
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
        public NetworkInterface GetInterface()
        {
            return NetworkInterface.FromId(ObjectType);
        }
        private static RGBLight FindLightFromName(string name)
        {
            RGBLight light = null;
            foreach (IObject obj in HomeAutomationServer.server.Objects)
            {
                if (obj.GetName().ToLower().Equals(name.ToLower()))
                {
                    light = (RGBLight)obj;
                    break;
                }
                if (obj.GetFriendlyNames() == null) continue;
                if (Array.IndexOf(obj.GetFriendlyNames(), name.ToLower()) > -1)
                {
                    light = (RGBLight)obj;
                    break;
                }
            }
            return light;
        }
        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("changeColor/RGB"))
            {
                RGBLight light = null;
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
                            light = FindLightFromName(command[1]);
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
            if (method.Equals("changeColor/name"))
            {
                return "NOT IMPLEMENTED";
            }
            if (method.Equals("switch"))
            {
                RGBLight light = null;
                bool status = false;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = FindLightFromName(command[1]);
                            break;
                        case "switch":
                            status = bool.Parse(command[1]);
                            break;
                    }
                    if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                }
                if (!login.HasAccess(light)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                if (status) light.Start(); else light.Stop();
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("dimm"))
            {
                RGBLight light = null;
                byte dimm_percentage = 255;
                int dimmer = 0;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            light = FindLightFromName(command[1]);
                            break;
                        case "percentage":
                            dimm_percentage = byte.Parse(command[1]);
                            break;
                        case "dimmer":
                            dimmer = int.Parse(command[1]);
                            break;
                    }
                    if (light == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                }
                if (!login.HasAccess(light)) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                light.Dimm(dimm_percentage, dimmer);
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }
            if (method.Equals("create"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                string[] friendlyNames = null;
                string description = null;

                uint pinR = 0;
                uint pinG = 0;
                uint pinB = 0;

                Client client = null;

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
                        case "pin_r":
                            string pinr = command[1];
                            pinR = uint.Parse(pinr);
                            break;
                        case "pin_g":
                            string ping = command[1];
                            pinG = uint.Parse(ping);
                            break;
                        case "pin_b":
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
                    }
                }
                if (room == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Room not found").Json();
                RGBLight light = new RGBLight(client, name, pinR, pinG, pinB, description, friendlyNames);
                room.AddItem(light);
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.light = light;
                return data.Json();
            }
            //OLD API
            if (string.IsNullOrEmpty(method))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                RGBLight light = null;
                uint R = 0;
                uint G = 0;
                uint B = 0;
                int dimmer = 0;
                bool nolog = false;
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
                            foreach (IObject obj in HomeAutomationServer.server.Objects)
                            {
                                if (obj.GetName().ToLower().Equals(command[1].ToLower()))
                                {
                                    light = (RGBLight)obj;
                                    break;
                                }
                                if (obj.GetFriendlyNames() == null) continue;
                                if (Array.IndexOf(obj.GetFriendlyNames(), command[1].ToLower()) > -1)
                                {
                                    light = (RGBLight)obj;
                                    break;
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

                        case "nolog":
                            nolog = true;
                            break;

                        case "switch":
                            status = command[1];
                            break;
                    }
                }
                if (status != null)
                {
                    light.Pause(bool.Parse(status));
                    return "";
                }
                if (color != null)
                {
                    uint[] vls = ColorConverter.ConvertNameToRGB(color);
                    light.Set(vls[0], vls[1], vls[2], dimmer);
                    return "";
                }
                if (dimm_percentage != 400)
                {
                    light.Dimm(dimm_percentage, dimmer);
                    return "";
                }
                light.Set(R, G, B, dimmer, nolog);
                return "";
            }
            return "";
        }
        void UploadValues(uint ValueR, uint ValueG, uint ValueB, int DimmerIntervals)
        {
            Client.Sendata("LIGHT_GPIO_RGB/changeColor/RGB?objname=" + this.Name + "&R=" + ValueR.ToString() + "&G=" + ValueG.ToString() + "&B=" + ValueB.ToString() + "&dimmer=" + DimmerIntervals);
        }
        public void Init()
        {
            if (Client.Name.Equals("local"))
            {
                PIGPIO.set_PWM_frequency(0, PinR, 4000);
                PIGPIO.set_PWM_frequency(0, PinG, 4000);
                PIGPIO.set_PWM_frequency(0, PinB, 4000);

                PIGPIO.set_PWM_dutycycle(0, (uint)PinR, (uint)ValueR);
                PIGPIO.set_PWM_dutycycle(0, (uint)PinG, (uint)ValueG);
                PIGPIO.set_PWM_dutycycle(0, (uint)PinB, (uint)ValueB);
            }
        }
        public static void Setup(Room room, dynamic device)
        {
            RGBLight light = new RGBLight();
            light.PinR = (uint)device.PinR;
            light.PinG = (uint)device.PinG;
            light.PinB = (uint)device.PinB;
            light.Name = device.Name;
            light.FriendlyNames = Array.ConvertAll(((List<object>)device.FriendlyNames).ToArray(), x => x.ToString());
            light.Description = device.Description;
            light.Switch = device.Switch;
            light.ValueR = (uint)device.ValueR;
            light.ValueG = (uint)device.ValueG;
            light.ValueB = (uint)device.ValueB;
            light.ClientName = device.Client.Name;
            light.SetClient(device.Client);

            HomeAutomationServer.server.Objects.Add(light);
            room.AddItem(light);
            light.Init();
        }
    }
}