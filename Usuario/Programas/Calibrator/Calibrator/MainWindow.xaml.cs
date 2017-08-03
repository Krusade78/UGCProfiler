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
        private System.Windows.Interop.HwndSource hWnd = null;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("");

            hWnd = (System.Windows.Interop.HwndSource)System.Windows.Interop.HwndSource.FromVisual(this);

            CRawInput.RAWINPUTDEVICE[] rdev = new CRawInput.RAWINPUTDEVICE[1];
            rdev[0].UsagePage = 0x01;
            rdev[0].Usage = 0x04;
            rdev[0].WindowHandle = hWnd.Handle;
            rdev[0].Flags = CRawInput.RawInputDeviceFlags.None;

            if (!CRawInput.RegisterRawInputDevices(rdev, 1, (uint)Marshal.SizeOf(typeof(CRawInput.RAWINPUTDEVICE))))
            {
                MessageBox.Show("No se pudo registrar la entrada de datos HID", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                hWnd = null;
                //this.Close();
            }
            else
                hWnd.AddHook(WndProc);
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

                            ucInfo.ActualizarEstado(hidData);
                            ucCalibrar.ActualizarEstado(hidData);
                        }
                    }
                }

            }
            return IntPtr.Zero;
        }

        private void toggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                tbCalibrar.IsChecked = false;
            }
        }

        private void toggleButton1_Checked(object sender, RoutedEventArgs e)
        {
            tbPrueba.IsChecked = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (hWnd != null)
            {
                hWnd.RemoveHook(WndProc);
                hWnd = null;
            }
        }
    }
}
