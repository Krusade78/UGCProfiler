using Microsoft.UI.Xaml.Controls;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Pages
{
    internal sealed partial class Generic : Page, IHidToButton
    {
        private readonly Devices.DeviceInfo deviceInfo;
        private readonly Controls.CtlProperties props;
        private Microsoft.UI.Xaml.Controls.Primitives.ToggleButton lastUse = null;
        private CHidToButton converter;

        private class TagData(ushort reportIdx, byte position)
        {
            public ushort ReportIdx = reportIdx;
            public byte Position = position;
        }

        public Generic(Devices.DeviceInfo di, Controls.CtlProperties props)
        {
            this.InitializeComponent();
            deviceInfo = di;
            this.props = props;
            bd1.Translation += new System.Numerics.Vector3(0, 0, 32);
            bd2.Translation += new System.Numerics.Vector3(0, 0, 32);
            bd3.Translation += new System.Numerics.Vector3(0, 0, 32);
            Fill();
        }

        private void Fill()
        {
            System.Collections.Generic.List<CHidToButton.Map> map = [];
            //Axes
            {
                byte[] order = [0, 1, 2, 3, 4, 5, 6];
                string[] name = ["X", "Y", "Z", "Rx", "Ry", "Rz", "Sl"];
                foreach(byte o in order)
                {
                    byte count = 1;
                    foreach (Shared.ProfileModel.DeviceInfo.CUsage u in deviceInfo.Usages.Where(x => x.Type == o))
                    {
                        Microsoft.UI.Xaml.Controls.Primitives.ToggleButton tb = new()
                        {
                            Content = $"{Translate.Get("axis")} {name[o]} {count++}",
                            Height = 70,
                            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                            FontSize = 20,
                            Margin = new(25, 5, 25, 5),
                            Tag = new TagData(u.ReportIdx, 0),
                            Shadow = new Microsoft.UI.Xaml.Media.ThemeShadow(),
                        };
                        tb.Translation += new System.Numerics.Vector3(0, 0, 8);
                        tb.Checked += Tb_Checked;
                        spAxes.Children.Add(tb);
                        map.Add(new(u.ReportIdx, tb));
                    }
                }
            }

            //Hats
            {
                byte count = 1;
                foreach (Shared.ProfileModel.DeviceInfo.CUsage u in deviceInfo.Usages.Where(x => x.Type == 253))
                {
                    for (byte pos = 0; pos <= u.Range; pos++)
                    {
                        Microsoft.UI.Xaml.Controls.Primitives.ToggleButton tb = new()
                        {
                            Content = $"{Translate.Get("hat")} {count++} - {Translate.Get("position")} {pos + 1}",
                            Height = 70,
                            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                            FontSize = 20,
                            Margin = new(25,5,25,5),
                            Tag = new TagData(u.ReportIdx, pos),
                            Shadow = new Microsoft.UI.Xaml.Media.ThemeShadow(),
                        };
                        tb.Translation += new System.Numerics.Vector3(0, 0, 8);
                        tb.Checked += Tb_Checked;
                        spHats.Children.Add(tb);
                        map.Add(new(u.ReportIdx, tb));
                    }
                }
            }

            //Buttons
            {
                byte count = 1;
                foreach (Shared.ProfileModel.DeviceInfo.CUsage u in deviceInfo.Usages.Where(x => x.Type == 254))
                {
                    for (byte pos = 0; pos <= u.Bits; pos++)
                    {
                        Microsoft.UI.Xaml.Controls.Primitives.ToggleButton tb = new()
                        {
                            Content = $"{Translate.Get("button")} {count++}",
                            Height = 70,
                            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
                            FontSize = 20,
                            Margin = new(25, 5, 25, 5),
                            Tag = new TagData((ushort)(u.ReportIdx + pos), 0),
                            Shadow = new Microsoft.UI.Xaml.Media.ThemeShadow(),
                        };
                        tb.Translation += new System.Numerics.Vector3(0, 0, 8);
                        tb.Checked += Tb_Checked;
                        spButtons.Children.Add(tb);
                        map.Add(new((ushort)(u.ReportIdx + pos), tb));
                    }
                }
            }

            converter = new(map);
        }

        private void Tb_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Microsoft.UI.Xaml.Controls.Primitives.ToggleButton tb = (Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)sender;
            props.Show(deviceInfo.Id, ((TagData)tb.Tag).ReportIdx, (string)tb.Content, ((TagData)tb.Tag).Position);
            if (lastUse != null) { lastUse.IsChecked = false; }
            tb.IsChecked = true;
            lastUse = tb;
            tb.Focus(Microsoft.UI.Xaml.FocusState.Keyboard);
            tb.Focus(Microsoft.UI.Xaml.FocusState.Pointer);
        }

        public void UpdateStatus(Devices.DeviceInfo di, byte[] rawData)
        {
            converter.Update(di, rawData);
        }
    }
}
