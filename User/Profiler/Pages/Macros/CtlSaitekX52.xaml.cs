using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using static Shared.CTypes;

namespace Profiler.Pages.Macros
{
    public partial class CtlSaitekX52 : UserControl, IChangeMode
    {
        public CtlSaitekX52()
        {
            this.InitializeComponent();
        }

        public void Load(bool MFDName) => CheckBox1.IsChecked = MFDName;
        public bool GetNameOnMFD() => CheckBox1.IsChecked == true;

        #region "MFD"
        private void ButtonLinea_Click(object sender, RoutedEventArgs e)
        {
            Line();
        }

        private void Button47_Click(object sender, RoutedEventArgs e)
        {
            Hour(true);
        }

        private void Button48_Click(object sender, RoutedEventArgs e)
        {
            Hour(false);
        }

        private void ButtonX52PinkieOn_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.X52MfdPinkie]);
        }

        private void ButtonX52PinkieOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.X52MfdPinkie + (1 << 8))]);
        }

        private void ButtonFecha1_Click(object sender, RoutedEventArgs e)
        {
            Date(1);
        }

        private void ButtonFecha2_Click(object sender, RoutedEventArgs e)
        {
            Date(2);
        }

        private void ButtonFecha3_Click(object sender, RoutedEventArgs e)
        {
            Date(3);
        }

        private void ButtonInfoOn_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([(byte)CommandType.X52InfoLight]);
        }

        private void ButtonInfoOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.X52InfoLight + (1 << 8))]);
        }

        private void ButtonLuzMfd_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.X52Light + ((uint)NumericUpDownLuzMfd.Value << 8))]);
        }

        private void ButtonLuzBotones_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).LimitReached(1)) return;
            ((EditedMacro)DataContext).Insert([((byte)CommandType.X52Light + ((uint)NumericUpDownLuzMfd.Value << 8))]);
        }
        #endregion

        #region "Texts"
        private void Line()
        {
            if (((EditedMacro)DataContext).LimitReached((byte)(2 + TextBox3.Text.Length)))
                return;

            List<uint> block =
            [
                ((byte)CommandType.X52MfdTextIni + ((uint)NumericUpDown9.Value << 8))
            ];
            string st = TextBox3.Text.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
            byte[] stb = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(st));
            for (byte i = 0; i < st.Length; i++)
            {
                block.Add((uint)((byte)CommandType.X52MfdText + (stb[i] << 8)));
            }
            block.Add((byte)CommandType.X52MfdTextEnd);
            ((EditedMacro)DataContext).Insert([.. block], true);
        }

        private async void Hour(bool f24h)
        {
            if (((EditedMacro)DataContext).LimitReached(3))
                return;
            if ((NumericUpDown10.Value < 0) && (NumericUpDown7.Value == 1))
            {
                await MessageBox.Show("El reloj 1 no puede tener horas negativas.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            uint[] block = new uint[3];
            CommandType type = (f24h) ? CommandType.X52MfdHour24 : CommandType.X52MfdHour;
            block[0] = (byte)type + ((uint)NumericUpDown7.Value << 8);
            if (NumericUpDown7.Value == 1)
            {
                block[1] = (uint)((byte)type + ((uint)NumericUpDown10.Value << 8));
                block[2] = (uint)((byte)type + ((uint)NumericUpDown11.Value << 8));
            }
            else
            {
                int minutes = (int)((NumericUpDown10.Value * 60) + NumericUpDown11.Value);
                if (minutes < 0)
                {
                    block[1] = (byte)type + ((((uint)-minutes >> 8) + 4) << 8);
                    block[2] = (byte)type + (((uint)-minutes & 0xff) << 8);
                }
                else
                {
                    block[1] = (byte)type + (((uint)minutes >> 8) << 8);
                    block[2] = (byte)type + (((uint)minutes & 0xff) << 8);
                }
            }
            ((EditedMacro)DataContext).Insert(block, true);
        }

        private void Date(ushort f)
        {
            if (((EditedMacro)DataContext).LimitReached(2)) return;
            uint[] block =
            [
                (uint)((byte)CommandType.x52MfdDate + (f << 8)),
                ((byte)CommandType.x52MfdDate + ((uint)NumericUpDown13.Value << 8)),
            ];
            ((EditedMacro)DataContext).Insert(block, true);
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
    }
}
