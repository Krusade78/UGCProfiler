using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Shared.CTypes;

namespace Profiler.Pages.Macros
{
    public partial class CtlStatusCommands : UserControl, IChangeMode
    {
        public CtlStatusCommands()
        {
            this.InitializeComponent();
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
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([(uint)((byte)CommandType.Delay + ((ushort)NumericUpDown6.Value << 8))]);
        }

        private void ButtonRepeatN_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(2)) return;
            uint[] bloque =
            [
                (uint)((byte)CommandType.RepeatN +((ushort)NumericUpDown4.Value << 8)),
                (byte)CommandType.RepeatN | (byte)CommandType.Release,
            ];
            ((EditedMacro)DataContext).Insert(bloque);
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
            if (((EditedMacro)DataContext).LimitReached(1))
                return;

            if (!await ((EditedMacro)DataContext).CheckHoldWithRepeat()) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.Hold]);
        }
        private async void Repeat()
        {
            if (((EditedMacro)DataContext).LimitReached(2))
                return;

            if (! await ((EditedMacro)DataContext).CheckHoldWithRepeat()) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.Repeat, (byte)CommandType.Repeat | (byte)CommandType.Release]);
        }
        #endregion
    }
}
