using HomeAutomationCore;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace HomeAutomation.Utilities
{
    static class ConfigurationFile
    {
        static void UpdateFile()
        {
            File.WriteAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/configuration.json", JsonConvert.SerializeObject(HomeAutomationServer.server.Rooms));
        }
    }
}