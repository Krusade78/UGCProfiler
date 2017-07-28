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

            
            for(int i = 0; i < 32; i++)
            {

            }

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
                    "\\\\.\\XUSBInterface",
                    0x40000000,//GENERIC_WRITE,
                    0x00000002, //FILE_SHARE_WRITE,
                    (System.IntPtr)0,
                    3,//OPEN_EXISTING,
                    0,
                    (System.IntPtr)0);
            if (driver.IsInvalid) return;
            System.UInt32 IOCTL_USR_RAW = ((0x22) << 16) | ((2) << 14) | ((0x0108) << 2) | (0);
            byte[] buff = new byte[] { 1 };
            if (!DeviceIoControl(driver, IOCTL_USR_RAW, buff, 1, null, 0, ref ret, System.IntPtr.Zero))
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
                    "\\\\.\\XUSBInterface",
                    0x40000000,//GENERIC_WRITE,
                    0x00000002, //FILE_SHARE_WRITE,
                    (System.IntPtr)0,
                    3,//OPEN_EXISTING,
                    0,
                    (System.IntPtr)0);
            if (driver.IsInvalid) return;
            System.UInt32 IOCTL_USR_RAW = ((0x22) << 16) | ((2) << 14) | ((0x0108) << 2) | (0);
            byte[] buff = new byte[] { 0 };
            if (!DeviceIoControl(driver, IOCTL_USR_RAW, buff, 1, null, 0, ref ret, System.IntPtr.Zero))
            {
                driver.Close();
                return;
            }

            driver.Close();
        }
        #endregion
    }
}
