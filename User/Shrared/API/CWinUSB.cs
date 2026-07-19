using System;
using System.Runtime.InteropServices;

namespace API
{
    public static partial class CWinUSB
    {
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
        public static partial bool WinUsb_ReadPipe(IntPtr InterfaceHandle, byte PipeID, IntPtr Buffer, uint BufferLength, ref uint LengthTransferred, IntPtr Overlapped);
        
        [LibraryImport("WINUSB.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static unsafe partial bool WinUsb_ReadPipe(IntPtr InterfaceHandle, byte PipeID, byte* Buffer, uint BufferLength, ref uint LengthTransferred, IntPtr Overlapped);
        public static bool WinUsb_ReadPipe(IntPtr InterfaceHandle, byte PipeID, Span<byte> Buffer, uint BufferLength, ref uint LengthTransferred, IntPtr Overlapped)
        {
            unsafe
            {
                fixed(byte* pBuffer = Buffer)
                {
                    return WinUsb_ReadPipe(InterfaceHandle, PipeID, pBuffer, BufferLength, ref LengthTransferred, Overlapped);
                }
            }
        }

        [LibraryImport("winusb.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WinUsb_Free(IntPtr InterfaceHandle);


        [StructLayout(LayoutKind.Sequential)]
        public struct WINUSB_SETUP_PACKET
        {
            public byte RequestType;
            public byte Request;
            public ushort Value;
            public ushort Index;
            public ushort Length;
        }

        [LibraryImport("winusb.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WinUsb_ControlTransfer(IntPtr InterfaceHandle, WINUSB_SETUP_PACKET SetupPacket, IntPtr Buffer, uint BufferLength, IntPtr LengthTransferred, IntPtr Overlapped);
    }
}
