using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
                if (((ToggleSwitch)sender).IsOn == true)
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
            txtName.Text = ((App)Application.Current).GetMainPage().GetData().Profile.Macros.Find(x => x.Id == id)?.Name;
        }

        public string GetName() => txtName.Text.Trim();
    }
}
