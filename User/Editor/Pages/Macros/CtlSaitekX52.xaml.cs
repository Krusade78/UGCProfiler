using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

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

        #region "MFD"
        private void ButtonLinea_Click(object sender, RoutedEventArgs e)
        {
            //Linea();
        }

        private void Button47_Click(object sender, RoutedEventArgs e)
        {
            //Hora(true);
        }

        private void Button48_Click(object sender, RoutedEventArgs e)
        {
            //Hora(false);
        }

        private void ButtonX52PinkieOn_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(byte)CommandType.X52MfdPinkie], false);
        }

        private void ButtonX52PinkieOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)((byte)CommandType.X52MfdPinkie + (1 << 8))], false);
        }

        private void ButtonFecha1_Click(object sender, RoutedEventArgs e)
        {
            //Fecha(1);
        }

        private void ButtonFecha2_Click(object sender, RoutedEventArgs e)
        {
            //Fecha(2);
        }

        private void ButtonFecha3_Click(object sender, RoutedEventArgs e)
        {
            //Fecha(3);
        }

        private void ButtonInfoOn_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(byte)CommandType.X52InfoLight], false);
        }

        private void ButtonInfoOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)((byte)CommandType.X52InfoLight + (1 << 8))], false);
        }

        private void ButtonLuzMfd_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)((byte)CommandType.X52Light + ((ushort)NumericUpDownLuzMfd.Valor << 8))], false);
        }

        private void ButtonLuzBotones_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)((byte)CommandType.X52Light + ((ushort)NumericUpDownLuzMfd.Valor << 8))], false);
        }
        #endregion

        //#region "MFD"
        //private void Linea()
        //{
        //	if ((GetCuenta() + 2 + TextBox3.Text.Length) > 237)
        //		return;

        //	List<ushort> bloque =
        //	[
        //		(ushort)((byte)CommandType.X52MfdTextIni + ((ushort)NumericUpDown9.Valor << 8))
        //	];
        //	string st = TextBox3.Text.Replace('ñ', 'ø').Replace('á', 'Ó').Replace('í', 'ß').Replace('ó', 'Ô').Replace('ú', 'Ò').Replace('Ñ', '£').Replace('ª', 'Ø').Replace('º', '×').Replace('¿', 'ƒ').Replace('¡', 'Ú').Replace('Á', 'A').Replace('É', 'E').Replace('Í', 'I').Replace('Ó', 'O').Replace('Ú', 'U');
        //	byte[] stb = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.GetEncoding(20127), System.Text.Encoding.Unicode.GetBytes(st));
        //	for (byte i = 0; i < st.Length; i++)
        //	{
        //		bloque.Add((ushort)((byte)CommandType.X52MfdText + (stb[i] << 8)));
        //	}
        //	bloque.Add((byte)CommandType.X52MfdTextEnd);
        //	Insertar([.. bloque], false);
        //}

        //private void Hora(bool f24h)
        //{
        //	if (GetCuenta() > 235)
        //		return;
        //	if ((NumericUpDown10.Valor < 0) && (NumericUpDown7.Valor == 1))
        //	{
        //		MessageBox.Show("El reloj 1 no puede tener horas negativas.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
        //		return;
        //	}

        //	ushort[] bloque = new ushort[3];
        //	CommandType tipo = (f24h) ? CommandType.X52MfdHour24 : CommandType.X52MfdHour;
        //	bloque[0] =(ushort)((byte)tipo + ((ushort)NumericUpDown7.Valor << 8));
        //	if (NumericUpDown7.Valor == 1)
        //	{
        //		bloque[1] = (ushort)((byte)tipo + ((ushort)NumericUpDown10.Valor << 8));
        //		bloque[2] = (ushort)((byte)tipo + ((ushort)NumericUpDown11.Valor << 8));
        //	}
        //	else
        //	{
        //		int minutos = (int)((NumericUpDown10.Valor * 60) + NumericUpDown11.Valor);
        //		if (minutos < 0)
        //		{
        //			bloque[1] = (ushort)((byte)tipo + ((((ushort)-minutos >> 8) + 4) << 8) );
        //			bloque[2] = (ushort)((byte)tipo + (((ushort)-minutos & 0xff) << 8) );
        //		}
        //		else
        //		{
        //			bloque[1] = (ushort)((byte)tipo + ((((ushort)minutos >> 8)) << 8));
        //			bloque[2] = (ushort)((byte)tipo + (((ushort)minutos & 0xff) << 8));
        //		}
        //	}
        //	Insertar(bloque, false);
        //}

        //private void Fecha(ushort f)
        //{
        //	if (GetCuenta() > 236) return;
        //	ushort[] bloque =
        //	[
        //		(ushort)((byte)CommandType.x52MfdDate + (f << 8)),
        //		(ushort)((byte)CommandType.x52MfdDate + ((ushort)NumericUpDown13.Valor << 8)),
        //	];
        //	Insertar(bloque, false);
        //}
        //#endregion

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
