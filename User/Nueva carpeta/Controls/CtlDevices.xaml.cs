using API;
using Profiler.Devices;
using System;
using System.Linq;
using System.Runtime.InteropServices;


namespace Editor
{
	internal partial class CtlDevices : UserControl, IDisposable
	{
		private System.Windows.Interop.HwndSource hWnd;
		private bool hooked = false;
		private readonly UsbX52 winusbX52 = new(); 

		private readonly System.Collections.ObjectModel.ObservableCollection<DeviceInfo> devices = [];

		public event EventHandler<System.ComponentModel.PropertyChangedEventArgs> DeviceSelected;

		#region IDisposable
		private bool disposedValue;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (hooked) { hWnd.RemoveHook(WndProc); }
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

		public CtlDevices()
		{
			InitializeComponent();
			this.DataContext = new System.Windows.Data.CollectionViewSource() {
				Source = devices
			};
		}

		public void SetHwnd(System.Windows.Interop.HwndSource hWnd) => this.hWnd = hWnd;

		public bool Prepare()
		{
			CRawInput.RAWINPUTDEVICE[] rdev = new CRawInput.RAWINPUTDEVICE[2];
			rdev[0].UsagePage = 0x01;
			rdev[0].Usage = 0x04;
			rdev[0].WindowHandle = hWnd.Handle;
			rdev[0].Flags = CRawInput.RawInputDeviceFlags.None;
			rdev[1].UsagePage = 0x01;
			rdev[1].Usage = 0x05;
			rdev[1].WindowHandle = hWnd.Handle;
			rdev[1].Flags = CRawInput.RawInputDeviceFlags.None;

			if (!CRawInput.RegisterRawInputDevices(rdev, 1, (uint)Marshal.SizeOf(typeof(CRawInput.RAWINPUTDEVICE))))
			{
				MessageBox.Show(Translate.Get("unable to register raw input device"), Translate.Get("Error"), MessageBoxButton.OK, MessageBoxImage.Stop);
				return false;
			}
			else
			{
				hooked = true;
				hWnd.AddHook(WndProc);
				//X52 via WinUSB
				System.Threading.Tasks.Task.Run(() => { winusbX52.Process(this); });
			}

			return true;
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == 0x00FF)
			{
				int size = 0;

				_ = CRawInput.GetRawInputData(lParam, 0x10000003, null, ref size, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
				if (size != 0)
				{
					byte[] buff = new byte[size];

					int outSize = CRawInput.GetRawInputData(lParam, 0x10000003, buff, ref size, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
					if (outSize == size)
					{

						IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
						Marshal.Copy(buff, 0, ptr, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)));
						CRawInput.RAWINPUTHEADER header = Marshal.PtrToStructure<CRawInput.RAWINPUTHEADER>(ptr);
						Marshal.FreeHGlobal(ptr);

						IntPtr pName = Marshal.AllocHGlobal(256);
						uint cbSize = 128;
						_ = CRawInput.GetRawInputDeviceInfoW(header.hDevice, CRawInput.RawInputDeviceInfoCommand.DeviceName, pName, ref cbSize);
						string name = Marshal.PtrToStringUni(pName);
						Marshal.FreeHGlobal(pName);

						ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CRawInput.RAWINPUTHID)));
						Marshal.Copy(buff, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHEADER)), ptr, Marshal.SizeOf(typeof(CRawInput.RAWINPUTHID)));
						CRawInput.RAWINPUTHID hid = Marshal.PtrToStructure<CRawInput.RAWINPUTHID>(ptr);
						Marshal.FreeHGlobal(ptr);

						byte[] hidData = new byte[hid.Size];
						Array.Copy(buff, size - hid.Size, hidData, 0, hid.Size);

						uint? hId = AddDevice(name);
						if (hId != null)
						{
							SetStatus(hId.Value, hidData);
						}
					}
				}

			}
			return IntPtr.Zero;
		}

		public uint? AddDevice(string devName)
		{
			if (!uint.TryParse(devName[12..16], System.Globalization.NumberStyles.AllowHexSpecifier, null, out uint vid) ||
				!uint.TryParse(devName[21..25], System.Globalization.NumberStyles.AllowHexSpecifier, null, out uint pid))
			{
				return null;
			}

			uint hId = (vid << 16) | pid;
			if (!devices.Any(x => x.Id == hId))
			{
				ReadDeviceData(devName, hId);
			}

			return hId;
		}

		private void ReadDeviceData(string devName, uint hId)
		{
			DeviceInfo di = DeviceInfo.Get(devName, hId);
			if (di != null)
			{
				devices.Add(di);
			}
		}

		public void SetStatus(uint hId, byte[] hidData)
		{
		}

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (listView.SelectedIndex != -1)
			{
				DeviceSelected.Invoke(this, new(((DeviceInfo)listView.SelectedItem).Id.ToString()));
			}
		}
	}
}
