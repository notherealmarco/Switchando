using HomeAutomation.Objects;

namespace HomeAutomation.ObjectInterfaces
{
    public class Condition
    {
        public ObjectInterface Property;
        public object Value;
        public string SwitchandoObject;
        public Condition(string device, ObjectInterface property, object value)
        {
            this.SwitchandoObject = device;
            this.Property = property;
            this.Value = value;
        }
        public bool Verify()
        {
            foreach (IObject iobj in HomeAutomationCore.HomeAutomationServer.server.Objects)
            {
                if (iobj.GetName().Equals(SwitchandoObject))
                {
                    object vle = ObjectInterface.GetPropertyValue(iobj, Property.Name);
                    if (vle.Equals(Value))
                    {
                        return true;
                    }
                    else
                    {
                        if (vle.ToString().ToLower().Equals(Value.ToString().ToLower()))
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }
            return false;
        }
    }
}