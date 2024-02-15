namespace Shared.Calibration
{
    public class Limits
    {
        public uint IdJoy { get; set; }
        public byte IdAxis { get; set; }
        public byte Cal { get; set; }
        public byte Null { get; set; }
        public ushort Left { get; set; }
        public ushort Center { get; set; }
        public ushort Right { get; set; }
        public ushort Range { get; set; }
    }
}
