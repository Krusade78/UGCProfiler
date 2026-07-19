using System;
using System.Runtime.InteropServices;


namespace API
{
    public partial class SetupDi
    {
        public const uint DIGCF_PRESENT = 0x2;
        public const uint DIGCF_DEVICEINTERFACE = 0x10;

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public Int32 cbSize;
            public Guid interfaceClassGuid;
            public Int32 flags;
            private readonly UIntPtr reserved;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int size;
            public char DevicePath;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid ClassGuid;
            public UInt32 DevInst;
            public IntPtr Reserved;
        }

        [LibraryImport("setupapi.dll", StringMarshalling = StringMarshalling.Utf16)]
        public static partial IntPtr SetupDiGetClassDevsW(
                                            ref Guid ClassGuid,
                                            [MarshalAs(UnmanagedType.LPWStr)] string Enumerator,
                                            IntPtr hwndParent,
                                            uint Flags
                                            );
        [LibraryImport(@"setupapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetupDiEnumDeviceInterfaces(
                                            IntPtr hDevInfo,
                                            IntPtr devInfo,
                                            ref Guid interfaceClassGuid,
                                            UInt32 memberIndex,
                                            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
                                            );
        [LibraryImport("setupapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetupDiDestroyDeviceInfoList(
                                            IntPtr DeviceInfoSet
                                            );
        [LibraryImport(@"setupapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetupDiGetDeviceInterfaceDetailW(
                                            IntPtr hDevInfo,
                                            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
                                            IntPtr deviceInterfaceDetailData,
                                            UInt32 deviceInterfaceDetailDataSize,
                                            out UInt32 requiredSize,
                                            IntPtr deviceInfoData
                                            );
    }
}
