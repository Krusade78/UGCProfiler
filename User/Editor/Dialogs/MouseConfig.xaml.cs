using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Profiler.Dialogs
{
    /// <summary>
    /// Lógica de interacción para VEditorRaton.xaml
    /// </summary>
    internal sealed partial class MouseConfig : Page
    {
        private MouseConfig()
        {
            InitializeComponent();
        }

        public static async System.Threading.Tasks.Task Show(MainWindow parent)
        {
            MouseConfig content = new();
            content.NumericUpDown1.Value = parent.GetData().Profile.MouseTick;
            ContentDialog dlg = new()
            {
                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                XamlRoot = ((App)Application.Current).GetMainWindow().Root.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
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
