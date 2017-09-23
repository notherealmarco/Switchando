 namespace HomeAutomation.Objects.External.Plugins
{
    public interface IPlugin
    {
        string GetDescription();
        string GetName();
        string GetDeveloper();
        string OnEnable();
    }
}