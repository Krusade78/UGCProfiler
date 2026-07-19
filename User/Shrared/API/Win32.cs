using System;
using System.Runtime.InteropServices;


namespace API
{
    public partial class Win32
    {
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;
        public const uint OPEN_EXISTING = 3;

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
        public static partial bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CloseHandle(IntPtr hHandle);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CancelIoEx(IntPtr hFile, IntPtr lpOverlapped);

        #region Window and RegisterDeviceNotification
        public const int WM_DEVICECHANGE = 0x0219;
        public const int WM_POWERBROADCAST = 0x0218;
        public const int WM_CLOSE = 0x0010;
        public const int WM_DESTROY = 0x0002;

        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;
        public const int DBT_DEVICEREMOVEPENDING = 0x8003;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        public const int DBT_DEVTYP_DEVICEINTERFACE = 5;

        public const int PBT_APMSUSPEND = 0x0004;
        public const int PBT_APMRESUMEAUTOMATIC = 0x0012;
        public const int PBT_APMRESUMECRITICAL = 0x0006;
        public const int PBT_APMRESUMESUSPEND = 0x0007;

        public static readonly IntPtr HWND_MESSAGE = new(-3);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate nint WndProc(nint hWnd, uint uMsg, nint wParam, nint lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct WNDCLASSEXW
        {
            public uint cbSize;
            public uint style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public  struct DEV_BROADCAST_HDR
        {
            public uint dbch_size;
            public uint dbch_devicetype;
            public uint dbch_reserved;
        }

        public const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern nint GetModuleHandleW([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern ushort RegisterClassExW(ref WNDCLASSEXW lpWndClass);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CreateWindowExW(
            uint dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
            uint dwStyle,
            int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SendNotifyMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool DestroyWindow(IntPtr hWnd);

        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial void PostQuitMessage(in int nExitCode);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetMessageW(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        //[LibraryImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static partial bool TranslateMessage(in MSG lpMsg);

        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial IntPtr DispatchMessageW(in MSG lpMsg);

        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial IntPtr RegisterDeviceNotificationW(IntPtr hRecipient, IntPtr NotificationFilter, uint Flags);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool UnregisterDeviceNotification(IntPtr Handle);
        #endregion

        #region SendInput
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public InputType type;
            public InputUnion U;
        }

        public enum InputType : uint
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public KeyEventFlags dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [Flags]
        public enum KeyEventFlags : uint
        {
            KEYDOWN = 0x0000,
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            UNICODE = 0x0004,
            SCANCODE = 0x0008
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public MouseEventFlags dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [Flags]
        public enum MouseEventFlags : uint
        {
            MOVE = 0x0001,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            MIDDLEDOWN = 0x0020,
            MIDDLEUP = 0x0040,
            XDOWN = 0x0080,
            XUP = 0x0100,
            WHEEL = 0x0800,
            HWHEEL = 0x01000,
            ABSOLUTE = 0x8000
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }


        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public enum VirtualKeys : uint
        { 
            VK_RCONTROL = 0xA3,
            VK_RMENU = 0xA5,
            VK_INSERT = 0x2D,
            VK_DELETE = 0x2E,
            VK_END = 0x23,
            VK_HOME = 0x024,
            VK_PRIOR = 0x21,
            VK_NEXT = 0x22,
            VK_LEFT = 0x25,
            VK_UP = 0x26,
            VK_RIGHT = 0x27,
            VK_DOWN = 0x28,
            VK_NUMLOCK = 0x90,
            VK_SNAPSHOT = 0x2C,
            VK_CANCEL = 0x03,
            VK_DIVIDE = 0x6F
        }
        public const uint MAPVK_VK_TO_VSC_EX = 4;

        [LibraryImport("user32.dll", SetLastError = true)]
        public static partial uint MapVirtualKeyW(uint uCode, uint uMapType);
        #endregion
    }
}
