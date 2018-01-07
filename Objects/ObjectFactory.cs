using HomeAutomationCore;

namespace HomeAutomation.Objects
{
    public class ObjectFactory
    {
        public static IObject FromString(string name)
        {
            foreach (IObject iobj in HomeAutomationServer.server.Objects)
            {
                if (iobj.GetName().Equals(name)) return iobj;
            }
            return null;
        }
    }
}