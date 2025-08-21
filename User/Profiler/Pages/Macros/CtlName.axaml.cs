using Avalonia.Interactivity;
using Avalonia.Controls;

namespace Profiler.Pages.Macros
{
    public sealed partial class CtlName : UserControl
    {
        public CtlName()
        {
            this.InitializeComponent();
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                if (((ToggleSwitch)sender).IsChecked == true)
                {
                    ((Main)DataContext).GoToAdvanced();
                }
                else
                {
                    ((Main)DataContext).GoToBasic();
                }
            }
        }

        public void Load(ushort id)
        {
            txtName.Text = ((App)Avalonia.Application.Current).GetMainWindow().GetData().Profile.Macros.Find(x => x.Id == id).Name;
        }

        public string GetName() => txtName.Text.Trim();
    }
}
