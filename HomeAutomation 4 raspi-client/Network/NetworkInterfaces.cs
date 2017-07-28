using HomeAutomationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation.Network
{
    public class NetworkInterface
    {
        public string Id;

        public delegate void Delegate(string[] request);
        Delegate Handler;

        public NetworkInterface(string id, Delegate handler)
        {
            this.Id = id;
            this.Handler = handler;
            
            HomeAutomationClient.client.NetworkInterfaces.Add(this);
            Console.WriteLine("registering interface: " + this.Id);
        }

        public void Run(string[] request)
        {
            this.Handler(request);
        }
    }
}