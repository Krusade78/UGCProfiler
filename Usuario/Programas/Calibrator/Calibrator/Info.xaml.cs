using System;
using System.Windows;
using System.Windows.Controls;

namespace Calibrator
{
    /// <summary>
    /// Lógica de interacción para Info.xaml
    /// </summary>
    public partial class Info : UserControl
    {

        IntPtr hJoy;
        CDirectInput di;
        System.Windows.Threading.DispatcherTimer timer;

        public Info()
        {
            InitializeComponent();
            timer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Input);
            timer.Interval = new System.TimeSpan(50);
            timer.Tick += Timer_Tick;
        }

        public void Iniciar(IntPtr hJoy)
        {
            //this.hJoy = hJoy;
            //di = new CDirectInput();
            //if (!di.AbrirDirectInput(Application.Current.MainWindow, nombreJoy))
            //    di = null;
            //System.Windows.Interop. HwndSource source = (System.Windows.Interop.HwndSource)PresentationSource.FromVisual(this);
            //source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x00FF)
            {
            }

            return IntPtr.Zero;
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            int cbsize = 0;
            //int ret = CRawInput.GetRawInputData(lParam, CRawInput.RawInputCommand.Input, IntPtr.Zero, ref cbsize, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
            int ret = CRawInput.GetRawInputBuffer(IntPtr.Zero, ref cbsize, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
            if (ret == 0 && cbsize != 0)
            {
                cbsize *= 8;
                IntPtr buff = System.Runtime.InteropServices.Marshal.AllocHGlobal(cbsize);
                //ret = CRawInput.GetRawInputData(lParam, CRawInput.RawInputCommand.Input, buff, ref cbsize, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
                ret = CRawInput.GetRawInputBuffer(buff, ref cbsize, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
                if (ret > 0)
                {
                    CRawInput.RawInput ri = System.Runtime.InteropServices.Marshal.PtrToStructure<CRawInput.RawInput>(buff);
                    int leidos = 0;
                    while (true)
                    {
                        if (ri.Header.Type == CRawInput.RawInputType.HID)
                        {
                            IntPtr buffdi = System.Runtime.InteropServices.Marshal.AllocHGlobal(256);
                            uint cbsizedi = 128;
                            uint uret = CRawInput.GetRawInputDeviceInfo(ri.Header.Device, 0x20000007, buffdi, ref cbsizedi);
                            String nombre = System.Runtime.InteropServices.Marshal.PtrToStringAuto(buffdi);
                            System.Runtime.InteropServices.Marshal.FreeHGlobal(buffdi);
                            if (nombre.StartsWith("\\\\?\\HID#NullVirtualHidDevice"))
                            {
                                byte[] data = new byte[ri.HID.Count * ri.HID.Size];
                                System.Runtime.InteropServices.Marshal.Copy((IntPtr)(buff.ToInt64() +
                                    System.Runtime.InteropServices.Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)) +
                                    System.Runtime.InteropServices.Marshal.SizeOf(typeof(CRawInput.RawHID))),
                                    data, 0, data.Length);
                                byte[] hidData = new byte[data.Length];
                                //for (int i = hidData.Length - 1; i >= 0; i--)
                                //    hidData[hidData.Length - 1 - i] = data[i];
                                ActualizarEstado(hidData);
                            }
                        }
                        leidos += ri.Header.Size + 1;
                        ret--;
                        if (ret > 0)
                            ri = System.Runtime.InteropServices.Marshal.PtrToStructure<CRawInput.RawInput>(buff + leidos);
                        else
                            break;
                    }
                }
                System.Runtime.InteropServices.Marshal.FreeHGlobal(buff);
            }
            if (di != null)
            {
                //Microsoft.DirectX.DirectInput.JoystickState st = new Microsoft.DirectX.DirectInput.JoystickState();
                //di.GetEstado(ref st);
                //ActualizarEstado(st);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void ActualizarEstado(byte[] hidData)
        {
            //int x = (hidData[0] << 24) | (hidData[1] << 16) | (hidData[2] << 8) | hidData[3];
            //int y = (hidData[4] << 24) | (hidData[5] << 16) | (hidData[6] << 8) | hidData[7];
            //int z = (hidData[8] << 24)| (hidData[9] << 16) | (hidData[10] << 8) | hidData[11];
            //int r = (hidData[12] << 24) | (hidData[13] << 16) | (hidData[14] << 8) | hidData[15];
            ////ejeXY.Margin = new Thickness(x, y, 0, 0);
            //Labelxy.Content = "X: " + r + " # Y: " + y;
            ////ejeZ.Height = z;
            //Labelz.Content = "Z: " + z;
            ////ejeR.Margin = new Thickness(r, 0, 0, 0);
            //Labelr.Content = "R: " + r;
            textBlock.Text = "";
            for (int i = 0; i < hidData.Length; i++)
                textBlock.Text += hidData[i].ToString() + ",";
        }

        #region "Saltar perfil"
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(string lpFileName, System.UInt32 dwDesiredAccess, System.UInt32 dwShareMode, System.IntPtr pSecurityAttributes, System.UInt32 dwCreationDisposition, System.UInt32 dwFlagsAndAttributes, System.IntPtr hTemplateFile);
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern bool DeviceIoControl(Microsoft.Win32.SafeHandles.SafeFileHandle handle, System.UInt32 dwIoControlCode, byte[] lpInBuffer, System.UInt32 nInBufferSize, byte[] lpOutBuffer, System.UInt32 nOutBufferSize, ref System.UInt32 lpBytesReturned, System.IntPtr lpOverlapped);
        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            System.UInt32 ret = 0;
            Microsoft.Win32.SafeHandles.SafeFileHandle driver = CreateFile(
                    "\\\\.\\XHOTASHidInterface",
                    0x80000000,//GENERIC_READ,
                    0x00000001, //FILE_SHARE_READ,
                    (System.IntPtr)0,
                    3,//OPEN_EXISTING,
                    0,
                    (System.IntPtr)0);
            if (driver.IsInvalid) return;
            System.UInt32 IOCTL_USR_DESCALIBRAR = ((0x22) << 16) | ((1) << 14) | ((0x0102) << 2) | (0);
            if (!DeviceIoControl(driver, IOCTL_USR_DESCALIBRAR, null, 0, null, 0, ref ret, System.IntPtr.Zero))
            {
                driver.Close();
                return;
            }

            driver.Close();
        }
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            System.UInt32 ret = 0;
            Microsoft.Win32.SafeHandles.SafeFileHandle driver = CreateFile(
                    "\\\\.\\XHOTASHidInterface",
                    0x80000000,//GENERIC_READ,
                    0x00000001, //FILE_SHARE_READ,
                    (System.IntPtr)0,
                    3,//OPEN_EXISTING,
                    0,
                    (System.IntPtr)0);
            if (driver.IsInvalid) return;
            System.UInt32 IOCTL_USR_CALIBRAR = ((0x22) << 16) | ((1) << 14) | ((0x0101) << 2) | (0);
            if (!DeviceIoControl(driver, IOCTL_USR_CALIBRAR, null, 0, null, 0, ref ret, System.IntPtr.Zero))
            {
                driver.Close();
                return;
            }

            driver.Close();
        }
        #endregion
    }
}
