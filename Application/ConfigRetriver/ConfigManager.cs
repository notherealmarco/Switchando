using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace HomeAutomation.ConfigRetriver
{
    public class ConfigManager
    {
        private string FilePath;
        private Dictionary<string, object> Database;

        public ConfigManager(string path)
        {
            this.FilePath = path;
            Load();
        }
        public void Load()
        {
            if (!File.Exists(FilePath))
            {
                this.Database = new Dictionary<string, object>();
                return;
            }
            string json = File.ReadAllText(FilePath);
            Database = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
        public void Save()
        {
            string json = JsonConvert.SerializeObject(Database);
            File.WriteAllText(FilePath, json);
        }
        public void Put(string path, object value)
        {
            if (Database.ContainsKey(path))
            {
                Database.Remove(path);
            }
            Database.Add(path, value);
            Save();
        }
        public void Put(string path, object value, bool save)
        {
            if (Database.ContainsKey(path))
            {
                Database.Remove(path);
            }
            Database.Add(path, value);
            if (save) Save();
        }
        public object Get(string path)
        {
            object value;
            if (!Database.TryGetValue(path, out value))
            {
                return null;
            }
            return value;
        }
        public T Get<T>(string path)
        {
            object value;
            if (!Database.TryGetValue(path, out value))
            {
                return default(T);
            }
            if (value is T)
            {
                return (T)value;
            }
            else
            {
                try
                {
                    try
                    {
                        return ((dynamic)value).ToObject<T>();
                    }
                    catch
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                }
                catch
                {
                    return default(T); //editMe
                }
            }
        }
        public string GetPath()
        {
            return FilePath;
        }
    }
}