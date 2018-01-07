using HomeAutomationCore;
using System;

namespace HomeAutomation.ConfigRetriver
{
    public class Configuration
    {
        public string Id;
        public delegate string Delegate(string[] data);
        Delegate Handler;

        public Configuration(string id, Delegate handler)
        {
            this.Id = id;
            this.Handler = handler;
            Console.WriteLine("Registering setup tool for " + id);
            HomeAutomationServer.server.Configs.Add(this);
        }

        public string Run(string[] data)
        {
            return this.Handler(data);
        }
    }
}