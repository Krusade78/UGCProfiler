using Avalonia.Controls;
using System;

namespace Profiler.Dialogs
{
    internal partial class SensibilityEditor : FluentAvalonia.UI.Controls.Frame
    {
        private MainWindow parent;
        private readonly Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData;

#if DEBUG
        public SensibilityEditor() { InitializeComponent(); }
#endif
        private SensibilityEditor(Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData)
        {
            InitializeComponent();
            this.axisData = axisData;
        }

        public static async System.Threading.Tasks.Task Show(Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData)
        {
            SensibilityEditor content = new(axisData);
            content.Init();
            FluentAvalonia.UI.Controls.ContentDialog dlg = new()
            {
                Title = Translate.Get("sensibility_curve"),
                PrimaryButtonText = Translate.Get("ok"),
                SecondaryButtonText = Translate.Get("cancel"),
                DefaultButton = FluentAvalonia.UI.Controls.ContentDialogButton.Primary,
                Content = content
            };

            if (await dlg.ShowAsync() == FluentAvalonia.UI.Controls.ContentDialogResult.Primary)
            {
                content.Save();
            }
        }

        private void Init()
        {
            parent = (MainWindow)((Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow;
            TrackBar1.Value = axisData.Sensibility[0];
            TrackBar2.Value = axisData.Sensibility[1];
            TrackBar3.Value = axisData.Sensibility[2];
            TrackBar4.Value = axisData.Sensibility[3];
            TrackBar5.Value = axisData.Sensibility[4];
            TrackBar6.Value = axisData.Sensibility[5];
            TrackBar7.Value = axisData.Sensibility[6];
            TrackBar8.Value = axisData.Sensibility[7];
            TrackBar9.Value = axisData.Sensibility[8];
            TrackBar10.Value = axisData.Sensibility[9];
            chkSlider.IsChecked = axisData.IsSensibilityForSlider;
        }

        private void Save()
        {
            axisData.Sensibility[0] = (byte)TrackBar1.Value;
            axisData.Sensibility[1] = (byte)TrackBar2.Value;
            axisData.Sensibility[2] = (byte)TrackBar3.Value;
            axisData.Sensibility[3] = (byte)TrackBar4.Value;
            axisData.Sensibility[4] = (byte)TrackBar5.Value;
            axisData.Sensibility[5] = (byte)TrackBar6.Value;
            axisData.Sensibility[6] = (byte)TrackBar7.Value;
            axisData.Sensibility[7] = (byte)TrackBar8.Value;
            axisData.Sensibility[8] = (byte)TrackBar9.Value;
            axisData.Sensibility[9] = (byte)TrackBar10.Value;
            axisData.IsSensibilityForSlider = chkSlider.IsChecked == true;
            parent.GetData().Modified = true;
        }

        private void TrackBar_ValueChanged(object sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            ((Slider)sender).Value = Math.Round(e.NewValue);
        }
    }
}
