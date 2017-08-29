using System;
using System.Runtime.InteropServices;

namespace Launcher
{
    class CSystem32
    {
        public const UInt32 IOCTL_MFD_LUZ =         ((0x22) << 16) | ((2) << 14) | ((0x0800) << 2) | (0);
        public const UInt32 IOCTL_GLOBAL_LUZ =      ((0x22) << 16) | ((2) << 14) | ((0x0801) << 2) | (0);
        public const UInt32 IOCTL_TEXTO =           ((0x22) << 16) | ((2) << 14) | ((0x0804) << 2) | (0);
        public const UInt32 IOCTL_HORA =            ((0x22) << 16) | ((2) << 14) | ((0x0805) << 2) | (0);
        public const UInt32 IOCTL_HORA24 =          ((0x22) << 16) | ((2) << 14) | ((0x0806) << 2) | (0);
        public const UInt32 IOCTL_FECHA =           ((0x22) << 16) | ((2) << 14) | ((0x0807) << 2) | (0);
        public const UInt32 IOCTL_USR_CALIBRADO =   ((0x22) << 16) | ((2) << 14) | ((0x0809) << 2) | (0);

        [StructLayout(LayoutKind.Sequential)]
        public struct CALIBRADO
        {
            internal ushort i;
            internal ushort c;
            internal ushort d;
            internal byte n;
            internal byte Margen;
            internal byte Resistencia;
            internal byte cal;
            internal byte antiv;
        };

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(string lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr pSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(Microsoft.Win32.SafeHandles.SafeFileHandle handle, UInt32 dwIoControlCode, byte[] lpInBuffer, UInt32 nInBufferSize, byte[] lpOutBuffer, UInt32 nOutBufferSize, out UInt32 lpBytesReturned, IntPtr lpOverlapped);
    }
}
