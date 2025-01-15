using System;
using System.Runtime.InteropServices;
using API;


namespace Profiler.Devices
{
    class UsbX52 : IDisposable
    {
        private Guid guidInterface = new(0xA57C1168, 0x7717, 0x4AF0, 0xB3, 0x0E, 0x6A, 0x4C, 0x62, 0x30, 0xBB, 0x10);
        private string hidInterface;
        private IntPtr usbh = IntPtr.Zero;
        private IntPtr hwusb = IntPtr.Zero;
        private CWinUSB.WINUSB_PIPE_INFORMATION pipe = new();
        private int exit = 0;

        #region IDIsposable
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Unload();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion


        private bool Prepare()
        {
            CWinUSB.SP_DEVICE_INTERFACE_DATA diData = new();

            IntPtr diDevs = CWinUSB.SetupDiGetClassDevsW(ref guidInterface, null, IntPtr.Zero, 0x2 | 0x10);
            if (new IntPtr(-1) == diDevs)
            {
                return false;
            }

            diData.cbSize = Marshal.SizeOf<CWinUSB.SP_DEVICE_INTERFACE_DATA>();
            if (!CWinUSB.SetupDiEnumDeviceInterfaces(diDevs, IntPtr.Zero, ref guidInterface, 0, ref diData))
            {
                //uint err = CWinUSB.GetLastError();
                CWinUSB.SetupDiDestroyDeviceInfoList(diDevs);
                return false; // (err == 259); // ERROR_NO_MORE_ITEMS);
            }

            uint size = 0;
            if ((false == CWinUSB.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, IntPtr.Zero, 0, ref size, IntPtr.Zero)) && (122 != CWinUSB.GetLastError()))
            {
                CWinUSB.SetupDiDestroyDeviceInfoList(diDevs);
                return false;
            }

            IntPtr buf = Marshal.AllocHGlobal((int)size);
            Marshal.WriteInt32(buf, 8);
            if (!CWinUSB.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, buf, size, ref size, IntPtr.Zero))
            {
                Marshal.FreeHGlobal(buf);
                CWinUSB.SetupDiDestroyDeviceInfoList(diDevs);
                return false;
            }

            hidInterface = Marshal.PtrToStringAuto(buf + 4);

            Marshal.FreeHGlobal(buf);
            CWinUSB.SetupDiDestroyDeviceInfoList(diDevs);
            return true;
        }

        private bool Open()
        {
            usbh = CWinUSB.CreateFileW(hidInterface, 0x80000000 | 0x40000000, 1 | 2, IntPtr.Zero, 3, 0x80 | 0x40000000, IntPtr.Zero);
            if (CWinUSB.INVALID_HANDLE_VALUE == usbh)
            {
                usbh = IntPtr.Zero;
                return false;
            }

            if(!CWinUSB.WinUsb_Initialize(usbh, out hwusb))
            {
                CWinUSB.CloseHandle(usbh);
                usbh = IntPtr.Zero;
                return false;
            }

            if (!CWinUSB.WinUsb_QueryPipe(hwusb, 0, 0, ref pipe))
            {
                CWinUSB.WinUsb_Free(hwusb);
                hwusb = IntPtr.Zero;
                CWinUSB.CloseHandle(usbh);
                usbh = IntPtr.Zero;
                return false;
            }

            return true;
        }

        private void Close()
        {
            if (hwusb != IntPtr.Zero)
            {
                CWinUSB.WinUsb_Free(hwusb);
                hwusb = IntPtr.Zero;
            }
            if (usbh != IntPtr.Zero)
            {
                CWinUSB.CloseHandle(usbh);
                usbh = IntPtr.Zero;
            }
        }

        public void Process(Controls.CtlDevices wnd)
        {
            byte reset = 0;
            while (System.Threading.Interlocked.Or(ref exit, 0) == 0)
            {
                if (hidInterface == null)
                {
                    if (!Prepare())
                    {
                        System.Threading.Thread.Sleep(4000);
                        continue;
                    }
                }
                if (hwusb == IntPtr.Zero)
                {
                    if (!Open())
                    {
                        System.Threading.Thread.Sleep(4000);
                        continue;
                    }
                }

                IntPtr usbbuf = Marshal.AllocHGlobal(14);
                IntPtr tam = Marshal.AllocHGlobal(8);
                if (!CWinUSB.WinUsb_ReadPipe(hwusb, pipe.PipeId, usbbuf, 14, tam, IntPtr.Zero))
                {
                    System.Threading.Thread.Sleep(2000);
                    reset++;
                }
                else
                {
                    byte[] buf = new byte[19];
                    Marshal.Copy(usbbuf, buf, 1, 14);
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        wnd.AddWinUSBX52Device();
                        wnd.SetStatus(0x06a30255, buf);
                    });
                }
                Marshal.FreeHGlobal(tam);
                Marshal.FreeHGlobal(usbbuf);
                if (reset == 10)
                {
                    reset = 0;
                    Close();
                }
            }
            System.Threading.Interlocked.And(ref exit, 0);
        }

        private void Unload()
        {
            System.Threading.Interlocked.Exchange(ref exit, 1);
            Close();
            while (System.Threading.Interlocked.Or(ref exit, 0) == 1)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        public static DeviceInfo GetInfo(uint joy)
        {
            DeviceInfo datos = new() { Id = joy, Name = "Saitek X52", NAxes = 9, NHats = 4, NButtons = 56 };

            {
                datos.Usages.Add(new()
                {
                    Bits = 11,
                    Type = 0,
                    Range = 2047
                });
                datos.Usages.Add(new()
                {
                    Bits = 11,
                    Type = 1,
                    Range = 2047
                });
                datos.Usages.Add(new()
                {
                    Bits = 10,
                    Type = 3,
                    Range = 1023
                });
            }
            {
                datos.Usages.Add(new()
                {
                    Bits = 8,
                    Type = 2,
                    Range = 255
                });
                datos.Usages.Add(new()
                {
                    Bits = 8,
                    Type = 4,
                    Range = 255
                });
                datos.Usages.Add(new()
                {
                    Bits = 8,
                    Type = 3,
                    Range = 255
                });
                datos.Usages.Add(new()
                {
                    Bits = 8,
                    Type = 6,
                    Range = 255
                });
                datos.Usages.Add(new()
                {
                    Bits = 36,
                    Type = 255,
                    Range = 0
                });
                datos.Usages.Add(new()
                {
                    Bits = 4,
                    Type = 254,
                    Range = 4
                });
                datos.Usages.Add(new()
                {
                    Bits = 4,
                    Type = 0,
                    Range = 15
                });
                datos.Usages.Add(new()
                {
                    Bits = 4,
                    Type = 1,
                    Range = 15
                });
            }

            return datos;
        }
    }
}
