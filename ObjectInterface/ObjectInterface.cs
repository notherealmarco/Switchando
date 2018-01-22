using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects;
using HomeAutomation.Users;
using HomeAutomationCore;
using System;
using System.Collections.Generic;

namespace HomeAutomation.ObjectInterfaces
{
    public class ObjectInterface
    {
        public string Interface;
        public string Name;
        public Type Type;
        public string Description;

        public ObjectInterface() { }
        public ObjectInterface(NetworkInterface networkInterface, string name, Type type, string description)
        {
            this.Interface = networkInterface.Id;
            this.Name = name;
            this.Type = type;
            this.Description = description;
            HomeAutomationServer.server.ObjectNetwork.ObjectInterfaces.Add(this);
        }

        public static string[] GetPropertiesFromObject(IObject obj)
        {
            List<string> properties = new List<string>();
            NetworkInterface networkInterface = obj.GetInterface();
            foreach (ObjectInterface property in HomeAutomationServer.server.ObjectNetwork.ObjectInterfaces)
            {
                if (networkInterface.Id.Equals(property.Interface))
                {
                    properties.Add(property.Name);
                }
            }
            return properties.ToArray();
        }
        public static string[] GetPropertiesFromInterface(string networkInterface)
        {
            List<string> properties = new List<string>();
            foreach (ObjectInterface property in HomeAutomationServer.server.ObjectNetwork.ObjectInterfaces)
            {
                if (networkInterface.ToLower().Equals(property.Interface.ToLower()))
                {
                    properties.Add(property.Name);
                }
            }
            return properties.ToArray();
        }
        public static object GetObjectValue(IObject obj, string propName)
        {
            return obj.GetType().GetProperty(propName).GetValue(obj, null);
        }

        public static object GetPropertyValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
        public static ObjectInterface GetProperty(IObject obj, string propName)
        {
            NetworkInterface networkInterface = obj.GetInterface();
            foreach (ObjectInterface property in HomeAutomationServer.server.ObjectNetwork.ObjectInterfaces)
            {
                if (networkInterface.Id.Equals(property.Interface))
                {
                    if (property.Name.Equals(propName)) return property;
                }
            }
            return null;
        }
        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("getProperties/device"))
            {
                IObject obj = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            foreach(IObject iobj in HomeAutomationServer.server.Objects)
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
                data.Object.properties = GetPropertiesFromObject(obj);
                return data.Json();
            }
            if (method.Equals("getProperties/interface"))
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
                data.Object.properties = GetPropertiesFromInterface(networkInterface);
                return data.Json();
            }
            if (method.Equals("getProperty"))
            {
                IObject obj = null;
                string property = null;
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
                        case "property":
                            property = command[1];
                            break;
                    }
                }
                if (obj == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                if (property == null) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.property = GetProperty(obj, property);
                data.Object.value = GetPropertyValue(obj, property);
                data.Object.type = GetPropertyValue(obj, property).GetType();
                return data.Json();
            }
            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED).Json();
        }
    }
}