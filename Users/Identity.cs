using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects;
using HomeAutomation.Rooms;
using HomeAutomationCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace HomeAutomation.Users
{
    public class Identity
    {
        public string Name;
        public UserType Rank;

        public List<string> Permissions;
        public List<string> Devices;
        public List<string> Rooms;

        public string Password;
        private List<string> Tokens;
        private List<string> IPs;
        public byte WrongAttempts;
        private SHA256 SHA256Hash;
                        
        public Identity()
        {
            SHA256Hash = SHA256Managed.Create();
            this.Permissions = new List<string>();
            this.Devices = new List<string>();
            this.Rooms = new List<string>();
            this.Tokens = new List<string>();
            this.IPs = new List<string>();
        }
        public Identity(string name, string password, UserType rank)
        {
            SHA256Hash = SHA256Managed.Create();
            this.Name = name;
            this.Password = Hash(password);
            this.Rank = rank;
            this.Tokens = new List<string>();
            this.IPs = new List<string>();

            if (rank == UserType.RESTRICTED)
            {
                this.Permissions = new List<string>();
                this.Devices = new List<string>();
                this.Rooms = new List<string>();
            }
            HomeAutomationServer.server.Identities.Add(this);
        }
        public void AddDevice(IObject device)
        {
            Devices.Add(device.GetName());
        }
        public void RemoveDevice(IObject device)
        {
            Devices.Remove(device.GetName());
        }
        public void AddDevice(string device)
        {
            Devices.Add(device);
        }
        public void RemoveDevice(string device)
        {
            Devices.Remove(device);
        }
        public void AddRoom(Room room)
        {
            Rooms.Add(room.Name);
        }
        public void RemoveRoom(Room room)
        {
            Rooms.Remove(room.Name);
        }
        public void AddRoom(string room)
        {
            Rooms.Add(room);
        }
        public void RemoveRoom(string room)
        {
            Rooms.Remove(room);
        }
        public void AddPermission(string pex)
        {
            Permissions.Add(pex);
        }
        public void RemovePermission(string pex)
        {
            Permissions.Remove(pex);
        }
        public bool HasAccess(IObject device)
        {
            if (Rank == UserType.ADMINISTRATOR || Rank == UserType.STANDARD) return true;
            if (Devices.Contains(device.GetName())) return true;
            else return false;
        }
        public bool HasAccess(Room room)
        {
            if (Rank == UserType.ADMINISTRATOR || Rank == UserType.STANDARD) return true;
            if (Rooms.Contains(room.Name)) return true;
            else return false;
        }
        public bool HasPermission(string pex)
        {
            if (Permissions.Contains(pex)) return true;
            else return false;
        }
        public static Identity StaticSessionLogin(string name, string token)
        {
            foreach (Identity identity in HomeAutomationServer.server.Identities)
            {
                if (identity.SessionLogin(name, token))
                {
                    return identity;
                }
            }
            return null;
        }
        public static Identity StaticSessionLogin(string name, string token, string IP)
        {
            foreach (Identity identity in HomeAutomationServer.server.Identities)
            {
                if (identity.SessionLogin(name, token, IP))
                {
                    return identity;
                }
            }
            return null;
        }
        public bool SessionLogin(string name, string token)
        {
            if (name.Equals(this.Name))
            {
                if (this.Tokens.Contains(token))
                {
                    return true;
                }
                else
                {
                    //if (this.Tokens.Count != 0) this.Tokens = new List<string>();
                    return false;
                }
            }
            return false;
        }
        public bool SessionLogin(string name, string token, string IP)
        {
            var ipNoPort = IP.Split(':')[0];
            if (name.Equals(this.Name))
            {
                if (this.Tokens.Contains(token))
                {
                    if (!IPs.Contains(ipNoPort)) IPs.Add(ipNoPort);
                    return true;
                }
                else
                {
                    if (!IPs.Contains(ipNoPort))
                    {
                        //if (this.Tokens.Count != 0) this.Tokens = new List<string>();
                    }
                    return false;
                }
            }
            return false;
        }
        public string GenerateNewToken()
        {
            byte[] bytes = new byte[128];
            var rng = new RNGCryptoServiceProvider();
            rng.GetNonZeroBytes(bytes);
            string token = BitConverter.ToString(bytes);
            Tokens.Add(token);
            return token;
        }
        public static Identity StaticLogin(string name, string password)
        {
            foreach(Identity identity in HomeAutomationServer.server.Identities)
            {
                if (identity.Login(name, password))
                {
                    return identity;
                }
            }
            return null;
        }
        public bool Login(string name, string password)
        {
            //if (WrongAttempts > 15) return false;
            if (name.Equals(this.Name))
            {
                if (Hash(password).Equals(this.Password))
                {
                    return true;
                }
                else
                {
                    WrongAttempts += 1;
                }
            }
            return false;
        }
        public void DestroyUser()
        {
            HomeAutomationServer.server.Identities.Remove(this);
        }
        public bool IsStandardRank()
        {
            if (Rank == UserType.STANDARD || Rank == UserType.ADMINISTRATOR) return true;
            else return false;
        }
        public bool IsAdmin()
        {
            if (Rank == UserType.ADMINISTRATOR) return true;
            else return false;
        }
        public static Identity GetAdminUser()
        {
            foreach(Identity identity in HomeAutomationServer.server.Identities)
            {
                if (identity.Name.Equals("admin")) return identity;
            }
            return null;
        }
        public static string SendParameters(string method, string[] request, Identity login)
        {

            if (method.Equals("createUser"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string username = null;
                string password = null;
                string usertype = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "username":
                            username = command[1];
                            break;
                        case "password":
                            password = command[1];
                            break;
                        case "user_type":
                            usertype = command[1];
                            break;
                    }
                }
                if (string.IsNullOrEmpty(username)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (string.IsNullOrEmpty(password)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (string.IsNullOrEmpty(usertype)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                UserType rank = (UserType)Enum.Parse(typeof(UserType), usertype, true);

                Identity identity = new Identity(username, password, rank);
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.identity = identity;
                return data.Json();
            }
            if (method.Equals("addDevice"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string username = null;
                string device = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "username":
                            username = command[1];
                            break;
                        case "device":
                            device = command[1];
                            break;
                    }
                }
                if (string.IsNullOrEmpty(username)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (string.IsNullOrEmpty(device)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                foreach(Identity identity in HomeAutomationServer.server.Identities)
                {
                    if (identity.Name.Equals(username))
                    {
                        identity.AddDevice(device);

                        ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                        data.Object.identity = identity;
                        return data.Json();
                    }
                }
                return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
            }
            if (method.Equals("addPermission"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string username = null;
                string permission = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "username":
                            username = command[1];
                            break;
                        case "permission":
                            permission = command[1];
                            break;
                    }
                }
                if (string.IsNullOrEmpty(username)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (string.IsNullOrEmpty(permission)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                foreach (Identity identity in HomeAutomationServer.server.Identities)
                {
                    if (identity.Name.Equals(username))
                    {
                        identity.AddPermission(permission);

                        ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                        data.Object.identity = identity;
                        return data.Json();
                    }
                }
                return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
            }
            if (method.Equals("removePermission"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string username = null;
                string permission = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "username":
                            username = command[1];
                            break;
                        case "permission":
                            permission = command[1];
                            break;
                    }
                }
                if (string.IsNullOrEmpty(username)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (string.IsNullOrEmpty(permission)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                foreach (Identity identity in HomeAutomationServer.server.Identities)
                {
                    if (identity.Name.Equals(username))
                    {
                        identity.RemovePermission(permission);

                        ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                        data.Object.identity = identity;
                        return data.Json();
                    }
                }
                return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
            }
            if (method.Equals("removeDevice"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string username = null;
                string device = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "username":
                            username = command[1];
                            break;
                        case "device":
                            device = command[1];
                            break;
                    }
                }
                if (string.IsNullOrEmpty(username)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (string.IsNullOrEmpty(device)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                foreach (Identity identity in HomeAutomationServer.server.Identities)
                {
                    if (identity.Name.Equals(username))
                    {
                        identity.RemoveDevice(device);

                        ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                        data.Object.identity = identity;
                        return data.Json();
                    }
                }
                return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
            }
            if (method.Equals("addRoom"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string username = null;
                string device = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "username":
                            username = command[1];
                            break;
                        case "device":
                            device = command[1];
                            break;
                    }
                }
                if (string.IsNullOrEmpty(username)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (string.IsNullOrEmpty(device)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                foreach (Identity identity in HomeAutomationServer.server.Identities)
                {
                    if (identity.Name.Equals(username))
                    {
                        identity.AddRoom(device);

                        ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                        data.Object.identity = identity;
                        return data.Json();
                    }
                }
                return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
            }
            if (method.Equals("removeRoom"))
            {
                if (!login.IsAdmin()) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Insufficient permissions").Json();
                string username = null;
                string device = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "username":
                            username = command[1];
                            break;
                        case "device":
                            device = command[1];
                            break;
                    }
                }
                if (string.IsNullOrEmpty(username)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (string.IsNullOrEmpty(device)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                foreach (Identity identity in HomeAutomationServer.server.Identities)
                {
                    if (identity.Name.Equals(username))
                    {
                        identity.RemoveRoom(device);

                        ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                        data.Object.identity = identity;
                        return data.Json();
                    }
                }
                return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
            }
            if (method.Equals("login"))
            {
                string username = null;
                string password = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "username":
                            username = command[1];
                            break;
                        case "password":
                            password = command[1];
                            break;
                    }
                }
                if (string.IsNullOrEmpty(username)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (string.IsNullOrEmpty(password)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                Identity identity = Identity.StaticLogin(username, password);
                if (identity == null) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Invalid username and / or password").Json();

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.identity = identity;
                return data.Json();
            }
            if (method.Equals("restoreSession"))
            {
                string username = null;
                string token = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "username":
                            username = command[1];
                            break;
                        case "token":
                            token = command[1];
                            break;
                    }
                }
                if (string.IsNullOrEmpty(username)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();
                if (string.IsNullOrEmpty(token)) return new ReturnStatus(CommonStatus.ERROR_BAD_REQUEST).Json();

                Identity identity = Identity.StaticSessionLogin(username, token);
                if (identity == null) return new ReturnStatus(CommonStatus.ERROR_FORBIDDEN_REQUEST, "Invalid username and / or token").Json();

                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.identity = identity;
                return data.Json();
            }
            if (method.Equals("openSession"))
            {
                ReturnStatus data = new ReturnStatus(CommonStatus.SUCCESS);
                data.Object.token = login.GenerateNewToken();
                data.Object.identity = login;
                return data.Json();
            }
            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED).Json();
        }
        string Hash(string randomString)
        {
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = SHA256Hash.ComputeHash(Encoding.UTF8.GetBytes(randomString), 0, Encoding.UTF8.GetByteCount(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
        public enum UserType
        {
            ADMINISTRATOR,
            STANDARD,
            RESTRICTED
        }
    }
}