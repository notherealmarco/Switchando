using HomeAutomation.Network;
using HomeAutomation.ObjectInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation.Objects
{
    public interface IObject
    {
        string GetName();
        string GetObjectType();
        NetworkInterface GetInterface();
        string[] GetFriendlyNames();
    }
}
