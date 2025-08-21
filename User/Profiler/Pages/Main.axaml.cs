using Avalonia.Controls;

namespace Profiler.Pages
{
    public partial class Main : Grid
    {
        private Devices.DeviceInfo deviceInfo;
        private MainWindow.Section currentSection = MainWindow.Section.None;

        public Main()
        {
            InitializeComponent();
        }

        public void GoToSection(MainWindow.Section newSection)
        {
            currentSection = newSection;
            Refresh();
        }

        public void Refresh()
        {
            if (currentSection == MainWindow.Section.Macros)
            {
                page.Content = new Macros.Main();
                properties.IsVisible = false;
            }
            else if (deviceInfo == null)
            {
                page.Content = null;
                properties.IsVisible = false;
            }
            else
            {
                properties.CurrentDevInfo = deviceInfo;
                properties.IsVisible = true;
                switch (currentSection)
                {
                    case MainWindow.Section.Calibrate:
                        page.Content = new Calibrator.HIDCal(deviceInfo);
                        properties.IsVisible = false;
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
                        properties.IsVisible = false;
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
