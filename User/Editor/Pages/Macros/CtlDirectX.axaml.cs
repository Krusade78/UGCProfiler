using Avalonia.Interactivity;
using Avalonia.Controls;
using static Shared.CTypes;

namespace Profiler.Pages.Macros
{
    public partial class CtlDirectX : UserControl, IChangeMode
    {
        public CtlDirectX()
        {
            this.InitializeComponent();
        }

        #region "DirectX"
        private void ButtonDXOn_Click(object sender, RoutedEventArgs e)
        {
            uint v = (((uint)NumericUpDown1.Value - 1) << 8) + (((uint)NumericUpDownJoy.Value - 1) << 28);
            if (PanelBasic1.IsVisible == true)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block =
                [
                    ((byte)CommandType.DxButton + v),
                    (byte)CommandType.Hold,
                    (((byte)CommandType.DxButton | (byte)CommandType.Release) + v),
                ];
                ((EditedMacro)DataContext).Insert(block);
            }
            else
            {
                if (((EditedMacro)DataContext).LimitReached(1)) return;
                ((EditedMacro)DataContext).Insert([((byte)CommandType.DxButton + v)]);
            }
        }

        private void ButtonDXOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            uint v = (((uint)NumericUpDown1.Value - 1) << 8) + (((uint)NumericUpDownJoy.Value - 1) << 28);
            ((EditedMacro)DataContext).Insert([(((byte)CommandType.DxButton | (byte)CommandType.Release) + v)]);
        }

        private void ButtonPovOn_Click(object sender, RoutedEventArgs e)
        {
            uint v = (((uint)NumericUpDownPov.Value - 1) << 16) + (((uint)NumericUpDownPosicion.Value - 1) << 8) + (((uint)NumericUpDownJoy.Value - 1) << 28);
            if (PanelBasic1.IsVisible == true)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block =
                [
                    (byte)CommandType.DxHat + v,
                    (byte)CommandType.Hold,
                    ((byte)CommandType.DxHat | (byte)CommandType.Release) + v,
                ];
                ((EditedMacro)DataContext).Insert(block);
            }
            else
            {
                if (((EditedMacro)DataContext).LimitReached(1)) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.DxHat + v]);
            }
        }

        private void ButtonPovOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            uint v = (((uint)NumericUpDownPov.Value - 1) << 16) + (((uint)NumericUpDownPosicion.Value - 1) << 8) + (((uint)NumericUpDownJoy.Value - 1) << 28);
            ((EditedMacro)DataContext).Insert([((byte)CommandType.DxHat | (byte)CommandType.Release) + v]);
        }

        private void ButtonMove_Click(object sender, RoutedEventArgs e)
        {
            if (ushort.TryParse(cbSensibility.Text, out ushort sb) && (sb > 0) && (sb <= 32768))
            {
                if (((EditedMacro)DataContext).LimitReached(1)) return;
                uint v = ((uint)sb << 8) + (((uint)NumericUpDownJoy.Value - 1) << 28);
                v += (cbAxis.SelectedIndex % 2 == 0) ? ((uint)(cbAxis.SelectedIndex / 2) << 25) : ((uint)((cbAxis.SelectedIndex - 1) / 2) << 25) + 1 << 24;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.DxAxis + v]);
            }
            else
            {
                _ = MessageBox.Show(Translate.Get("value_out_of_range"), Translate.Get("warning"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        public void GoToBasic()
        {
            PanelBasic1.IsVisible = true;
            PanelBasic2.IsVisible = true;
        }

        public void GoToAdvanced()
        {
            PanelBasic1.IsVisible = false;
            PanelBasic2.IsVisible = false;
        }
    }
}
