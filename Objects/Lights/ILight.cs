using HomeAutomation.Objects.Switches;

namespace HomeAutomation.Objects.Lights
{
    public interface ILight : ISwitch
    {
        void Dimm(uint percentage, int dimmerIntervals);
    }
    public interface IColorableLight : ILight
    {
        void Set(uint ValueR, uint ValueG, uint ValueB, int DimmerIntervals);
        double[] GetValues();
    }
}