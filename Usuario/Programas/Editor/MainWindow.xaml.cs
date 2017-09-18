using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Editor
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.GetCommandLineArgs().Length == 1)
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
            //GuardarComo();
        }
        private void RibbonButtonImprimir_Click(object sender, RoutedEventArgs e)
        {
            //VImprimir imp = new VImprimir();
            //imp.PreVer();
        }

        #region "Perfil"
        private void RibbonButtonLanzar_Click(object sender, RoutedEventArgs e)
        {
            //Lanzar();
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
            VEditorRaton v = new VEditorRaton();
            v.Owner = this;
            v.ShowDialog();
        }

        private void RibbonButtonEdicion_Click(object sender, RoutedEventArgs e)
        {
            //if (report != null)
            //{
            //    Fondo.Visible = True;
            //    this.Controls.Remove(report);
            //    report = null;
            //}
        }
        private void RibbonButtonListado_Click(object sender, RoutedEventArgs e)
        {
            //if (report == null)
            //{
            //    report = new VistaReport();
            //    this.Controls.Add(report);
            //    Fondo.Visible = False;
            //}
        }
        #endregion
    }
}
