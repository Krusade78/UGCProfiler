using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading;

namespace Launcher
{
    internal class CMFD : IDisposable
    {
        private System.Windows.Interop.HwndSource hWnd = null;
        private DataSetConfiguracion conf;

        private bool hidOn = false;
        private SemaphoreSlim semActivado = new SemaphoreSlim(1, 1);

        private byte estadoCursor = 0;
        private byte estadoPagina = 0;
        private short auxHora = 0;
        private byte auxMinuto = 0;
        private bool aux24h = false;

        public CMFD(System.Windows.Interop.HwndSource hWnd, ref DataSetConfiguracion dsc)
        {
            this.hWnd = hWnd;
            this.conf = dsc;
        }

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (!semActivado.Wait(0)) //si está activado
                        CerrarMenu();
                    semActivado.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region "Driver"
        private bool SetPedales(bool onoff)
        {
            if (!CSystem32.AbrirDriver())
                return false;

            UInt32 ret = 0;
            byte[] buffer = new byte[1] { (onoff) ? (byte)1 : (byte)0 };
            if (!CSystem32.DeviceIoControl(CSystem32.IOCTL_PEDALES, buffer, 1, null, 0, out ret, IntPtr.Zero))
            {
                CSystem32.CerrarDriver();
                MainWindow.MessageBox("Error de acceso al dispositivo", "[CMFD][2.1]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            CSystem32.CerrarDriver();
            return true;
        }

        private bool SetLuz(byte nivel, bool global)
        {
            if (!CSystem32.AbrirDriver())
                return false;

            UInt32 ret = 0;
            byte[] buffer = new byte[1] { nivel };
            uint ioctl = global ? CSystem32.IOCTL_GLOBAL_LUZ :CSystem32.IOCTL_MFD_LUZ;

            if (!CSystem32.DeviceIoControl(ioctl, buffer, 1, null, 0, out ret, IntPtr.Zero))
            {
                CSystem32.CerrarDriver();
                MainWindow.MessageBox("Error de acceso al dispositivo", "[CMFD][3.1]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            CSystem32.CerrarDriver();
            return true;
        }

        private bool SetHoras2y3(byte reloj, short hora, byte minuto, bool f24h)
        {
            if (!CSystem32.AbrirDriver())
                return false;

            UInt32 ret = 0;
            byte[] buffer = new byte[3];
            buffer[0] = reloj;
            short minutos = (short)(hora * 60 + (((hora < 0) ? -1 : 1) * minuto));
            if (minutos < 0)
            {
                buffer[1] = (byte)(((-minutos) >> 8) + 4);
                buffer[2] = (byte)((-minutos) & 0xff);
            }
            else
            {
                buffer[1] = (byte)(minutos >> 8);
                buffer[2] = (byte)(minutos & 0xff);
            }
            if (f24h)
            {
                if (!CSystem32.DeviceIoControl(CSystem32.IOCTL_HORA24, buffer, 3, null, 0, out ret, IntPtr.Zero))
                {
                    CSystem32.CerrarDriver();
                    MainWindow.MessageBox("Error de acceso al dispositivo", "[CMFD][4.1]", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            else
            {
                if (!CSystem32.DeviceIoControl(CSystem32.IOCTL_HORA, buffer, 3, null, 0, out ret, IntPtr.Zero))
                {
                    CSystem32.CerrarDriver();
                    MainWindow.MessageBox("Error de acceso al dispositivo", "[CMFD][4.2]", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            CSystem32.CerrarDriver();
            return true;
        }
        #endregion

        public bool ComprobarEstado()
        {
            if (!semActivado.Wait(0))
                return true;

            if (!CSystem32.AbrirDriver())
            {
                semActivado.Release();
                return false;
            }

            UInt32 ret = 0;
            byte[] buf = new byte[1];
            if (!CSystem32.DeviceIoControl(CSystem32.IOCTL_GET_MENU, null, 0, buf, 1, out ret, IntPtr.Zero))
            {
                CSystem32.CerrarDriver();
                MainWindow.MessageBox("No se puede enviar la orden al driver", "[CMFD][5.1]", MessageBoxButton.OK, MessageBoxImage.Warning);
                semActivado.Release();
                return false;
            }

            if (buf[0] == 1)
            {
                if (!IniciarMenu())
                {
                    CerrarMenu();
                    CSystem32.CerrarDriver();
                    semActivado.Release();
                    return false;
                }
            }
            else
                semActivado.Release();

            CSystem32.CerrarDriver();
            return true;
        }

        private bool IniciarMenu()
        {
            if (IniciarHID())
            {
                VerPantalla1(0, 0);
                return true;
            }
            else
                return false;
        }

        private void CerrarMenu()
        {
            CerrarHID();

            if (!CSystem32.AbrirDriver())
                return;

            UInt32 ret = 0;
            if (!CSystem32.DeviceIoControl(CSystem32.IOCTL_DESACTIVAR_MENU, null, 0, null, 0, out ret, IntPtr.Zero))
            {
                CSystem32.CerrarDriver();
                MainWindow.MessageBox("No se puede enviar la orden al driver", "[CMFD][7.1]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //Limpiar pantalla
            byte[] fila = new byte[2] { 1, 0 };
            CSystem32.DeviceIoControl(CSystem32.IOCTL_TEXTO, fila, 2, null, 0, out ret, IntPtr.Zero);
            fila[0] = 2;
            CSystem32.DeviceIoControl(CSystem32.IOCTL_TEXTO, fila, 2, null, 0, out ret, IntPtr.Zero);
            fila[0] = 3;
            if (!CSystem32.DeviceIoControl(CSystem32.IOCTL_TEXTO, fila, 2, null, 0, out ret, IntPtr.Zero))
                MainWindow.MessageBox("No se puede enviar la orden al driver", "[CMFD][7.2]", MessageBoxButton.OK, MessageBoxImage.Warning);

            CSystem32.CerrarDriver();

            //GuardarConfiguracion();
            try
            {
                conf.WriteXml("configuracion.dat");
            }
            catch (Exception ex)
            {
                MainWindow.MessageBox(ex.Message, "[CMFD][7.3]", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            semActivado.Release();
            return;
        }

        #region "Pantallas"
        private bool VerPantalla1(byte cursor, byte pagina)
        {
            if (!CSystem32.AbrirDriver())
                return false;

            String[] filas = new String[] { " Pedales        ",
                                            " Luz botones    ",
                                            " Luz MFD       ³",
                                            " Hora 1        ¹",
                                            " Hora 2         ",
                                            " Hora 3        ³", //251
                                            " Salir         ¹", //252
                                            "                ",
                                            "                "
                                            };
            filas[cursor + (pagina * 3)] = ">" + filas[cursor + (pagina * 3)].Remove(0, 1);

            for (byte i = 0; i < 3; i++)
            {
                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(filas[i + (pagina * 3)]));
                byte[] buffer = new byte[17];
                for (byte c = 1; c < 17; c++)
                    buffer[c] = texto[c - 1];

                buffer[0] = (byte)(i + 1);

                UInt32 ret = 0;
                if (!CSystem32.DeviceIoControl(CSystem32.IOCTL_TEXTO, buffer, 17, null, 0, out ret, IntPtr.Zero))
                {
                    CSystem32.CerrarDriver();
                    return false;
                }
            }

            CSystem32.CerrarDriver();
            return true;
        }

        private bool VerPantallaOnOff(byte cursor, bool estado)
        {
            if (!CSystem32.AbrirDriver())
                return false;

            String[] filas = new String[] { " On  ",
                                            " Off "};

            filas[cursor] = ">" + filas[cursor].Remove(0, 1);
            filas[(estado) ? 0 : 1] = filas[(estado) ? 0 : 1] + "(*)";

            for (byte i = 0; i < 2; i++)
            {

                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(filas[i]));
                byte[] buffer = new byte[17];
                for (byte c = 1; c < (texto.Length + 1); c++)
                    buffer[c] = texto[c - 1];

                buffer[0] = (byte)(i + 1);

                UInt32 ret = 0;
                if (!CSystem32.DeviceIoControl(CSystem32.IOCTL_TEXTO, buffer, (uint)texto.Length + 1, null, 0, out ret, IntPtr.Zero))
                {
                    CSystem32.CerrarDriver();
                    return false;
                }
            }

            CSystem32.CerrarDriver();
            return true;
        }

        private bool VerPantallaLuz(byte cursor, byte estado)
        {
            if (!CSystem32.AbrirDriver())
                return false;

            String[] filas = new String[] { " Bajo  ",
                                            " Medio ",
                                            " Alto  "};

            filas[cursor] = ">" + filas[cursor].Remove(0, 1);
            filas[estado] = filas[estado] + "(*)";

            for (byte i = 0; i < 3; i++)
            {

                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(filas[i]));
                byte[] buffer = new byte[17];
                for (byte c = 1; c < (texto.Length + 1); c++)
                    buffer[c] = texto[c - 1];

                buffer[0] = (byte)(i + 1);

                UInt32 ret = 0;
                if (!CSystem32.DeviceIoControl(CSystem32.IOCTL_TEXTO, buffer, (uint)texto.Length + 1, null, 0, out ret, IntPtr.Zero))
                {
                    CSystem32.CerrarDriver();
                    return false;
                }
            }

            CSystem32.CerrarDriver();
            return true;
        }

        private bool VerPantallaHora(byte cursor, byte pagina, bool sel, short hora, byte minuto, bool h24)
        {
            if (!CSystem32.AbrirDriver())
                return false;

            String[] filas = new String[] { " Hora:   ",
                                            " Minuto: ",
                                            " AM/PM: ",
                                            ">Volver       ¹",
                                            "",
                                            ""};

            filas[cursor] = ((sel) ? "#" : ">") + filas[cursor].Remove(0, 1);
            filas[0] += hora.ToString();
            filas[1] += minuto.ToString();
            filas[2] += (!h24) ? " Si    ³" : " No    ³";

            for (byte i = 0; i < 3; i++)
            {
                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(850), System.Text.Encoding.Unicode.GetBytes(filas[i + (3 * pagina)]));
                byte[] buffer = new byte[17];
                for (byte c = 1; c < (texto.Length + 1); c++)
                    buffer[c] = texto[c - 1];

                buffer[0] = (byte)(i + 1);

                UInt32 ret = 0;
                if (!CSystem32.DeviceIoControl(CSystem32.IOCTL_TEXTO, buffer, (uint)texto.Length + 1, null, 0, out ret, IntPtr.Zero))
                {
                    CSystem32.CerrarDriver();
                    return false;
                }
            }

            CSystem32.CerrarDriver();
            return true;
        }
        #endregion

        #region "Botones"
        private bool IniciarHID()
        {
            if (!hidOn)
            {
                CRawInput.RAWINPUTDEVICE[] rdev = new CRawInput.RAWINPUTDEVICE[3];
                rdev[0].UsagePage = 0x01;
                rdev[0].Usage = 0x04;
                rdev[0].WindowHandle = hWnd.Handle;
                rdev[0].Flags = CRawInput.RawInputDeviceFlags.InputSink;

                if (!CRawInput.RegisterRawInputDevices(rdev, 1, (uint)Marshal.SizeOf(typeof(CRawInput.RAWINPUTDEVICE))))
                {
                    MainWindow.MessageBox("No se pudo registrar la entrada de datos HID", "[CMFD][7.1]", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return false;
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                    hidOn = true;
                    hWnd.AddHook(WndProc);
                }
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
                hidOn = false;
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

                                ComprobarEstadoHID(hidData);
                            }
                        }
                    }
                }

            }
            return IntPtr.Zero;
        }

        private byte datoAntiguo = 0;
        private void ComprobarEstadoHID(byte[] hidData)
        {
            if (datoAntiguo == hidData[20 + 1]) //los tres botones están en byte 21
                return;
            else
                datoAntiguo = hidData[20 + 1];

            bool btIntro = ((hidData[20 + (12/ 8)] >> (11 % 8)) & 1) == 1;
            bool btAbajo = ((hidData[20 + (13 / 8)] >> (12 % 8)) & 1) == 1;
            bool btArriba = ((hidData[20 + (14 / 8)] >> (13 % 8)) & 1) == 1;

            #region "Boton intro"
            if (btIntro)
            {
                switch (estadoPagina)
                {
                    case 0: //principal
                        switch (estadoCursor)
                        {
                            case 0:
                                estadoCursor = 0;
                                estadoPagina = 1;
                                VerPantallaOnOff(0, conf.CONFIGURACION[0].Pedales);
                                break;
                            case 1:
                                estadoCursor = 0;
                                estadoPagina = 2;
                                VerPantallaLuz(0, conf.CONFIGURACION[0].LuzGlobal);
                                break;
                            case 2:
                                estadoCursor = 0;
                                estadoPagina = 3;
                                VerPantallaLuz(0, conf.CONFIGURACION[0].LuzMfd);
                                break;
                            case 3:
                                estadoCursor = 0;
                                estadoPagina = 4;
                                auxHora = (short)(conf.CONFIGURACION[0].hora1 / 60);
                                auxMinuto = (byte)(Math.Abs(conf.CONFIGURACION[0].hora1) % 60);
                                aux24h = conf.CONFIGURACION[0].hora1_24h;
                                VerPantallaHora(0, 0, false, auxHora, auxMinuto, aux24h);
                                break;
                            case 4:
                                estadoCursor = 0;
                                estadoPagina = 5;
                                auxHora = (short)(conf.CONFIGURACION[0].hora2 / 60);
                                auxMinuto = (byte)(Math.Abs(conf.CONFIGURACION[0].hora2) % 60);
                                aux24h = conf.CONFIGURACION[0].hora2_24h;
                                VerPantallaHora(0, 0, false, auxHora, auxMinuto, aux24h);
                                break;
                            case 5:
                                estadoCursor = 0;
                                estadoPagina = 6;
                                auxHora = (short)(conf.CONFIGURACION[0].hora3 / 60);
                                auxMinuto = (byte)(Math.Abs(conf.CONFIGURACION[0].hora3) % 60);
                                aux24h = conf.CONFIGURACION[0].hora3_24h;
                                VerPantallaHora(0, 0, false, auxHora, auxMinuto, aux24h);
                                break;
                            case 6:
                                estadoCursor = 0;
                                estadoPagina = 0;
                                CerrarMenu();
                                break;
                        }
                        break;
                    case 1: //pedales
                        bool pedales = (estadoCursor == 0);
                        if (SetPedales(pedales))
                               conf.CONFIGURACION[0].Pedales = pedales;
                        estadoCursor = 0;
                        estadoPagina = 0;
                        VerPantalla1(0, 0);
                        break;
                    case 2: // luz global
                        if (SetLuz(estadoCursor, true))
                            conf.CONFIGURACION[0].LuzGlobal = estadoCursor;
                        estadoCursor = 1;
                        estadoPagina = 0;
                        VerPantalla1(1, 0);
                        break;
                    case 3: //luz mfd
                        if (SetLuz(estadoCursor, false))
                            conf.CONFIGURACION[0].LuzMfd = estadoCursor;
                        estadoCursor = 2;
                        estadoPagina = 0;
                        VerPantalla1(2, 0);
                        break;                       
                    case 4: //hora 1
                    case 5: //hora 2
                    case 6: //hora 3
                        if (estadoCursor == 3)
                        {
                            estadoCursor = (byte)(estadoPagina - 1);
                            estadoPagina = 0;
                            VerPantalla1((byte)(estadoCursor % 3), 1);
                        }
                        else
                        {
                            estadoPagina = (byte)(estadoPagina * 10 + estadoCursor);
                            VerPantallaHora((byte)(estadoCursor % 3), (byte)(estadoCursor / 3), true, auxHora, auxMinuto, aux24h);
                        }
                        break;
                    case 40:
                    case 41:
                    case 42:
                        conf.CONFIGURACION[0].hora1 = (short)(auxHora * 60 + (((auxHora < 0) ? -1 : 1) * auxMinuto));
                        conf.CONFIGURACION[0].hora1_24h = aux24h;

                        estadoPagina = 4;
                        VerPantallaHora(estadoCursor, 0, false, auxHora, auxMinuto, aux24h);
                        break;
                    case 50:
                    case 51:
                    case 52:
                        if (SetHoras2y3(3, auxHora, auxMinuto, aux24h))
                        {
                            conf.CONFIGURACION[0].hora2 = (short)(auxHora * 60 + (((auxHora < 0) ? -1 : 1) * auxMinuto));
                            conf.CONFIGURACION[0].hora2_24h = aux24h;
                        }
                        estadoPagina = 5;
                        VerPantallaHora(estadoCursor, 0, false, auxHora, auxMinuto, aux24h);
                        break;
                    case 60:
                    case 61:
                    case 62:
                        if (SetHoras2y3(3, auxHora, auxMinuto, aux24h))
                        {
                            conf.CONFIGURACION[0].hora3 = (short)(auxHora * 60 + (((auxHora < 0) ? -1 : 1) * auxMinuto));
                            conf.CONFIGURACION[0].hora3_24h = aux24h;
                        }
                        estadoPagina = 6;
                        VerPantallaHora(estadoCursor, 0, false, auxHora, auxMinuto, aux24h);
                        break;
                }
            }
            #endregion

            #region "botón arriba"
            if (btArriba)
            {
                switch (estadoPagina)
                {
                    case 0:
                        if (estadoCursor != 0)
                            estadoCursor--;

                        VerPantalla1((byte)(estadoCursor % 3), (byte)(estadoCursor / 3));
                        break;
                    case 1:
                        if (estadoCursor != 0)
                            estadoCursor--;

                        VerPantallaOnOff(estadoCursor, estadoCursor == 0);
                        break;
                    case 2:
                    case 3:
                        if (estadoCursor != 0)
                            estadoCursor--;

                        VerPantallaLuz(estadoCursor, estadoCursor);
                        break;
                    case 4:
                    case 5:
                    case 6:
                        if (estadoCursor != 0)
                            estadoCursor--;

                        VerPantallaHora(estadoCursor, (byte)(estadoCursor / 3), false, auxHora, auxMinuto, aux24h);
                        break;
                    case 40:
                    case 50:
                    case 60:
                        if (auxHora != 23)
                            auxHora++;

                        VerPantallaHora(estadoCursor, 0, true, auxHora, auxMinuto, aux24h);
                        break;
                    case 41:
                    case 51:
                    case 61:
                        if (auxMinuto != 59)
                            auxMinuto++;

                        VerPantallaHora(estadoCursor, 0, true, auxHora, auxMinuto, aux24h);
                        break;
                    case 42:
                    case 52:
                    case 62:
                        aux24h = !aux24h;
                        VerPantallaHora(estadoCursor, 0, true, auxHora, auxMinuto, aux24h);
                        break;
                }
            }
            #endregion
            #region "botón abajo"
            else if (btAbajo)
            {
                switch (estadoPagina)
                {
                    case 0:
                        if (estadoCursor != 6)
                            estadoCursor++;

                        VerPantalla1((byte)(estadoCursor % 3), (byte)(estadoCursor / 3));
                        break;
                    case 1:
                        if (estadoCursor != 1)
                            estadoCursor++;

                        VerPantallaOnOff(estadoCursor, estadoCursor == 0);
                        break;
                    case 2:
                    case 3:
                        if (estadoCursor != 2)
                            estadoCursor++;

                        VerPantallaLuz(estadoCursor, estadoCursor);
                        break;
                    case 4:
                    case 5:
                    case 6:
                        if (estadoCursor != 3)
                            estadoCursor++;

                        VerPantallaHora((byte)(estadoCursor % 3), (byte)(estadoCursor / 3), false, auxHora, auxMinuto, aux24h);
                        break;
                    case 40:
                    case 50:
                    case 60:
                        if (auxHora != -23)
                            auxHora--;

                        VerPantallaHora(estadoCursor, 0, true, auxHora, auxMinuto, aux24h);
                        break;
                    case 41:
                    case 51:
                    case 61:
                        if (auxMinuto != 0)
                            auxMinuto--;

                        VerPantallaHora(estadoCursor, 0, true, auxHora, auxMinuto, aux24h);
                        break;
                    case 42:
                    case 52:
                    case 62:
                        aux24h = !aux24h;
                        VerPantallaHora(estadoCursor, 0, true, auxHora, auxMinuto, aux24h);
                        break;
                }
            }
            #endregion
        }
        #endregion
    }
}
