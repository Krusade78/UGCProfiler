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

        #region "Status commands"
        private void ButtonHold_Click_1(object sender, RoutedEventArgs e)
        {
            Hold();
        }

        private void ButtonRepeat_Click(object sender, RoutedEventArgs e)
        {
            Repeat();
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([(uint)((byte)CommandType.Delay + ((ushort)NumericUpDown6.Value << 8))], false);
        }

        private void ButtonRepeatN_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 236) return;
            uint[] bloque =
            [
                (uint)((byte)CommandType.RepeatN +((ushort)NumericUpDown4.Value << 8)),
                (byte)CommandType.RepeatN | (byte)CommandType.Release,
            ];
            ((EditedMacro)DataContext).Insert(bloque, true);
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
            if (((EditedMacro)DataContext).GetCount() > 237)
                return;

            if (!await ((EditedMacro)DataContext).CheckHoldWithRepeat()) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.Hold], false);
        }
        private async void Repeat()
        {
            if (((EditedMacro)DataContext).GetCount() > 236)
                return;

            if (! await ((EditedMacro)DataContext).CheckHoldWithRepeat()) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.Repeat, (byte)CommandType.Repeat | (byte)CommandType.Release], true);
        }
        #endregion
    }
}
