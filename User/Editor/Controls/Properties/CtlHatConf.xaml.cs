using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Controls.Properties
{
    internal sealed partial class CtlHatConf : UserControl
    {
        private readonly CtlProperties parent;
        public CtlHatConf(CtlProperties parent)
        {
            this.parent = parent;
            this.InitializeComponent();

            spHat.Translation += new System.Numerics.Vector3(0, 0, 16);
        }

        public void Init(bool isButton)
        {
            if (isButton)
            {
                buttonSection.Visibility = Visibility.Visible;
            }
            else
            {
                hatSection.Visibility = Visibility.Visible;
            }
        }

        private async void ButtonAssignPOV_Click(object sender, RoutedEventArgs e)
        {
            await Dialogs.HatEditor.Show(CtlProperties.CurrentSel.Joy, parent.GetMode(), CtlProperties.CurrentSel.Usage);
            //            VEditorPOV dlg = new VEditorPOV(idActual)
            //            {
            //                Owner = App.Current.MainWindow
            //            };
            //            if (dlg.ShowDialog() == true)
            //            {
            //                padre.GetData().Modified = true;
            //                Ver(idActual, tipoActual, "");
            //            }
        }
    }
}
