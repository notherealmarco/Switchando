using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HomeAutomation.Objects.External.Plugins
{
    public class Plugins
    {
        public static List<IPlugin> PluginsList { get; set; }

        public static void LoadAll(string path)
        {
            PluginsList = new List<IPlugin>();

            //Load the DLLs from the Plugins directory
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    if (file.EndsWith(".dll"))
                    {
                        Assembly.LoadFile(Path.GetFullPath(file));
                    }
                }
            }

            Type interfaceType = typeof(IPlugin);
            //Fetch all types that implement the interface IPlugin and are a class
            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass)
                .ToArray();
            foreach (Type type in types)
            {
                //Create a new instance of all found types
                IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                Console.WriteLine("Registering plugin " + plugin.GetName() + " by " + plugin.GetDeveloper() + "...");
                PluginsList.Add(plugin);
                Console.WriteLine("Sucessfully loaded " + plugin.GetName());
                Console.WriteLine("[" + plugin.GetName() + "] (OnEnable) " + plugin.OnEnable());
            }
        }
    }
}