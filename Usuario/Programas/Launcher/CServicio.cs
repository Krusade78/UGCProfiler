using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Launcher
{
    class CServicio
    {
        private struct STLIMITES
        {
            internal byte cal;
            internal ushort i;
            internal ushort c;
            internal ushort d;
            internal byte n;
        };
        private struct STJITTER
        {
            internal byte antiv;
            internal long PosElegida;
            internal byte PosRepetida;
            internal byte Margen;
            internal byte Resistencia;
        };

        public void IniciarServicio()
        {
            CargarCalibrado();
            //CargarConfiguracion();
            //SetTextoInicio();
        }

        private void CargarCalibrado()
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
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
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
                MessageBox.Show("No se puede abrir el driver", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UInt32 ret = 0;
            UInt32 IOCTL_USR_CALIBRADO = ((0x22) << 16) | ((2) << 14) | ((0x0809) << 2) | (0);
            if (!CSystem32.DeviceIoControl(driver, IOCTL_USR_CALIBRADO, buf, (uint)buf.Length, null, 0, out ret, IntPtr.Zero))
            {
                driver.Close();
                MessageBox.Show("No se puede enviar la orden al driver", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            driver.Close();
        }

        private void CargarConfiguracion()
        {
            HKEY key;
            DWORD tipo, tam;
            LONG res;

            this->luzMFD = 1;
            this->luces = 1;
            this->hora24[0] = false;
            this->hora[0] = 0;
            this->hora24[1] = false;
            this->hora[1] = 0;
            this->hora24[2] = false;
            this->hora[2] = 0;

            if (ERROR_SUCCESS != RegOpenKeyEx(HKEY_CURRENT_USER, L"SOFTWARE\\XHOTAS\\Calibrado", 0, KEY_READ, &key))
                return;

            tam = 1; res = RegQueryValueEx(key, L"LuzMFD", 0, &tipo, (BYTE*)&this->luzMFD, &tam);
            if (ERROR_SUCCESS != res || tam != 1) this->luzMFD = 1;

            tam = 1; res = RegQueryValueEx(key, L"Luz", 0, &tipo, (BYTE*)&this->luces, &tam);
            if (ERROR_SUCCESS != res || tam != 1) this->luces = 1;

            BYTE bf[3];
            tam = 3; res = RegQueryValueEx(key, L"Hora1", 0, &tipo, bf, &tam);
            if (ERROR_SUCCESS != res || tam != 3)
            {
                this->hora24[0] = false;
                this->hora[0] = 0;
            }
            else
            {
                this->hora24[0] = bf[0] ? true : false;
                this->hora[0] = *((WORD*)&bf[1]);
            }
            tam = 3; res = RegQueryValueEx(key, L"Hora2", 0, &tipo, bf, &tam);
            if (ERROR_SUCCESS != res || tam != 3)
            {
                this->hora24[1] = false;
                this->hora[1] = 0;
            }
            else
            {
                this->hora24[1] = bf[0] ? true : false;
                this->hora[1] = *((WORD*)&bf[1]);
            }
            tam = 3; res = RegQueryValueEx(key, L"Hora3", 0, &tipo, bf, &tam);
            if (ERROR_SUCCESS != res || tam != 3)
            {
                this->hora24[2] = false;
                this->hora[2] = 0;
            }
            else
            {
                this->hora24[2] = bf[0] ? true : false;
                this->hora[2] = *((WORD*)&bf[1]);
            }

            RegCloseKey(key);

            DWORD ret;
            HANDLE driver = CreateFile(
                    L"\\\\.\\XUSBInterface",
                    GENERIC_WRITE,
                    FILE_SHARE_WRITE,
                    NULL,
                    OPEN_EXISTING,
                    0,
                    NULL);
            if (driver == INVALID_HANDLE_VALUE)
            {
                MessageBox(NULL, L"Error opening device", L"[X52-Service][1.1]", MB_ICONWARNING);
                return;
            }
            if (!DeviceIoControl(driver, IOCTL_MFD_LUZ, &this->luzMFD, 1, NULL, 0, &ret, NULL))
                MessageBox(NULL, L"Error accesing device", L"[X52-Service][1.2]", MB_ICONWARNING);
            if (!DeviceIoControl(driver, IOCTL_GLOBAL_LUZ, &this->luces, 1, NULL, 0, &ret, NULL))
                MessageBox(NULL, L"Error accesing device", L"[X52-Service][1.3]", MB_ICONWARNING);
            SYSTEMTIME t;
            GetLocalTime(&t);

            bf[0] = 1; bf[1] = (BYTE)t.wDay;
            if (!DeviceIoControl(driver, IOCTL_FECHA, bf, 2, NULL, 0, &ret, NULL))
                MessageBox(NULL, L"Error accesing device", L"[X52-Service][1.4]", MB_ICONWARNING);
            bf[0] = 2; bf[1] = (BYTE)t.wMonth;
            if (!DeviceIoControl(driver, IOCTL_FECHA, bf, 2, NULL, 0, &ret, NULL))
                MessageBox(NULL, L"Error accesing device", L"[X52-Service][1.5]", MB_ICONWARNING);
            bf[0] = 3; bf[1] = t.wYear % 100;
            if (!DeviceIoControl(driver, IOCTL_FECHA, bf, 2, NULL, 0, &ret, NULL))
                MessageBox(NULL, L"Error accesing device", L"[X52-Service][1.6]", MB_ICONWARNING);

            FILETIME ft;
            ULARGE_INTEGER uft;
            SystemTimeToFileTime(&t, &ft);
            uft.LowPart = ft.dwLowDateTime; uft.HighPart = ft.dwHighDateTime;
            uft.QuadPart += (__int64)((__int16)this->hora[0]) * 600000000;
            ft.dwLowDateTime = uft.LowPart; ft.dwHighDateTime = uft.HighPart;
            FileTimeToSystemTime(&ft, &t);

            WORD auxHora = this->hora[0];
            this->hora[0] = (t.wMinute << 8) + t.wHour;
            for (char i = 0; i < 3; i++)
            {
                bf[0] = i + 1;
                *((WORD*)&bf[1]) = this->hora[i];
                if (this->hora24[i])
                {
                    if (!DeviceIoControl(driver, IOCTL_HORA24, bf, 3, NULL, 0, &ret, NULL))
                        MessageBox(NULL, L"Error accesing device", L"[X52-Service][1.7]", MB_ICONWARNING);
                }
                else
                {
                    if (!DeviceIoControl(driver, IOCTL_HORA, bf, 3, NULL, 0, &ret, NULL))
                        MessageBox(NULL, L"Error accesing device", L"[X52-Service][1.8]", MB_ICONWARNING);
                }
            }
            this->hora[0] = auxHora;

            CloseHandle(driver);
        }

        private void SetTextoInicio()
        {
            UInt32 IOCTL_TEXTO = ((0x22) << 16) | ((2) << 14) | ((0x0804) << 2) | (0);
            String[] filas = new String[] { "  Saitek X52",
                                            "  Driver v.7" };

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
                MessageBox.Show("No se puede abrir el driver", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
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
                CSystem32.DeviceIoControl(driver, IOCTL_TEXTO, buffer, (uint)texto.Length + 1, null, 0, out ret, IntPtr.Zero);
            }
            //siguientes lineas en blanco
            byte[] fila3 = new byte[2] { 3, 0 };
            CSystem32.DeviceIoControl(driver, IOCTL_TEXTO, fila3, 2, null, 0, out ret, IntPtr.Zero);

            driver.Close();
        }
    }
}
