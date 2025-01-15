using Avalonia.Controls;
using FluenUI = FluentAvalonia.UI.Controls;

namespace Profiler.Dialogs
{
    internal partial class ZoneEditor : FluenUI.Frame
    {
        private class ZoneControls
        {
            public byte Zone { get; set; }
            public Grid GridText { get; set; }
            public FluenUI.NumberBox Number {  get; set; }
            public Grid Area { get; set; }
        }
        private readonly Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData;
        private readonly System.Collections.Generic.List<ZoneControls> zones = [];
        private bool events = true;
        private readonly ushort range;

#if DEBUG
        public ZoneEditor() { InitializeComponent(); }
#endif
        private ZoneEditor(ushort range, Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData)
        {
            InitializeComponent();
            this.axisData = axisData;
            this.range = range;
        }

        public static async System.Threading.Tasks.Task<FluenUI.ContentDialogResult> Show(ushort range, Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData)
        {
            ZoneEditor content = new(range, axisData);
            content.Init();
            FluenUI.ContentDialog dlg = new()
            {
                Title = Translate.Get("zone_editor"),
                PrimaryButtonText = Translate.Get("ok"),
                SecondaryButtonText = Translate.Get("cancel"),
                DefaultButton = FluenUI.ContentDialogButton.Primary,
                Content = content
            };

            FluenUI.ContentDialogResult res = await dlg.ShowAsync();
            if (res == FluenUI.ContentDialogResult.Primary)
            {
                content.Save();
            }

            return res;
        }

        private void Init()
        {
            grb.Height = range;
            foreach (byte b in axisData.Zones)
            {
                AddBand(b);
            }

            numBands.Maximum = (int)(range * 0.25);
            if (numBands.Maximum > 100) { numBands.Maximum = 100; }

            numBands.Value = axisData.Zones.Count + 1;

        }

        private void NumBands_TextChanged(FluenUI.NumberBox sender, FluenUI.NumberBoxValueChangedEventArgs args)
        {
            if (events)
                ChangeStrips();
        }

        private void Save()
        {
            axisData.Zones.Clear();
            foreach (ZoneControls bc in zones)
            {
                axisData.Zones.Add(bc.Zone);
            }
        }

        private void Flbl_TextChanged(FluenUI.NumberBox sender, FluenUI.NumberBoxValueChangedEventArgs args)
        {
            if (events)
            {
                foreach (ZoneControls bc in zones)
                {
                    if (bc.Number == sender)
                    {
                        bc.Zone = (byte)sender.Value;
                        break;
                    }
                }
                ChangeStrips();
            }
        }

        private void AddBand(byte b = 0)
        {
            Grid grTxt = new() { Margin = new(0, 0, 0, 4) };
            grTxt.ColumnDefinitions.Add(new() { Width = new(75) });
            grTxt.ColumnDefinitions.Add(new());
            grTxt.Children.Add(new TextBlock() { Text = $"{Translate.Get("zone")} {zones.Count + 1}:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new(0, 0, 5, 0), FontSize = 14, FontWeight = Avalonia.Media.FontWeight.SemiBold });

            FluenUI.NumberBox nb = new() { Minimum = zones.Count == 0 ? 1 : zones[^1].Zone + 1, Maximum = 99, SpinButtonPlacementMode = FluenUI.NumberBoxSpinButtonPlacementMode.Compact};
            nb.Value = b == 0 ? nb.Minimum : b;
            nb.ValueChanged += Flbl_TextChanged;
            Grid.SetColumn(nb, 1);
            grTxt.Children.Add(nb);

            zones.Add(new()
            {
                Zone = b == 0 ? (byte)((zones.Count > 0) ? zones[^1].Zone + 1 : 1) : b,
                GridText = grTxt,
                Number = nb,
                Area = new()
            });
            zones[^1].Area.Bind(Grid.BackgroundProperty, Resources.GetResourceObservable(zones.Count % 2 != 0 ? "ControlAAFillColorDefaultBrush" : "ControlAAFillColorDisabledBrush"));
            spNumber.Children.Add(grTxt);
            grb.Children.Add(zones[^1].Area);
        }

        private void ChangeStrips()
        {
            events = false;

            while (zones.Count > (numBands.Value - 1))
            {
                spNumber.Children.RemoveAt(zones.Count - 1);
                grb.Children.RemoveAt(zones.Count - 1);
                zones.RemoveAt(zones.Count -1);
            }

            byte newBands = 0;
            while (zones.Count < (numBands.Value - 1))
            {
                if (((zones.Count > 0) ? zones[^1].Zone : 1) == 99)
                {
                    numBands.Value = zones.Count + 1;
                    break;
                }
                AddBand();
                newBands++;
            }

            if (zones.Count == 0)
            {
                area0.Height = range;
                events = true;
                return;
            }

            if (newBands != 0)
            {
                byte available = (byte)((zones.Count == 0) || (zones.Count - newBands == 0) ? 98 : 99 - zones[zones.Count - newBands - 1].Zone);
                available /= (byte)(newBands + 1);
                for (byte i = (byte)(zones.Count - newBands); i < zones.Count; i++)
                {
                    zones[i].Zone = (byte)(i == 0 ? available + 1 : zones[i - 1].Zone + available);
                    zones[i].Number.Value = zones[i].Zone;
                }
            }
            area0.Height = zones[0].Zone * range / 100;
            for (byte i = 0; i < zones.Count - 1; i++)
            {
                zones[i].Number.Maximum = zones[i + 1].Zone - 1;
                zones[i].Area.Height = (zones[i + 1].Zone - zones[i].Zone) * range / 100;
            }
            zones[^1].Area.Height = (100 - zones[^1].Zone) * range / 100;

            events = true;
        }

        private void NumBands_GettingFocus(object sender, Avalonia.Input.GotFocusEventArgs args)
        {
            if (!IsLoaded)
            {
                args.Handled = true;
            }
        }
    }
}
