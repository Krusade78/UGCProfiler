using System;
using System.Runtime.InteropServices;

namespace RawInputHelper
{
    internal static partial class Win32
    {
        #region Window
        [LibraryImport("kernel32.dll", EntryPoint = "CreateEventW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        internal static partial IntPtr CreateEventW(
            IntPtr lpEventAttributes, // normalmente se pasa IntPtr.Zero
            [MarshalAs(UnmanagedType.Bool)] bool bManualReset,
            [MarshalAs(UnmanagedType.Bool)] bool bInitialState,
            string? lpName // puede ser null
        );

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetEvent(IntPtr hEvent);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ResetEvent(IntPtr hEvent);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        internal static partial uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [LibraryImport("user32.dll", SetLastError = true)]
        internal static partial uint MsgWaitForMultipleObjects(
            uint nCount,
            [In] IntPtr[] pHandles,
            [MarshalAs(UnmanagedType.Bool)] bool bWaitAll,
            uint dwMilliseconds,
            uint dwWakeMask
        );
        #endregion

        #region RawInput structs
        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RAWMOUSE
        {
            [FieldOffset(0)]
            public ushort usFlags;

            [FieldOffset(4)]
            public uint ulButtons;

            [FieldOffset(4)]
            public ushort usButtonFlags;

            [FieldOffset(6)]
            public ushort usButtonData;

            [FieldOffset(8)]
            public uint ulRawButtons;

            [FieldOffset(12)]
            public int lLastX;

            [FieldOffset(16)]
            public int lLastY;

            [FieldOffset(20)]
            public uint ulExtraInformation;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWHID
        {
            public uint dwSizeHid;
            public uint dwCount;
            //public byte bRawData;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct RAWINPUT
        {
            [FieldOffset(0)]
            public RAWINPUTHEADER header;

            [FieldOffset(24)]
            public RAWMOUSE mouse;

            [FieldOffset(24)]
            public RAWKEYBOARD keyboard;

            [FieldOffset(24)]
            public RAWHID hid;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct HIDP_DATA
        {
            [FieldOffset(0)]
            public ushort DataIndex;
            [FieldOffset(2)]
            public ushort Reserved;
            [FieldOffset(4)]
            public uint RawValue;
            [FieldOffset(4)]
            public byte On;
        }
        #endregion

        #region RawInput functions
        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool RegisterRawInputDevices(
                [In] RAWINPUTDEVICE[] pRawInputDevices,
                uint uiNumDevices,
                uint cbSize
            );

        [LibraryImport("User32.dll", SetLastError = true)]
        internal static partial uint GetRawInputBuffer(
            [Out] byte[] pData,
            ref uint pcbSize,
            uint cbSizeHeader);
        [LibraryImport("User32.dll", SetLastError = true)]
        internal static partial uint GetRawInputBuffer(
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader);

        [LibraryImport("User32.dll", EntryPoint = "GetRawInputDeviceInfoW", SetLastError = true)]
        internal static partial uint GetRawInputDeviceInfoW(
            IntPtr hDevice,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize);
        #endregion
    }
}
