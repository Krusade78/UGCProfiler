using System;
using Microsoft.UI.Xaml.Controls;

namespace Profiler.Controls
{
    internal partial class CtlDevices_NavItem : NavigationViewItem, IDisposable
    {
        private Devices.DeviceInfo? Item {  get; set; }
        private readonly NavigationView parent;

        #region IDisposable
        private bool disposedValue;
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    parent.PaneOpening -= Parent_PaneOpening;
                    parent.PaneClosing -= Parent_PaneClosing;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public new void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public CtlDevices_NavItem(NavigationView parent, Devices.DeviceInfo item)
        {
            this.parent = parent;
            Item = item;
            InitializeComponent();

            DataContext = item;
            parent.PaneOpening += Parent_PaneOpening;
            parent.PaneClosing += Parent_PaneClosing;
            if (item.FromProfile)
            {
                icoProfile.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
            else
            {
                iconHardware.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
        }

        private void Parent_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            iconHardware.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center;
            icoProfile.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center;
            text.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            info.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        private void Parent_PaneOpening(NavigationView sender, object args)
        {
            iconHardware.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
            icoProfile.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Left;
            text.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            info.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
    }
}
