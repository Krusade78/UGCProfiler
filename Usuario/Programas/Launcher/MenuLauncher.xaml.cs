using System;
using System.Windows;


namespace Launcher
{
    /// <summary>
    /// Lógica de interacción para UserControl1.xaml
    /// </summary>
    public partial class MenuLauncher : Window
    {
        private bool salir = false;

        public MenuLauncher()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate();
            this.ContextMenu.IsOpen = true;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (!salir)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void MenuSalir_Click(object sender, RoutedEventArgs e)
        {
            salir = true;
            this.DialogResult = true;
            this.Close();
        }
    }
}
