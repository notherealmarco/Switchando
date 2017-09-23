using HomeAutomationCore;
using System;

namespace HomeAutomation.Application.ConfigRetriver
{
    public class SetupTool
    {
        public string Id;
        public delegate void Delegate(dynamic device);
        Delegate Handler;

        public SetupTool(string id, Delegate handler)
        {
            this.Id = id;
            this.Handler = handler;
            Console.WriteLine("Registering setup tool for " + id);
            HomeAutomationClient.client.Setups.Add(this);
        }

        public void Run(dynamic device)
        {
            this.Handler(device);
        }

        public static bool Exists(string id)
        {
            foreach (SetupTool sTool in HomeAutomationClient.client.Setups)
            {
                if (sTool.Id.Equals(id)) return true;
            }
            return false;
        }
        public static SetupTool FromId(string id)
        {
            foreach (SetupTool sTool in HomeAutomationClient.client.Setups)
            {
                if (sTool.Id.Equals(id)) return sTool;
            }
            return null;
        }
    }
}
