using Homeautomation.GPIO;
using HomeAutomation.Network;
using HomeAutomationCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomationMain
{
    class HomeAutomationMain
    {
        static void Main(string[] args)
        {
            string[] nameip = File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/raspi-client.json").Split('@');
            Console.WriteLine("PIGPIOID -> " + PIGPIO.pigpio_start(null, null));

            Console.WriteLine("Welcome to HomeAutomation 4 RC1 ALPHA Raspi-Client by Marco Realacci!");
            new HomeAutomationClient(nameip[0]);
            new TCPClient().StartClient(nameip[1]);
            Console.ReadKey();
        }
    }
}