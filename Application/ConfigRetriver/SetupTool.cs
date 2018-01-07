using HomeAutomation.Rooms;
using HomeAutomationCore;
using System;

namespace HomeAutomation.Application.ConfigRetriver
{
    public class SetupTool
    {
        public string Id;
        public delegate void Delegate(Room room, dynamic device);
        Delegate Handler;

        public SetupTool(string id, Delegate handler)
        {
            this.Id = id;
            this.Handler = handler;
            Console.WriteLine("Registering setup tool for " + id);
            HomeAutomationServer.server.Setups.Add(this);
        }

        public void Run(Room room, dynamic device)
        {
            this.Handler(room, device);
        }

        public static bool Exists(string id)
        {
            foreach (SetupTool sTool in HomeAutomationServer.server.Setups)
            {
                if (sTool.Id.Equals(id)) return true;
            }
            return false;
        }
        public static SetupTool FromId(string id)
        {
            foreach (SetupTool sTool in HomeAutomationServer.server.Setups)
            {
                if (sTool.Id.Equals(id)) return sTool;
            }
            return null;
        }
    }
}