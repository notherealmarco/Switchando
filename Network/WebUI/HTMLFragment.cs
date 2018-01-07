using HomeAutomationCore;

namespace HomeAutomation.Network.WebUI
{
    public class HTMLFragment
    {
        public HTMLFragment(string objectModel, string path)
        {
            HomeAutomationServer.server.HTMLFragments.Add(objectModel, path);
        }
    }
}