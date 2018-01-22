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
        public List<KeyValuePair<string, string>> Actions;

        public Event() { }
        public Event(string name, string description, NetworkInterface deviceType)
        {
            this.Name = name;
            this.Description = description;
            this.DeviceType = deviceType.Id;
            this.Actions = new List<KeyValuePair<string, string>>();
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
            bool exists = false;
            foreach(KeyValuePair<string, string> kvp in Actions)
            {
                if (kvp.Key.Equals(device.GetName()))
                {
                    if (kvp.Value.Equals(action.Name)) exists = true;
                }
            }
            if (!exists) Actions.Add(new KeyValuePair<string, string>(device.GetName(), action.Name));
        }
        public void RemoveAction(IObject device, Action action)
        {
            KeyValuePair<string, string> toRemove = new KeyValuePair<string, string>("", "");
            foreach (KeyValuePair<string, string> kvp in Actions)
            {
                if (kvp.Key.Equals(device.GetName()))
                {
                    if (kvp.Value.Equals(action.Name)) toRemove = kvp;
                }
            }
            if (!toRemove.Key.Equals("")) Actions.Remove(toRemove);
        }
        public NetworkInterface GetInterface()
        {
            return NetworkInterface.FromId(DeviceType);
        }
    }
}