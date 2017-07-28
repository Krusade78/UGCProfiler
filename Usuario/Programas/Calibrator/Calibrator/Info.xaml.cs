using System;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace Calibrator
{
    /// <summary>
    /// Lógica de interacción para Info.xaml
    /// </summary>
    public partial class Info : UserControl
    {
        IntPtr hJoy;
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
            this.hJoy = hJoy;
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            if (hJoy == IntPtr.Zero)
                return;

            int cbsize = 0;
            int ret = CRawInput.GetRawInputData(hJoy, CRawInput.RawInputCommand.Input, IntPtr.Zero, ref cbsize, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
            int err = Marshal.GetLastWin32Error();
            if (ret == 0 && cbsize != 0)
            {
                cbsize *= 8;
                IntPtr buff = Marshal.AllocHGlobal(cbsize);
                ret = CRawInput.GetRawInputData(hJoy, CRawInput.RawInputCommand.Input, buff, ref cbsize, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
                if (ret > 0)
                {
                    CRawInput.RawInput ri = Marshal.PtrToStructure<CRawInput.RawInput>(buff);
                    int leidos = 0;
                    while (true)
                    {
                        if (ri.Header.Type == CRawInput.RawInputType.HID)
                        {
                            byte[] data = new byte[ri.HID.Count * ri.HID.Size];
                            Marshal.Copy((IntPtr)(buff.ToInt64() + Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)) + Marshal.SizeOf(typeof(CRawInput.RawHID))), data, 0, data.Length);
                            byte[] hidData = new byte[data.Length];
                            ActualizarEstado(hidData);
                        }
                        leidos += ri.Header.Size + 1;
                        ret--;
                        if (ret > 0)
                            ri = Marshal.PtrToStructure<CRawInput.RawInput>(buff + leidos);
                        else
                            break;
                    }
                }
                Marshal.FreeHGlobal(buff);
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
            int x = (hidData[0] << 8) | hidData[1];
            int y = (hidData[2] << 8) | hidData[3];
            Labelxy.Text =  x + " # " + y;
            ejeXY.Margin = new Thickness(x, y, 0, 0);

            int r = (hidData[4] << 8) | hidData[5];
            Labelr.Text = r.ToString();
            ejeR.Height = r;

            int z = (hidData[6] << 8) | hidData[7];
            Labelz.Text = z.ToString();
            ejeZ.Height = z;

            int rx = (hidData[8] << 8) | hidData[9];
            Labelrx.Text = rx.ToString();
            double angulo = (Math.PI * ((360 / 2048) * rx)) / 180;
            ejeRX.Data = System.Windows.Media.Geometry.Parse("M25,0 A25,25 0 " + ((angulo > Math.PI) ? "0": "1") + "0 " + (Math.Cos(angulo)*25) + "," + (Math.Sin(angulo) * 25) + " L25,25");

            int ry = (hidData[10] << 8) | hidData[11];
            angulo = (Math.PI * ((360 / 2048) * ry)) / 180;
            Labelry.Text = ry.ToString();
            ejeRY.Data = System.Windows.Media.Geometry.Parse("M25,0 A25,25 0 " + ((angulo > Math.PI) ? "0" : "1") + "0 " + (Math.Cos(angulo) * 25) + "," + (Math.Sin(angulo) * 25) + " L25,25");

            int sl1 = (hidData[12] << 8) | hidData[13];
            Labelsl1.Text = sl1.ToString();
            ejeSl1.Width = sl1;

            int sl2 = (hidData[14] >> 8) | hidData[15];
            Labelsl2.Text = sl2.ToString();
            ejeSl2.Width = sl2;

            int mx = hidData[24] >> 4;
            int my = hidData[24] & 4;
            Labelxy.Text = "mX: " + mx + "\n" + "mY: " + my;
            ejeMXY.Margin = new Thickness(mx, my, 0, 0);

            System.Windows.Shapes.Ellipse[] bts = new System.Windows.Shapes.Ellipse[] { bt1, bt2, bt3, bt4, bt5, bt6, bt7, bt8,
                                                                                        bt9, bt10, bt11, bt12, bt13, bt14, bt15, bt16,
                                                                                        bt17, bt18, bt19, bt20, bt21, bt22, bt23, bt24,
                                                                                        bt25, bt26, bt27, bt28, bt29, bt30, bt31, bt32};
            for(int i = 0; i < 32; i++)
                bts[i].Visibility = ((hidData[20 + (i / 8)] & (1 << (i % 8))) != 0) ? Visibility.Visible : Visibility.Hidden;

            pathP1.Visibility = (hidData[16] == 0) ? Visibility.Hidden : Visibility.Visible;
            pathP1.RenderTransform.Value.Rotate((hidData[16] > 5) ? (hidData[16] - 9) * 45 : (hidData[16] - 1) * 45);
            pathP2.Visibility = (hidData[17] == 0) ? Visibility.Hidden : Visibility.Visible;
            pathP2.RenderTransform.Value.Rotate((hidData[16] > 5) ? (hidData[16] - 9) * 45 : (hidData[16] - 1) * 45);
            pathP3.Visibility = (hidData[18] == 0) ? Visibility.Hidden : Visibility.Visible;
            pathP3.RenderTransform.Value.Rotate((hidData[16] > 5) ? (hidData[16] - 9) * 45 : (hidData[16] - 1) * 45);
            pathP4.Visibility = (hidData[19] == 0) ? Visibility.Hidden : Visibility.Visible;
            pathP4.RenderTransform.Value.Rotate((hidData[16] > 5) ? (hidData[16] - 9) * 45 : (hidData[16] - 1) * 45);
        }

        #region "Saltar perfil"
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(string lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr pSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(Microsoft.Win32.SafeHandles.SafeFileHandle handle, UInt32 dwIoControlCode, byte[] lpInBuffer, UInt32 nInBufferSize, byte[] lpOutBuffer, UInt32 nOutBufferSize, out UInt32 lpBytesReturned, IntPtr lpOverlapped);
        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SafeHandles.SafeFileHandle driver = CreateFile(
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
            UInt32 IOCTL_USR_RAW = ((0x22) << 16) | ((2) << 14) | ((0x0808) << 2) | (0);
            byte[] buff = new byte[] { 1 };
            if (!DeviceIoControl(driver, IOCTL_USR_RAW, buff, 1, null, 0, out ret, IntPtr.Zero))
            {
                int err = Marshal.GetLastWin32Error();
                driver.Close();
                return;
            }

            driver.Close();
        }
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SafeHandles.SafeFileHandle driver = CreateFile(
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
            UInt32 IOCTL_USR_RAW = ((0x22) << 16) | ((2) << 14) | ((0x0808) << 2) | (0);
            byte[] buff = new byte[] { 0 };
            if (!DeviceIoControl(driver, IOCTL_USR_RAW, buff, 1, null, 0, out ret, System.IntPtr.Zero))
            {
                int err = Marshal.GetLastWin32Error();
                driver.Close();
                return;
            }

            driver.Close();
        }
        #endregion
    }
}
