using System;

namespace HomeAutomation.ObjectInterfaces
{
    public class MethodParameter
    {
        public string Name;
        public Type Type;
        public string Description;

        public MethodParameter(string name, Type type, string description)
        {
            this.Name = name;
            this.Type = type;
            this.Description = description;
        }
    }
}
