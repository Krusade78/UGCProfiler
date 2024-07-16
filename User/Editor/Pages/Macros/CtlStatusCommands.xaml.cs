using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Shared.CTypes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Pages.Macros
{
    public sealed partial class CtlStatusCommands : UserControl, IChangeMode
    {
        public CtlStatusCommands()
        {
            this.InitializeComponent();
            Panel2.Translation += new System.Numerics.Vector3(0, 0, 16);
        }

        #region "comandos de estado"
        private void ButtonMantener_Click_1(object sender, RoutedEventArgs e)
        {
            Hold();
        }

        private void ButtonRepetir_Click(object sender, RoutedEventArgs e)
        {
            Repeat();
        }

        private void ButtonPausa_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([(uint)((byte)CommandType.Delay + ((ushort)NumericUpDown6.Value << 8))], false);
        }

        private void ButtonRepetirN_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 236) return;
            uint[] bloque =
            [
                (uint)((byte)CommandType.RepeatN +((ushort)NumericUpDown4.Value << 8)),
                (byte)CommandType.RepeatN | (byte)CommandType.Release,
            ];
            ((EditedMacro)DataContext).Insertar(bloque, true);
        }
        #endregion

        public void GoToBasic()
        {
            PanelBasic.Visibility = Visibility.Visible;
        }

        public void GoToAdvanced()
        {
            PanelBasic.Visibility = Visibility.Collapsed;
        }

        #region "status commands"
        private async void Hold()
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237)
                return;

            if (!await ((EditedMacro)DataContext).CheckHoldWithRepeat()) return;
            ((EditedMacro)DataContext).Insertar([(byte)CommandType.Hold], false);
        }
        private async void Repeat()
        {
            if (((EditedMacro)DataContext).GetCuenta() > 236)
                return;

            if (! await ((EditedMacro)DataContext).CheckHoldWithRepeat()) return;
            ((EditedMacro)DataContext).Insertar([(byte)CommandType.Repeat, (byte)CommandType.Repeat | (byte)CommandType.Release], true);
        }
        #endregion
    }
}
