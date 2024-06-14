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
    public sealed partial class CtlModes : UserControl, IChangeMode
    {
        public CtlModes()
        {
            this.InitializeComponent();
            Panel2.Translation += new System.Numerics.Vector3(0, 0, 16);
        }


        #region "Modos"
        private void ButtonModo1_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)CommandType.Mode], false);
        }

        private void ButtonModo2_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)CommandType.Mode + (1 << 8)] ,false);
        }

        private void ButtonModo3_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)CommandType.Mode + (2 << 8)], false);
        }

        private void ButtonPinkieOn_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)CommandType.SubMode + (1 << 8)], false);
        }

        private void ButtonPinkieOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //Insertar([(ushort)CommandType.SubMode], false);
        }

        private void ButtonPrecisoOn_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //ushort dato = (ushort)(((byte)((cbJoy.SelectedIndex * 8) + cbEje.SelectedIndex) | ((NumericUpDownPr.Valor - 1) << 5)) << 8);
            //Insertar([(ushort)((ushort)CommandType.PrecisionMode + dato)], false);
        }

        private void ButtonPrecisoOff_Click(object sender, RoutedEventArgs e)
        {
            //if (GetCuenta() > 237) return;
            //ushort dato = (ushort)(((byte)((cbJoy.SelectedIndex * 8) + cbEje.SelectedIndex) | (byte)CommandType.Release) << 8); 
            //Insertar([(ushort)(((byte)CommandType.PrecisionMode | (byte)CommandType.Release) + dato)], false);
        }
        #endregion

        public void GoToBasic()
        {
        }

        public void GoToAdvanced()
        {
        }
    }
}
