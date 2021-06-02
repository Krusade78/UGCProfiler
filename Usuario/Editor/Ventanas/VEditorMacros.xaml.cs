using System;
using System.Windows;
using System.Windows.Controls;
using static Comunes.CTipos;

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
                Insertar(new ushort[] {(ushort)((byte)TipoComando.TipoComando_Tecla + (ushort)(ComboBox1.SelectedIndex << 8)) }, false);
            }
        }

        private void ButtonSoltar_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237)
                return;
            else
            {
                Insertar(new ushort[] { (ushort)(((byte)TipoComando.TipoComando_Tecla | (byte)TipoComando.TipoComando_Soltar) + (ushort)(ComboBox1.SelectedIndex << 8)) }, false);
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
            Insertar(new ushort[] { (ushort)TipoComando.TipoComando_Modo }, false);
        }

        private void ButtonModo2_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)TipoComando.TipoComando_Modo + (1 << 8) } ,false);
        }

        private void ButtonModo3_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)TipoComando.TipoComando_Modo + (2 << 8) }, false);
        }

        private void ButtonPinkieOn_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)TipoComando.TipoComando_Pinkie + (1 << 8) }, false);
        }

        private void ButtonPinkieOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)TipoComando.TipoComando_Pinkie }, false);
        }

        private void ButtonPrecisoOn_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            ushort dato = (ushort)(((byte)((cbJoy.SelectedIndex * 8) + (cbEje.SelectedIndex)) | ((NumericUpDownPr.Valor - 1) << 5)) << 8);
            Insertar(new ushort[] { (ushort)((ushort)TipoComando.TipoComando_ModoPreciso + dato) }, false);
        }

        private void ButtonPrecisoOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            ushort dato = (ushort)(((byte)((cbJoy.SelectedIndex * 8) + (cbEje.SelectedIndex)) | (byte)TipoComando.TipoComando_Soltar) << 8); 
            Insertar(new ushort[] { (ushort)(((byte)TipoComando.TipoComando_ModoPreciso | (byte)TipoComando.TipoComando_Soltar) + dato) }, false);
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
            Insertar(new ushort[] { (ushort)((byte)TipoComando.TipoComando_Delay + ((ushort)NumericUpDown6.Valor << 8)) }, false);
        }

        private void ButtonRepetirN_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 236) return;
            ushort[] bloque = new ushort[2];
            bloque[0] = (ushort)((byte)TipoComando.TipoComando_RepeatN +((ushort)NumericUpDown4.Valor << 8));
            bloque[1] = (byte)TipoComando.TipoComando_RepeatN | (byte)TipoComando.TipoComando_Soltar;
            Insertar(bloque, true);
        }
        #endregion

        #region "Ratón"
        private void ButtonIzquierdoOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                dsMacros.MACROS.Clear();
                ushort[] bloque = new ushort[3];
                bloque[0] = (byte)TipoComando.TipoComando_RatonBt1;
                bloque[1] = (byte)TipoComando.TipoComando_Hold;
                bloque[2] = (byte)TipoComando.TipoComando_RatonBt1 | (byte)TipoComando.TipoComando_Soltar;
                Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar(new ushort[] { (byte)TipoComando.TipoComando_RatonBt1 }, false);
            }
        }

        private void ButtonIzquierdoOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (byte)TipoComando.TipoComando_RatonBt1 | (byte)TipoComando.TipoComando_Soltar }, false);
        }

        private void ButtonCentroOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                dsMacros.MACROS.Clear();
                ushort[] bloque = new ushort[3];
                bloque[0] = (byte)TipoComando.TipoComando_RatonBt2;
                bloque[1] = (byte)TipoComando.TipoComando_Hold;
                bloque[2] = (byte)TipoComando.TipoComando_RatonBt2 | (byte)TipoComando.TipoComando_Soltar;
                Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar(new ushort[] { (byte)TipoComando.TipoComando_RatonBt2}, false);
            }
        }

        private void ButtonCentroOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (byte)TipoComando.TipoComando_RatonBt2 | (byte)TipoComando.TipoComando_Soltar } ,false);
        }

        private void ButtonDerechoOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                dsMacros.MACROS.Clear();
                ushort[] bloque = new ushort[3];
                bloque[0] = (byte)TipoComando.TipoComando_RatonBt3;
                bloque[1] = (byte)TipoComando.TipoComando_Hold;
                bloque[2] = (byte)TipoComando.TipoComando_RatonBt3 | (byte)TipoComando.TipoComando_Soltar;
                Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar(new ushort[] { (byte)TipoComando.TipoComando_RatonBt3 }, false);
            }
        }

        private void ButtonDerechoOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (byte)TipoComando.TipoComando_RatonBt3 | (byte)TipoComando.TipoComando_Soltar }, false);
        }

        private void ButtonArribaOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                dsMacros.MACROS.Clear();
                ushort[] bloque = new ushort[2];
                bloque[0] = (byte)TipoComando.TipoComando_RatonWhArr;
                bloque[1] = (byte)TipoComando.TipoComando_RatonWhArr | (byte)TipoComando.TipoComando_Soltar;
                Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar(new ushort[] { (byte)TipoComando.TipoComando_RatonWhArr }, false);
            }
        }

        private void ButtonArribaOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (byte)TipoComando.TipoComando_RatonWhArr | (byte)TipoComando.TipoComando_Soltar }, false);
        }

        private void ButtonAbajoOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                dsMacros.MACROS.Clear();
                ushort[] bloque = new ushort[2];
                bloque[0] = (byte)TipoComando.TipoComando_RatonWhAba;
                bloque[1] = (byte)TipoComando.TipoComando_RatonWhAba | (byte)TipoComando.TipoComando_Soltar;
                Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar(new ushort[] { (byte)TipoComando.TipoComando_RatonWhAba }, false);
            }
        }

        private void ButtonAbajoOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (byte)TipoComando.TipoComando_RatonWhAba | (byte)TipoComando.TipoComando_Soltar }, false);
        }

        private void ButtonMovIzquierda_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)((byte)TipoComando.TipoComando_RatonIzq + ((ushort)NumericUpDownSensibilidad.Valor << 8)) }, false);
        }

        private void ButtonMovArriba_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)((byte)TipoComando.TipoComando_RatonArr + ((ushort)NumericUpDownSensibilidad.Valor << 8)) }, false);
        }

        private void ButtonMovDerecha_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)((byte)TipoComando.TipoComando_RatonDer + ((ushort)NumericUpDownSensibilidad.Valor << 8)) }, false);
        }

        private void ButtonMovAbajo_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)((byte)TipoComando.TipoComando_RatonAba + ((ushort)NumericUpDownSensibilidad.Valor << 8)) } , false);
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
                ushort[] bloque = new ushort[3];                
                bloque[0] = (ushort)((byte)TipoComando.TipoComando_DxBoton + (ushort)v);
                bloque[1] = (byte)TipoComando.TipoComando_Hold;
                bloque[2] = (ushort)(((byte)TipoComando.TipoComando_DxBoton | (byte)TipoComando.TipoComando_Soltar) + (ushort)v);
                Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar(new ushort[] { (ushort)((byte)TipoComando.TipoComando_DxBoton + (ushort)v) }, false);
            }
        }

        private void ButtonDXOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            int v = (((NumericUpDown1.Valor - 1) << 3) + (NumericUpDownJoy.Valor - 1)) << 8;
            Insertar(new ushort[] { (ushort)(((byte)TipoComando.TipoComando_DxBoton | (byte)TipoComando.TipoComando_Soltar) + (ushort)v) }, false);
        }

        private void ButtonPovOn_Click(object sender, RoutedEventArgs e)
        {
            int v = (((((4 - NumericUpDownPov.Valor) * 8) + (NumericUpDownPosicion.Valor - 1)) << 3) + (NumericUpDownJoy.Valor - 1)) << 8;
            if (RadioButtonBasico.IsChecked == true)
            {
                if (GetCuenta() > 235) return;
                dsMacros.MACROS.Clear();
                ushort[] bloque = new ushort[3];
                bloque[0] = (ushort)((byte)TipoComando.TipoComando_DxSeta + (ushort)v);
                bloque[1] = (byte)TipoComando.TipoComando_Hold;
                bloque[2] = (ushort)(((byte)TipoComando.TipoComando_DxSeta | (byte)TipoComando.TipoComando_Soltar) + (ushort)v);
                Insertar(bloque, true);
            }
            else
            {
                if (GetCuenta() > 237) return;
                Insertar(new ushort[] { (ushort)((byte)TipoComando.TipoComando_DxSeta + (ushort)v) }, false);
            }
        }

        private void ButtonPovOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            int v = (((((4 - NumericUpDownPov.Valor) * 8) + (NumericUpDownPosicion.Valor - 1)) << 3) + (NumericUpDownJoy.Valor - 1)) << 8;
            Insertar(new ushort[] { (ushort)(((byte)TipoComando.TipoComando_DxSeta | (byte)TipoComando.TipoComando_Soltar) + (ushort)v) }, false);
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
            Insertar(new ushort[] { (byte)TipoComando.TipoComando_MfdPinkie }, false);
        }

        private void ButtonX52PinkieOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)((byte)TipoComando.TipoComando_MfdPinkie + (1 << 8)) }, false);
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
            Insertar(new ushort[] { (byte)TipoComando.TipoComando_InfoLuz }, false);
        }

        private void ButtonInfoOff_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)((byte)TipoComando.TipoComando_InfoLuz + (1 << 8)) }, false);
        }

        private void ButtonLuzMfd_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)((byte)TipoComando.TipoComando_MfdLuz + ((ushort)NumericUpDownLuzMfd.Valor << 8)) }, false);
        }

        private void ButtonLuzBotones_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuenta() > 237) return;
            Insertar(new ushort[] { (ushort)((byte)TipoComando.TipoComando_Luz + ((ushort)NumericUpDownLuzMfd.Valor << 8)) }, false);
        }
        #endregion

        #region "Leds NXT"
        private void ButtonLed_Click(object sender, RoutedEventArgs e)
        {
            Leds((byte)((cbLed.SelectedIndex == 1) ? 11 : ((cbLed.SelectedIndex == 2) ? 10 : 0)) , (CEnums.OrdenLed)cbOrden.SelectedIndex, (CEnums.ModoColor)cbModo.SelectedIndex, txtColor1.Text, txtColor2.Text);
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
                    txtColor1.Text = txtColor1.Text.Substring(0, 1) + ";0;0;";
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
                    txtColor1.Text = txtColor1.Text.Substring(0, 1) + ";0;0;";
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
