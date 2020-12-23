using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;

namespace Comunes
{
    public class CIoCtl
    {
        //public const UInt32 IOCTL_MFD_LUZ = ((0x22) << 16) | ((2) << 14) | ((0x0800) << 2) | (0);
        //public const UInt32 IOCTL_GLOBAL_LUZ = ((0x22) << 16) | ((2) << 14) | ((0x0801) << 2) | (0);
        //public const UInt32 IOCTL_INFO_LUZ =        ((0x22) << 16) | ((2) << 14) | ((0x0802) << 2) | (0);
        //public const UInt32 IOCTL_PEDALES =         ((0x22) << 16) | ((2) << 14) | ((0x0803) << 2) | (0);
        public const UInt32 IOCTL_TEXTO =           ((0x22) << 16) | ((2) << 14) | ((0x0804) << 2) | (0);
        public const UInt32 IOCTL_HORA =            ((0x22) << 16) | ((2) << 14) | ((0x0805) << 2) | (0);
        public const UInt32 IOCTL_HORA24 =          ((0x22) << 16) | ((2) << 14) | ((0x0806) << 2) | (0);
        public const UInt32 IOCTL_FECHA =           ((0x22) << 16) | ((2) << 14) | ((0x0807) << 2) | (0);
        public const UInt32 IOCTL_USR_RAW =         ((0x22) << 16) | ((2) << 14) | ((0x0808) << 2) | (0);
        public const UInt32 IOCTL_USR_CALIBRADO =   ((0x22) << 16) | ((2) << 14) | ((0x0809) << 2) | (0);
        public const UInt32 IOCTL_USR_ANTIVIBRACION = ((0x22) << 16) | ((2) << 14) | ((0x080e) << 2) | (0);
        public const UInt32 IOCTL_USR_MAPA =        ((0x22) << 16) | ((2) << 14) | ((0x080a) << 2) | (0);
        public const UInt32 IOCTL_USR_COMANDOS =    ((0x22) << 16) | ((2) << 14) | ((0x080b) << 2) | (0);
        //public const UInt32 IOCTL_GET_MENU =        ((0x22) << 16) | ((1) << 14) | ((0x080c) << 2) | (0);
        //public const UInt32 IOCTL_DESACTIVAR_MENU = ((0x22) << 16) | ((2) << 14) | ((0x080d) << 2) | (0);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFile(string lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr pSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(SafeFileHandle handle, UInt32 dwIoControlCode, byte[] lpInBuffer, UInt32 nInBufferSize, byte[] lpOutBuffer, UInt32 nOutBufferSize, out UInt32 lpBytesReturned, IntPtr lpOverlapped);

        private static SafeFileHandle driver = null;
        private static int driverRefs = 0;
        private static readonly SemaphoreSlim driverMutex = new SemaphoreSlim(1, 1);
        public static bool AbrirDriver()
        {
            driverMutex.Wait();
            driverRefs++;
            if (driverRefs == 1)
            {
                driver = CreateFile(
                        "\\\\.\\XHOTASControl",
                        0x80000000 | 0x40000000,//GENERIC_WRITE | GENERIC_READ,
                        0x00000002 | 0x00000001, //FILE_SHARE_WRITE | FILE_SHARE_READ,
                        IntPtr.Zero,
                        3,//OPEN_EXISTING,
                        0,
                        IntPtr.Zero);
                if (driver.IsInvalid)
                {
                    driver = null;
                    driverRefs--;
                    System.Windows.MessageBox.Show("No se puede abrir el driver", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    driverMutex.Release();
                    return false;
                }
            }

            driverMutex.Release();
            return true;
        }

        public static void CerrarDriver()
        {
            driverMutex.Wait();
            driverRefs--;
            if (driverRefs == 0)
            {
                driver.Close();
                driver = null;
            }
            driverMutex.Release();
        }

        public static bool DeviceIoControl(UInt32 dwIoControlCode, byte[] lpInBuffer, UInt32 nInBufferSize, byte[] lpOutBuffer, UInt32 nOutBufferSize, out UInt32 lpBytesReturned, IntPtr lpOverlapped)
        {
            bool ok = false;
            driverMutex.Wait();
            {
                if (driver != null)
                    ok = DeviceIoControl(driver, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize, out lpBytesReturned, lpOverlapped);
                else
                    lpBytesReturned = 0;
            }
            driverMutex.Release();
            return ok;
        }
    }
}
