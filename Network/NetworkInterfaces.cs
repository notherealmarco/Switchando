using HomeAutomation.Users;
using HomeAutomationCore;
using System;

namespace HomeAutomation.Network
{
    public class NetworkInterface
    {
        public string Id;

        public delegate string Delegate(string method, string[] request, Identity login);
        Delegate Handler;

        public NetworkInterface(string id, Delegate handler)
        {
            foreach (NetworkInterface netInt in HomeAutomationServer.server.NetworkInterfaces)
            {
                if (netInt.Id.Equals(id)) return;
            }
            this.Id = id;
            this.Handler = handler;
            Console.WriteLine("registering " + id);
            HomeAutomationServer.server.NetworkInterfaces.Add(this);
        }

        public string Run(string method, string[] request, Identity login)
        {
            return this.Handler(method, request, login);
        }

        public static NetworkInterface FromId(string id)
        {
            foreach(NetworkInterface networkInterface in HomeAutomationServer.server.NetworkInterfaces)
            {
                if (id.StartsWith(networkInterface.Id))
                {
                    return networkInterface;
                }
            }
            return null;
        }
    }
}