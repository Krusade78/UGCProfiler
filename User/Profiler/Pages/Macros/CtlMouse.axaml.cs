using Avalonia.Interactivity;
using Avalonia.Controls;
using static Shared.CTypes;

namespace Profiler.Pages.Macros
{
    public partial class CtlMouse : UserControl
    {
        public CtlMouse()
        {
            this.InitializeComponent();
        }

        #region "Mouse"
        private void ButtonLeftOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.IsVisible == true)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block =
                [
                    (byte)CommandType.MouseBt1,
                    (byte)CommandType.Hold,
                    (byte)CommandType.MouseBt1 | (byte)CommandType.Release,
                ];
                ((EditedMacro)DataContext).Insert(block);
            }
            else
            {
                if (((EditedMacro)DataContext).LimitReached(1)) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt1]);
            }
        }

        private void ButtonLeftOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt1 | (byte)CommandType.Release]);
        }

        private void ButtonCenterOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.IsVisible == true)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block =
                [
                    (byte)CommandType.MouseBt2,
                    (byte)CommandType.Hold,
                    (byte)CommandType.MouseBt2 | (byte)CommandType.Release,
                ];
                ((EditedMacro)DataContext).Insert(block);
            }
            else
            {
                if (((EditedMacro)DataContext).LimitReached(1)) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt2]);
            }
        }

        private void ButtonCenterOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt2 | (byte)CommandType.Release]);
        }

        private void ButtonRightOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.IsVisible == true)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block =
                [
                    (byte)CommandType.MouseBt3,
                    (byte)CommandType.Hold,
                    (byte)CommandType.MouseBt3 | (byte)CommandType.Release,
                ];
                ((EditedMacro)DataContext).Insert(block);
            }
            else
            {
                if (((EditedMacro)DataContext).LimitReached(1)) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt3]);
            }
        }

        private void ButtonRightOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseBt3 | (byte)CommandType.Release]);
        }

        private void ButtonUpOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.IsVisible == true)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block = [(byte)CommandType.MouseWhUp, (byte)CommandType.MouseWhUp | (byte)CommandType.Release];
                ((EditedMacro)DataContext).Insert(block);
            }
            else
            {
                if (((EditedMacro)DataContext).LimitReached(1)) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseWhUp]);
            }
        }

        private void ButtonUpOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseWhUp | (byte)CommandType.Release]);
        }

        private void ButtonDownOn_Click(object sender, RoutedEventArgs e)
        {
            if (PanelBasic1.IsVisible == true)
            {
                ((EditedMacro)DataContext).Clear();
                uint[] block = [(byte)CommandType.MouseWhDown, (byte)CommandType.MouseWhDown | (byte)CommandType.Release];
                ((EditedMacro)DataContext).Insert(block);
            }
            else
            {
                if (((EditedMacro)DataContext).LimitReached(1)) return;
                ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseWhDown]);
            }
        }

        private void ButtonDownOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.MouseWhDown | (byte)CommandType.Release]);
        }

        private void ButtonMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.MouseLeft + ((uint)NumericUpDownSensibilidad.Value << 8))]);
        }

        private void ButtonMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.MouseUp + ((uint)NumericUpDownSensibilidad.Value << 8))]);
        }

        private void ButtonMoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.MouseRight + ((uint)NumericUpDownSensibilidad.Value << 8))]);
        }

        private void ButtonMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.MouseDown + ((uint)NumericUpDownSensibilidad.Value << 8))]);
        }
        #endregion

        public void GoToBasic()
        {
            PanelBasic1.IsVisible = true;
            PanelBasic2.IsVisible = true;
            PanelBasic3.IsVisible = true;
        }

        public void GoToAdvanced()
        {
            PanelBasic1.IsVisible = false;
            PanelBasic2.IsVisible = false;
            PanelBasic3.IsVisible = false;
        }
    }
}
