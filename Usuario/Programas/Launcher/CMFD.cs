using System;
using System.Windows;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Launcher
{
    internal class CMFD
    {
        private System.Windows.Interop.HwndSource hWnd = null;
        private DataSetConfiguracion conf;

        private bool activado = false;
        private bool hidOn = false;
        private SafeFileHandle driver = null;

        private byte estadoCursor = 0;
        private byte estadoPagina = 0;

        public CMFD(System.Windows.Interop.HwndSource hWnd, ref DataSetConfiguracion dsc)
        {
            this.hWnd = hWnd;
            this.conf = dsc;
        }

        private bool AbrirDriver()
        {
            lock(this)
            {
                if (driver != null)
                    return true;

                driver = CSystem32.CreateFile(
                        "\\\\.\\XUSBInterface",
                        0x80000000 | 0x40000000,//GENERIC_WRITE | GENERIC_READ,
                        0x00000002 | 0x00000001, //FILE_SHARE_WRITE | FILE_SHARE_READ,
                        IntPtr.Zero,
                        3,//OPEN_EXISTING,
                        0,
                        IntPtr.Zero);
                if (driver.IsInvalid)
                {
                    driver = null;
                    MessageBox.Show("No se puede abrir el driver", "[CMFD][0.1]", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }
        private void CerrarDriver()
        {
            lock (this)
            {
                if (driver == null)
                    return;
                else
                {
                    driver.Close();
                    driver = null;
                }
            }
        }

        public bool ComprobarEstado()
        {
            if (activado)
                return true;

            if (!AbrirDriver())
               return false;

            UInt32 ret = 0;
            byte[] buf = new byte[1];
            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_GET_MENU, null, 0, buf, 1, out ret, IntPtr.Zero))
            {
                CerrarDriver();
                MessageBox.Show("No se puede enviar la orden al driver", "[CMFD][0.2]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (buf[0] == 1)
            {
                activado = true;
                if (!IniciarMenu() || !IniciarHID())
                {
                    activado = false;
                    CerrarDriver();
                    CerrarHID();
                    return false;
                }
            }

            CerrarDriver();
            return true;
        }

        private bool IniciarMenu()
        {
            UInt32 ret = 0;
            byte[] buf = new byte[] { 1 };

            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_INFO_LUZ, buf, 1, null, 0, out ret, IntPtr.Zero))
            {
                MessageBox.Show("No se puede enviar la orden al driver", "[CMFD][1.1]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_USR_RAW, buf, 1, null, 0, out ret, IntPtr.Zero))
            {
                MessageBox.Show("No se puede enviar la orden al driver", "[CMFD][1.2]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return VerPantalla1(0, 0);
        }

        private void CerrarMenu()
        {
            if (AbrirDriver())
                return;

            UInt32 ret = 0;
            byte[] buf = new byte[] { 0 };

            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_INFO_LUZ, buf, 1, null, 0, out ret, IntPtr.Zero))
                MessageBox.Show("No se puede enviar la orden al driver", "[CMFD][2.1]", MessageBoxButton.OK, MessageBoxImage.Warning);

            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_USR_RAW, buf, 1, null, 0, out ret, IntPtr.Zero))
                MessageBox.Show("No se puede enviar la orden al driver", "[CMFD][2.2]", MessageBoxButton.OK, MessageBoxImage.Warning);

            //Limpiar pantalla
            byte[] fila = new byte[2] { 1, 0 };
            CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, fila, 2, null, 0, out ret, IntPtr.Zero);
            fila[0] = 2;
            CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, fila, 2, null, 0, out ret, IntPtr.Zero);
            fila[0] = 3;
            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, fila, 2, null, 0, out ret, IntPtr.Zero))
                MessageBox.Show("No se puede enviar la orden al driver", "[CMFD][2.3]", MessageBoxButton.OK, MessageBoxImage.Warning);

            CerrarDriver();
            return;
        }

        #region "Pantallas"
        private bool VerPantalla1(byte cursor, byte pagina)
        {
            if (!AbrirDriver())
                return false;

            String[] filas = new String[] { " Pedales        ",
                                            " Luz botones    ",
                                            " Luz MFD        ",
                                            " Hora 1         ",
                                            " Hora 2         ",
                                            " Hora 3         ",
                                            " Salir          "
                                            };
            filas[cursor] = ">" + filas[cursor].Remove(0, 1);

            for (byte i = 0; i < 3; i++)
            {
                if ((pagina == 2) && (i == 1))
                    break;
                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.ASCII, System.Text.Encoding.Unicode.GetBytes(filas[i + (pagina * 3)]));
                byte[] buffer = new byte[17];
                for (byte c = 1; c < 17; i++)
                    buffer[c] = texto[c - 1];

                buffer[0] = (byte)(i + 1);

                if ((i == 0) && (pagina != 0))
                    buffer[16] = 174; //«
                if (i == 2)
                    buffer[16] = 175; //»

                UInt32 ret = 0;
                CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, buffer, 17, null, 0, out ret, IntPtr.Zero);
            }

            CerrarDriver();
            return true;
        }

        private bool VerPantallaOnOff(byte cursor, bool estado)
        {
            if (!AbrirDriver())
                return false;

            String[] filas = new String[] { " On  ",
                                            " Off "};

            filas[cursor] = ">" + filas[cursor].Remove(0, 1);
            filas[(estado) ? 0 : 1] = filas[(estado) ? 0 : 1] + "(*)";

            for (byte i = 0; i < 2; i++)
            {

                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.ASCII, System.Text.Encoding.Unicode.GetBytes(filas[i]));
                byte[] buffer = new byte[17];
                for (byte c = 1; c < (texto.Length + 1); i++)
                    buffer[c] = texto[c - 1];

                buffer[0] = (byte)(i + 1);

                UInt32 ret = 0;
                CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, buffer, (uint)texto.Length + 1, null, 0, out ret, IntPtr.Zero);
            }

            CerrarDriver();
            return true;
        }

        private bool VerPantallaLuz(byte cursor, byte estado)
        {
            if (!AbrirDriver())
                return false;

            String[] filas = new String[] { " Bajo  ",
                                            " Medio ",
                                            " Alto  "};

            filas[cursor] = ">" + filas[cursor].Remove(0, 1);
            filas[estado] = filas[estado] + "(*)";

            for (byte i = 0; i < 3; i++)
            {

                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.ASCII, System.Text.Encoding.Unicode.GetBytes(filas[i]));
                byte[] buffer = new byte[17];
                for (byte c = 1; c < (texto.Length + 1); i++)
                    buffer[c] = texto[c - 1];

                buffer[0] = (byte)(i + 1);

                UInt32 ret = 0;
                CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, buffer, (uint)texto.Length + 1, null, 0, out ret, IntPtr.Zero);
            }

            CerrarDriver();
            return true;
        }

        private bool VerPantallaHora(byte cursor, byte pagina, bool sel, byte hora, byte minuto, bool ampm)
        {
            if (!AbrirDriver())
                return false;

            String[] filas = new String[] { " Hora:   ",
                                            " Minuto: ",
                                            " AM/PM: ",
                                            " Volver        «"};

            filas[cursor] = ">" + filas[cursor].Remove(0, 1);
            if (sel)
                filas[cursor] = ">" + filas[cursor];
            filas[0] += hora.ToString();
            filas[1] += minuto.ToString();
            filas[2] += (ampm) ? "Si     »" : "No     »";

            for (byte i = 0; i < 2; i++)
            {
                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.ASCII, System.Text.Encoding.Unicode.GetBytes(filas[i]));
                byte[] buffer = new byte[17];
                for (byte c = 1; c < (texto.Length + 1); i++)
                    buffer[c] = texto[c - 1];

                buffer[0] = (byte)(i + 1);

                UInt32 ret = 0;
                CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, buffer, (uint)texto.Length + 1, null, 0, out ret, IntPtr.Zero);

                if (pagina == 1)
                    break;
            }

            CerrarDriver();
            return true;
        }
        #endregion

        #region "Botones"
        private bool IniciarHID()
        {
            CRawInput.RAWINPUTDEVICE[] rdev = new CRawInput.RAWINPUTDEVICE[3];
            rdev[0].UsagePage = 0x01;
            rdev[0].Usage = 0x04;
            rdev[0].WindowHandle = hWnd.Handle;
            rdev[0].Flags = CRawInput.RawInputDeviceFlags.None;

            if (!CRawInput.RegisterRawInputDevices(rdev, 1, (uint)Marshal.SizeOf(typeof(CRawInput.RAWINPUTDEVICE))))
            {
                MessageBox.Show("No se pudo registrar la entrada de datos HID", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                return false;
            }
            else
            {
                hidOn = true;
                hWnd.AddHook(WndProc);
            }

            return true;
        }
        private void CerrarHID()
        {
            if (hidOn)
            {
                hWnd.RemoveHook(WndProc);

                CRawInput.RAWINPUTDEVICE[] rdev = new CRawInput.RAWINPUTDEVICE[3];
                rdev[0].UsagePage = 0x01;
                rdev[0].Usage = 0x04;
                rdev[0].WindowHandle = hWnd.Handle;
                rdev[0].Flags = CRawInput.RawInputDeviceFlags.Remove;

                CRawInput.RegisterRawInputDevices(rdev, 1, (uint)Marshal.SizeOf(typeof(CRawInput.RAWINPUTDEVICE)));
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x00FF)
            {
                int outSize = 0;
                int size = 0;

                outSize = CRawInput.GetRawInputData(lParam, 0x10000003, null, ref size, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
                if (size != 0)
                {
                    byte[] buff = new byte[size];

                    outSize = CRawInput.GetRawInputData(lParam, 0x10000003, buff, ref size, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
                    if (outSize != -1)
                    {
                        CRawInput.RAWINPUTHEADER header = new CRawInput.RAWINPUTHEADER();

                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
                        Marshal.Copy(buff, 0, ptr, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
                        header = Marshal.PtrToStructure<CRawInput.RAWINPUTHEADER>(ptr);
                        Marshal.FreeHGlobal(ptr);
                        if (header.dwType == 2)
                        {
                            IntPtr pNombre = Marshal.AllocHGlobal(256);
                            uint cbSize = 128;
                            uint ret = CRawInput.GetRawInputDeviceInfo(header.hDevice, CRawInput.RawInputDeviceInfoCommand.DeviceName, pNombre, ref cbSize);
                            String nombre = Marshal.PtrToStringAnsi(pNombre);
                            Marshal.FreeHGlobal(pNombre);
                            if (nombre.StartsWith("\\\\?\\HID#VID_06A3&PID_0255"))
                            {
                                CRawInput.RAWINPUTHID hid = new CRawInput.RAWINPUTHID();

                                ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CRawInput.RAWINPUTHID)));
                                Marshal.Copy(buff, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)), ptr, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHID)));
                                hid = Marshal.PtrToStructure<CRawInput.RAWINPUTHID>(ptr);
                                Marshal.FreeHGlobal(ptr);

                                byte[] hidData = new byte[hid.Size - 1];
                                for (int i = 0; i < hidData.Length; i++)
                                    hidData[i] = buff[i + 1 + size - hid.Size];

                                ActualizarEstadoHID(hidData);
                            }
                        }
                    }
                }

            }
            return IntPtr.Zero;
        }

        private void ActualizarEstadoHID(byte[] hidData)
        {
            bool btIntro = (hidData[20 + (10/ 8)] & (1 << (10 % 8))) == 1;
            bool btArriba = (hidData[20 + (11 / 8)] & (1 << (11 % 8))) == 1;
            bool btAbajo = (hidData[20 + (12 / 8)] & (1 << (12 % 8))) == 1;

            if (btIntro)
            {
                switch (estadoPagina)
                {
                    case 0:
                        switch (estadoCursor)
                        {
                            case 0:
                                estadoPagina = 1;
                                VerPantallaOnOff(0, conf.CONFIGURACION[0].Pedales);
                                break;
                            case 1:
                                estadoPagina = 2;
                                VerPantallaLuz(0, conf.CONFIGURACION[0].LuzGlobal);
                                break;
                            case 2:
                                estadoPagina = 3;
                                VerPantallaLuz(0, conf.CONFIGURACION[0].LuzMfd);
                                break;
                            case 3:
                                estadoPagina = 4;
                                VerPantallaHora(0, 0, false, (byte)(conf.CONFIGURACION[0].hora1 / 60), (byte)(conf.CONFIGURACION[0].hora1 % 60), conf.CONFIGURACION[0].hora1_24h);
                                break;
                            case 4:
                                estadoPagina = 5;
                                VerPantallaHora(0, 0, false, (byte)(conf.CONFIGURACION[0].hora2 / 60), (byte)(conf.CONFIGURACION[0].hora2 % 60), conf.CONFIGURACION[0].hora2_24h);
                                break;
                            case 5:
                                estadoPagina = 6;
                                VerPantallaHora(0, 0, false, (byte)(conf.CONFIGURACION[0].hora3 / 60), (byte)(conf.CONFIGURACION[0].hora3 % 60), conf.CONFIGURACION[0].hora3_24h);
                                break;
                            case 6:
                                CerrarHID();
                                CerrarMenu();
                                break;
                        }
                        break;
                    case 1:
                        bool pedales = (estadoCursor == 0);
                        if (SetPedales(pedales))
                               conf.CONFIGURACION[0].Pedales = pedales;
                        estadoPagina = 0;
                        VerPantalla1(0, 0);
                        break;
                    case 2:
                        if (SetLuzGlobal(estadoCursor))
                            conf.CONFIGURACION[0].LuzGlobal = estadoCursor;
                        estadoPagina = 0;
                        VerPantalla1(1, 0);
                        break;
                    case 3:
                        if (SetLuzMFD(estadoCursor))
                            conf.CONFIGURACION[0].LuzMfd = estadoCursor;
                        estadoPagina = 0;
                        VerPantalla1(2, 0);
                        break;
                    case 4:
                        estadoPagina = (byte)(40 + estadoCursor);
                        VerPantallaHora(estadoCursor, 0, true, (byte)(conf.CONFIGURACION[0].hora1 / 60), (byte)(conf.CONFIGURACION[0].hora1 % 60), conf.CONFIGURACION[0].hora1_24h);
                        break;
                    case 40:
                    case 41:
                    case 42:
                        if (SetHora())
                            conf.CONFIGURACION[0].hora1 = xxx;
                        estadoPagina = 4;
                        VerPantallaHora(0, 0, false, (byte)(conf.CONFIGURACION[0].hora1 / 60), (byte)(conf.CONFIGURACION[0].hora1 % 60), conf.CONFIGURACION[0].hora1_24h);
                        break;
                    case 43:
                        estadoPagina = 0;
                        VerPantalla1(3, 0);
                        break;

                }
            }
        }
        #endregion
    }
}
