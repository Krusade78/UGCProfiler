using FluentAvalonia.UI.Controls;

namespace Profiler.Dialogs
{
    internal partial class CopyFrom : Frame
    {
        private static byte lastMode = 1;
        private static byte lastSubmode = 1;
        private readonly Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData;
        private readonly uint joyId;
        private readonly byte axisId;

#if DEBUG
        public CopyFrom() { InitializeComponent(); }
#endif
        private CopyFrom(Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData, uint joyId, byte axisId)
        {
            InitializeComponent();
            this.axisData = axisData;
            this.joyId = joyId;
            this.axisId = axisId;
            NumericUpDownM.Value = lastMode;
            NumericUpDownP.Value = lastSubmode;
        }

        public static async System.Threading.Tasks.Task Show(Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData, uint joyId, byte axisId)
        {
            CopyFrom content = new(axisData, joyId, axisId);
            ContentDialog dlg = new()
            {
                Title = Translate.Get("copy_sensibility_curve"),
                PrimaryButtonText = Translate.Get("ok"),
                SecondaryButtonText = Translate.Get("cancel"),
                DefaultButton = ContentDialogButton.Primary,
                Content = content
            };

            if (await dlg.ShowAsync() == ContentDialogResult.Primary)
            {
                content.Save();
            }
        }

        private void Save()
        {
            MainWindow parent = (MainWindow)((Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow;

            Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis = null;

            if (parent.GetData().Profile.AxesMap.TryGetValue(joyId, out Shared.ProfileModel.AxisMapModel axesMap))
            {
                if (axesMap.Modes.TryGetValue((byte)(((byte)(NumericUpDownM.Value - 1)  << 4) | (byte)(NumericUpDownP.Value - 1)), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
                {
                    mode.Axes.TryGetValue(axisId, out axis);
                }
            }
            if (axis != null) 
            {
                for (byte i = 0; i < 10; i++)
                {
                    axisData.Sensibility[i] = axis.Sensibility[i];
                }
                axisData.IsSensibilityForSlider = axis.IsSensibilityForSlider;
                parent.GetData().Modified = true;
                lastMode = (byte)NumericUpDownM.Value;
                lastSubmode = (byte)NumericUpDownP.Value;
            }
        }

        private void NumericUpDownM_GettingFocus(object sender, Avalonia.Input.GotFocusEventArgs args)
        {
            if (!IsLoaded)
            {
                args.Handled = true;
            }
        }
    }
}
