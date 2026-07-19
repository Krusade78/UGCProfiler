using System.Runtime.InteropServices;

namespace API
{
    public partial class Vjoy
    {
        public enum VjdStat : uint
        {
            VJD_STAT_OWN = 0,
            VJD_STAT_FREE = 1,
            VJD_STAT_BUSY = 2,
            VJD_STAT_MISS = 3,
            VJD_STAT_UNKN = 4
        }

        [LibraryImport("vJoyInterface.dll", EntryPoint = "vJoyEnabled")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool vJoyEnabled();

        [LibraryImport("vJoyInterface.dll", EntryPoint = "isVJDExists")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool isVJDExists(uint rID);

        [LibraryImport("vJoyInterface.dll", EntryPoint = "GetVJDStatus")]
        public static partial VjdStat GetVJDStatus(uint rID);

        [LibraryImport("vJoyInterface.dll", EntryPoint = "AcquireVJD")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool AcquireVJD(uint rID);

        [LibraryImport("vJoyInterface.dll", EntryPoint = "RelinquishVJD")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool RelinquishVJD(uint rID);

        [LibraryImport("vJoyInterface.dll", EntryPoint = "UpdateVJD")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool UpdateVJD(
            uint rID,
            ref JOYSTICK_POSITION_V2 pData
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct JOYSTICK_POSITION_V2
        {
            public byte bDevice;
            public int wThrottle;
            public int wRudder;
            public int wAileron;
            public int wAxisX;
            public int wAxisY;
            public int wAxisZ;
            public int wAxisXRot;
            public int wAxisYRot;
            public int wAxisZRot;
            public int wSlider;
            public int wDial;
            public int wWheel;
            public int wAxisVX;
            public int wAxisVY;
            public int wAxisVZ;
            public int wAxisVBRX;
            public int wAxisVBRY;
            public int wAxisVBRZ;

            public uint lButtons;
            public uint bHats;
            public uint bHatsEx1;
            public uint bHatsEx2;
            public uint bHatsEx3;

            public int lButtonsEx1; // Buttons 33-64
            public int lButtonsEx2; // Buttons 65-96
            public int lButtonsEx3; // Buttons 97-128
        }
    }
}
