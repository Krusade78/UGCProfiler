using System;
using System.Windows;
using System.Windows.Controls;
using static Shared.CTypes;

namespace Editor
{
    /// <summary>
    /// Lógica de interacción para VEditorMacros.xaml
    /// </summary>
    internal partial class VEditorMacros : Window
    {
        public VEditorMacros(int idx)
        {
            InitializeComponent();
            indicep = idx;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.padre = (MainWindow)this.Owner;
            Iniciar();
        }

        private void ButtonAcepta_Click(object sender, RoutedEventArgs e)
        {
            Guardar();
            this.Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ButtonBorrar_Click(object sender, RoutedEventArgs e)
        {
            BorrarMacroLista();
        }

        private void ButtonSubir_Click(object sender, RoutedEventArgs e)
        {
            SubirMacroLista();
        }

        private void ButtonBajar_Click(object sender, RoutedEventArgs e)
        {
            BajarMacroLista();
        }

        private void RadioButtonBasico_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
                PasarABasico();
        }

        private void RadioButtonAvanzado_Checked(object sender, RoutedEventArgs e)
        {
            PasarAAvanzado();
        }

        #region "teclas"
        private void FvtSelPlantilla_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarPlantilla();
        }

        private void ButtonPresionar_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237)
                return;
            else
            {
                Insertar([(ushort)((byte)CommandType.Key + (ushort)(ComboBox1.SelectedIndex << 8))], false);
            }
        }

        private void ButtonSoltar_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237)
                return;
            else
            {
                Insertar([(ushort)(((byte)CommandType.Key | (byte)CommandType.Release) + (ushort)(ComboBox1.SelectedIndex << 8))], false);
            }
        }

        private void ButtonNormal_Click(object sender, RoutedEventArgs e)
        {
            TeclasPulsar(false);
        }

        private void ButtonMantener_Click(object sender, RoutedEventArgs e)
        {
            TeclasPulsar(true);
        }

        private bool teclasActivado = false;
        private void TextBoxTecla_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxTecla.Background = System.Windows.Media.Brushes.Black;
            TextBoxTecla.Foreground = System.Windows.Media.Brushes.GreenYellow;
            TextBoxTecla.FontWeight = FontWeights.Normal;
            teclasActivado = false;
        }

        private void TextBoxTecla_GotFocus(object sender, RoutedEventArgs e)
        {
            teclas.Clear();
            TextBoxTecla.Text = "";
            TextBoxTecla.Background = System.Windows.Media.Brushes.LimeGreen;
            TextBoxTecla.Foreground = System.Windows.Media.Brushes.Black;
            TextBoxTecla.FontWeight = FontWeights.Bold;
            teclasActivado = true;
        }

        private void TextBoxTecla_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (teclasActivado)
            {
                LeerTeclado();
                e.Handled = true;
            }
        }

        private void TextBoxTecla_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (teclasActivado)
            {
                LeerTeclado();
                e.Handled = true;
            }
        }
        #endregion

        #region "Modos"
        private void ButtonModo1_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)CommandType.Mode], false);
        }

        private void ButtonModo2_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)CommandType.Mode + (1 << 8)] ,false);
        }

        private void ButtonModo3_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)CommandType.Mode + (2 << 8)], false);
        }

        private void ButtonPinkieOn_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)CommandType.SubMode + (1 << 8)], false);
        }

        private void ButtonPinkieOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)CommandType.SubMode], false);
        }

        private void ButtonPrecisoOn_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            ushort dato = (ushort)(((byte)((cbJoy.SelectedIndex * 8) + cbEje.SelectedIndex) | ((NumericUpDownPr.Valor - 1) << 5)) << 8);
            Insertar([(ushort)((ushort)CommandType.PrecisionMode + dato)], false);
        }

        private void ButtonPrecisoOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            ushort dato = (ushort)(((byte)((cbJoy.SelectedIndex * 8) + cbEje.SelectedIndex) | (byte)CommandType.Release) << 8); 
            Insertar([(ushort)(((byte)CommandType.PrecisionMode | (byte)CommandType.Release) + dato)], false);
        }
        #endregion

        #region "comandos de estado"
        private void ButtonMantener_Click_1(object sender, RoutedEventArgs e)
        {
            Mantener();
        }

        private void ButtonRepetir_Click(object sender, RoutedEventArgs e)
        {
            Repetir();
        }

        private void ButtonPausa_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)((byte)CommandType.Delay + ((ushort)NumericUpDown6.Valor << 8))], false);
        }

        private void ButtonRepetirN_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 236) return;
            ushort[] bloque =
			[
				(ushort)((byte)CommandType.RepeatN +((ushort)NumericUpDown4.Valor << 8)),
				(byte)CommandType.RepeatN | (byte)CommandType.Release,
			];
			Insertar(bloque, true);
        }
        #endregion

        #region "Ratón"
        private void ButtonIzquierdoOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                dsMacros.MACROS.Clear();
                ushort[] bloque =
				[
					(byte)CommandType.MouseBt1,
					(byte)CommandType.Hold,
					(byte)CommandType.MouseBt1 | (byte)CommandType.Release,
				];
				Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar([(byte)CommandType.MouseBt1], false);
            }
        }

        private void ButtonIzquierdoOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(byte)CommandType.MouseBt1 | (byte)CommandType.Release], false);
        }

        private void ButtonCentroOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                dsMacros.MACROS.Clear();
                ushort[] bloque =
				[
					(byte)CommandType.MouseBt2,
					(byte)CommandType.Hold,
					(byte)CommandType.MouseBt2 | (byte)CommandType.Release,
				];
				Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar([(byte)CommandType.MouseBt2], false);
            }
        }

        private void ButtonCentroOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(byte)CommandType.MouseBt2 | (byte)CommandType.Release] ,false);
        }

        private void ButtonDerechoOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                dsMacros.MACROS.Clear();
                ushort[] bloque =
				[
					(byte)CommandType.MouseBt3,
					(byte)CommandType.Hold,
					(byte)CommandType.MouseBt3 | (byte)CommandType.Release,
				];
				Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar([(byte)CommandType.MouseBt3], false);
            }
        }

        private void ButtonDerechoOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(byte)CommandType.MouseBt3 | (byte)CommandType.Release], false);
        }

        private void ButtonArribaOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                dsMacros.MACROS.Clear();
                ushort[] bloque = [(byte)CommandType.MouseWhUp, (byte)CommandType.MouseWhUp | (byte)CommandType.Release];
				Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar([(byte)CommandType.MouseWhUp], false);
            }
        }

        private void ButtonArribaOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(byte)CommandType.MouseWhUp | (byte)CommandType.Release], false);
        }

        private void ButtonAbajoOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                dsMacros.MACROS.Clear();
                ushort[] bloque = [(byte)CommandType.MouseWhDown, (byte)CommandType.MouseWhDown | (byte)CommandType.Release];
				Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar([(byte)CommandType.MouseWhDown], false);
            }
        }

        private void ButtonAbajoOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(byte)CommandType.MouseWhDown | (byte)CommandType.Release], false);
        }

        private void ButtonMovIzquierda_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)((byte)CommandType.MouseLeft + ((ushort)NumericUpDownSensibilidad.Valor << 8))], false);
        }

        private void ButtonMovArriba_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)((byte)CommandType.MouseUp + ((ushort)NumericUpDownSensibilidad.Valor << 8))], false);
        }

        private void ButtonMovDerecha_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)((byte)CommandType.MouseRight + ((ushort)NumericUpDownSensibilidad.Valor << 8))], false);
        }

        private void ButtonMovAbajo_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)((byte)CommandType.MouseDown + ((ushort)NumericUpDownSensibilidad.Valor << 8))] , false);
        }
        #endregion

        #region "DirectX"
        private void ButtonDXOn_Click(object sender, RoutedEventArgs e)
        {
            int v = (((NumericUpDown1.Valor - 1) << 3) + (NumericUpDownJoy.Valor - 1)) << 8;
            if (RadioButtonBasico.IsChecked == true)
            {
                if (GetCuenta() > 235) return;
                dsMacros.MACROS.Clear();
                ushort[] bloque =
				[
					(ushort)((byte)CommandType.DxButton + (ushort)v),
					(byte)CommandType.Hold,
					(ushort)(((byte)CommandType.DxButton | (byte)CommandType.Release) + (ushort)v),
				];
				Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar([(ushort)((byte)CommandType.DxButton + (ushort)v)], false);
            }
        }

        private void ButtonDXOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            int v = (((NumericUpDown1.Valor - 1) << 3) + (NumericUpDownJoy.Valor - 1)) << 8;
            Insertar([(ushort)(((byte)CommandType.DxButton | (byte)CommandType.Release) + (ushort)v)], false);
        }

        private void ButtonPovOn_Click(object sender, RoutedEventArgs e)
        {
            int v = (((((4 - NumericUpDownPov.Valor) * 8) + (NumericUpDownPosicion.Valor - 1)) << 3) + (NumericUpDownJoy.Valor - 1)) << 8;
            if (RadioButtonBasico.IsChecked == true)
            {
                if (GetCuenta() > 235) return;
                dsMacros.MACROS.Clear();
                ushort[] bloque =
				[
					(ushort)((byte)CommandType.DxHat + (ushort)v),
					(byte)CommandType.Hold,
					(ushort)(((byte)CommandType.DxHat | (byte)CommandType.Release) + (ushort)v),
				];
				Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar([(ushort)((byte)CommandType.DxHat + (ushort)v)], false);
            }
        }

        private void ButtonPovOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            int v = (((((4 - NumericUpDownPov.Valor) * 8) + (NumericUpDownPosicion.Valor - 1)) << 3) + (NumericUpDownJoy.Valor - 1)) << 8;
            Insertar([(ushort)(((byte)CommandType.DxHat | (byte)CommandType.Release) + (ushort)v)], false);
        }

        #endregion

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
            if (GetCuenta() > 237) return;
            Insertar([(byte)CommandType.X52MfdPinkie], false);
        }

        private void ButtonX52PinkieOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)((byte)CommandType.X52MfdPinkie + (1 << 8))], false);
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
            if (GetCuenta() > 237) return;
            Insertar([(byte)CommandType.X52InfoLight], false);
        }

        private void ButtonInfoOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)((byte)CommandType.X52InfoLight + (1 << 8))], false);
        }

        private void ButtonLuzMfd_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)((byte)CommandType.X52Light + ((ushort)NumericUpDownLuzMfd.Valor << 8))], false);
        }

        private void ButtonLuzBotones_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar([(ushort)((byte)CommandType.X52Light + ((ushort)NumericUpDownLuzMfd.Valor << 8))], false);
        }
        #endregion

        #region "Leds NXT"
        private void ButtonLed_Click(object sender, RoutedEventArgs e)
        {
            Leds((byte)((cbLed.SelectedIndex == 1) ? 11 : ((cbLed.SelectedIndex == 2) ? 10 : 0)) , (CEnums.LedOrder)cbOrden.SelectedIndex, (CEnums.ModoColor)cbModo.SelectedIndex, txtColor1.Text, txtColor2.Text);
        }

        private void FtxtColor1_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Ventanas.VColor vc = new(txtColor1.Text)
            {
                Owner = this
            };
            if (vc.ShowDialog() == true)
            {
                txtColor1.Text = vc.ColorSt;
                System.Windows.Media.Color c = vc.ColorSB;
                if (cbLed.SelectedIndex == 0)
                {
                    txtColor1.Text = "0;0;" + txtColor1.Text.Remove(0, 4);
                    c.R = 0;
                    c.G = 0;
                }
                else if (cbLed.SelectedIndex == 1)
                {
                    txtColor1.Text = txtColor1.Text[..1] + ";0;0;";
                    c.B = 0;
                    c.G = 0;
                }
                rColor1.Fill = new System.Windows.Media.SolidColorBrush(c);
            }
        }

        private void FtxtColor2_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Ventanas.VColor vc = new (txtColor2.Text)
            {
                Owner = this
            };
            if (vc.ShowDialog() == true)
            {
                txtColor2.Text = vc.ColorSt;
                System.Windows.Media.Color c = vc.ColorSB;
                if (cbLed.SelectedIndex == 0)
                {
                    txtColor1.Text = txtColor1.Text[..1] + ";0;0;";
                    c.B = 0;
                    c.G = 0;
                }
                rColor2.Fill = new System.Windows.Media.SolidColorBrush(c);
            }
        }

        private void FcbLed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded) return;
            ((ComboBoxItem)cbModo.Items[1]).IsEnabled = true;
            ((ComboBoxItem)cbModo.Items[2]).IsEnabled = true;
            ((ComboBoxItem)cbModo.Items[3]).IsEnabled = true;
            ((ComboBoxItem)cbModo.Items[4]).IsEnabled = true;
            ((ComboBoxItem)cbModo.Items[6]).IsEnabled = true;
            if (cbLed.SelectedIndex == 0)
            {
                txtColor1.Text = "0;0;7";
                txtColor2.Text = "7;0;0";
                txtColor2.IsEnabled = true;
                rColor1.Fill = System.Windows.Media.Brushes.Blue;
                rColor2.Fill = System.Windows.Media.Brushes.Red;
                ((ComboBoxItem)cbModo.Items[2]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[3]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[4]).IsEnabled = false;
            }
            else if (cbLed.SelectedIndex == 1)
            {
                txtColor1.Text = "7;0;0";
                txtColor2.Text = "0;0;0";
                txtColor2.IsEnabled = false;
                rColor1.Fill = System.Windows.Media.Brushes.Red;
                rColor2.Fill = System.Windows.Media.Brushes.Black;
                ((ComboBoxItem)cbModo.Items[1]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[2]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[3]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[4]).IsEnabled = false;
                ((ComboBoxItem)cbModo.Items[6]).IsEnabled = false;
            }
            else
            {
                txtColor1.Text = "7;7;7";
                txtColor2.Text = "7;7;7";
                txtColor1.IsEnabled = true;
                txtColor2.IsEnabled = true;
                rColor1.Fill = System.Windows.Media.Brushes.White;
                rColor2.Fill = System.Windows.Media.Brushes.White;
            }
        }
        #endregion
    }
}
