using HomeAutomation.Application.ConfigRetriver;
using HomeAutomation.Network;
using HomeAutomation.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomationCore
{
    public class HomeAutomationClient
    {
        public static HomeAutomationClient client;

        public List<IObject> Objects { get; set; }
        public List<NetworkInterface> NetworkInterfaces { get; set; }
        public List<SetupTool> Setups { get; set; }
        public TCPClient TcpClient { get; set; }

        public string ClientName;
        public string Password;

        public bool ConnectionEstabilished { get; set; }
        public HomeAutomationClient(string clientName)
        {
            this.ClientName = clientName;
            this.NetworkInterfaces = new List<NetworkInterface>();
            this.Setups = new List<SetupTool>();

            client = this;

            this.Objects = new List<IObject>();
        }
    }
}
