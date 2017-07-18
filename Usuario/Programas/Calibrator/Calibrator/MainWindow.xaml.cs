using System;
using System.Runtime.InteropServices;
using System.Windows;


namespace Calibrator
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte tipo = 255;
        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Background);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //uint n = 0;
            IntPtr hJoy = IntPtr.Zero;
            //CRawInput.GetRawInputDeviceList(null, ref n, (uint)Marshal.SizeOf(typeof(CRawInput.RAWINPUTDEVICELIST)));
            //if (n > 0)
            //{
            //    CRawInput.RAWINPUTDEVICELIST[] lid = new CRawInput.RAWINPUTDEVICELIST[n];
            //    CRawInput.GetRawInputDeviceList(lid, ref n, (uint)Marshal.SizeOf(typeof(CRawInput.RAWINPUTDEVICELIST)));
            //    foreach (CRawInput.RAWINPUTDEVICELIST l in lid)
            //    {
            //        IntPtr buff = Marshal.AllocHGlobal(256);
            //        uint cbsize = 128;
            //        uint ret = CRawInput.GetRawInputDeviceInfo(l.hDevice, 0x20000007, buff, ref cbsize);
            //        String nombre = Marshal.PtrToStringAuto(buff);
            //        Marshal.FreeHGlobal(buff);
            //        if (nombre.StartsWith("\\\\?\\HID#VID_06A3&PID_0255#"))
            //        {
            //            hJoy = l.hDevice;
            //            break;
            //        }
            //cbsize = (uint)Marshal.SizeOf(typeof(CRawInput.DeviceInfo));
            //buff = Marshal.AllocHGlobal((int)cbsize);
            //CRawInput.GetRawInputDeviceInfo(l.hDevice, 0x2000000b, buff, ref cbsize);
            //CRawInput.DeviceInfo di = Marshal.PtrToStructure<CRawInput.DeviceInfo>(buff);
            //Marshal.FreeHGlobal(buff);
            //    }
            //}

            CRawInput.RAWINPUTDEVICE[] rid = new CRawInput.RAWINPUTDEVICE[1];

            rid[0].UsagePage = 0x01;
            rid[0].Usage = 0x04;
            rid[0].Flags = 0;
            rid[0].WindowHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            CRawInput.RegisterRawInputDevices(rid, 1, Marshal.SizeOf(rid[0]));
            ucInfo.Iniciar(hJoy);           
        }

        private void toggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                //ev = true;
                //tbCalibrar.IsChecked = false;
                //ev = false;
            }
        }

        private void toggleButton1_Checked(object sender, RoutedEventArgs e)
        {
                //ev = true;
                //tbPrueba.IsChecked = false;
                //ev = false;
        }
    }
}
