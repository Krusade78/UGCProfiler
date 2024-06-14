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
    public sealed partial class CtlStatusCommands : UserControl, IChangeMode
    {
        public CtlStatusCommands()
        {
            this.InitializeComponent();
            Panel2.Translation += new System.Numerics.Vector3(0, 0, 16);
        }

        #region "comandos de estado"
        private void ButtonMantener_Click_1(object sender, RoutedEventArgs e)
        {
            //Mantener();
        }

        private void ButtonRepetir_Click(object sender, RoutedEventArgs e)
        {
            //Repetir();
        }

        private void ButtonPausa_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)((byte)CommandType.Delay + ((ushort)NumericUpDown6.Valor << 8))], false);
        }

        private void ButtonRepetirN_Click(object sender, RoutedEventArgs e)
        {
            //         if (GetCuenta() > 236) return;
            //         ushort[] bloque =
            //[
            //	(ushort)((byte)CommandType.RepeatN +((ushort)NumericUpDown4.Valor << 8)),
            //	(byte)CommandType.RepeatN | (byte)CommandType.Release,
            //];
            //Insertar(bloque, true);
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

        //#region "comandos estado"
        //private void Mantener()
        //{
        //	if (GetCuenta() > 237)
        //		return;

        //	if (dsMacros.MACROS.DefaultView.Count > 0)
        //	{
        //		if (!ComprobarManternerConRepetir()) return;
        //	}
        //	Insertar([(byte)CommandType.Hold], false);
        //}
        //private void Repetir()
        //{
        //	if (GetCuenta() > 236)
        //		return;

        //	if (dsMacros.MACROS.DefaultView.Count > 0)
        //	{
        //		if (!ComprobarManternerConRepetir()) return;
        //	}
        //	Insertar([(byte)CommandType.Repeat, (byte)CommandType.Repeat | (byte)CommandType.Release], true);
        //}

        //private bool ComprobarManternerConRepetir()
        //{
        //	foreach (DataSetMacros.MACROSRow r in dsMacros.MACROS)
        //	{
        //		byte tipo = (byte)(r.comando[0] & 0x7f);
        //		if ((tipo == (byte)CommandType.Repeat) || (tipo == (byte)CommandType.Hold))
        //		{
        //			MessageBox.Show("Repetir y Mantener deben ser únicos", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
        //			return false;
        //		}
        //	}

        //	if ((ListBox1.SelectedIndex == -1) || (ListBox1.SelectedIndex == 0))
        //	{
        //		return true;
        //	}

        //	int reps = 0;
        //	foreach (System.Data.DataRowView rv in dsMacros.MACROS.DefaultView)
        //	{
        //		if (rv == (System.Data.DataRowView)ListBox1.SelectedItem)
        //		{
        //			break;
        //		}
        //		byte tipo = (byte)(((DataSetMacros.MACROSRow)rv.Row).comando[0] & 0xff);
        //		if ((CommandType)(tipo & 0x7f) == CommandType.RepeatN)
        //		{
        //			if ((tipo & (byte)CommandType.Release) != 0)
        //			reps--;
        //		else
        //			reps++;
        //		}
        //	}
        //	if (reps != 0)
        //	{
        //		MessageBox.Show("Repetir y Mantener no pueden estar dentro de Repetir N.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Information);
        //		return false;
        //	}
        //	return true;
        //}
        //#endregion
    }
}
