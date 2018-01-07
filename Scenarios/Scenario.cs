using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Users;
using HomeAutomationCore;
using System.Collections.Generic;

namespace HomeAutomation.Scenarios
{
    public class Scenario
    {
        public string Name;
        public string Description;
        public List<string> Actions;

        public Scenario(string name, string description)
        {
            this.Name = name;
            this.Description = description;
            HomeAutomationServer.server.Scenarios.Add(this);
        }

        public static void SaveAll()
        {
            //TODO
        }

        public void AddAction(string action)
        {
            this.Actions.Add(action);
            SaveAll();
        }

        public void RemoveAction(string action)
        {
            this.Actions.Remove(action);
            SaveAll();
        }

        public static void RemoveScenario(string name)
        {
            Scenario scenario = null;
            foreach (Scenario obj in HomeAutomationServer.server.Scenarios)
            {
                if (obj.Name.ToLower().Equals(name.ToLower()))
                {
                    scenario = obj;
                    break;
                }
            }
            HomeAutomationServer.server.Scenarios.Remove(scenario);
            SaveAll();
        }

        public void Run(Identity login)
        {
            foreach (string action in Actions)
            {
                ObjectInterfaces.Action.FromName(action).Run(login);
            }
        }

        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("createScenario"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string name = null;
                string description = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            name = command[1];
                            break;
                        case "description":
                            description = command[1];
                            break;
                    }
                }
                if (name == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                new Scenario(name, description);
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                return data.Json();
            }
            if (method.Equals("addAction"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                Scenario scenario = null;

                string name = null;
                string action = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            name = command[1];
                            break;
                        case "action":
                            action = command[1];
                            break;
                    }
                }
                foreach (Scenario obj in HomeAutomationServer.server.Scenarios)
                {
                    if (obj.Name.ToLower().Equals(name.ToLower()))
                    {
                        scenario = obj;
                        break;
                    }
                }
                if (scenario == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, name + " not found").Json();

                scenario.AddAction(action);

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                return data.Json();
            }
            if (method.Equals("removeAction"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                Scenario scenario = null;

                string name = null;
                string action = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            name = command[1];
                            break;
                        case "action":
                            action = command[1];
                            break;
                    }
                }
                foreach (Scenario obj in HomeAutomationServer.server.Scenarios)
                {
                    if (obj.Name.ToLower().Equals(name.ToLower()))
                    {
                        scenario = obj;
                        break;
                    }
                }
                if (scenario == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, name + " not found").Json();

                scenario.RemoveAction(action);

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                return data.Json();
            }
            if (method.Equals("run") || method.Equals("execute"))
            {
                Scenario scenario = null;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            foreach (Scenario obj in HomeAutomationServer.server.Scenarios)
                            {
                                if (obj.Name.ToLower().Equals(command[1].ToLower()))
                                {
                                    scenario = obj;
                                    break;
                                }
                            }
                            break;
                    }
                }
                if (scenario == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Scenario not found").Json();

                scenario.Run(login);

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                return data.Json();
            }
            return "";
        }
    }
}