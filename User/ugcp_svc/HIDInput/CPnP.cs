using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace ugcp_svc.HIDInput
{
    sealed class CPnP : IDisposable
    {
        private readonly Profile.CProfile pProfile;
        private nint pnpHWnd = nint.Zero;
        private nint pnpHdn = nint.Zero;
        private readonly Lock mutexDevices = new();
        private readonly List<CHIDDevice> hidDevices = [];

        public CPnP(Profile.CProfile refProfile)
        {
            pProfile = refProfile;
            refProfile.SetRefreshDevicesCallback(CallbackRefreshDevices);
            refProfile.SetPauseWinUSBCallback(CallbackPauseWinUSB);
        }

        void IDisposable.Dispose()
        {
            if (pnpHdn != nint.Zero) { API.Win32.UnregisterDeviceNotification(pnpHdn); }
            if (pnpHWnd != nint.Zero) { API.Win32.DestroyWindow(pnpHWnd);  }

            lock (mutexDevices)
            {
                while (hidDevices.Count > 0)
                {
                    CHIDDevice dev = hidDevices[0];
                    hidDevices.RemoveAt(0);
                    dev?.Dispose();
                }
            }
        }

        public bool Init()
        {
            return PnpNotification();
        }

        public Action GetCloseHwndCallback() => CallbackCloseHwnd;

        public void CallbackRefreshDevices(HashSet<uint> ids) { RefreshDevices(ids); }
        public void CallbackPauseWinUSB(bool onoff) { PauseWinUSB(onoff); }

        public void LockDevices() => mutexDevices.Enter();
        public CHIDDevice? GetDevice(uint id)
        {
            ReadOnlySpan<CHIDDevice> span = CollectionsMarshal.AsSpan(hidDevices);
            for (byte i = 0; i < span.Length; i++)
            {
                if (span[i].HardwareId == id)
                {
                    return span[i];
                }
            }

            return null;
        }
        public void UnlockDevices() => mutexDevices.Exit();

        private bool PnpNotification()
        {
            _wndProc = PnpMsjProc;
            API.Win32.WNDCLASSEXW wcex = default;
            {
                wcex.cbSize = (uint)Marshal.SizeOf<API.Win32.WNDCLASSEXW>();
                wcex.lpfnWndProc = _wndProc;
                wcex.lpszClassName = "UGCP pnp notification";
                wcex.hInstance = API.Win32.GetModuleHandleW(null);
            };

            if (API.Win32.RegisterClassExW(ref wcex) != 0)
            {
                pnpHWnd = API.Win32.CreateWindowExW(0, wcex.lpszClassName, "", 0, 0, 0, 0, 0, API.Win32.HWND_MESSAGE, nint.Zero, wcex.hInstance, nint.Zero);
                int err = Marshal.GetLastWin32Error();
                if (pnpHWnd != nint.Zero)
                {
                    Guid guid = Guid.Empty;
                    API.HID.HidD_GetHidGuid(ref guid);
                    API.Win32.DEV_BROADCAST_DEVICEINTERFACE dbbd = new()
                    {
                        dbcc_size = Marshal.SizeOf<API.Win32.DEV_BROADCAST_DEVICEINTERFACE>(),
                        dbcc_devicetype = API.Win32.DBT_DEVTYP_DEVICEINTERFACE,
                        dbcc_classguid = guid
                    };
                    using Shared.RAII.HGlobalHandle ptr = new(dbbd.dbcc_size);
                    if (!ptr.IsInvalid)
                    {
                        Marshal.StructureToPtr(dbbd, ptr.DangerousGetHandle(), false);
                        pnpHdn = API.Win32.RegisterDeviceNotificationW(pnpHWnd, ptr.DangerousGetHandle(), API.Win32.DEVICE_NOTIFY_WINDOW_HANDLE);
                        return pnpHdn != nint.Zero;
                    }
                }
            }

            return false;
        }

        private static API.Win32.WndProc? _wndProc;
        private nint PnpMsjProc(nint hWnd, uint uMsg, nint wParam, nint lParam)
        {
            switch (uMsg)
            {
                case API.Win32.WM_POWERBROADCAST:
                    switch (wParam)
                    {
                        case API.Win32.PBT_APMSUSPEND:
                            lock (mutexDevices)
                            {
                                foreach (CHIDDevice dev in hidDevices)
                                {
                                    dev.SetSuspended(true);
                                }
                            }
                            PauseWinUSB(true);
                            return 1;

                        case API.Win32.PBT_APMRESUMEAUTOMATIC or
                                API.Win32.PBT_APMRESUMECRITICAL or
                                API.Win32.PBT_APMRESUMESUSPEND:
                            lock (mutexDevices)
                            {
                                foreach (CHIDDevice dev in hidDevices)
                                {
                                    dev.SetSuspended(false);
                                }
                            }
                            PauseWinUSB(false);
                            return 1;
                    }
                    return 1;
                case API.Win32.WM_DEVICECHANGE:
                    switch (wParam)
                    {
                        case API.Win32.DBT_DEVICEARRIVAL:
                            {
                                API.Win32.DEV_BROADCAST_HDR hdr = Marshal.PtrToStructure<API.Win32.DEV_BROADCAST_HDR>(lParam);
                                if (API.Win32.DBT_DEVTYP_DEVICEINTERFACE == hdr.dbch_devicetype)
                                {
                                    API.Win32.DEV_BROADCAST_DEVICEINTERFACE dbdi = Marshal.PtrToStructure<API.Win32.DEV_BROADCAST_DEVICEINTERFACE>(lParam);
                                    uint joyId = IHIDInput.GetHardwareId(ref dbdi.dbcc_name);
                                    if (pProfile.GetProfile().DeviceIncluded(joyId))
                                    {
                                        RefreshDevices([joyId]);
                                    }
                                    return 1;
                                }
                            }
                            break;
                        case API.Win32.DBT_DEVICEQUERYREMOVE or
                                API.Win32.DBT_DEVICEREMOVEPENDING or
                                API.Win32.DBT_DEVICEREMOVECOMPLETE:
                            {
                                API.Win32.DEV_BROADCAST_HDR hdr = Marshal.PtrToStructure<API.Win32.DEV_BROADCAST_HDR>(lParam);
                                if (API.Win32.DBT_DEVTYP_DEVICEINTERFACE == hdr.dbch_devicetype)
                                {
                                    API.Win32.DEV_BROADCAST_DEVICEINTERFACE dbdi = Marshal.PtrToStructure<API.Win32.DEV_BROADCAST_DEVICEINTERFACE>(lParam);
                                    uint joyId = IHIDInput.GetHardwareId(ref dbdi.dbcc_name);
                                    lock (mutexDevices)
                                    {
                                        RemoveDevice(joyId);
                                    }
                                }
                            }
                            return 1;
                        default:
                            break;
                    }
                    break;
                case API.Win32.WM_CLOSE:
                    API.Win32.DestroyWindow(hWnd);
                    break;
                case API.Win32.WM_DESTROY:
                    API.Win32.PostQuitMessage(0);
                    break;
                default:
                    return API.Win32.DefWindowProcW(hWnd, uMsg, wParam, lParam);
            }
            return nint.Zero;
        }

        private void RefreshDevices(HashSet<uint> ids)
        {
            lock (mutexDevices)
            {
                foreach (uint id in ids)
                {
                    CHIDDevice? dev = GetDevice(id);
                    if (dev == null)
                    {
                        if (id == X52.CWinUSBX52.HARDWARE_ID_X52)
                        {
                            dev = new X52.CWinUSBX52(pProfile);
                        }
                        else
                        {
                            dev = new CHIDDevice(id, pProfile);
                        }
                        hidDevices.Add(dev);
                    }
                }

                Queue<uint> notFound = [];
                foreach (uint id in ids)
                {
                    CHIDDevice? dev = GetDevice(id);
                    if (dev == null)
                    {
                        notFound.Enqueue(id);
                    }
                }
                while (notFound.Count > 0)
                {
                    uint id = notFound.Dequeue();
                    RemoveDevice(id);
                }
            }
        }

        private void PauseWinUSB(bool onoff)
        {
            lock (mutexDevices)
            {
                X52.CWinUSBX52? pX52 = (X52.CWinUSBX52?)GetDevice(X52.CWinUSBX52.HARDWARE_ID_X52);
                pX52?.SetPause(onoff);
            }
        }


        /// <summary>
        /// Warning function is not lock protected
        /// </summary>
        /// <param name="id"></param>
        private void RemoveDevice(uint id)
        {
            ReadOnlySpan<CHIDDevice> dev = CollectionsMarshal.AsSpan(hidDevices);
            for (byte i = 0; i < dev.Length; i++)
            {
                if (dev[i].HardwareId == id)
                {
                    dev[i].Dispose();
                    hidDevices.RemoveAt(i);
                }
            }
        }

        public void CallbackCloseHwnd()
        {
            if (pnpHWnd != nint.Zero)
            {
                API.Win32.SendNotifyMessageW(pnpHWnd, API.Win32.WM_CLOSE, nint.Zero, nint.Zero);
            }
        }

        public void LoopWnd()
        {
            API.Win32.MSG msg;
            while (API.Win32.GetMessageW(out msg, nint.Zero, 0, 0) != 0)
            {
                //API.Win32.TranslateMessage(msg);
                API.Win32.DispatchMessageW(msg);
            }
            pnpHWnd = nint.Zero;
        }
    }
}
