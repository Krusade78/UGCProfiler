using System;
using System.Windows;
using System.Windows.Controls;

namespace Editor
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.GetCommandLineArgs().Length == 2)
                Abrir(Environment.GetCommandLineArgs()[0]);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (datos.Perfil.GENERAL.Rows.Count != 0)
            {
                MessageBoxResult r = MessageBox.Show("¿Quieres guardar los cambios?", "Advertencia", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
                if (r == MessageBoxResult.Cancel)
                    e.Cancel = true;
                else if (r == MessageBoxResult.Yes)
                {
                    if (!Guardar())
                        e.Cancel = true;
                }
            }
        }

        #region "Archivo"
        private void RibbonButtonNuevo_Click(object sender, RoutedEventArgs e)
        {
            Nuevo();
        }
        private void RibbonButtonAbrir_Click(object sender, RoutedEventArgs e)
        {
            Abrir();
        }
        private void RibbonButtonGuardar_Click(object sender, RoutedEventArgs e)
        {
            Guardar();
        }
        private void RibbonButtonGuardarComo_Click(object sender, RoutedEventArgs e)
        {
            GuardarComo();
        }
        private void RibbonButtonImprimir_Click(object sender, RoutedEventArgs e)
        {
            //VImprimir imp = new VImprimir();
            //imp.PreVer();
        }
        #endregion

        #region "Perfil"
        private void RibbonButtonLanzar_Click(object sender, RoutedEventArgs e)
        {
            if (datos.Perfil.GENERAL.Rows.Count != 0)
                Lanzar();
        }
        //private void RibbonButtonModos_Click(object sender, RoutedEventArgs e)
        //{
        //    VEditorModos v = new VEditorModos();
        //    v.Owner = this;
        //    if (v.ShowDialog();
        //    RefrescarModos();
        //}
        private void RibbonButtonRaton_Click(object sender, RoutedEventArgs e)
        {
            if (datos.Perfil.GENERAL.Rows.Count != 0)
            {
                VEditorRaton v = new VEditorRaton();
                v.Owner = this;
                v.ShowDialog();
            }
        }
        #endregion

        #region "Vista"
        private void rtbEdicion_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                rtbListado.IsChecked = false;
                if (datos.Perfil.GENERAL.Rows.Count != 0)
                {
                    if (gridVista.Children.Count != 0)
                    {
                        gridVista.Children.RemoveAt(0);
                        gridVista.Children.Clear();
                    }
                    gridVista.Children.Add(new CtlEditar());
                }
            }
        }
        private void rtbListado_Checked(object sender, RoutedEventArgs e)
        {
            rtbEdicion.IsChecked = false;
            if (datos.Perfil.GENERAL.Rows.Count != 0)
            {
                if (gridVista.Children.Count != 0)
                {
                    gridVista.Children.RemoveAt(0);
                    gridVista.Children.Clear();
                }
                gridVista.Children.Add(new CtlListar());
            }
        }
        #endregion

        private void cbModo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
                SetModos();
        }
    }
}
