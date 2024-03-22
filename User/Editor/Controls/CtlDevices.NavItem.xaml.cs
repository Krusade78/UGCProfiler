using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Profiler.Controls
{
	internal sealed partial class CtlDevices_NavItem : NavigationViewItem
	{
		public CtlDevices_NavItem(Devices.DeviceInfo item)
		{
			this.InitializeComponent();
			this.DataContext = item;
			if (item.FromProfile)
			{
				icoProfile.Visibility = Visibility.Visible;
			}
            else
            {
                iconHardware.Visibility = Visibility.Visible;
            }
        }
	}
}
