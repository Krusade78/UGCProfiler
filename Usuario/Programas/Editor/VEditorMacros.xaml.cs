using System;
using System.Windows;
using System.Windows.Controls;

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
        #endregion

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
            teclas.Clear();
            TextBoxTecla.Text = "";
            TextBoxTecla.Background = System.Windows.Media.Brushes.Black;
            TextBoxTecla.Foreground = System.Windows.Media.Brushes.GreenYellow;
            TextBoxTecla.FontWeight = FontWeights.Normal;
            teclasActivado = false;
        }

        private void TextBoxTecla_GotFocus(object sender, RoutedEventArgs e)
        {
            teclas.Clear();
            TextBoxTecla.Text = "";
            TextBoxTecla.Background = System.Windows.Media.Brushes.GreenYellow;
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
    }
}
