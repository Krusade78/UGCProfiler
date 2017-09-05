using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading.Tasks;

namespace Launcher
{
    class CServicio : IDisposable
    {
        //Nota: Los datos de horas están en minutos
        private Window wnd;
        private CMFD mfd;

        private DataSetConfiguracion dsc = new DataSetConfiguracion();
        private Timer timer = new Timer(2000);

        private bool fechaActiva = true;
        private bool horaActiva = true;
        private bool timerMenu = false;

        public CServicio(Window wnd)
        {
            mfd = new CMFD((System.Windows.Interop.HwndSource)PresentationSource.FromVisual(wnd), ref dsc);
        }

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timer.Stop();
                    timer.Close(); timer = null;
                    dsc.Dispose(); dsc = null;
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

        public bool Iniciar()
        {
            if (!CargarCalibrado())
                return false;
            if (!CargarConfiguracion())
                return false;

            SetTextoInicio();
            timer.Elapsed += Tick;
            timer.Start();

            return true;
        }

        private bool CargarCalibrado()
        {
            byte[] buf = new byte[Marshal.SizeOf(typeof(CSystem32.CALIBRADO)) * 4];

            System.IO.FileStream archivo = null;
            try
            {
                archivo = new System.IO.FileStream("calibrado.dat", System.IO.FileMode.Open);
                if (archivo.Read(buf, 0, buf.Length) != buf.Length)
                    throw new Exception("Error de lectura del archivo de calibrado");
            }
            catch (System.IO.FileNotFoundException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "[X52-Service][0.1]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            finally
            {
                if (archivo != null) { try { archivo.Close(); archivo = null; } catch { } }
            }

            Microsoft.Win32.SafeHandles.SafeFileHandle driver = CSystem32.CreateFile(
                    "\\\\.\\XUSBInterface",
                    0x80000000 | 0x40000000,//GENERIC_WRITE | GENERIC_READ,
                    0x00000002 | 0x00000001, //FILE_SHARE_WRITE | FILE_SHARE_READ,
                    IntPtr.Zero,
                    3,//OPEN_EXISTING,
                    0,
                    IntPtr.Zero);
            if (driver.IsInvalid)
            {
                MessageBox.Show("No se puede abrir el driver", "[X52-Service][0.2]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            UInt32 ret = 0;
            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_USR_CALIBRADO, buf, (uint)buf.Length, null, 0, out ret, IntPtr.Zero))
            {
                driver.Close();
                MessageBox.Show("No se puede enviar la orden al driver", "[X52-Service][0.3]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            driver.Close();

            return true;
        }

        private bool CargarConfiguracion()
        {
            if (System.IO.File.Exists("configuracion.dat"))
            {
                try
                {
                    dsc.ReadXml("configuracion.dat");
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                dsc.CONFIGURACION.AddCONFIGURACIONRow(dsc.CONFIGURACION.NewCONFIGURACIONRow());
            }

            Microsoft.Win32.SafeHandles.SafeFileHandle driver = CSystem32.CreateFile(
                "\\\\.\\XUSBInterface",
                0x80000000 | 0x40000000,//GENERIC_WRITE | GENERIC_READ,
                0x00000002 | 0x00000001, //FILE_SHARE_WRITE | FILE_SHARE_READ,
                IntPtr.Zero,
                3,//OPEN_EXISTING,
                0,
                IntPtr.Zero);
            if (driver.IsInvalid)
            {
                MessageBox.Show("No se puede abrir el driver", "[X52-Service][1.1]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            UInt32 ret = 0;

            byte[] buffer = new byte[1] { dsc.CONFIGURACION[0].LuzMfd };
            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_MFD_LUZ, buffer, 1, null, 0, out ret, IntPtr.Zero))
            {
                driver.Close();
                MessageBox.Show("Error de acceso al dispositivo", "[X52-Service][1.2]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            buffer[0] = dsc.CONFIGURACION[0].LuzGlobal;
            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_GLOBAL_LUZ, buffer, 1, null, 0, out ret, IntPtr.Zero))
            {
                driver.Close();
                MessageBox.Show("Error de acceso al dispositivo", "[X52-Service][1.3]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            DateTime fecha = DateTime.Now;
            buffer = new byte[2] { 1, (byte)fecha.Day };

            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_FECHA, buffer, 2, null, 0, out ret, IntPtr.Zero))
            {
                driver.Close();
                MessageBox.Show("Error de acceso al dispositivo", "[X52-Service][1.4]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            buffer[0] = 2; buffer[1] = (byte)fecha.Month;
            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_FECHA, buffer, 2, null, 0, out ret, IntPtr.Zero))
            {
                driver.Close();
                MessageBox.Show("Error de acceso al dispositivo", "[X52-Service][1.5]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            buffer[0] = 3; buffer[1] = (byte)(fecha.Year % 100);
            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_FECHA, buffer, 2, null, 0, out ret, IntPtr.Zero))
            {
                driver.Close();
                MessageBox.Show("Error de acceso al dispositivo", "[X52-Service][1.6]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            buffer = new byte[3];
            UInt16[] horas = new UInt16[] { dsc.CONFIGURACION[0].hora1, dsc.CONFIGURACION[0].hora2, dsc.CONFIGURACION[0].hora3 };
            bool[] horas24 = new bool[] { dsc.CONFIGURACION[0].hora1_24h, dsc.CONFIGURACION[0].hora2_24h, dsc.CONFIGURACION[0].hora3_24h };

            fecha = fecha.AddMinutes(dsc.CONFIGURACION[0].hora1);
            horas[0] = (UInt16)((fecha.Minute << 8) + fecha.Hour);

            for (byte i = 0; i < 3; i++)
            {
                buffer[0] = (byte)(i + 1);
                buffer[1] = (byte)(horas[i] >> 8);
                buffer[2] = (byte)(horas[i] & 0xff);
                if (horas24[i])
                {
                    if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_HORA24, buffer, 3, null, 0, out ret, IntPtr.Zero))
                    {
                        driver.Close();
                        MessageBox.Show("Error de acceso al dispositivo", "[X52-Service][1.7]", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
                else
                {
                    if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_HORA, buffer, 3, null, 0, out ret, IntPtr.Zero))
                    {
                        driver.Close();
                        MessageBox.Show("Error de acceso al dispositivo", "[X52-Service][1.8]", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
            }

            buffer[0] = (dsc.CONFIGURACION[0].Pedales) ? (byte)1 : (byte)0;
            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_PEDALES, buffer, 1, null, 0, out ret, IntPtr.Zero))
            {
                driver.Close();
                MessageBox.Show("Error de acceso al dispositivo", "[X52-Service][1.9]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            driver.Close();

            return true;
        }

        private bool SetTextoInicio()
        {
            String[] filas = new String[] { "  Saitek X-52",
                                            "  Driver v7.0" };

            Microsoft.Win32.SafeHandles.SafeFileHandle driver = CSystem32.CreateFile(
                            "\\\\.\\XUSBInterface",
                            0x80000000 | 0x40000000,//GENERIC_WRITE | GENERIC_READ,
                            0x00000002 | 0x00000001, //FILE_SHARE_WRITE | FILE_SHARE_READ,
                            IntPtr.Zero,
                            3,//OPEN_EXISTING,
                            0,
                            IntPtr.Zero);
            if (driver.IsInvalid)
            {
                MessageBox.Show("No se puede abrir el driver", "[X52-Service][2.1]", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            UInt32 ret = 0;
            for (byte i = 0; i < 2; i++)
            {
                String fila = filas[i];
                byte[] texto = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.ASCII, System.Text.Encoding.Unicode.GetBytes(fila));
                byte[] buffer = new byte[18];
                for (byte c = 1; c < 18; c++)
                {
                    if (texto.Length >= c)
                        buffer[c] = texto[c - 1];
                    else
                        buffer[c] = 0;
                }

                buffer[0] = (byte)(i + 1);

                ret = 0;
                if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, buffer, (uint)texto.Length + 1, null, 0, out ret, IntPtr.Zero))
                {
                    driver.Close();
                    return false;
                }
            }
            //siguientes lineas en blanco
            byte[] fila3 = new byte[2] { 3, 0 };
            if (!CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, fila3, 2, null, 0, out ret, IntPtr.Zero))
            {
                driver.Close();
                return false;
            }

            driver.Close();

            return true;
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            if (timerMenu) //cada dos salto comprueba (4 segundos)
            {
                mfd.ComprobarEstado();
                timerMenu = false;
            }
            else
                timerMenu = true;

            if (!this.fechaActiva && !this.horaActiva)
                return;

            UInt32 ret = 0;
            Microsoft.Win32.SafeHandles.SafeFileHandle driver = CSystem32.CreateFile(
                            "\\\\.\\XUSBInterface",
                            0x80000000 | 0x40000000,//GENERIC_WRITE | GENERIC_READ,
                            0x00000002 | 0x00000001, //FILE_SHARE_WRITE | FILE_SHARE_READ,
                            IntPtr.Zero,
                            3,//OPEN_EXISTING,
                            0,
                            IntPtr.Zero);
            if (driver.IsInvalid)
                return;

            byte[] bf = new byte[3];
            DateTime t = DateTime.Now;

            if (this.fechaActiva)
            {
                bf[0] = 1; bf[1] = (byte)t.Day;
                CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_FECHA, bf, 2, null, 0, out ret, IntPtr.Zero);
                bf[0] = 2; bf[1] = (byte)t.Month;
                CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_FECHA, bf, 2, null, 0, out ret, IntPtr.Zero);
                bf[0] = 3; bf[1] = (byte)(t.Year % 100);
                CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_FECHA, bf, 2, null, 0, out ret, IntPtr.Zero);
            }

            if (this.horaActiva)
            {
                t = t.AddMinutes(dsc.CONFIGURACION[0].hora1);
                UInt16 auxHora = (UInt16)((t.Minute << 8) + t.Hour);
                bf[0] = 1;
                bf[1] = (byte)(auxHora >> 8);
                bf[2] = (byte)(auxHora & 0xff);
                if (dsc.CONFIGURACION[0].hora1_24h)
                    CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_HORA24, bf, 3, null, 0, out ret, IntPtr.Zero);
                else
                    CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_HORA, bf, 3, null, 0, out ret, IntPtr.Zero);
            }

            driver.Close();
        }

        private void CargarPerfil(String archivo)
        {
            bool horaModificada = false;
            bool fechaModificada = false;
            CPerfil.CargarMapa(archivo, ref horaModificada, ref fechaModificada);
            horaActiva = !horaModificada;
            fechaActiva = !fechaModificada;
        }

        private void ReiniciarX52()
        {
            CargarConfiguracion();

            String[] filas = new String[] { "  Saitek X-52",
                                            "  Driver v7.0" };

            Microsoft.Win32.SafeHandles.SafeFileHandle driver = CSystem32.CreateFile(
                            "\\\\.\\XUSBInterface",
                            0x80000000 | 0x40000000,//GENERIC_WRITE | GENERIC_READ,
                            0x00000002 | 0x00000001, //FILE_SHARE_WRITE | FILE_SHARE_READ,
                            IntPtr.Zero,
                            3,//OPEN_EXISTING,
                            0,
                            IntPtr.Zero);
            if (driver.IsInvalid)
                return;

            UInt32 ret = 0;
            byte[] fila = new byte[2] { 1, 0 };
            CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, fila, 2, null, 0, out ret, IntPtr.Zero);
            fila[0] = 2;
            CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, fila, 2, null, 0, out ret, IntPtr.Zero);
            fila[0] = 3;
            CSystem32.DeviceIoControl(driver, CSystem32.IOCTL_TEXTO, fila, 2, null, 0, out ret, IntPtr.Zero);

            driver.Close();
        }
    }
}
