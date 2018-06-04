using HomeAutomation.Application.ConfigRetriver;
using HomeAutomation.ConfigRetriver;
using HomeAutomation.Events;
using HomeAutomation.Network;
using HomeAutomation.Network.Getters;
using HomeAutomation.Network.WebUI;
using HomeAutomation.ObjectInterfaces;
using HomeAutomation.Objects;
using HomeAutomation.Objects.Blinds;
using HomeAutomation.Objects.External.Plugins;
using HomeAutomation.Rooms;
using HomeAutomation.Scenarios;
using HomeAutomation.Users;
using HomeAutomation.Utilities;
using Switchando.Cloud;
using Switchando.Collector;
using System;
using System.IO;
using System.Reflection;

namespace HomeAutomationCore
{
    static class HomeAutomationMain
    {
        static void Main(string[] args)
        {
            var server = new HomeAutomationServer("A Switchando family", "password");
            Console.Write("Switchando Loader mode: ");
            int pos = Array.IndexOf(args, "cloud");
            if (pos == 0)
            {
                Console.WriteLine("CUSTOM (Cloud configuration only)");
                Console.WriteLine("Loading Switchando Cloud...");
                HomeAutomationServer.server.Cloud = new SwitchandoCloud(true, args);
            }
            Console.WriteLine("FULL");
            
            if (HomeAutomationServer.server.MQTTClient == null)
            {
                if (File.Exists(Paths.GetServerPath() + "/autoconfigscript.txt"))
                {
                    string file = File.ReadAllText(Paths.GetServerPath() + "/autoconfigscript.txt");
                    string[] data = file.Split(new[] { "\r\n", "\r", "\n" },StringSplitOptions.None);
                    string mqttAddr = null;
                    string mqttUser = null;
                    string mqttPasswd = null;
                    string adminPasswd = null;
                    foreach (string s in data)
                    {
                        if (!s.Contains("=")) continue;
                        string[] str = s.Split('=');
                        string var = str[0];
                        string value = s.Substring(var.Length + 1);
                        if (var.Equals("mqtt-address"))
                        {
                            mqttAddr = value;
                        }
                        if (var.Equals("mqtt-username"))
                        {
                            mqttUser = value;
                        }
                        if (var.Equals("mqtt-password"))
                        {
                            mqttPasswd = value;
                        }
                        if (var.Equals("admin-password"))
                        {
                            adminPasswd = value;
                        }
                    }
                    if (!string.IsNullOrEmpty(mqttUser) && !string.IsNullOrEmpty(mqttPasswd))
                    {
                        MQTTClient mqtt = new MQTTClient(mqttAddr, mqttUser, mqttPasswd);
                        HomeAutomationServer.server.MQTTClient = mqtt;
                    }
                    else
                    {
                        MQTTClient mqtt = new MQTTClient(mqttAddr);
                        HomeAutomationServer.server.MQTTClient = mqtt;
                    }
                    if (Identity.GetAdminUser() == null)
                    {
                        new Identity("admin", adminPasswd, Identity.UserType.ADMINISTRATOR);
                    }
                }
                else
                {
                    Console.WriteLine("\n\nIt seems MQTT is not setted up yet, write down your MQTT broker's address:");
                    string addr = Console.ReadLine();
                    if (!string.IsNullOrEmpty(addr))
                    {
                        Console.WriteLine("MQTT broker username (leave it blank if your broker doesn't need login):");
                        string uname = Console.ReadLine();
                        if (!string.IsNullOrEmpty(uname))
                        {
                            Console.WriteLine("MQTT broker password:");
                            string passwd = Console.ReadLine();
                            MQTTClient mqtt = new MQTTClient(addr, uname, passwd);
                            HomeAutomationServer.server.MQTTClient = mqtt;
                        }
                        else
                        {
                            MQTTClient mqtt = new MQTTClient(addr);
                            HomeAutomationServer.server.MQTTClient = mqtt;
                        }
                    }
                }
            }
            else
            {
                HomeAutomationServer.server.MQTTClient.Init();
            }
            Console.WriteLine("Loading main components...");

            new NetworkInterface("DEVICE_MANAGER", ConfigRetriver.SendParameters);
            new NetworkInterface("OBJECT_INTERFACE", ObjectInterface.SendParameters);
            new NetworkInterface("METHOD_INTERFACE", MethodInterface.SendParameters);
            new NetworkInterface("ACTION", HomeAutomation.ObjectInterfaces.Action.SendParameters);
            new NetworkInterface("ROOM", Room.SendParameters);
            new NetworkInterface("SCENARIO", Scenario.SendParameters);
            new NetworkInterface("GET", ObjectGetter.SendParameters);
            new NetworkInterface("USER", Identity.SendParameters);
            new NetworkInterface("EVENTS", EventsManager.SendParameters);

            new NetworkInterface("DEVICE_GROUP", DeviceGroup.SendParameters);
            new SetupTool("DEVICE_GROUP", DeviceGroup.Setup);

            new HTMLFragment("SWITCH", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/web/standard-fragments/switch.htmlfragment");
            new HTMLFragment("COLORABLE_LIGHT", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/web/standard-fragments/colorable_light.htmlfragment");
            new HTMLFragment("COLOR_AMBIANCE_LIGHT", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/web/standard-fragments/color_ambiance_light.htmlfragment");
            new HTMLFragment("DIMMABLE_LIGHT", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/web/standard-fragments/dimmable_light.htmlfragment");

            var blinds = new NetworkInterface("BLINDS", Blinds.SendParameters);
            new HTMLFragment("BLINDS", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/web/standard-fragments/blinds.htmlfragment");
            new ObjectInterface(blinds, "Switch", typeof(uint), "ON / OFF state");
            //add step in percentage
            var blinds_mi = new MethodInterface(blinds, "switch", "Open / close");
            blinds_mi.AddParameter(new MethodParameter("objname", typeof(string), "Device name"));
            blinds_mi.AddParameter(new MethodParameter("switch", typeof(string), "Switch on (true / false)"));
            var blinds_mv = new MethodInterface(blinds, "move", "");
            blinds_mv.AddParameter(new MethodParameter("objname", typeof(string), "Device name"));
            blinds_mv.AddParameter(new MethodParameter("value", typeof(string), "Closing percentage (%)"));

            new SetupTool("BLINDS", Blinds.Setup);

            Console.WriteLine("Raspi-Client support is ENABLED, but it's deprecated");
            new Client.Client(null, 0, "local"); //Raspi-Client support is ended, however this will stay enabled for compatibility

            if (Identity.GetAdminUser() == null)
            {
                Console.WriteLine("\n\nIt seems you don't have an admin account yet, please wite down the new password for the 'admin' user:");
                string passwd = Console.ReadLine();
                new Identity("admin", passwd, Identity.UserType.ADMINISTRATOR);
            }

            Console.WriteLine("Loading plugins...");

            Plugins.LoadAll(Paths.GetServerPath() + "/plugins");

            Console.WriteLine("Loading user settings...");

            server.LoadCore();

            foreach (Room room in HomeAutomationServer.server.Rooms)
            {
                Console.WriteLine(room.Name + " -> ");
                foreach (IObject iobj in room.Objects)
                {
                    Console.WriteLine(iobj.GetName());
                }
                Console.WriteLine();
            }
            new HTTPHandler(new string[] { "http://*:8080/api/" });

            HomeAutomationServer.server.MQTTClient.Connect();
            HomeAutomationServer.server.Web = new HTTPWebUI(Paths.GetServerPath() + "/web", 8080);

            HomeAutomationServer.server.Cloud = new SwitchandoCloud();

            Console.WriteLine(">> Switchando is ready (web interface running on port 8080) <<");

            if (!File.Exists(Paths.GetServerPath() + "/collector.disabled"))
            {
                Console.WriteLine("\n\nSwitchando Collector is ENABLED \n");
                Console.WriteLine("OS Description: " + System.Runtime.InteropServices.RuntimeInformation.OSDescription);
                Console.WriteLine("OS Architecture: " + System.Runtime.InteropServices.RuntimeInformation.OSArchitecture);
                SwitchandoCollector.Enable();
            }

            Console.WriteLine();
            Console.ReadLine();
        }
    }
}