using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects;
using HomeAutomation.Rooms;
using HomeAutomation.Users;
using HomeAutomationCore;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace HomeAutomation.Network
{
    static class APICommand
    {
        public static string Run(string command, Identity login)
        {
            string[] interfaceMethod = command.Split('/');
            string netInterface = null;
            string methodsRaw;
            netInterface = interfaceMethod[0].ToLower();
            methodsRaw = command.Substring(netInterface.Length + 1);
            string method = methodsRaw.Split('?')[0];
            string[] parameters = methodsRaw.Split('?')[1].Split('&');
            foreach (NetworkInterface networkInterface in HomeAutomationServer.server.NetworkInterfaces)
            {
                if (networkInterface.Id.ToLower().Equals(netInterface))
                {
                    string returnMessage = networkInterface.Run(method, parameters, login);
                    if (!command.Contains("nosave=true"))
                    {
                        HomeAutomationServer.server.SaveData();
                        //File.WriteAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/configuration.json", JsonConvert.SerializeObject(HomeAutomationServer.server.Rooms));
                        //HomeAutomationServer.server.ObjectNetwork.Save();
                    }
                    return returnMessage;
                }
            }
            return null;
        }
        public static string Run(string command)
        {
            string[] interfaceMethod = command.Split('/');
            string netInterface = null;
            string methodsRaw;
            netInterface = interfaceMethod[0].ToLower();
            methodsRaw = command.Substring(netInterface.Length + 1);
            string method = methodsRaw.Split('?')[0];
            string[] parameters = methodsRaw.Split('?')[1].Split('&');

            string username = null;
            string password = null;
            string token = null;

            foreach (string cmd in parameters)
            {
                string[] command_ = cmd.Split('=');
                switch (command_[0])
                {
                    case "login_username":
                        username = command_[1];
                        break;
                    case "login_password":
                        password = command_[1];
                        break;
                    case "login_token":
                        token = command_[1];
                        break;
                }
            }

            Identity login = null;
            if (password != null) login = Identity.StaticLogin(username, password);
            if (token != null) Identity.StaticSessionLogin(username, token);
            if (login == null) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Wrong username and / or password").Json();
            if (netInterface.StartsWith("id"))
            {
                string[] dataRaw = method.Split('/');
                NetworkInterface nInterface = null;

                foreach (IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (iobj.GetName().Equals(dataRaw[0])) nInterface = iobj.GetInterface();
                }
                foreach (Room iobj in HomeAutomationServer.server.Rooms)
                {
                    if (iobj.Name.Equals(dataRaw[0])) nInterface = NetworkInterface.FromId("ROOM");
                }
                if (nInterface == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, dataRaw[0] + " not found").Json();

                method = method.Substring(dataRaw[0].Length + 1);
                string returnMessage = nInterface.Run(method, parameters, login);
                if (!command.Contains("nosave=true"))
                {
                    HomeAutomationServer.server.SaveData();
                }
                return returnMessage;
            }
            foreach (NetworkInterface networkInterface in HomeAutomationServer.server.NetworkInterfaces)
            {
                if (networkInterface.Id.ToLower().Equals(netInterface))
                {
                    string returnMessage = networkInterface.Run(method, parameters, login);
                    if (!command.Contains("nosave=true"))
                    {
                        HomeAutomationServer.server.SaveData();
                    }
                    return returnMessage;
                }
            }
            return null;
        }
    }
}
