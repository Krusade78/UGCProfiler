namespace Profiler.Controls
{
	internal partial class CtlDevices_NavItem : Avalonia.Controls.Grid
    {
        public CtlDevices_NavItem()
		{
            this.InitializeComponent();
        }

        public CtlDevices_NavItem(Devices.DeviceInfo item)
		{
			this.InitializeComponent();
			this.DataContext = item;
			if (item.FromProfile)
			{
				icoProfile.IsVisible = true;
			}
            else
            {
				iconHardware.IsVisible = true;
			}
        }
	}
}
