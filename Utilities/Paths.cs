using HomeAutomation.Objects.External.Plugins;
using System.IO;
using System.Reflection;

namespace HomeAutomation.Utilities
{
    public static class Paths
    {
        public static string GetServerPath()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }
        public static string GetPluginDirectory(string plugin)
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/plugins/" + plugin;
        }
        public static string GetPluginDirectory(IPlugin plugin)
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/plugins/" + plugin.GetName();
        }
    }
}