using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace Profiler.Pages
{
    internal partial class Generic : Frame, IHidToButton
    {
        private readonly Devices.DeviceInfo deviceInfo;
        private readonly Controls.Properties.CtlProperties props;
        private Microsoft.UI.Xaml.Controls.Primitives.ToggleButton? lastUse = null;
        private CHidToButton converter;

        private class TagData(ushort reportIdx, byte position)
        {
            public ushort ReportIdx = reportIdx;
            public byte Position = position;
        }

#if DEBUG
        public Generic() { InitializeComponent(); deviceInfo = new(); props = new(); converter = new([]); }
#endif
        public Generic(Devices.DeviceInfo di, Controls.Properties.CtlProperties props)
        {
            this.InitializeComponent();
            deviceInfo = di;
            this.props = props;
            Fill();
        }

        [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(converter))]
        private void Fill()
        {
            System.Collections.Generic.List<CHidToButton.Map> map = [];
            //Axes
            {
                byte[] order = [0, 1, 2, 3, 4, 5, 6];
                string[] name = ["X", "Y", "Z", "Rx", "Ry", "Rz", "Sl"];
                foreach (byte o in order)
                {
                    byte count = 1;
                    foreach (Shared.ProfileModel.DeviceInfo.CUsage u in deviceInfo.Usages.Where(x => x.Type == o))
                    {
                        Uno.UI.Toolkit.ElevatedView bd = new()
                        {
                            Elevation = 5,
                            Margin = new(25, 5, 25, 5),
                            CornerRadius = new CornerRadius(4)
                        };
                        Microsoft.UI.Xaml.Controls.Primitives.ToggleButton tb = new()
                        {
                            Content = $"{Translate.Get("axis")} {name[o]} {count++}",
                            Height = 60,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            FontSize = 18,
                            Tag = new TagData(u.ReportIdx, 0),
                        };
                        bd.ElevatedContent = tb;
                        tb.Checked += Tb_Checked;
                        spAxes.Children.Add(bd);
                        map.Add(new(u.ReportIdx, tb));
                    }
                }
            }

            //Hats
            {
                byte count = 1;
                foreach (Shared.ProfileModel.DeviceInfo.CUsage u in deviceInfo.Usages.Where(x => x.Type == 253))
                {
                    byte posMin = (byte)(u.Range & 0xf);
                    System.Collections.Generic.List<Microsoft.UI.Xaml.Controls.Primitives.ToggleButton> hatPos = [];
                    for (byte pos = posMin; pos <= (u.Range >> 4); pos++)
                    {
                        Uno.UI.Toolkit.ElevatedView bd = new()
                        {
                            Elevation = 5,
                            Margin = new(25, 5, 25, 5),
                            CornerRadius = new CornerRadius(4)
                        };
                        Microsoft.UI.Xaml.Controls.Primitives.ToggleButton tb = new()
                        {
                            Content = $"{Translate.Get("hat")} {count} - {Translate.Get("position")} {pos - posMin + 1}",
                            Height = 60,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            FontSize = 18,
                            Tag = new TagData(u.ReportIdx, pos),
                        };
                        bd.ElevatedContent = tb;
                        tb.Checked += Tb_Checked;
                        spHats.Children.Add(bd);
                        hatPos.Add(tb);
                    }
                    map.Add(new(u.ReportIdx, hatPos));
                    count++;
                }
            }

            //Buttons
            {
                byte count = 1;
                foreach (Shared.ProfileModel.DeviceInfo.CUsage u in deviceInfo.Usages.Where(x => x.Type == 254))
                {
                    for (byte pos = 0; pos <= u.Range; pos++)
                    {
                        Uno.UI.Toolkit.ElevatedView bd = new()
                        {
                            Elevation = 5,
                            Margin = new(25, 5, 25, 5),
                            CornerRadius = new CornerRadius(4)
                        };
                        Microsoft.UI.Xaml.Controls.Primitives.ToggleButton tb = new()
                        {
                            Content = $"{Translate.Get("button")} {count++}",
                            Height = 60,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            FontSize = 18,
                            Tag = new TagData((ushort)(u.ReportIdx + pos), 0),
                        };
                        bd.ElevatedContent = tb;
                        tb.Checked += Tb_Checked;
                        spButtons.Children.Add(bd);
                        map.Add(new((ushort)(u.ReportIdx + pos), tb));
                    }
                }
            }

            converter = new(map);
        }

        private void Tb_Checked(object sender, RoutedEventArgs e)
        {
            Microsoft.UI.Xaml.Controls.Primitives.ToggleButton tb = (Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)sender;
            props.Show(deviceInfo.Id, ((TagData)tb.Tag).ReportIdx, (string)tb.Content, ((TagData)tb.Tag).Position);
            if (lastUse != null) { lastUse.IsChecked = false; }
            tb.IsChecked = true;
            lastUse = tb;
            tb.Focus(FocusState.Keyboard);
            tb.Focus(FocusState.Pointer);
        }

        public void UpdateStatus(Devices.DeviceInfo di, byte[] rawData)
        {
            converter.Update(di, rawData);
        }
    }
}
