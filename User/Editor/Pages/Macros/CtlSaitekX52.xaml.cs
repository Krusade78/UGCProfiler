using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Shared.CTypes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Pages.Macros
{
    public sealed partial class CtlSaitekX52 : UserControl, IChangeMode
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
            Linea();
        }

        private void Button47_Click(object sender, RoutedEventArgs e)
        {
            Hora(true);
        }

        private void Button48_Click(object sender, RoutedEventArgs e)
        {
            Hora(false);
        }

        private void ButtonX52PinkieOn_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([(byte)CommandType.X52MfdPinkie], false);
        }

        private void ButtonX52PinkieOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([((byte)CommandType.X52MfdPinkie + (1 << 8))], false);
        }

        private void ButtonFecha1_Click(object sender, RoutedEventArgs e)
        {
            Fecha(1);
        }

        private void ButtonFecha2_Click(object sender, RoutedEventArgs e)
        {
            Fecha(2);
        }

        private void ButtonFecha3_Click(object sender, RoutedEventArgs e)
        {
            Fecha(3);
        }

        private void ButtonInfoOn_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([(byte)CommandType.X52InfoLight], false);
        }

        private void ButtonInfoOff_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([((byte)CommandType.X52InfoLight + (1 << 8))], false);
        }

        private void ButtonLuzMfd_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([((byte)CommandType.X52Light + ((uint)NumericUpDownLuzMfd.Value << 8))], false);
        }

        private void ButtonLuzBotones_Click(object sender, RoutedEventArgs e)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 237) return;
            ((EditedMacro)DataContext).Insertar([((byte)CommandType.X52Light + ((uint)NumericUpDownLuzMfd.Value << 8))], false);
        }
        #endregion

        #region "MFD"
        private void Linea()
        {
            if ((((EditedMacro)DataContext).GetCuenta() + 2 + TextBox3.Text.Length) > 237)
                return;

            List<uint> block =
            [
                ((byte)CommandType.X52MfdTextIni + ((uint)NumericUpDown9.Value << 8))
            ];
            string st = TextBox3.Text.Replace('�', '�').Replace('�', '�').Replace('�', '�').Replace('�', '�').Replace('�', '�').Replace('�', '�').Replace('�', '�').Replace('�', '�').Replace('�', '�').Replace('�', '�').Replace('�', 'A').Replace('�', 'E').Replace('�', 'I').Replace('�', 'O').Replace('�', 'U');
            byte[] stb = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(st));
            for (byte i = 0; i < st.Length; i++)
            {
                block.Add((uint)((byte)CommandType.X52MfdText + (stb[i] << 8)));
            }
            block.Add((byte)CommandType.X52MfdTextEnd);
            ((EditedMacro)DataContext).Insertar([.. block], false);
        }

        private async void Hora(bool f24h)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 235)
                return;
            if ((NumericUpDown10.Value < 0) && (NumericUpDown7.Value == 1))
            {
                await MessageBox.Show("El reloj 1 no puede tener horas negativas.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            uint[] block = new uint[3];
            CommandType tipo = (f24h) ? CommandType.X52MfdHour24 : CommandType.X52MfdHour;
            block[0] = (byte)tipo + ((uint)NumericUpDown7.Value << 8);
            if (NumericUpDown7.Value == 1)
            {
                block[1] = (uint)((byte)tipo + ((uint)NumericUpDown10.Value << 8));
                block[2] = (uint)((byte)tipo + ((uint)NumericUpDown11.Value << 8));
            }
            else
            {
                int minutos = (int)((NumericUpDown10.Value * 60) + NumericUpDown11.Value);
                if (minutos < 0)
                {
                    block[1] = (byte)tipo + ((((uint)-minutos >> 8) + 4) << 8);
                    block[2] = (byte)tipo + (((uint)-minutos & 0xff) << 8);
                }
                else
                {
                    block[1] = (byte)tipo + (((uint)minutos >> 8) << 8);
                    block[2] = (byte)tipo + (((uint)minutos & 0xff) << 8);
                }
            }
            ((EditedMacro)DataContext).Insertar(block, false);
        }

        private void Fecha(ushort f)
        {
            if (((EditedMacro)DataContext).GetCuenta() > 236) return;
            uint[] block =
            [
                (uint)((byte)CommandType.x52MfdDate + (f << 8)),
                ((byte)CommandType.x52MfdDate + ((uint)NumericUpDown13.Value << 8)),
            ];
            ((EditedMacro)DataContext).Insertar(block, false);
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
