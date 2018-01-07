using HomeAutomation.Network;
using HomeAutomationCore;
using HomeAutomation.Network.APIStatus;
using System.Collections.Generic;
using HomeAutomation.Objects;
using HomeAutomation.Users;

namespace HomeAutomation.ObjectInterfaces
{
    public class MethodInterface
    {

        public string Interface;
        public string Name;
        public List<MethodParameter> Parameters;
        public string Description;
        
        public MethodInterface() { }
        public MethodInterface(NetworkInterface networkInterface, string name, string description)
        {
            this.Interface = networkInterface.Id;
            this.Name = name;
            this.Description = description;
            this.Parameters = new List<MethodParameter>();
            HomeAutomationServer.server.ObjectNetwork.MethodInterfaces.Add(this); //TODO
        }
        public void AddParameter(MethodParameter parameter)
        {
            Parameters.Add(parameter);
        }
        public static MethodInterface[] GetMethodsFromObject(IObject obj)
        {
            List<MethodInterface> methods = new List<MethodInterface>();
            NetworkInterface networkInterface = obj.GetInterface();
            foreach (MethodInterface method in HomeAutomationServer.server.ObjectNetwork.MethodInterfaces)
            {
                if (networkInterface.Id.Equals(method.Interface))
                {
                    methods.Add(method);
                }
            }
            return methods.ToArray();
        }
        public static MethodInterface[] GetMethodsFromInterface(string netInterface)
        {
            List<MethodInterface> methods = new List<MethodInterface>();
            NetworkInterface networkInterface = NetworkInterface.FromId(netInterface);
            foreach (MethodInterface method in HomeAutomationServer.server.ObjectNetwork.MethodInterfaces)
            {
                if (networkInterface.Id.Equals(method.Interface))
                {
                    methods.Add(method);
                }
            }
            return methods.ToArray();
        }
        public string Run(Dictionary<string, object> request, Identity user)
        {
            foreach (MethodInterface mi in HomeAutomationServer.server.ObjectNetwork.MethodInterfaces) continue;
            List<string> parameters = new List<string>();
            foreach(MethodParameter parameter in Parameters)
            {
                object parameter_value;
                if (!request.TryGetValue(parameter.Name, out parameter_value))
                {
                    return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Parameter " + parameter.Name + " not found").Json();
                }
                parameters.Add(parameter.Name + "=" + parameter_value);
            }
            return NetworkInterface.FromId(Interface).Run(Name, parameters.ToArray(), user);
        }
        public string GetRequest(Dictionary<string, object> request)
        {
            List<string> parameters = new List<string>();
            foreach (MethodParameter parameter in Parameters)
            {
                object parameter_value;
                if (!request.TryGetValue(parameter.Name, out parameter_value))
                {
                    return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Parameter " + parameter.Name + " not found").Json();
                }
                parameters.Add(parameter.Name + "=" + parameter_value);
            }
            return Interface + "/" + Name + "?" + string.Join("&", parameters);
        }
        public static MethodInterface FromString(string methodInterface, string method)
        {
            foreach (MethodInterface methodobj in HomeAutomationServer.server.ObjectNetwork.MethodInterfaces)
            {
                if (methodobj.Interface.Equals(NetworkInterface.FromId(methodInterface).Id))
                {
                    if (methodobj.Name.Equals(method))
                    {
                        return methodobj;
                    }
                }
            }
            return null;
        }
        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("getMethods/device"))
            {
                IObject obj = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            foreach (IObject iobj in HomeAutomationServer.server.Objects)
                            {
                                if (iobj.GetName().Equals(command[1]))
                                {
                                    obj = iobj;
                                }
                            }
                            break;
                    }
                }
                if (obj == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.methods = GetMethodsFromObject(obj);
                return data.Json();
            }
            if (method.Equals("getMethods/interface"))
            {
                string networkInterface = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            networkInterface = command[1];
                            break;
                    }
                }
                if (networkInterface == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.methods = GetMethodsFromInterface(networkInterface);
                return data.Json();
            }
            if (method.StartsWith("runMethod/device"))
            {
                string[] dataRow = method.Substring("runMethod/device/".Length).Split('/');

                if (dataRow == null || dataRow[0] == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                IObject obj = null;
                string methodName = null;

                foreach (IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (iobj.GetName().Equals(dataRow[0]))
                    {
                        obj = iobj;
                    }
                }
                methodName = dataRow[1];

                if (obj == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Device not found").Json();
                if (methodName == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                NetworkInterface networkInterface = obj.GetInterface();
                MethodInterface methodInterface = null;

                foreach (MethodInterface methodInt in HomeAutomationServer.server.ObjectNetwork.MethodInterfaces)
                {
                    if (methodInt.Interface.Equals(networkInterface))
                    {
                        if (methodInt.Name.Equals(methodName))
                        {
                            methodInterface = methodInt;
                        }
                    }
                }
                if (methodInterface == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Method not found").Json();

                Dictionary<string, object> parameters = new Dictionary<string, object>();

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    parameters.Add(command[0], command[1]);
                }

                return methodInterface.Run(parameters, login);
            }
            if (method.StartsWith("runMethod/interface"))
            {
                string[] dataRow = method.Substring("runMethod/interface/".Length).Split('/');

                if (dataRow == null || dataRow[0] == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                NetworkInterface networkInterface = null;
                string methodName = null;

                foreach (NetworkInterface networkInt in HomeAutomationServer.server.NetworkInterfaces)
                {
                    if (networkInt.Id.Equals(dataRow[0]))
                    {
                        networkInterface = networkInt;
                    }
                }
                methodName = dataRow[1];

                if (networkInterface == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "API Interface not found").Json();
                if (methodName == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                MethodInterface methodInterface = null;

                foreach (MethodInterface methodInt in HomeAutomationServer.server.ObjectNetwork.MethodInterfaces)
                {
                    if (methodInt.Interface.Equals(networkInterface))
                    {
                        if (methodInt.Name.Equals(methodName))
                        {
                            methodInterface = methodInt;
                        }
                    }
                }
                if (methodInterface == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Method not found").Json();

                Dictionary<string, object> parameters = new Dictionary<string, object>();

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    parameters.Add(command[0], command[1]);
                }

                return methodInterface.Run(parameters, login);
            }

            if (method.StartsWith("getCommand/device"))
            {
                string[] dataRow = method.Substring("getCommand/device/".Length).Split('/');

                if (dataRow == null || dataRow[0] == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                IObject obj = null;
                string methodName = null;

                foreach (IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (iobj.GetName().Equals(dataRow[0]))
                    {
                        obj = iobj;
                    }
                }
                methodName = dataRow[1];

                if (obj == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Device not found").Json();
                if (methodName == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                NetworkInterface networkInterface = obj.GetInterface();
                MethodInterface methodInterface = null;

                foreach (MethodInterface methodInt in HomeAutomationServer.server.ObjectNetwork.MethodInterfaces)
                {
                    if (methodInt.Interface.Equals(networkInterface.Id))
                    {
                        if (methodInt.Name.Equals(methodName))
                        {
                            methodInterface = methodInt;
                        }
                    }
                }
                if (methodInterface == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Method not found").Json();

                Dictionary<string, object> parameters = new Dictionary<string, object>();

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    parameters.Add(command[0], command[1]);
                }

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.command = methodInterface.GetRequest(parameters);
                return data.Json();
            }
            if (method.StartsWith("getCommand/interface"))
            {
                string[] dataRow = method.Substring("getCommand/interface/".Length).Split('/');

                if (dataRow == null || dataRow[0] == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                NetworkInterface networkInterface = null;
                string methodName = null;

                foreach (NetworkInterface networkInt in HomeAutomationServer.server.NetworkInterfaces)
                {
                    if (networkInt.Id.Equals(dataRow[0]))
                    {
                        networkInterface = networkInt;
                    }
                }
                methodName = dataRow[1];

                if (networkInterface == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "API Interface not found").Json();
                if (methodName == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                MethodInterface methodInterface = null;

                foreach (MethodInterface methodInt in HomeAutomationServer.server.ObjectNetwork.MethodInterfaces)
                {
                    if (methodInt.Interface.Equals(networkInterface))
                    {
                        if (methodInt.Name.Equals(methodName))
                        {
                            methodInterface = methodInt;
                        }
                    }
                }
                if (methodInterface == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND, "Method not found").Json();

                Dictionary<string, object> parameters = new Dictionary<string, object>();

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    parameters.Add(command[0], command[1]);
                }

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.command = methodInterface.GetRequest(parameters);
                return data.Json();
            }
            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED, "Not implemented").Json();
        }
    }
}
