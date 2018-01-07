using HomeAutomation.Application.ConfigRetriver;
using HomeAutomation.ConfigRetriver;
using HomeAutomation.Objects;
using HomeAutomation.Objects.External.Plugins;
using HomeAutomation.Rooms;
using HomeAutomationCore;
using HomeAutomationCore.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation.Core
{
    class CoreLoader
    {
        public static void LoadFromJson()
        {
            /*if (File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/telegram.config"))
            {
                string telegramRaw = File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/telegram.config"); //token@log@alert
                string[] telegramData = telegramRaw.Split('@');
                if (telegramData.Length == 3)
                {
                    HomeAutomationServer.server.Telegram = new TelegramBot(telegramData[0]);
                    HomeAutomationServer.server.Telegram.SetLogChat(long.Parse(telegramData[1]));
                    HomeAutomationServer.server.Telegram.SetAlertChat(long.Parse(telegramData[2]));
                }
            }*/
            dynamic rooms = HomeAutomationServer.server.Database.Get<dynamic>("switchando.rooms");

            if (rooms == null) return;

            foreach (dynamic mRoom in rooms)
            {
                //string[] friendlyNames = Array.ConvertAll(((List<object>)mRoom.FriendlyNames).ToArray(), x => x.ToString());

                var friendlyNames = mRoom.FriendlyNames.ToObject<List<string>>();

                Room room = new Room((string)mRoom.Name, friendlyNames.ToArray(), (bool)mRoom.Hidden);

                var objs = mRoom.Objects.ToObject<List<ExpandoObject>>();

                foreach (dynamic device in objs)
                {
                    Console.WriteLine(device.ClientName + " <<->> " + device.Name + " -> " + device.ObjectType.ToString());

                    Client client = null;

                    bool toAdd = true;

                    if (device.ClientName != null)
                    {
                        foreach (Client clnt in HomeAutomationServer.server.Clients)
                        {
                            if (clnt.Name.Equals(device.ClientName))
                            {
                                client = clnt;
                                toAdd = false;
                            }
                        }
                        if (toAdd) client = new Client(null, 0, (string)device.ClientName);

                        if (HomeAutomationServer.server.Clients.Count == 0)
                        {
                            client = new Client(null, 0, device.ClientName);
                        }
                    }
                    bool exit = false;
                    foreach (IObject iobj in HomeAutomationServer.server.Objects)
                    {
                        if (iobj.GetName().Equals(device.Name))
                        {
                            room.AddItem(iobj);
                            exit = true;
                        }
                    }
                    if (exit) continue;

                    if (SetupTool.Exists((string)device.ObjectType))
                    {
                        device.Client = client;
                        SetupTool.FromId(device.ObjectType).Run(room, device);
                    }
                }
            }
        }
    }
}
