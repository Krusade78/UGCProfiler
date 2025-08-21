using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RawInputHelper
{
    internal static class Win32
    {
        #region Window
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateEventW(
            IntPtr lpEventAttributes, // normalmente se pasa IntPtr.Zero
            bool bManualReset,
            bool bInitialState,
            string? lpName // puede ser null
        );

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowExW(
            uint dwExStyle,
            string lpClassName,
            string? lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam
        );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandleW(string? lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ResetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint MsgWaitForMultipleObjects(
            uint nCount,
            IntPtr[] pHandles,
            bool bWaitAll,
            uint dwMilliseconds,
            uint dwWakeMask
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);
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

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWMOUSE
        {
            public ushort usFlags;
            public uint ulButtons;
            public ushort usButtonFlags;
            public ushort usButtonData;
            public uint ulRawButtons;
            public int lLastX;
            public int lLastY;
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
            // Este es un buffer variable, así que se maneja como IntPtr
            public IntPtr bRawData;
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

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDP_DATA
        {
            public ushort DataIndex;
            public ushort Reserved;
            public uint RawValue;
        }
        #endregion

        #region RawInput functions
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterRawInputDevices(
                [In] RAWINPUTDEVICE[] pRawInputDevices,
                uint uiNumDevices,
                uint cbSize
            );

        [DllImport("User32.dll", SetLastError = true)]
        public static extern uint GetRawInputBuffer(
            [Out] byte[] pData,
            ref uint pcbSize,
            uint cbSizeHeader);
        [DllImport("User32.dll", SetLastError = true)]
        public static extern uint GetRawInputBuffer(
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint GetRawInputDeviceInfoW(
            IntPtr hDevice,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize);

        public enum HidP_ReportType
        {
            HidP_Input,
            HidP_Output,
            HidP_Feature
        }

        [DllImport("hid.dll", SetLastError = true)]
        public static extern long HidP_GetData(
            HidP_ReportType ReportType,
            IntPtr DataList,
            ref uint DataLength,
            IntPtr PreparsedData,
            byte[] Report,
            uint ReportLength);
        #endregion
    }
}
