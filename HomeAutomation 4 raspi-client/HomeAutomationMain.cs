using Homeautomation.GPIO;
using HomeAutomation.Application.ConfigRetriver;
using HomeAutomation.Network;
using HomeAutomation.Objects.Blinds;
using HomeAutomation.Objects.External.Plugins;
using HomeAutomation.Objects.Fans;
using HomeAutomation.Objects.Inputs;
using HomeAutomation.Objects.Lights;
using HomeAutomationCore;
using System;
using System.IO;
using System.Reflection;

namespace HomeAutomationMain
{
    class HomeAutomationMain
    {
        static void Main(string[] args)
        {
            string[] nameip = File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/raspi-client.config").Split('@');
            Console.WriteLine("PIGPIOID -> " + PIGPIO.pigpio_start(null, null));
            new HomeAutomationClient(nameip[0]);
            new SetupTool("LIGHT_GPIO_RGB", RGBLight.Setup);
            new SetupTool("LIGHT_GPIO_W", WLight.Setup);
            new SetupTool("GENERIC_SWITCH", Relay.Setup);
            new SetupTool("BUTTON", Button.Setup);
            new SetupTool("SWITCH_BUTTON", SwitchButton.Setup);
            //new SetupTool("EXTERNAL_SWITCH", WebRelay.Setup);
            new SetupTool("BLINDS", Blinds.Setup);

            Plugins.LoadAll("plugins");

            Console.WriteLine("Welcome to HomeAutomation 4 RC1 ALPHA Raspi-Client by Marco Realacci!");
            
            HomeAutomationClient.client.Password = nameip[2];
            new TCPClient().StartClient(nameip[1]);
            Console.ReadKey();
        }
    }
}