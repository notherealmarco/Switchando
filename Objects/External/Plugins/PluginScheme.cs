using HomeAutomation.Utilities;

namespace Switchando.Objects.External.Plugins
{
    class PluginScheme
    {
        public string Name;
        public string Description;
        public string Developer;
        public string ConfigIndex;
        public PluginScheme(string name, string description, string developer)
        {
            Name = name;
            Description = description;
            Developer = developer;
            ConfigIndex = "/plugins/" + name + "/index.html";
        }
        public PluginScheme(string name, string description, string developer, string configIndex)
        {
            Name = name;
            Description = description;
            Developer = developer;
            ConfigIndex = configIndex;
        }
    }
}
