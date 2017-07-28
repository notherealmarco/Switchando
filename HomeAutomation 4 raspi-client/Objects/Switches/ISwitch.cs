namespace HomeAutomation.Objects.Switches
{
    public interface ISwitch : IObject
    {
        void Start();
        void Stop();
        bool IsOn();
    }
}