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
            //        if (RadioButtonBasico.IsChecked == true)
            //        {
            //            dsMacros.MACROS.Clear();
            //            ushort[] bloque =
            //[
            //	(byte)CommandType.MouseBt1,
            //	(byte)CommandType.Hold,
            //	(byte)CommandType.MouseBt1 | (byte)CommandType.Release,
            //];
            //Insertar(bloque, true);
            //        }
            //        else
            //        {
            //            if (GetCuenta() > 237) return;
            //            Insertar([(byte)CommandType.MouseBt1], false);
            //        }
        }

        private void ButtonIzquierdoOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(byte)CommandType.MouseBt1 | (byte)CommandType.Release], false);
        }

        private void ButtonCentroOn_Click(object sender, RoutedEventArgs e)
        {
            //        if (RadioButtonBasico.IsChecked == true)
            //        {
            //            dsMacros.MACROS.Clear();
            //            ushort[] bloque =
            //[
            //	(byte)CommandType.MouseBt2,
            //	(byte)CommandType.Hold,
            //	(byte)CommandType.MouseBt2 | (byte)CommandType.Release,
            //];
            //Insertar(bloque, true);
            //        }
            //        else
            //        {
            //            if (GetCuenta() > 237) return;
            //            Insertar([(byte)CommandType.MouseBt2], false);
            //        }
        }

        private void ButtonCentroOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(byte)CommandType.MouseBt2 | (byte)CommandType.Release] ,false);
        }

        private void ButtonDerechoOn_Click(object sender, RoutedEventArgs e)
        {
            //        if (RadioButtonBasico.IsChecked == true)
            //        {
            //            dsMacros.MACROS.Clear();
            //            ushort[] bloque =
            //[
            //	(byte)CommandType.MouseBt3,
            //	(byte)CommandType.Hold,
            //	(byte)CommandType.MouseBt3 | (byte)CommandType.Release,
            //];
            //Insertar(bloque, true);
            //        }
            //        else
            //        {
            //            if (GetCuenta() > 237) return;
            //            Insertar([(byte)CommandType.MouseBt3], false);
            //        }
        }

        private void ButtonDerechoOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(byte)CommandType.MouseBt3 | (byte)CommandType.Release], false);
        }

        private void ButtonArribaOn_Click(object sender, RoutedEventArgs e)
        {
            //        if (RadioButtonBasico.IsChecked == true)
            //        {
            //            dsMacros.MACROS.Clear();
            //            ushort[] bloque = [(byte)CommandType.MouseWhUp, (byte)CommandType.MouseWhUp | (byte)CommandType.Release];
            //Insertar(bloque, true);
            //        }
            //        else
            //        {
            //            if (GetCuenta() > 237) return;
            //            Insertar([(byte)CommandType.MouseWhUp], false);
            //        }
        }

        private void ButtonArribaOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(byte)CommandType.MouseWhUp | (byte)CommandType.Release], false);
        }

        private void ButtonAbajoOn_Click(object sender, RoutedEventArgs e)
        {
            //        if (RadioButtonBasico.IsChecked == true)
            //        {
            //            dsMacros.MACROS.Clear();
            //            ushort[] bloque = [(byte)CommandType.MouseWhDown, (byte)CommandType.MouseWhDown | (byte)CommandType.Release];
            //Insertar(bloque, true);
            //        }
            //        else
            //        {
            //            if (GetCuenta() > 237) return;
            //            Insertar([(byte)CommandType.MouseWhDown], false);
            //        }
        }

        private void ButtonAbajoOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(byte)CommandType.MouseWhDown | (byte)CommandType.Release], false);
        }

        private void ButtonMovIzquierda_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)((byte)CommandType.MouseLeft + ((ushort)NumericUpDownSensibilidad.Valor << 8))], false);
        }

        private void ButtonMovArriba_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)((byte)CommandType.MouseUp + ((ushort)NumericUpDownSensibilidad.Valor << 8))], false);
        }

        private void ButtonMovDerecha_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)((byte)CommandType.MouseRight + ((ushort)NumericUpDownSensibilidad.Valor << 8))], false);
        }

        private void ButtonMovAbajo_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)((byte)CommandType.MouseDown + ((ushort)NumericUpDownSensibilidad.Valor << 8))] , false);
        }
        #endregion
    }
}
