using HomeAutomation.Objects;
using HomeAutomation.Objects.Lights;
namespace HomeAutomation.ServerRetriver
{
    class HomeAutomationModel
    {
        public uint PinR, PinG, PinB;
        public uint ValueR, ValueG, ValueB, Brightness;
        uint PauseR, PauseG, PauseB;
        public bool Switch;

        public string Name;
        public string[] FriendlyNames;
        public string Description;

        public uint Pin;
        public uint Value;
        public uint PauseValue;

        public HomeAutomationObject ObjectType;
        public LightType LightType;

        public bool Enabled;

        public string ClientName;
        public string[] Commands;
        public string[] CommandsOn;
        public string[] CommandsOff;
        public string[] Objects;
    }
}
