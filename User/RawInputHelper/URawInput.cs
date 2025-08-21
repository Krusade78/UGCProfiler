using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RawInputHelper
{
    internal class URawInput
    {
        private RawInput? ptrm = null;
        private IntPtr hEvClose = IntPtr.Zero;
        private IntPtr hWnd = IntPtr.Zero;

        public void Init(RawInput ptrm)
        {
            this.ptrm = ptrm;

            hEvClose = Win32.CreateEventW(0, true, false, null);
            if (hEvClose != IntPtr.Zero)
            {
                hWnd = Win32.CreateWindowExW(0, "STATIC", null, 0, 0, 0, 0, 0, -3, 0, Win32.GetModuleHandleW(null) , 0);
                if (hWnd != IntPtr.Zero)
                {
                    Win32.RAWINPUTDEVICE[] rdev = new Win32.RAWINPUTDEVICE[1];
                    rdev[0].usUsagePage = 0x01;
                    rdev[0].usUsage = 0x04;
                    rdev[0].hwndTarget = hWnd;
                    rdev[0].dwFlags = 0;
                    //rdev[1].usUsagePage = 0x01;
                    //rdev[1].usUsage = 0x05;
                    //rdev[1].hwndTarget = hWnd;
                    //rdev[1].dwFlags = 0;

                    if (!Win32.RegisterRawInputDevices(rdev, 1, (uint)Marshal.SizeOf<Win32.RAWINPUTDEVICE>()))
                    {
                        Win32.DestroyWindow(hWnd);
                        return;
                    }

                    while (true)
                    {
                        if (Win32.WaitForSingleObject(hEvClose, 0) == 0)
                        {
                            break;
                        }
                        uint ret = Win32.MsgWaitForMultipleObjects(1, [hEvClose], false, 0xffffffff, 0x0400);
                        if (ret != (0 + 1))
                        {
                            break;
                        }
                        else
                        {
                            ProcessRawInput();
                        }
                    }

                    Win32.DestroyWindow(hWnd);
                    hWnd = IntPtr.Zero;
                }
            }

            Win32.ResetEvent(hEvClose);
        }

        public void Close()
        {
            if (hEvClose != IntPtr.Zero)
            {
                Win32.SetEvent(hEvClose);
                while (Win32.WaitForSingleObject(hEvClose, 500) == 0)
                {
                    System.Threading.Thread.Sleep(100);
                }
                Win32.CloseHandle(hEvClose);
            }
        }

        private void ProcessRawInput()
        {
            uint size = 0;

            if (Win32.GetRawInputBuffer(IntPtr.Zero, ref size, (uint)Marshal.SizeOf<Win32.RAWINPUTHEADER>()) != 0)
            {
                System.Threading.Thread.Sleep(100);
                return;
            }
            if (size == 0)
            {
                System.Threading.Thread.Sleep(50);
                return;
            }
            size *= 128; // up to 128 messages

            IntPtr pRawInput = Marshal.AllocHGlobal((int)size);
            IntPtr pNextRawInput = pRawInput;
            while (true)
            {
                uint sizeT = size;
                uint nInput = Win32.GetRawInputBuffer(pRawInput, ref sizeT, (uint)Marshal.SizeOf<Win32.RAWINPUTHEADER>());
                if (nInput == 0)
                {
                    break;
                }

                Win32.RAWINPUT raw = Marshal.PtrToStructure<Win32.RAWINPUT>(pNextRawInput);
                for (; nInput > 0; nInput--)
                {
                    uint cbSize = 0;
                    _ = Win32.GetRawInputDeviceInfoW(raw.header.hDevice, 0x20000007, IntPtr.Zero, ref cbSize);
                    if (cbSize != 0)
                    {
                        IntPtr pName = Marshal.AllocHGlobal((int)cbSize * 2);
                        _ = Win32.GetRawInputDeviceInfoW(raw.header.hDevice, 0x20000007, pName, ref cbSize);
                        {
                            string? cmps = Marshal.PtrToStringUni(pName);
                            if (cmps != null)
                            {
                                try
                                {
                                    uint deviceId = Convert.ToUInt32(cmps[12..16], 16) << 16;
                                    deviceId |= Convert.ToUInt32(cmps[21..25], 16);

                                    uint ppdSize = 0;
                                    _ = Win32.GetRawInputDeviceInfoW(raw.header.hDevice, 0x20000005, IntPtr.Zero, ref ppdSize);
                                    IntPtr ppdBuffer = Marshal.AllocHGlobal((int)ppdSize);
                                    cbSize = ppdSize;
                                    if (Win32.GetRawInputDeviceInfoW(raw.header.hDevice, 0x20000005, ppdBuffer, ref cbSize) == ppdSize)
                                    {
                                        byte[] report = new byte[raw.hid.dwSizeHid];
                                        Marshal.Copy(IntPtr.Add(pNextRawInput, Marshal.SizeOf<Win32.RAWINPUTHEADER>() + Marshal.SizeOf<Win32.RAWHID>()), report, 0, (int)raw.hid.dwSizeHid);

                                        uint dataSize = 0;
                                        if (0xC0110007 == Win32.HidP_GetData(Win32.HidP_ReportType.HidP_Input, IntPtr.Zero, ref dataSize, ppdBuffer, report, raw.hid.dwSizeHid))
                                        {
                                            IntPtr data = Marshal.AllocHGlobal((int)dataSize * Marshal.SizeOf<Win32.HIDP_DATA>());
                                            if (0x00110000 == Win32.HidP_GetData(Win32.HidP_ReportType.HidP_Input, data, ref dataSize, ppdBuffer, report, raw.hid.dwSizeHid))
                                            {
                                                byte[] datab = new byte[Marshal.SizeOf<Win32.HIDP_DATA>() * dataSize];
                                                Marshal.Copy(data, datab, 0, datab.Length);
                                                ptrm?.Call(cmps, datab);
                                            }
                                            Marshal.FreeHGlobal(data);
                                        }
                                    }
                                    Marshal.FreeHGlobal(ppdBuffer);
                                }
                                catch { }
                            }
                        }

                        Marshal.FreeHGlobal(pName);
                    }
                    pNextRawInput = IntPtr.Add(pRawInput, Marshal.SizeOf<Win32.RAWINPUT>());
                }
            }

            Marshal.FreeHGlobal(pRawInput);
        }
    }
}
