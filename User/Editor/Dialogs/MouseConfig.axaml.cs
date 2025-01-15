using FluentAvalonia.UI.Controls;

namespace Profiler.Dialogs
{
    internal partial class MouseConfig : Frame
    {
#if DEBUG
        public MouseConfig() { InitializeComponent(); }
#endif
        private MouseConfig(bool _)
        {
            InitializeComponent();
        }

        public static async System.Threading.Tasks.Task Show(MainWindow parent)
        {
            MouseConfig content = new(true);
            content.NumericUpDown1.Value = parent.GetData().Profile.MouseTick;
            ContentDialog dlg = new()
            {
                Title = Translate.Get("mouse_configuration"),
                PrimaryButtonText = Translate.Get("ok"),
                SecondaryButtonText = Translate.Get("cancel"),
                DefaultButton = ContentDialogButton.Primary,
                Content = content
            };

            if (await dlg.ShowAsync() == ContentDialogResult.Primary)
            {
                parent.GetData().Profile.MouseTick = (byte)content.NumericUpDown1.Value;
                parent.GetData().Modified = true;
            }
        }
    }
}
