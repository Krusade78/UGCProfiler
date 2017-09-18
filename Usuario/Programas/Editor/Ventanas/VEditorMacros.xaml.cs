using System;
using System.Windows;
using System.Windows.Controls;
using static Editor.CEnums;

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
            this.padre = (MainWindow)this.Owner;
            indicep = idx;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Iniciar();
        }

        private void ButtonAcepta_Click(object sender, RoutedEventArgs e)
        {
            Guardar();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ButtonBorrar_Click(object sender, RoutedEventArgs e)
        {
            BorrarMacroLista(ListBox1.SelectedIndex, false);
        }

        private void ButtonSubir_Click(object sender, RoutedEventArgs e)
        {
            SubirMacroLista(ListBox1.SelectedIndex);
        }

        private void ButtonBajar_Click(object sender, RoutedEventArgs e)
        {
            BajarMacroLista(ListBox1.SelectedIndex);
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
        private void vtSelPlantilla_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CargarPlantilla(vtSelPlantilla.SelectedIndex);
        }

        private void ButtonPresionar_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237)
                return;
            else
            {
                macro.Insert(GetIndice(), (ushort)(ComboBox1.SelectedIndex << 8));
                CargarLista();
            }
        }

        private void ButtonSoltar_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237)
                return;
            else
            {
                macro.Insert(GetIndice(), (ushort)(32 + (ComboBox1.SelectedIndex << 8)));
                CargarLista();
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
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)TipoC.TipoComando_Modo);
            CargarLista();
        }

        private void ButtonModo2_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)TipoC.TipoComando_Modo + (1 << 8));
            CargarLista();
        }

        private void ButtonModo3_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)TipoC.TipoComando_Modo + (2 << 8));
            CargarLista();
        }

        private void ButtonPinkieOn_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)TipoC.TipoComando_Pinkie + (1 << 8));
            CargarLista();
        }

        private void ButtonPinkieOff_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)TipoC.TipoComando_Pinkie);
            CargarLista();
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
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_Delay + ((ushort)NumericUpDown6.Value << 8)));
            CargarLista();
        }

        private void ButtonRepetirN_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 236) return;
            int idc = GetIndice();
            macro.Insert(idc, (ushort)((byte)TipoC.TipoComando_RepeatN +((ushort)NumericUpDown4.Value << 8)));
            macro.Insert(idc + 1, (byte)TipoC.TipoComando_RepeatNFin);
            CargarLista();
        }
        #endregion

        #region "Ratón"
        private void ButtonIzquierdoOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                macro.Clear();
                ListBox1.Items.Clear();
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt1);
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_Hold);
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt1 + 32);
            }
            else
            {
                if (macro.Count > 237) return;
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt1);
            }
            CargarLista();
        }

        private void ButtonIzquierdoOff_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt1 + 32);
            CargarLista();
        }

        private void ButtonCentroOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                macro.Clear();
                ListBox1.Items.Clear();
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt2);
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_Hold);
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt2 + 32);
            }
            else
            {
                if (macro.Count > 237) return;
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt2 + 32);
            }
            CargarLista();
        }

        private void ButtonCentroOff_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt2 + 32);
            CargarLista();
        }

        private void ButtonDerechoOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                macro.Clear();
                ListBox1.Items.Clear();
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt3);
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_Hold);
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt3 + 32);
            }
            else
            {
                if (macro.Count > 237) return;
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt3);
            }
            CargarLista();
        }

        private void ButtonDerechoOff_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonBt3 + 32);
            CargarLista();
        }

        private void ButtonArribaOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                macro.Clear();
                ListBox1.Items.Clear();
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonWhArr);
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonWhArr + 32);
            }
            else
            {
                if (macro.Count > 237) return;
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonWhArr);
            }
            CargarLista();
        }

        private void ButtonArribaOff_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonWhArr + 32);
            CargarLista();
        }

        private void ButtonAbajoOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                macro.Clear();
                ListBox1.Items.Clear();
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonWhAba);
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonWhAba + 32);
            }
            else
            {
                if (macro.Count > 237) return;
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonWhAba);
            }
            CargarLista();
        }

        private void ButtonAbajoOff_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (byte)TipoC.TipoComando_RatonWhAba + 32);
            CargarLista();
        }

        private void ButtonMovIzquierda_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_RatonIzq + ((ushort)NumericUpDownSensibilidad.Value << 8)));
            CargarLista();
        }

        private void ButtonMovArriba_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_RatonArr + ((ushort)NumericUpDownSensibilidad.Value << 8)));
            CargarLista();
        }

        private void ButtonMovDerecha_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_RatonDer + ((ushort)NumericUpDownSensibilidad.Value << 8)));
            CargarLista();
        }

        private void ButtonMovAbajo_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_RatonAba + ((ushort)NumericUpDownSensibilidad.Value << 8)));
            CargarLista();
        }
        #endregion

        #region "DirectX"
        private void ButtonDXOn_Click(object sender, RoutedEventArgs e)
        {
            if (RadioButtonBasico.IsChecked == true)
            {
                macro.Clear();
                ListBox1.Items.Clear();
                macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_DxBoton + ((ushort)(NumericUpDown1.Value - 1) << 8)));
                macro.Insert(GetIndice(), (byte)TipoC.TipoComando_Hold);
                macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_DxBoton + 32 + ((ushort)(NumericUpDown1.Value - 1) << 8)));
            }
            else
            {
                if (macro.Count > 237) return;
                macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_DxBoton + ((ushort)(NumericUpDown1.Value - 1) << 8)));
            }
            CargarLista();
        }

        private void ButtonDXOff_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_DxBoton + 32 + ((ushort)(NumericUpDown1.Value - 1) << 8)));
            CargarLista();
        }

        private void ButtonPovOn_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_DxSeta + ((ushort)(NumericUpDownPov.Value - 1) << 8)));
            CargarLista();
        }

        private void ButtonPovOff_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_DxSeta + 32 + ((ushort)(NumericUpDownPov.Value - 1) << 8)));
            CargarLista();
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
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (byte)TipoC.TipoComando_MfdPinkie);
            CargarLista();
        }

        private void ButtonX52PinkieOff_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_MfdPinkie + (1 << 8)));
            CargarLista();
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
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (byte)TipoC.TipoComando_InfoLuz);
            CargarLista();
        }

        private void ButtonInfoOff_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_InfoLuz + (1 << 8)));
            CargarLista();
        }

        private void ButtonLuzMfd_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_MfdLuz + ((ushort)NumericUpDownLuzMfd.Value << 8)));
            CargarLista();
        }

        private void ButtonLuzBotones_Click(object sender, RoutedEventArgs e)
        {
            if (macro.Count > 237) return;
            macro.Insert(GetIndice(), (ushort)((byte)TipoC.TipoComando_Luz + ((ushort)NumericUpDownLuzMfd.Value << 8)));
            CargarLista();
        }
        #endregion
    }
}
