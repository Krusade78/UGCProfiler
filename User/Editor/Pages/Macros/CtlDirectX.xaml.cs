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
            //        int v = (((NumericUpDown1.Valor - 1) << 3) + (NumericUpDownJoy.Valor - 1)) << 8;
            //        if (RadioButtonBasico.IsChecked == true)
            //        {
            //            if (GetCuenta() > 235) return;
            //            dsMacros.MACROS.Clear();
            //            ushort[] bloque =
            //[
            //	(ushort)((byte)CommandType.DxButton + (ushort)v),
            //	(byte)CommandType.Hold,
            //	(ushort)(((byte)CommandType.DxButton | (byte)CommandType.Release) + (ushort)v),
            //];
            //Insertar(bloque, true);
            //        }
            //        else
            //        {
            //            if (GetCuenta() > 237) return;
            //            Insertar([(ushort)((byte)CommandType.DxButton + (ushort)v)], false);
            //        }
        }

        private void ButtonDXOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //int v = (((NumericUpDown1.Valor - 1) << 3) + (NumericUpDownJoy.Valor - 1)) << 8;
            //Insertar([(ushort)(((byte)CommandType.DxButton | (byte)CommandType.Release) + (ushort)v)], false);
        }

        private void ButtonPovOn_Click(object sender, RoutedEventArgs e)
        {
            //        int v = (((((4 - NumericUpDownPov.Valor) * 8) + (NumericUpDownPosicion.Valor - 1)) << 3) + (NumericUpDownJoy.Valor - 1)) << 8;
            //        if (RadioButtonBasico.IsChecked == true)
            //        {
            //            if (GetCuenta() > 235) return;
            //            dsMacros.MACROS.Clear();
            //            ushort[] bloque =
            //[
            //	(ushort)((byte)CommandType.DxHat + (ushort)v),
            //	(byte)CommandType.Hold,
            //	(ushort)(((byte)CommandType.DxHat | (byte)CommandType.Release) + (ushort)v),
            //];
            //Insertar(bloque, true);
            //        }
            //        else
            //        {
            //            if (GetCuenta() > 237) return;
            //            Insertar([(ushort)((byte)CommandType.DxHat + (ushort)v)], false);
            //        }
        }

        private void ButtonPovOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //int v = (((((4 - NumericUpDownPov.Valor) * 8) + (NumericUpDownPosicion.Valor - 1)) << 3) + (NumericUpDownJoy.Valor - 1)) << 8;
            //Insertar([(ushort)(((byte)CommandType.DxHat | (byte)CommandType.Release) + (ushort)v)], false);
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
