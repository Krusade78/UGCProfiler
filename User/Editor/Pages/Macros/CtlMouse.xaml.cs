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

        #region "Mouse"
        private void ButtonLeftOn_Click(object sender, RoutedEventArgs e)
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
                ((EditedMacro)DataContext).Insert(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCount() > 237) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt1], false);
            }
        }

        private void ButtonLeftOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt1 | (byte)CommandType.Release], false);
        }

        private void ButtonCenterOn_Click(object sender, RoutedEventArgs e)
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
                ((EditedMacro)DataContext).Insert(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCount() > 237) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt2], false);
            }
        }

        private void ButtonCenterOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt2 | (byte)CommandType.Release], false);
        }

        private void ButtonRightOn_Click(object sender, RoutedEventArgs e)
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
                ((EditedMacro)DataContext).Insert(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCount() > 237) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt3], false);
            }
        }

        private void ButtonRightOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt3 | (byte)CommandType.Release], false);
        }

        private void ButtonUpOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.Visibility == Visibility.Visible)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block = [(byte)CommandType.MouseWhUp, (byte)CommandType.MouseWhUp | (byte)CommandType.Release];
                ((EditedMacro)DataContext).Insert(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCount() > 237) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseWhUp], false);
            }
        }

        private void ButtonUpOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseWhUp | (byte)CommandType.Release], false);
        }

        private void ButtonDownOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.Visibility == Visibility.Visible)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block = [(byte)CommandType.MouseWhDown, (byte)CommandType.MouseWhDown | (byte)CommandType.Release];
                ((EditedMacro)DataContext).Insert(block, true);
            }
            else
            {
                if (((EditedMacro)DataContext).GetCount() > 237) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseWhDown], false);
            }
        }

        private void ButtonDownOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseWhDown | (byte)CommandType.Release], false);
        }

        private void ButtonMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.MouseLeft + ((uint)NumericUpDownSensibilidad.Value << 8))], false);
        }

        private void ButtonMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.MouseUp + ((uint)NumericUpDownSensibilidad.Value << 8))], false);
        }

        private void ButtonMoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.MouseRight + ((uint)NumericUpDownSensibilidad.Value << 8))], false);
        }

        private void ButtonMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCount() > 237) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.MouseDown + ((uint)NumericUpDownSensibilidad.Value << 8))], false);
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
