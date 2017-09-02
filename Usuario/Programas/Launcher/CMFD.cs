using System;
using System.Windows;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Launcher
{
    internal class CMFD
    {
        private bool activado = false;
        private SafeFileHandle driver = null;

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
                if (!IniciarMenu())
                {
                    activado = false;
                    CerrarDriver();
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
                                            " Hora 1 (24h)   ",
                                            " Hora 2 (24h)   ",
                                            " Hora 3 (24h)   ",
                                            " Hora 1 (AM/PM) ",
                                            " Hora 2 (AM/PM) ",
                                            " Hora 3 (AM/PM) ",
                                            " Salir          "
                                            };
            filas[cursor] = ">" + filas[cursor].Remove(0, 1);

            for (byte i = 0; i < 3; i++)
            {
                if ((pagina == 3) && (i == 1))
                    break;
                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.ASCII, System.Text.Encoding.Unicode.GetBytes(filas[i + (pagina * 3)]));
                byte[] buffer = new byte[17];
                for (byte c = 1; c < 17; i++)
                    buffer[c] = texto[c - 1];

                buffer[0] = (byte)(i + 1);

                if ((i == 0) && (pagina != 0))
                    buffer[16] = 174; //«
                if (i == 3)
                    buffer[16] = 175; //»

                UInt32 ret = 0;
                CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, buffer, 17, null, 0, out ret, IntPtr.Zero);
            }

            CerrarDriver();
            return true;
        }

        private bool VerPantallaPedales(byte cursor, bool estado)
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

        private bool VerPantallaHora(byte cursor, byte hora, byte minuto)
        {
            if (!AbrirDriver())
                return false;

            String[] filas = new String[] { " Hora:   ",
                                            " Minuto: "};

            filas[cursor] = ">" + filas[cursor].Remove(0, 1);
            filas[0] = filas[0] + hora.ToString();
            filas[1] = filas[0] + minuto.ToString();

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
        #endregion
    }
}
