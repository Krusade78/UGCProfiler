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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("");
            uint n = 0;
            IntPtr hJoy = IntPtr.Zero;
            CRawInput.GetRawInputDeviceList(IntPtr.Zero, ref n, (uint)Marshal.SizeOf(typeof(CRawInput.RawInputDeviceList)));
            if (n > 0)
            {
                IntPtr lid = Marshal.AllocHGlobal((int)((uint)Marshal.SizeOf(typeof(CRawInput.RawInputDeviceList)) * n));
                if ((int)CRawInput.GetRawInputDeviceList(lid, ref n, (uint)Marshal.SizeOf(typeof(CRawInput.RawInputDeviceList))) != -1)
                {
                    for (uint i = 0; i < n; i++)
                    {
                        CRawInput.RawInputDeviceList l = (CRawInput.RawInputDeviceList)Marshal.PtrToStructure(IntPtr.Add(lid, (int)(Marshal.SizeOf(typeof(CRawInput.RawInputDeviceList)) * i)), typeof(CRawInput.RawInputDeviceList));
                        if (l.DeviceType == CRawInput.RawInputDeviceType.HumanInterfaceDevice)
                        {
                            IntPtr buff = Marshal.AllocHGlobal(256);
                            uint cbsize = 128;

                            uint ret = CRawInput.GetRawInputDeviceInfo(l.DeviceHandle, CRawInput.RawInputDeviceInfoCommand.DeviceName, buff, ref cbsize);
                            String nombre = Marshal.PtrToStringAnsi(buff);
                            Marshal.FreeHGlobal(buff);
                            if (nombre.StartsWith("\\\\?\\HID#VID_06A3&PID_0255"))
                            {
                                hJoy = l.DeviceHandle;
                                break;
                            }
                        }
                        //cbsize = (uint)Marshal.SizeOf(typeof(CRawInput.DeviceInfo));
                        //buff = Marshal.AllocHGlobal((int)cbsize);
                        //CRawInput.GetRawInputDeviceInfo(l.DeviceHandle, 0x2000000b, buff, ref cbsize);
                        //CRawInput.DeviceInfo di = Marshal.PtrToStructure<CRawInput.DeviceInfo>(buff);
                        //Marshal.FreeHGlobal(buff);
                    }
                }
                Marshal.FreeHGlobal(lid);
            }

            ucInfo.Iniciar(hJoy);
            //ucCalibrar.Iniciar(hJoy);
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
