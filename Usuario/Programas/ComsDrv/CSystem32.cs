using System;
using System.Runtime.InteropServices;

namespace ComsDrv
{
    internal static class CSystem32
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(string lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr pSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(Microsoft.Win32.SafeHandles.SafeFileHandle handle, UInt32 dwIoControlCode, byte[] lpInBuffer, UInt32 nInBufferSize, byte[] lpOutBuffer, UInt32 nOutBufferSize, out UInt32 lpBytesReturned, IntPtr lpOverlapped);
    }
}