using Microsoft.UI.Xaml.Controls;

namespace Profiler.Pages
{
    public partial class Main : Grid
    {
        private Devices.DeviceInfo? deviceInfo;
        private MainPage.Section currentSection = MainPage.Section.None;

        public Main()
        {
            InitializeComponent();
        }

        public void GoToSection(MainPage.Section newSection)
        {
            currentSection = newSection;
            Refresh();
        }

        public void Refresh()
        {
            if (currentSection == MainPage.Section.Macros)
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
                    case MainPage.Section.Calibrate:
                        if (deviceInfo != null)
                        {
                            page.Content = new Calibrator.HIDCal(deviceInfo);
                            properties.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        }
                        break;
                    case MainPage.Section.View:
                        break;
                    case MainPage.Section.Edit:
                    default:
                        page.Content = deviceInfo.Id switch
                        {
                            0x06a30763 => new SaitekPedals(properties),
                            0x6a30255 => new SaitekX52(properties),
                            0x231d0200 => new VkbGladiatorNXT(properties),
                            _ => new Generic(deviceInfo, properties),
                        };
                        break;
                        //page.Content = null;
                        //properties.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        //break;
                }
            }
        }

        public void ChangeDevice(Devices.DeviceInfo? di)
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
            else if ((deviceInfo != null) && (deviceInfo.Id == hId) && (currentSection == MainPage.Section.Edit))
            {
                ((IHidToButton)page.Content).UpdateStatus(deviceInfo, hidData);
            }
        }
    }
}
