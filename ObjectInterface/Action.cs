using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects;
using HomeAutomation.Users;
using HomeAutomationCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HomeAutomation.ObjectInterfaces
{
    public class Action
    {
        public string Name;
        public string Description;
        public MethodInterface Method;
        public Dictionary<string, object> Parameters;
        public Condition[] Conditions;

        public Action() { }
        public Action(string name, string description, MethodInterface methodInterface, Dictionary<string, object> parameters, Condition[] conditions)
        {
            this.Name = name;
            this.Description = description;
            this.Method = methodInterface;
            this.Parameters = parameters;
            this.Conditions = conditions;
            HomeAutomationServer.server.Actions.Add(this);
        }
        public string Run(Identity login)
        {
            foreach(Condition condition in Conditions)
            {
                if (!condition.Verify())
                {
                    return new ReturnStatus(CommonStatus.SUCCESS, "Action didn't run because of some unverified conditions").Json();
                }
            }
            return Method.Run(Parameters, login);
        }
        public static Action FromName(string name)
        {
            foreach(Action action in HomeAutomationServer.server.Actions)
            {
                if (action.Name.Equals(name)) return action;
            }
            return null;
        }
        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("createAction"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                /*string[] dataRow = method.Substring("createAction/".Length).Split('/');
                if (dataRow == null || dataRow[0] == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                string name = dataRow[0];*/
                string name = null;
                string description = null;

                string methodInterface = null;
                string device = null;
                string methodName = null;
                string jsonParameters = null;
                string jsonConditions = null;
                MethodInterface actionMethod = null;

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
                        case "interface":
                            methodInterface = command[1];
                            break;
                        case "device":
                            device = command[1];
                            break;
                        case "method":
                            methodName = command[1];
                            break;
                        case "parameters":
                            jsonParameters = command[1];
                            break;
                        case "conditions":
                            jsonConditions = cmd.Substring(command[0].Length + 1);
                            break;
                    }
                }
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                if (methodInterface != null)
                {
                    actionMethod = MethodInterface.FromString(methodInterface, methodName);
                }
                else
                {
                    IObject iobj = ObjectFactory.FromString(device);
                    if (iobj == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Device not found").Json();
                    var methods = MethodInterface.GetMethodsFromObject(iobj);
                    foreach (MethodInterface mthd in methods)
                    {
                        if (mthd.Name.Equals(methodName)) actionMethod = mthd;
                    }
                }
                if (actionMethod == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Method not found").Json();

                Dictionary<string, object> parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonParameters);

                //List<Condition> conditions = new List<Condition>();
                Condition[] conditions = new Condition[0];
                if (!string.IsNullOrEmpty(jsonConditions))
                {
                    jsonConditions = Regex.Unescape(jsonConditions);
                    //jsonConditions = jsonConditions.Replace(@"\", string.Empty);
                    conditions = JsonConvert.DeserializeObject<Condition[]>(jsonConditions);
                   /* foreach (dynamic cond in conds)
                    {
                        ObjectInterface objInt = (ObjectInterface)cond.Property;
                        object value = cond.Value;
                        string swObj = (string)cond.SwitchandoObject;
                        conditions.Add(new Condition(swObj, objInt, value));
                    }*/
                }

                Action action = new Action(name, description, actionMethod, parameters, conditions.ToArray());

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.action = action;
                return data.Json();
            }
            if (method.Equals("getAction"))
            {
                Action action = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            action = Action.FromName(command[1]);
                            break;
                    }
                }
                if (action == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.action = action;
                return data.Json();
            }
            if (method.Equals("getActions"))
            {
                List<string> actions = new List<string>();
                foreach(Action action in HomeAutomationServer.server.Actions)
                {
                    actions.Add(action.Name);
                }
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.actions = actions;
                return data.Json();
            }

            if (method.StartsWith("runAction/name"))
            {
                Action action = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            action = Action.FromName(command[1]);
                            break;
                    }
                }
                if (action == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();

                return action.Run(login);
            }

            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED, "Not implemented").Json();
        }
    }
}
