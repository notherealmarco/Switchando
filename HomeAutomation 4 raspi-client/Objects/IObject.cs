namespace HomeAutomation.Objects
{
    public interface IObject
    {
        string GetName();
        HomeAutomationObject GetObjectType();
    }
    public enum HomeAutomationObject
    {
        LIGHT,
        FAN,
        GENERIC_SWITCH,
        ROOM,
        BUTTON,
        SWITCH_BUTTON
    }
}
