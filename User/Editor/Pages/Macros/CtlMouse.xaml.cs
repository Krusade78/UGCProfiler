using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Shared.CTypes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Pages.Macros
{
    public sealed partial class CtlMouse : UserControl
    {
        public CtlMouse()
        {
            this.InitializeComponent();
            Panel1.Translation += new System.Numerics.Vector3(0, 0, 16);
        }

        #region "Ratón"
        private void ButtonIzquierdoOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.Visibility == Visibility.Visible)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block =
                [
                    (byte)CommandType.MouseBt1,
                    (byte)CommandType.Hold,
                    (byte)CommandType.MouseBt1 | (byte)CommandType.Release,
                ];
                ((EditedMacro)DataContext).Insertar(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCuenta() > 237) return;
                ((EditedMacro)DataContext).Insertar([(byte)CommandType.MouseBt1], false);
            }
        }

        private void ButtonIzquierdoOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([(byte)CommandType.MouseBt1 | (byte)CommandType.Release], false);
        }

        private void ButtonCentroOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.Visibility == Visibility.Visible)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block =
                [
                    (byte)CommandType.MouseBt2,
                    (byte)CommandType.Hold,
                    (byte)CommandType.MouseBt2 | (byte)CommandType.Release,
                ];
                ((EditedMacro)DataContext).Insertar(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCuenta() > 237) return;
                ((EditedMacro)DataContext).Insertar([(byte)CommandType.MouseBt2], false);
            }
        }

        private void ButtonCentroOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([(byte)CommandType.MouseBt2 | (byte)CommandType.Release], false);
        }

        private void ButtonDerechoOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.Visibility == Visibility.Visible)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block =
                [
                    (byte)CommandType.MouseBt3,
                    (byte)CommandType.Hold,
                    (byte)CommandType.MouseBt3 | (byte)CommandType.Release,
                ];
                ((EditedMacro)DataContext).Insertar(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCuenta() > 237) return;
                ((EditedMacro)DataContext).Insertar([(byte)CommandType.MouseBt3], false);
            }
        }

        private void ButtonDerechoOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([(byte)CommandType.MouseBt3 | (byte)CommandType.Release], false);
        }

        private void ButtonArribaOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.Visibility == Visibility.Visible)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block = [(byte)CommandType.MouseWhUp, (byte)CommandType.MouseWhUp | (byte)CommandType.Release];
                ((EditedMacro)DataContext).Insertar(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCuenta() > 237) return;
                ((EditedMacro)DataContext).Insertar([(byte)CommandType.MouseWhUp], false);
            }
        }

        private void ButtonArribaOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([(byte)CommandType.MouseWhUp | (byte)CommandType.Release], false);
        }

        private void ButtonAbajoOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.Visibility == Visibility.Visible)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block = [(byte)CommandType.MouseWhDown, (byte)CommandType.MouseWhDown | (byte)CommandType.Release];
                ((EditedMacro)DataContext).Insertar(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCuenta() > 237) return;
                ((EditedMacro)DataContext).Insertar([(byte)CommandType.MouseWhDown], false);
            }
        }

        private void ButtonAbajoOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([(byte)CommandType.MouseWhDown | (byte)CommandType.Release], false);
        }

        private void ButtonMovIzquierda_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([((byte)CommandType.MouseLeft + ((uint)NumericUpDownSensibilidad.Value << 8))], false);
        }

        private void ButtonMovArriba_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([((byte)CommandType.MouseUp + ((uint)NumericUpDownSensibilidad.Value << 8))], false);
        }

        private void ButtonMovDerecha_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([((byte)CommandType.MouseRight + ((uint)NumericUpDownSensibilidad.Value << 8))], false);
        }

        private void ButtonMovAbajo_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([((byte)CommandType.MouseDown + ((uint)NumericUpDownSensibilidad.Value << 8))], false);
        }
        #endregion

        public void GoToBasic()
        {
            PanelBasic1.Visibility = Visibility.Visible;
            PanelBasic2.Visibility = Visibility.Visible;
            PanelBasic3.Visibility = Visibility.Visible;
        }

        public void GoToAdvanced()
        {
            PanelBasic1.Visibility = Visibility.Collapsed;
            PanelBasic2.Visibility = Visibility.Collapsed;
            PanelBasic3.Visibility = Visibility.Collapsed;
        }
    }
}
