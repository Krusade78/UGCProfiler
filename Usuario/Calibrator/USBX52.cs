using System;
using System.Runtime.InteropServices;


namespace Calibrator
{
    class UsbX52
    {
        private Guid guidInterface = new(0xA57C1168, 0x7717, 0x4AF0, 0xB3, 0x0E, 0x6A, 0x4C, 0x62, 0x30, 0xBB, 0x10);
        private string hidInterface;
        private IntPtr usbh = IntPtr.Zero;
        private IntPtr hwusb = IntPtr.Zero;
        private CWinUSB.WINUSB_PIPE_INFORMATION pipe = new();
        private bool cerrar = false;

        private bool Preparar()
        {
            CWinUSB.SP_DEVICE_INTERFACE_DATA diData = new();

            IntPtr diDevs = CWinUSB.SetupDiGetClassDevs(ref guidInterface, null, IntPtr.Zero, 0x2 | 0x10);
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

            uint tam = 0;
            if ((false == CWinUSB.SetupDiGetDeviceInterfaceDetail(diDevs, ref diData, IntPtr.Zero, 0, ref tam, IntPtr.Zero)) && (122 != CWinUSB.GetLastError()))
            {
                CWinUSB.SetupDiDestroyDeviceInfoList(diDevs);
                return false;
            }

            IntPtr buf = Marshal.AllocHGlobal((int)tam);
            Marshal.WriteInt32(buf, 8);
            if (!CWinUSB.SetupDiGetDeviceInterfaceDetail(diDevs, ref diData, buf, tam, ref tam, IntPtr.Zero))
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

        private bool Abrir()
        {
            usbh = CWinUSB.CreateFileW(hidInterface, 0x80000000 | 0x40000000, 1 | 2, IntPtr.Zero, 3, 0x80 | 0x40000000, IntPtr.Zero);
            if (new IntPtr(-1) == usbh)
            {
                return false;
            }

            if(!CWinUSB.WinUsb_Initialize(usbh, out hwusb))
            {
                CWinUSB.CloseHandle(usbh);
                return false;
            }

            if (!CWinUSB.WinUsb_QueryPipe(hwusb, 0, 0, ref pipe))
            {
                CWinUSB.WinUsb_Free(hwusb);
                CWinUSB.CloseHandle(usbh);
                return false;
            }

            return true;
        }

        public void Leer(MainWindow wnd)
        {
            while(!cerrar)
            {
                if (hidInterface == null)
                {
                    if (!Preparar())
                    {
                        System.Threading.Thread.Sleep(4000);
                        continue;
                    }
                }
                if (hwusb == IntPtr.Zero)
                {
                    if (!Abrir())
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
                }
                else
                {
                    byte[] buf = new byte[19];
                    Marshal.Copy(usbbuf, buf, 1, 14);
                    wnd.Dispatcher.BeginInvoke(() => {
                        wnd.ucCalibrar.ActualizarEstado("WinUSBX52", buf, 0x06a30255);
                        wnd.ucCalibrar.ActualizarEstado("WinUSBX52", buf, 0x06a30256);
                    });
                }
                Marshal.FreeHGlobal(tam);
                Marshal.FreeHGlobal(usbbuf);
            }
            cerrar = true;
        }

        public void Cerrar()
        {
            cerrar = true;
            if (hwusb != IntPtr.Zero)
            {
                CWinUSB.WinUsb_Free(hwusb);
            }
            if (usbh != IntPtr.Zero)
            {
                CWinUSB.CloseHandle(usbh);
            }
            while (!cerrar)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        public static DatosJoy GetInfo(uint joy)
        {
            string nombre = joy == 0x06a30255 ? "Saitek X52 Joystick" : "Saitek X52 Acelerador";
            DatosJoy datos = new() { Id = joy, Nombre = nombre };

            if (joy == 0x06a30255)
            {
                datos.Usages.Add(new()
                {
                    Bits = 11,
                    Eje = 0,
                    Rango = 2047
                });
                datos.Usages.Add(new()
                {
                    Bits = 11,
                    Eje = 1,
                    Rango = 2047
                });
                datos.Usages.Add(new()
                {
                    Bits = 10,
                    Eje = 3,
                    Rango = 1023
                });
            }
            else
            {
                datos.Usages.Add(new()
                {
                    Bits = 11,
                    Eje = 254,
                    Rango = 0
                });
                datos.Usages.Add(new()
                {
                    Bits = 11,
                    Eje = 254,
                    Rango = 0
                });
                datos.Usages.Add(new()
                {
                    Bits = 10,
                    Eje = 254,
                    Rango = 0
                });
                datos.Usages.Add(new()
                {
                    Bits = 8,
                    Eje = 2,
                    Rango = 255
                });
                datos.Usages.Add(new()
                {
                    Bits = 8,
                    Eje = 4,
                    Rango = 255
                });
                datos.Usages.Add(new()
                {
                    Bits = 8,
                    Eje = 3,
                    Rango = 255
                });
                datos.Usages.Add(new()
                {
                    Bits = 8,
                    Eje = 6,
                    Rango = 255
                });
                datos.Usages.Add(new()
                {
                    Bits = 40,
                    Eje = 254,
                    Rango = 0
                });
                datos.Usages.Add(new()
                {
                    Bits = 4,
                    Eje = 0,
                    Rango = 15
                });
                datos.Usages.Add(new()
                {
                    Bits = 4,
                    Eje = 1,
                    Rango = 15
                });
            }

            return datos;
        }
    }
}
