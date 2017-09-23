using Homeautomation.GPIO;
using HomeAutomation.Network;
using HomeAutomationCore;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HomeAutomation.Objects.Lights
{
    class RGBLight : IColorableLight
    {
        //Client Client;

        public uint PinR, PinG, PinB;
        public uint ValueR, ValueG, ValueB, Brightness;
        uint PauseR, PauseG, PauseB;
        public bool Switch;

        public string Name;
        public string[] FriendlyNames;
        public string Description;

        Semaphore Semaphore;

        public string ObjectType = "LIGHT_GPIO_RGB";
        public LightType LightType = LightType.RGB_LIGHT;

        public RGBLight()
        {
            foreach (NetworkInterface netInt in HomeAutomationClient.client.NetworkInterfaces)
            {
                if (netInt.Id.Equals("light_rgb")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("light_rgb", requestHandler);
            this.Semaphore = new Semaphore(0, 1);
            Semaphore.Release();
        }

        public void Init()
        {
            PIGPIO.set_PWM_frequency(0, PinR, 4000);
            PIGPIO.set_PWM_frequency(0, PinG, 4000);
            PIGPIO.set_PWM_frequency(0, PinB, 4000);

            PIGPIO.set_PWM_dutycycle(0, (uint)PinR, (uint)ValueR);
            PIGPIO.set_PWM_dutycycle(0, (uint)PinG, (uint)ValueG);
            PIGPIO.set_PWM_dutycycle(0, (uint)PinB, (uint)ValueB);
        }

        public RGBLight(string Name, uint PinR, uint PinG, uint PinB, string Description, string[] FriendlyNames)
        {
            //this.Client = Client;
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

            HomeAutomationClient.client.Objects.Add(this);

            PIGPIO.set_PWM_dutycycle(0, PinR, ValueR);
            PIGPIO.set_PWM_dutycycle(0, PinG, ValueG);
            PIGPIO.set_PWM_dutycycle(0, PinB, ValueB);

            PIGPIO.set_PWM_frequency(0, PinR, 4000);
            PIGPIO.set_PWM_frequency(0, PinG, 4000);
            PIGPIO.set_PWM_frequency(0, PinB, 4000);

            foreach (NetworkInterface netInt in HomeAutomationClient.client.NetworkInterfaces)
            {
                if (netInt.Id.Equals("light_rgb")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("light_rgb", requestHandler);
        }

        public void Set(uint ValueR, uint ValueG, uint ValueB, int DimmerIntervals)
        {

            if (ValueR == this.ValueR && ValueG == this.ValueG && ValueB == this.ValueB)
            {
                return;
            }

            if (ValueR == 0 && ValueG == 0 & ValueB == 0) this.Switch = false;
            else this.Switch = true;

            if (DimmerIntervals == 0)
            {
                this.ValueR = ValueR;
                this.ValueG = ValueG;
                this.ValueB = ValueB;

                PIGPIO.set_PWM_dutycycle(0, (uint)PinR, (uint)ValueR);
                PIGPIO.set_PWM_dutycycle(0, (uint)PinG, (uint)ValueG);
                PIGPIO.set_PWM_dutycycle(0, (uint)PinB, (uint)ValueB);
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

            R = R * percentage / Brightness;
            G = G * percentage / Brightness;
            B = B * percentage / Brightness;
            Set(R, G, B, dimmer);

            this.Brightness = percentage;
        }

        public async void DimmerThread(object data)
        {
            await Task.Delay(1);
            double[] values = (double[])data;
            int led = (int)values[2];
            if (values[3] < 0) values[3] *= -1;
            if (values[0] <= values[1])
            {
                for (double i = values[0]; i <= values[1]; i = i + 1)
                {
                    Semaphore.WaitOne();
                    PIGPIO.set_PWM_dutycycle(0, (uint)led, (uint)i);
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
                    PIGPIO.set_PWM_dutycycle(0, (uint)led, (uint)i);
                    Semaphore.Release();
                    if (values[3] == 0) values[3] = 1;
                    Thread.Sleep((int)values[3]);
                }
            }
            PIGPIO.set_PWM_dutycycle(0, (uint)led, (uint)values[1]);
        }
        void Block(long durationTicks)
        {
            Stopwatch sw;
            sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds <= durationTicks)
            {
                Console.WriteLine(sw.ElapsedTicks + " " + durationTicks);
                /*if (sw.Elapsed.Milliseconds % 100 == 0)
                {
                    i++;
                }*/
            }
            sw.Stop();
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
        public LightType GetLightType()
        {
            return LightType.RGB_LIGHT;
        }
        public new string GetObjectType()
        {
            return "LIGHT_GPIO_RGB";
    }
        public string GetName()
        {
            return Name;
        }
        public double[] GetValues()
        {
            return new double[3] { ValueR, ValueG, ValueB };
        }
        public static void SendParameters(string[] request)
        {
            RGBLight light = null;
            uint R = 0;
            uint G = 0;
            uint B = 0;
            int dimmer = 0;
            string status = null;
            foreach(string cmd in request)
            {
                string[] command = cmd.Split('=');
                if (command[0].Equals("interface")) continue;
                switch (command[0])
                {
                    case "objname":
                        foreach (IObject obj in HomeAutomationClient.client.Objects)
                        {
                            if (obj.GetName().Equals(command[1]))
                            {
                                light = (RGBLight)obj;
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

                    case "switch":
                        status = command[1];
                        break;
                }
            }
            if (status != null)
            {
                light.Pause(bool.Parse(status));
                return;
            }
            light.Set(R, G, B, dimmer);
        }
        public static void Setup(dynamic device)
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

            HomeAutomationClient.client.Objects.Add(light);
            light.Init();
        }
        /*void UploadValues(uint ValueR, uint ValueG, uint ValueB, int DimmerIntervals)
        {
            Client.Sendata("update=" + this.Name + "&R=" + ValueR.ToString() + "&G=" + ValueG.ToString() + "&B=" + ValueB.ToString() + "&dimmer=" + DimmerIntervals);
        }*/
    }
}