using System;
using Microsoft.UI.Xaml.Controls;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Pages
{
    public sealed partial class Main : Grid
    {
        private Devices.DeviceInfo deviceInfo;
        private MainWindow.Section currentSection = MainWindow.Section.None;

        public Main()
        {
            InitializeComponent();
        }

        public void Refresh(MainWindow.Section newSection)
        {
            currentSection = newSection;
            Refresh();
        }
        private void Refresh()
        {
            if (currentSection == MainWindow.Section.Macros)
            {
                page.Content = new Macros.Main();
                properties.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            else if (deviceInfo == null)
            {
                page.Content = null;
                properties.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                properties.CurrentDevInfo = deviceInfo;
                properties.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                switch (currentSection)
                {
                    case MainWindow.Section.Calibrate:
                        page.Content = new Calibrator.HIDCal(deviceInfo);
                        properties.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        break;
                    case MainWindow.Section.View:
                        break;
                    case MainWindow.Section.Edit:
                        page.Content = deviceInfo.Id switch
                        {
                            0x06a30763 => new SaitekPedals(properties),
                            0x6a30255 => new SaitekX52(properties),
                            0x231d0200 => new VkbGladiatorNXT(properties),
                            _ => new Generic(deviceInfo, properties),
                        };
                        break;
                    default:
                        page.Content = null;
                        properties.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        break;
                }
            }
        }

        public void ChangeDevice(Devices.DeviceInfo di)
        {
            deviceInfo = di;
            Refresh();
        }

        public void UpdateStatus(uint hId, byte[] hidData)
        {
            if (page.Content is Calibrator.HIDCal cal)
            {
                cal.UpdateStatus(hidData, hId);
            }
            else if ((deviceInfo != null) && (deviceInfo.Id == hId) && (currentSection == MainWindow.Section.Edit))
            {
                ((IHidToButton)page.Content).UpdateStatus(deviceInfo, hidData);
            }
        }
    }
}
