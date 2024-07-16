using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Pages.Macros
{
    public sealed partial class CtlName : UserControl
    {
        public CtlName()
        {
            this.InitializeComponent();
            GroupBox2.Translation += new System.Numerics.Vector3(0, 0, 32);
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                if (((ToggleSwitch)sender).IsOn)
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
            txtName.Text = ((App)Application.Current).GetMainWindow().GetData().Profile.Macros.Find(x => x.Id == id).Name;
        }

        public string GetName() => txtName.Text.Trim();
    }
}
