namespace Shared.Calibration
{
    public class Jitter
    {
        public uint IdJoy { get; set; }
        public byte IdAxis { get; set; }
        public byte Antiv { get; set; }
        public byte Margin { get; set; }
        public byte Strength { get; set; }
    }
}
