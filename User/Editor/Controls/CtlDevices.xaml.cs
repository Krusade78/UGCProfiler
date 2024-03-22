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
			MainWindow parent = ((App)Microsoft.UI.Xaml.Application.Current).GetMainWindow();
			if (parent.CurrentSection == MainWindow.Section.Calibrate)
			{
				((Calibrator.HIDCal)frContent.Content).UpdateStatus(hidData, hId);
			}
		}

		private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{
			Refresh();
		}

		public void Refresh()
		{
			if (this.SelectedItem == null)
			{
				frContent.Content = null;
				return;
			}
			MainWindow parent = ((App)Microsoft.UI.Xaml.Application.Current).GetMainWindow();
			switch(parent.CurrentSection)
			{
				case MainWindow.Section.Calibrate:
					this.Header = ((Devices.DeviceInfo)((CtlDevices_NavItem)this.SelectedItem).DataContext).Name;
					frContent.Content = new Calibrator.HIDCal((Devices.DeviceInfo)((CtlDevices_NavItem)this.SelectedItem).DataContext);
					break;
				case MainWindow.Section.View:
					break;
				case MainWindow.Section.Edit:
					//	switch (uint.Parse(e.PropertyName))
					//	{
					//		case 0x6a30713:
					//			gridView.Children.Add(new CtlPedales(ctlProperties));
					//			break;
					//		case 0x6a30255:
					//			gridView.Children.Add(new CtlX52Joystick(ctlProperties));
					//			break;
					//		case 0x6a30700:
					//			gridView.Children.Add(new CtlNXTJoystick(ctlProperties));
					//			break;
					//		default:
					//			gridView.Children.Add(new CtlOther(ctlProperties));
					//			break;
					//	}
					break;
				default:
					frContent.Content = null;
					break;
			}
		}
	}
}
