using System;
using System.Runtime.InteropServices;

namespace API
{
    public static partial class CWinUSB
    {
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
                                            ref UInt32 requiredSize,
                                            IntPtr deviceInfoData
                                            );

        [LibraryImport("kernel32.dll", EntryPoint = "GetLastError")]
        [return: MarshalAs(UnmanagedType.U4)]
        private static partial uint W32GetLastError();
        public static uint GetLastError() => W32GetLastError();

        public static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

        [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        public static partial IntPtr CreateFileW(
                                             [MarshalAs(UnmanagedType.LPWStr)] string filename,
                                             [MarshalAs(UnmanagedType.U4)] uint access,
                                             [MarshalAs(UnmanagedType.U4)] uint share,
                                             IntPtr securityAttributes,
                                             [MarshalAs(UnmanagedType.U4)] uint creationDisposition,
                                             [MarshalAs(UnmanagedType.U4)] uint flagsAndAttributes,
                                             IntPtr templateFile);
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CloseHandle(IntPtr hHandle);

        public partial struct WINUSB_PIPE_INFORMATION
        {
            /// <summary>A <a href="https://docs.microsoft.com/windows-hardware/drivers/ddi/content/usb/ne-usb-_usbd_pipe_type">USBD_PIPE_TYPE</a>-type enumeration value that specifies the pipe type.</summary>
            public USBD_PIPE_TYPE PipeType;
            /// <summary>The pipe identifier (ID).</summary>
            public byte PipeId;
            /// <summary>The maximum size, in bytes, of the packets that are transmitted on the pipe.</summary>
            public ushort MaximumPacketSize;
            /// <summary>The pipe interval.</summary>
            public byte Interval;
        }
        public enum USBD_PIPE_TYPE
        {
            UsbdPipeTypeControl = 0,
            UsbdPipeTypeIsochronous = 1,
            UsbdPipeTypeBulk = 2,
            UsbdPipeTypeInterrupt = 3,
        }

        [LibraryImport("winusb.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WinUsb_Initialize(IntPtr DeviceHandle, out IntPtr InterfaceHandle);

        [LibraryImport("WINUSB.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WinUsb_QueryPipe(IntPtr InterfaceHandle, byte AlternateInterfaceNumber, byte PipeIndex, ref WINUSB_PIPE_INFORMATION PipeInformation);

        [LibraryImport("WINUSB.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WinUsb_ReadPipe(IntPtr InterfaceHandle, byte PipeID, IntPtr Buffer, uint BufferLength, IntPtr LengthTransferred, IntPtr Overlapped);


        [LibraryImport("winusb.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WinUsb_Free(IntPtr InterfaceHandle);
    }
}
