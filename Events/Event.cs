using HomeAutomation.Network;
using HomeAutomation.ObjectInterfaces;
using HomeAutomation.Objects;
using HomeAutomation.Users;
using HomeAutomationCore;
using System.Collections.Generic;

namespace Switchando.Events
{
    public class Event
    {
        public string Name;
        public string Description;
        public string DeviceType;
        public Dictionary<string, string> Actions;

        public Event() { }
        public Event(string name, string description, NetworkInterface deviceType)
        {
            this.Name = name;
            this.Description = description;
            this.DeviceType = deviceType.Id;
            this.Actions = new Dictionary<string, string>();
            HomeAutomationServer.server.Events.RegisterEvent(this);
        }
        public void Throw(IObject device)
        {
            foreach (KeyValuePair<string, string> action in Actions)
            {
                if (action.Key.Equals(device.GetName())) Action.FromName(action.Value).Run(Identity.GetAdminUser());
            }
        }
        public void AddAction(IObject device, Action action)
        {
            if (!Actions.ContainsKey(device.GetName())) Actions.Add(device.GetName(), action.Name);
        }
        public void RemoveAction(IObject device, Action action)
        {
            if (Actions.ContainsKey(device.GetName())) Actions.Remove(action.Name);
        }
        public NetworkInterface GetInterface()
        {
            return NetworkInterface.FromId(DeviceType);
        }
    }
}