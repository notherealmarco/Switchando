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
    class WLight : ILight
    {
        //Client Client;

        public uint Pin;
        public uint Value, Brightness;
        public uint PauseValue;
        public bool Switch;

        public string Name;
        public string[] FriendlyNames;
        public string Description;

        public string ObjectType = "LIGHT_GPIO_RGB";
        public LightType LightType = LightType.W_LIGHT;

        public WLight(string Name, uint pin, string description, string[] FriendlyNames)
        {
            //this.Client = client;
            this.Switch = true;
            this.Description = description;
            this.Name = Name;
            this.Pin = pin;
            this.Value = 255;
            this.Brightness = 100;

            PIGPIO.set_PWM_dutycycle(0, Pin, Value);

            PIGPIO.set_PWM_frequency(0, Pin, 4000);

            HomeAutomationClient.client.Objects.Add(this);

            foreach (NetworkInterface netInt in HomeAutomationClient.client.NetworkInterfaces)
            {
                if (netInt.Id.Equals("light_w")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("light_w", requestHandler);
        }

        public WLight()
        {
            foreach (NetworkInterface netInt in HomeAutomationClient.client.NetworkInterfaces)
            {
                if (netInt.Id.Equals("light_w")) return;
            }
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface("light_w", requestHandler);
        }

        public void Init()
        {
            PIGPIO.set_PWM_dutycycle(0, Pin, Value);

            PIGPIO.set_PWM_frequency(0, Pin, 4000);
        }

        public void Set(uint value, int dimmer)
        {
            if (value == this.Value) return;

            if (value == 0) Switch = false; else Switch = true;

            if (dimmer == 0)
            {
                PIGPIO.set_PWM_dutycycle(0, Pin, value);
                this.Value = value;
                return;
            }
            double[] values = new double[4];
            values[0] = this.Value;
            values[1] = value;
            values[2] = this.Pin;
            values[3] = ((dimmer / (this.Value - (value))));
            if (((this.Value) - (value) == 0)) values[3] = 0;
            DimmerThread(values);

            this.Value = value;
        }

        public void Dimm(uint percentage, int dimmer)
        {
            uint W = Value;

            W = W * percentage / Brightness;
            Set(W, dimmer);

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
                    PIGPIO.set_PWM_dutycycle(0, (uint)led, (uint)i);
                    if (values[3] == 0) values[3] = 1;
                    Thread.Sleep((int)values[3]);
                }
            }
            else
            {
                for (double i = values[0]; i >= values[1]; i = i - 1)
                {
                    PIGPIO.set_PWM_dutycycle(0, (uint)led, (uint)i);
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
            int i = 0;

            while (sw.ElapsedTicks <= durationTicks)
            {
                if (sw.Elapsed.Ticks % 100 == 0)
                {
                    i++;
                }
            }
            sw.Stop();
        }

        public void Pause()
        {
            if (Switch)
            {
                PauseValue = Value;
                Set(0, 1000);
            }
            else
            {
                if (PauseValue == 0)
                    Set(255, 1000);
                else
                    Set(PauseValue, 1000);
            }
        }

        public void Pause(bool status)
        {
            if (!status)
            {
                PauseValue = Value;
                Set(0, 1000);
            }
            else
            {
                if (PauseValue == 0)
                    Set(255, 1000);
                else
                    Set(PauseValue, 1000);
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
            return LightType.W_LIGHT;
        }
        public string GetObjectType()
        {
            return "LIGHT_GPIO_RGB";
        }
        public string GetName()
        {
            return Name;
        }
        public uint GetValue()
        {
            return Value;
        }
        /*void UploadValues(uint Value, int DimmerIntervals)
        {
            Client.Sendata("update=" + this.Name + "&W=" + Value.ToString() + "&dimmer=" + DimmerIntervals);
        }*/
        public static void SendParameters(string[] request)
        {
            WLight light = null;
            uint Value = 0;
            int dimmer = 0;
            string status = null;
            foreach (string cmd in request)
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
                                light = (WLight)obj;
                            }
                        }
                        break;

                    case "W":
                        Value = uint.Parse(command[1]);
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
            light.Set(Value, dimmer);
        }
        public static void Setup(dynamic device)
        {
            WLight light = new WLight();
            light.Pin = (uint)device.Pin;
            light.Name = device.Name;
            light.FriendlyNames = Array.ConvertAll(((List<object>)device.FriendlyNames).ToArray(), x => x.ToString());
            light.Description = device.Description;
            light.Switch = device.Switch;
            light.Value = (uint)device.Value;

            HomeAutomationClient.client.Objects.Add(light);
            light.Init();
        }
    }
}