using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Shared.CTypes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Pages.Macros
{
    public sealed partial class CtlDirectX : UserControl, IChangeMode
    {
        public CtlDirectX()
        {
            this.InitializeComponent();
            Panel3.Translation += new System.Numerics.Vector3(0, 0, 16);
        }

        #region "DirectX"
        private void ButtonDXOn_Click(object sender, RoutedEventArgs e)
        {
            uint v = (((uint)NumericUpDown1.Value - 1) << 8) + (((uint)NumericUpDownJoy.Value - 1) << 16);
            if (PanelBasic1.Visibility == Visibility.Visible)
            {
                if (((EditedMacro)DataContext).GetCount() > 235) return;
                ((EditedMacro)DataContext).Clear();
                uint[] block =
                [
                    ((byte)CommandType.DxButton + v),
                    (byte)CommandType.Hold,
                    (((byte)CommandType.DxButton | (byte)CommandType.Release) + v),
                ];
                ((EditedMacro)DataContext).Insert(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCount() > 237) return;
                ((EditedMacro)DataContext).Insert([((byte)CommandType.DxButton + v)], false);
            }
        }

        private void ButtonDXOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            uint v = (((uint)NumericUpDown1.Value - 1) << 3) + (((uint)NumericUpDownJoy.Value - 1) << 8);
            ((EditedMacro)DataContext).Insert([(ushort)(((byte)CommandType.DxButton | (byte)CommandType.Release) + (ushort)v)], false);
        }

        private void ButtonPovOn_Click(object sender, RoutedEventArgs e)
        {
            uint v = ((((4 - (uint)NumericUpDownPov.Value) * 8) + ((uint)NumericUpDownPosicion.Value - 1)) << 8) + (((uint)NumericUpDownJoy.Value - 1) << 16);
            if (PanelBasic1.Visibility == Visibility.Visible)
            {
                if (((EditedMacro)DataContext).GetCount() > 235) return;
                ((EditedMacro)DataContext).Clear();
                uint[] block =
                [
                    (byte)CommandType.DxHat + v,
                    (byte)CommandType.Hold,
                    ((byte)CommandType.DxHat | (byte)CommandType.Release) + v,
                ];
                ((EditedMacro)DataContext).Insert(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCount() > 237) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.DxHat + v], false);
            }
        }

        private void ButtonPovOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            uint v = ((((4 - (uint)NumericUpDownPov.Value) * 8) + ((uint)NumericUpDownPosicion.Value - 1)) << 8) + (((uint)NumericUpDownJoy.Value - 1) << 16);
            ((EditedMacro)DataContext).Insert([((byte)CommandType.DxHat | (byte)CommandType.Release) + v], false);
        }

        private void ButtonMove_Click(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(cbSensibility.Text, out uint v) && (v > 0) && (v < 32769))
            {
                if (((EditedMacro)DataContext).GetCount() > 237) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.DxAxis + (v << 8)], false);
            }   
            else
            {
                _ = MessageBox.Show(Translate.Get("value_out_of_range"), Translate.Get("warning"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        public void GoToBasic()
        {
            PanelBasic1.Visibility = Visibility.Visible;
            PanelBasic2.Visibility = Visibility.Visible;
        }

        public void GoToAdvanced()
        {
            PanelBasic1.Visibility = Visibility.Collapsed;
            PanelBasic2.Visibility = Visibility.Collapsed;
        }
    }
}
