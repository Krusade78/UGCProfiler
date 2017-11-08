using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;


namespace Launcher
{
    /// <summary>
    /// Lógica de interacción para UserControl1.xaml
    /// </summary>
    internal partial class MenuLauncher : Window
    {
        private bool salir = false;
        private CServicio svc;

        public MenuLauncher(CServicio svc)
        {
            this.svc = svc;
            InitializeComponent();     
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarListaArchivos();
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

        private void CargarListaArchivos()
        {
            foreach (String f in Directory.GetFiles(".", "*.xhp"))
            {
                MenuItem miL = new MenuItem();
                miL.Header = Path.GetFileName(f).Remove(Path.GetFileName(f).Length - 4, 4);
                miL.Click += MenuItemLanzar_Click;
                mnLanzar.Items.Add(miL);
                MenuItem miE = new MenuItem();
                miE.Header = Path.GetFileName(f).Remove(Path.GetFileName(f).Length - 4, 4);
                miE.Click += MenuItemEditar_Click;
                mnEditar.Items.Add(miE);
            }
        }

        private void MenuItemLanzar_Click(object sender, RoutedEventArgs e)
        {
            svc.CargarPerfil(Directory.GetCurrentDirectory() + "\\" + (String)((MenuItem)sender).Header + ".xhp");
        }

        private void MenuItemEditar_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("Editor.exe", "\"" + (String)((MenuItem)sender).Header + ".xhp\"");
        }

        private void MenuEditor_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("Editor.exe");
        }

        private void MenuCalibrador_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("Calibrator.exe");
        }
    }
}
