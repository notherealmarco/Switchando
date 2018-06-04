using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects;
using HomeAutomation.Objects.Switches;
using HomeAutomation.Rooms;
using HomeAutomation.Users;
using HomeAutomationCore;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;

namespace HomeAutomation.Network
{
    class HTTPHandler
    {
        HttpServer server;
        public HTTPHandler(string[] ip)
        {
            server = new HttpServer(SendResponse, ip);
            server.Run();
        }

        public static string SendResponse(HttpListenerContext ctx)
        {
            var request = ctx.Request;
            string url = request.Url.PathAndQuery.Substring(5);
            url = HttpUtility.UrlDecode(url);
            //Console.WriteLine("HTTP API command (from " + request.RemoteEndPoint.ToString() + ") -> " + url);

            //if (!url.Contains("&password=" + HomeAutomationServer.server.GetPassword())) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST, "Invalid password").Json();

            string[] interfaceMethod = url.Split('/');
            string netInterface = null;
            string methodsRaw;
            netInterface = interfaceMethod[0].ToLower();
            methodsRaw = url.Substring(netInterface.Length + 1);
            string method = methodsRaw.Split('?')[0];
            string[] parameters = methodsRaw.Split('?')[1].Split('&');

            string username = null;
            string password = null;
            string token = null;

            foreach (string cmd in parameters)
            {
                string[] command = cmd.Split('=');
                switch (command[0])
                {
                    case "login_username":
                        username = command[1];
                        break;
                    case "login_password":
                        password = command[1];
                        break;
                    case "login_token":
                        token = command[1];
                        break;
                }
            }

            Identity login = null;
            if (password != null) login = Identity.StaticLogin(username, password);
            if (token != null) login = Identity.StaticSessionLogin(username, token, request.RemoteEndPoint.ToString());
            if (login == null && token == null) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Wrong username and / or password").Json();
            if (login == null && password == null) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Invalid session").Json();

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
                //File.WriteAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/configuration.json", JsonConvert.SerializeObject(HomeAutomationServer.server.Rooms));
                //HomeAutomationServer.server.ObjectNetwork.Save();
                HomeAutomationServer.server.SaveData();
                return returnMessage;
            }
            foreach (NetworkInterface networkInterface in HomeAutomationServer.server.NetworkInterfaces)
            {
                if (networkInterface.Id.ToLower().Equals(netInterface))
                {
                    string returnMessage = networkInterface.Run(method, parameters, login);
                    //File.WriteAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/configuration.json", JsonConvert.SerializeObject(HomeAutomationServer.server.Rooms));
                    //HomeAutomationServer.server.ObjectNetwork.Save();
                    HomeAutomationServer.server.SaveData();
                    return returnMessage;
                }
            }
            return string.Format("<html><body>Switchando Automation is running!<br />" + request.Url.Query + "<br />{0}</body></html>", DateTime.Now);
        }
        public static string SendCloudResponse(string url)
        {
            //var request = ctx.Request;
            //string url = request;
            url = HttpUtility.UrlDecode(url);
            //Console.WriteLine("HTTP API command (from " + request.RemoteEndPoint.ToString() + ") -> " + url);

            //if (!url.Contains("&password=" + HomeAutomationServer.server.GetPassword())) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST, "Invalid password").Json();

            string[] interfaceMethod = url.Split('/');
            string netInterface = null;
            string methodsRaw;
            netInterface = interfaceMethod[0].ToLower();
            methodsRaw = url.Substring(netInterface.Length + 1);
            string method = methodsRaw.Split('?')[0];
            string[] parameters = methodsRaw.Split('?')[1].Split('&');

            string username = null;
            string password = null;
            string token = null;

            foreach (string cmd in parameters)
            {
                string[] command = cmd.Split('=');
                switch (command[0])
                {
                    case "login_username":
                        username = command[1];
                        break;
                    case "login_password":
                        password = command[1];
                        break;
                    case "login_token":
                        token = command[1];
                        break;
                }
            }

            Identity login = null;
            if (password != null) login = Identity.StaticLogin(username, password);
            if (token != null) login = Identity.StaticSessionLogin(username, token);
            if (login == null && token == null) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Wrong username and / or password").Json();
            if (login == null && password == null) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Invalid session").Json();

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
                //File.WriteAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/configuration.json", JsonConvert.SerializeObject(HomeAutomationServer.server.Rooms));
                //HomeAutomationServer.server.ObjectNetwork.Save();
                HomeAutomationServer.server.SaveData();
                return returnMessage;
            }
            foreach (NetworkInterface networkInterface in HomeAutomationServer.server.NetworkInterfaces)
            {
                if (networkInterface.Id.ToLower().Equals(netInterface))
                {
                    string returnMessage = networkInterface.Run(method, parameters, login);
                    //File.WriteAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/configuration.json", JsonConvert.SerializeObject(HomeAutomationServer.server.Rooms));
                    //HomeAutomationServer.server.ObjectNetwork.Save();
                    HomeAutomationServer.server.SaveData();
                    return returnMessage;
                }
            }
            return string.Format("<html><body>Switchando Automation is running!<br />{0}</body></html>", DateTime.Now);
        }
    }
}
