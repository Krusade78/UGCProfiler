using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Controls
{
	public sealed partial class CtlDevices : NavigationView, IDisposable
	{
		private bool wndProcReady;
		private readonly Devices.UsbX52 winusbX52 = new();
        System.Threading.Tasks.Task thWinusbX52 = null;
        private readonly CPP2CS.RawInput rawInput = null;
        System.Threading.Tasks.Task thRawInput = null;


        private readonly System.Collections.Generic.SortedList<string, Devices.DeviceInfo> devices = [];

		#region IDisposable
		private bool disposedValue;
		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					rawInput?.Close();
					thRawInput?.Wait();
					winusbX52?.Dispose();
					thWinusbX52.Wait();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~CtlDevices()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion

		public CtlDevices()
		{
			InitializeComponent();
			rawInput = new(WndProc);
		}

		public  bool Prepare()
		{
			AddConnectedDevices();

			thRawInput = System.Threading.Tasks.Task.Run(() => { rawInput.Init(); });
			if (thRawInput.Wait(2000))
			{
				return false;
			}
			wndProcReady = true;

			//X52 via WinUSB
			thWinusbX52 = System.Threading.Tasks.Task.Run(() => { winusbX52.Process(this); });

			return true;
		}

		private void AddConnectedDevices()
		{
            API.CWinUSB.SP_DEVICE_INTERFACE_DATA diData = new();
            Guid hidGuid = new();
            API.HID.HidD_GetHidGuid(ref hidGuid);
            IntPtr diDevs = API.CWinUSB.SetupDiGetClassDevsW(ref hidGuid, null, IntPtr.Zero, 0x2 | 0x10);
            if (new IntPtr(-1) == diDevs)
            {
                return;
            }

            diData.cbSize = System.Runtime.InteropServices.Marshal.SizeOf<API.CWinUSB.SP_DEVICE_INTERFACE_DATA>();
            uint idx = 0;
            while (API.CWinUSB.SetupDiEnumDeviceInterfaces(diDevs, IntPtr.Zero, ref hidGuid, idx++, ref diData))
            {
                uint tam = 0;
                if ((false == API.CWinUSB.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, IntPtr.Zero, 0, ref tam, IntPtr.Zero)) && (122 != API.CWinUSB.GetLastError()))
                {
                    continue;
                }

                IntPtr buf = System.Runtime.InteropServices.Marshal.AllocHGlobal((int)tam);
                System.Runtime.InteropServices.Marshal.WriteInt32(buf, 8);
                if (!API.CWinUSB.SetupDiGetDeviceInterfaceDetailW(diDevs, ref diData, buf, tam, ref tam, IntPtr.Zero))
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(buf);
                    continue;
                }

                string ninterface = System.Runtime.InteropServices.Marshal.PtrToStringAuto(buf + 4);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(buf);
                if (!ninterface.Contains("vid", StringComparison.InvariantCultureIgnoreCase) || !ninterface.Contains("pid", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                IntPtr hDev = API.CWinUSB.CreateFileW(ninterface, 0x80000000 | 0x40000000, 1 | 2, IntPtr.Zero, 3, 0x00000080 | 0x40000000, IntPtr.Zero);
                if (hDev == API.CWinUSB.INVALID_HANDLE_VALUE)
                {
                    continue;
                }

                IntPtr pdata = IntPtr.Zero;
                if (!API.HID.HidD_GetPreparsedData(hDev, ref pdata))
                {
                    API.CWinUSB.CloseHandle(hDev);
                    continue;
                }

                IntPtr pcaps = System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf<API.HID.HIDP_CAPS>());
                if (API.HID.HidP_GetCaps(pdata, pcaps) == 0x110000)
                {
                    API.HID.HIDP_CAPS caps = System.Runtime.InteropServices.Marshal.PtrToStructure<API.HID.HIDP_CAPS>(pcaps);
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(pcaps);
                    if ((caps.UsagePage == 1) && ((caps.Usage == 4) || (caps.Usage == 5)))
                    {
                        AddHardwareDevice(ninterface);
                    }
                }
                else
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(pcaps);
                }
                API.HID.HidD_FreePreparsedData(pdata);
                API.CWinUSB.CloseHandle(hDev);
            }

            API.CWinUSB.SetupDiDestroyDeviceInfoList(diDevs);
        }

		public void AddWinUSBX52Device()
		{
			AddHardwareDevice("_WUSBX52vid_06a3&pid_0255");
		}

		private void WndProc(string ninterface, byte[] hidData)
		{
			if (wndProcReady)
			{
				uint? hId = AddHardwareDevice(ninterface);
				if (hId != null)
				{
					this.DispatcherQueue.TryEnqueue(() => {	SetStatus(hId.Value, hidData); });
				}
			}
		}


		private uint? AddHardwareDevice(string devName)
		{
			if (!uint.TryParse(devName[12..16], System.Globalization.NumberStyles.AllowHexSpecifier, null, out uint vid) ||
				!uint.TryParse(devName[21..25], System.Globalization.NumberStyles.AllowHexSpecifier, null, out uint pid))
			{
				return null;
			}

			uint hId = (vid << 16) | pid;
			Devices.DeviceInfo di = devices.Select(x => x.Value).FirstOrDefault(x => x.Id == hId);
			if (di == null)
			{
				ReadDeviceData(devName, hId);
			}
			else
			{
				if (di.FromProfile)
				{
					devices.Remove(di.Name);
					ReadDeviceData(devName, hId);
				}
			}

			return hId;
		}

		private void ReadDeviceData(string devName, uint hId)
		{
			Devices.DeviceInfo di = Devices.DeviceInfo.Get(devName, hId);
			if (di != null)
			{
				devices.Add(di.Name, di);
				this.DispatcherQueue.TryEnqueue(() =>
				{
					MenuItems.Insert(devices.IndexOfKey(di.Name), new CtlDevices_NavItem(di));
				});
			}
		}

		public void SetStatus(uint hId, byte[] hidData)
		{
			((Pages.Main)frContent.Content)?.UpdateStatus(hId, hidData);
		}

		public void SetMainFrame(Pages.Main mainFrame)
		{
			frContent.Content = mainFrame;
		}

		private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			if (this.SelectedItem == null)
			{
				this.Header = null;
				((Pages.Main)frContent.Content)?.ChangeDevice(null);
			}
			else
			{
				this.Header = ((Devices.DeviceInfo)((CtlDevices_NavItem)this.SelectedItem).DataContext).Name;
				((Pages.Main)frContent.Content)?.ChangeDevice((Devices.DeviceInfo)((CtlDevices_NavItem)this.SelectedItem).DataContext);
			}
		}
	}
}
