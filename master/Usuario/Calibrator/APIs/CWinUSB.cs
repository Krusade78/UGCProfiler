using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Calibrator
{
    internal static class CWinUSB
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public Int32 cbSize;
            public Guid interfaceClassGuid;
            public Int32 flags;
            private UIntPtr reserved;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int size;
            public char devicePath;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid ClassGuid;
            public UInt32 DevInst;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(
                                            ref Guid ClassGuid,
                                            [MarshalAs(UnmanagedType.LPWStr)] string Enumerator,
                                            IntPtr hwndParent,
                                            uint Flags
                                            );
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiEnumDeviceInterfaces(
                                            IntPtr hDevInfo,
                                            IntPtr devInfo,
                                            ref Guid interfaceClassGuid,
                                            UInt32 memberIndex,
                                            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
                                            );
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(
                                            IntPtr DeviceInfoSet
                                            );
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Boolean SetupDiGetDeviceInterfaceDetail(
                                            IntPtr hDevInfo,
                                            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
                                            IntPtr deviceInterfaceDetailData,
                                            UInt32 deviceInterfaceDetailDataSize,
                                            ref UInt32 requiredSize,
                                            IntPtr deviceInfoData
                                            );

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFileW(
                                             [MarshalAs(UnmanagedType.LPWStr)] string filename,
                                             [MarshalAs(UnmanagedType.U4)] UInt32 access,
                                             [MarshalAs(UnmanagedType.U4)] UInt32 share,
                                             IntPtr securityAttributes,
                                             [MarshalAs(UnmanagedType.U4)] UInt32 creationDisposition,
                                             [MarshalAs(UnmanagedType.U4)] UInt32 flagsAndAttributes,
                                             IntPtr templateFile);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hHandle);

        internal partial struct WINUSB_PIPE_INFORMATION
        {
            /// <summary>A <a href="https://docs.microsoft.com/windows-hardware/drivers/ddi/content/usb/ne-usb-_usbd_pipe_type">USBD_PIPE_TYPE</a>-type enumeration value that specifies the pipe type.</summary>
            internal USBD_PIPE_TYPE PipeType;
            /// <summary>The pipe identifier (ID).</summary>
            internal byte PipeId;
            /// <summary>The maximum size, in bytes, of the packets that are transmitted on the pipe.</summary>
            internal ushort MaximumPacketSize;
            /// <summary>The pipe interval.</summary>
            internal byte Interval;
        }
        internal enum USBD_PIPE_TYPE
        {
            UsbdPipeTypeControl = 0,
            UsbdPipeTypeIsochronous = 1,
            UsbdPipeTypeBulk = 2,
            UsbdPipeTypeInterrupt = 3,
        }

        [DllImport("winusb.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern Boolean WinUsb_Initialize(IntPtr DeviceHandle, out IntPtr InterfaceHandle);

        [DllImport("WINUSB.dll", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern Boolean WinUsb_QueryPipe(IntPtr InterfaceHandle, byte AlternateInterfaceNumber, byte PipeIndex, ref WINUSB_PIPE_INFORMATION PipeInformation);

        [DllImport("WINUSB.dll", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern Boolean WinUsb_ReadPipe(IntPtr InterfaceHandle, byte PipeID, IntPtr Buffer, uint BufferLength, IntPtr LengthTransferred, IntPtr Overlapped);


        [DllImport("winusb.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern Boolean WinUsb_Free(IntPtr InterfaceHandle);


    }
}
