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
        private bool modoRaw = false;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hWnd = (System.Windows.Interop.HwndSource)PresentationSource.FromVisual(this);

            CRawInput.RAWINPUTDEVICE[] rdev = new CRawInput.RAWINPUTDEVICE[3];
            rdev[0].UsagePage = 0x01;
            rdev[0].Usage = 0x04;
            rdev[0].WindowHandle = hWnd.Handle;
            rdev[0].Flags = CRawInput.RawInputDeviceFlags.None;
            rdev[1].UsagePage = 0x01;
            rdev[1].Usage = 0x02;
            rdev[1].Flags = CRawInput.RawInputDeviceFlags.None;   // adds HID mouse and also ignores legacy mouse messages
            rdev[1].WindowHandle = hWnd.Handle;
            rdev[2].UsagePage = 0x01;
            rdev[2].Usage = 0x06;
            rdev[2].Flags = CRawInput.RawInputDeviceFlags.NoHotKeys;   // adds HID keyboard and also ignores legacy keyboard messages
            rdev[2].WindowHandle = hWnd.Handle;

            if (!CRawInput.RegisterRawInputDevices(rdev, 3, (uint)Marshal.SizeOf(typeof(CRawInput.RAWINPUTDEVICE))))
            {
                MessageBox.Show("No se pudo registrar la entrada de datos HID", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                hWnd = null;
                this.Close();
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
                    if (outSize == size)
                    {
                        //CRawInput.RAWINPUTHEADER header = new CRawInput.RAWINPUTHEADER();

                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
                        Marshal.Copy(buff, 0, ptr, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
                        CRawInput.RAWINPUTHEADER header = Marshal.PtrToStructure<CRawInput.RAWINPUTHEADER>(ptr);
                        Marshal.FreeHGlobal(ptr);
                        switch (header.dwType)
                        {
                            case 2:
                                {
                                    IntPtr pNombre = Marshal.AllocHGlobal(256);
                                    uint cbSize = 128;
                                    uint ret = CRawInput.GetRawInputDeviceInfo(header.hDevice, CRawInput.RawInputDeviceInfoCommand.DeviceName, pNombre, ref cbSize);
                                    String nombre = Marshal.PtrToStringAnsi(pNombre);
                                    Marshal.FreeHGlobal(pNombre);
                                    if (nombre.StartsWith("\\\\?\\HID#VID_06A3&PID_0255"))
                                    {
                                        ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CRawInput.RAWINPUTHID)));
                                        Marshal.Copy(buff, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)), ptr, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHID)));
                                        CRawInput.RAWINPUTHID hid = Marshal.PtrToStructure<CRawInput.RAWINPUTHID>(ptr);
                                        Marshal.FreeHGlobal(ptr);

                                        byte[] hidData = new byte[hid.Size - 1];
                                        for (int i = 0; i < hidData.Length; i++)
                                            hidData[i] = buff[i + 1 + size - hid.Size];

                                        ucInfo.ActualizarEstado(hidData, (hid.Size == 8));
                                        ucCalibrar.ActualizarEstado(hidData, (hid.Size == 8));
                                    }
                                }
                                break;
                            case 1:
                                {
                                    IntPtr pNombre = Marshal.AllocHGlobal(256);
                                    uint cbSize = 128;
                                    uint ret = CRawInput.GetRawInputDeviceInfo(header.hDevice, CRawInput.RawInputDeviceInfoCommand.DeviceName, pNombre, ref cbSize);
                                    String nombre = Marshal.PtrToStringAnsi(pNombre);
                                    Marshal.FreeHGlobal(pNombre);
                                    if (nombre.StartsWith("\\\\?\\HID#VID_06A3&PID_0255"))
                                    {
                                        ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CRawInput.RAWINPUTKEYBOARD)));
                                        Marshal.Copy(buff, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)), ptr, Marshal.SizeOf(typeof(CRawInput.RAWINPUTKEYBOARD)));
                                        CRawInput.RAWINPUTKEYBOARD keyb = Marshal.PtrToStructure<CRawInput.RAWINPUTKEYBOARD>(ptr);
                                        Marshal.FreeHGlobal(ptr);

                                        ucInfo.ActualizarTeclado(keyb);
                                    }
                                }
                                break;
                            case 0:
                                {
                                    IntPtr pNombre = Marshal.AllocHGlobal(256);
                                    uint cbSize = 128;
                                    uint ret = CRawInput.GetRawInputDeviceInfo(header.hDevice, CRawInput.RawInputDeviceInfoCommand.DeviceName, pNombre, ref cbSize);
                                    String nombre = Marshal.PtrToStringAnsi(pNombre);
                                    Marshal.FreeHGlobal(pNombre);
                                    if (nombre.StartsWith("\\\\?\\HID#VID_06A3&PID_0255"))
                                    {
                                        ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CRawInput.RAWINPUTMOUSE)));
                                        Marshal.Copy(buff, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)), ptr, Marshal.SizeOf(typeof(CRawInput.RAWINPUTMOUSE)));
                                        CRawInput.RAWINPUTMOUSE mouse = Marshal.PtrToStructure<CRawInput.RAWINPUTMOUSE>(ptr);
                                        Marshal.FreeHGlobal(ptr);

                                        ucInfo.ActualizarRaton(mouse);
                                    }
                                }
                                break;
                        }
                    }
                }

            }
            return IntPtr.Zero;
        }

        private void PestañaPruebas_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                tbCalibrar.IsChecked = false;
                SetRawMode(modoRaw);
            }
        }

        private void PestañaCalibrar_Checked(object sender, RoutedEventArgs e)
        {
            tbPrueba.IsChecked = false;
            SetRawMode(true);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (hWnd != null)
            {
                hWnd.RemoveHook(WndProc);
                hWnd = null;
            }
            SetRawMode(false);
        }

        #region "Saltar perfil"
        public void SetModoRaw(bool on)
        {
            modoRaw = on;
            SetRawMode(modoRaw);
        }

        private void SetRawMode(bool on)
        {
            if (Comunes.CIoCtl.AbrirDriver())
            {
                byte[] buff = (on) ? new byte[] { 1 } : new byte[] { 0 };
                Comunes.CIoCtl.DeviceIoControl(Comunes.CIoCtl.IOCTL_USR_RAW, buff, 1, null, 0, out _, IntPtr.Zero);
                Comunes.CIoCtl.CerrarDriver();
            }
        }
        #endregion
    }
}
