using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects;
using HomeAutomation.Users;
using HomeAutomationCore;
using Switchando.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation.Events
{
    public class EventsManager
    {
        private List<Event> Events;
        public EventsManager()
        {
            this.Events = HomeAutomationServer.server.Database.Get<List<Event>>("switchando.events");
            if (this.Events == null) this.Events = new List<Event>();
        }
        public void RegisterEvent(Event eventObj)
        {
            Events.Add(eventObj);
        }
        public Event GetEvent(IObject device, string name)
        {
            foreach (Event evnt in Events)
            {
                if (evnt.GetInterface() == device.GetInterface())
                {
                    if (evnt.Name.Equals(name)) return evnt;
                }
            }
            return null;
        }
        public List<Event> GetEvents(IObject device)
        {
            List<Event> data = new List<Event>();
            NetworkInterface deviceType = device.GetInterface();
            foreach(Event evnt in Events)
            {
                if (evnt.GetInterface() == deviceType) data.Add(evnt);
            }
            return data;
        }
        public void Save()
        {
            HomeAutomationServer.server.Database.Put("switchando.events", Events);
        }

        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("getEvents/device"))
            {
                IObject device = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            device = ObjectFactory.FromString(command[1]);
                            break;
                    }
                }
                if (device == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Device not found").Json();
                var data = HomeAutomationServer.server.Events.GetEvents(device);
                var json = new ReturnStatus(CommonStatus.SUCCESS);
                json.Object.events = data;
                return json.Json();
            }
            if (method.Equals("addAction"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                IObject device = null;
                HomeAutomation.ObjectInterfaces.Action action = null;
                string evnt = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "objname":
                            device = ObjectFactory.FromString(command[1]);
                            break;
                        case "event":
                            evnt = command[1];
                            break;
                        case "action":
                            foreach(HomeAutomation.ObjectInterfaces.Action act in HomeAutomationServer.server.Actions)
                            {
                                if (act.Name.Equals(command[1])) action = act;
                            }
                            break;
                    }
                }
                if (device == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Device not found").Json();
                if (action == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Action not found").Json();

                foreach (Event ev in HomeAutomationServer.server.Events.GetEvents(device))
                {
                    if (ev.Name.Equals(evnt))
                    {
                        ev.AddAction(device, action);
                        return new ReturnStatus(CommonStatus.SUCCESS).Json();
                    }
                }
                return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Event not found").Json();
            }
            if (method.Equals("removeAction"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                IObject device = null;
                HomeAutomation.ObjectInterfaces.Action action = null;
                string evnt = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "objname":
                            device = ObjectFactory.FromString(command[1]);
                            break;
                        case "event":
                            evnt = command[1];
                            break;
                        case "action":
                            foreach (HomeAutomation.ObjectInterfaces.Action act in HomeAutomationServer.server.Actions)
                            {
                                if (act.Name.Equals(command[1])) action = act;
                            }
                            break;
                    }
                }
                if (device == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Device not found").Json();
                if (action == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Action not found").Json();

                foreach (Event ev in HomeAutomationServer.server.Events.GetEvents(device))
                {
                    if (ev.Name.Equals(evnt))
                    {
                        ev.RemoveAction(device, action);
                        return new ReturnStatus(CommonStatus.SUCCESS).Json();
                    }
                }
                return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Event not found").Json();
            }
            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED).Json();
        }
    }
}
