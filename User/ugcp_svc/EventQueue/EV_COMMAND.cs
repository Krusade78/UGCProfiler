using System.Runtime.InteropServices;


namespace ugcp_svc.EventQueue
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct EV_COMMAND
    {
        public CommandType Type;
        public EV_COMMAND_UNION Data;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    struct EV_COMMAND_UNION
    {
        [FieldOffset(0)]
        public BasicCommand Basic;

        [FieldOffset(0)]
        public AxisPreciseCommand AxisPrecise;

        [FieldOffset(0)]
        public VHidCommand VHid;

        [FieldOffset(0)]
        public ExtendedCommand Extended;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BasicCommand
    {
        public byte Data1;
        public byte Data2;
        public byte Extra;
        public byte OutputJoy;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AxisPreciseCommand
    {
        public uint InputJoy;
        public byte OnOff;
        public byte Axis;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VHidCommand
    {
        public ushort Map;
        public byte OutputJoyId;

        public ProcessOutput.VHID_INPUT_DATA Data;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ExtendedCommand
    {
        public byte Origin;
        public byte Mode;
        public byte Submode;
        public byte Band;

        public ushort Incremental;
        public uint InputJoy;
    }
}
